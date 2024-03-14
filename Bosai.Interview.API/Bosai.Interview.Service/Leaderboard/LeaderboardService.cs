using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using Bosai.Interview.Service.Contracts.Leaderboard;
using Bosai.Interview.Service.Contracts.Leaderboard.Dto;
using System.Buffers;

namespace Bosai.Interview.Service.Leaderboard
{
    /// <summary>
    /// Leaderboard handler
    /// </summary>
    public class LeaderboardService : ILeaderboardService
    {
        private readonly ConcurrentDictionary<long, ScoreCustomerKey> _customerScores;
        private readonly SortedDictionary<ScoreCustomerKey, (long CustomerId, double Score)> _leaderboard;
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
        private readonly ScoreUpdateQueue _scoreQueue;
        private readonly ArrayPool<GetLeaderboardResponse> _arrayPool;
        public LeaderboardService()
        {
            _customerScores = new ConcurrentDictionary<long, ScoreCustomerKey>();
            _leaderboard = new SortedDictionary<ScoreCustomerKey, (long CustomerId, double Score)>();
            _scoreQueue = new ScoreUpdateQueue();
            _arrayPool = ArrayPool<GetLeaderboardResponse>.Shared;
        }

        /// <summary>
        /// Update Customer Score
        /// </summary>
        /// <param name="customer"></param>
        /// <returns></returns>
        public async Task<double> UpdateScore(UpdateScoreRequest customer)
        {
            _lock.EnterWriteLock();
            try
            {
                var scoreKey = new ScoreCustomerKey(customer.score, customer.customerid);

                //if customer exists then update score
                if (_customerScores.TryGetValue(customer.customerid, out ScoreCustomerKey? currentScoreKey))
                {
                    if (customer.score != currentScoreKey.Score)
                    {
                        _customerScores[customer.customerid] = scoreKey;
                        await UpdateLeaderboard(customer.customerid, scoreKey);
                    }
                }
                else
                {
                    _customerScores.TryAdd(customer.customerid, scoreKey);
                    //Update Leaderboard
                    await UpdateLeaderboard(customer.customerid, scoreKey);
                }
                return scoreKey.Score;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public async Task UpdateLeaderboard(long customerId, ScoreCustomerKey scoreKey)
        {
            if (_leaderboard.ContainsKey(scoreKey))
            {
                _leaderboard.Remove(scoreKey);
            }

            if (scoreKey.Score > 0)
            {
                //Insert Queue
                _scoreQueue.EnqueueUpdate(scoreKey.CustomerId, scoreKey.Score);
                _leaderboard.Add(scoreKey, (customerId, scoreKey.Score));
            }
        }

        /// <summary>
        /// Get customer by rank
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<List<GetLeaderboardResponse>> GetCustomersByRank(GetCustomersByRankRequest request)
        {
            if (request.Start.HasValue && request.End.HasValue)
            {
                var leaderboardCount = _leaderboard.Count;
                if (request.Start > leaderboardCount)
                    return new List<GetLeaderboardResponse>();
                int rank = request.Start.Value;
                _lock.EnterReadLock();
                try
                {
                    // Calculate the actual score range
                    var startScore = _leaderboard.Keys.ElementAt(leaderboardCount - request.Start.Value);
                    var endCount = request.End.Value > leaderboardCount ? 0 : leaderboardCount - request.End.Value;
                    var endScore = _leaderboard.Keys.ElementAt(endCount);

                    // Gets customer scores for the specified ranking range
                    var rankedScores = _leaderboard.Keys.Where(score => score.Score >= endScore.Score && score.Score <= startScore.Score).Reverse().ToList();

                    var responses = _arrayPool.Rent(rankedScores.Count);
                    try
                    {
                        for (int i = 0; i < rankedScores.Count; i++)
                        {
                            responses[i] = new GetLeaderboardResponse
                            {
                                CustomerId = rankedScores[i].CustomerId,
                                Score = rankedScores[i].Score,
                                Rank = rank + i
                            };
                        }
                        return new List<GetLeaderboardResponse>(responses.Where(t => t != null));
                    }
                    finally
                    {
                        _arrayPool.Return(responses);
                    }
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }
            return new List<GetLeaderboardResponse>();
        }

        /// <summary>
        /// Get customer by customerid and high/low
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<List<GetLeaderboardResponse>> GetCustomersByCustomerId(long customerId, GetCustomersRequest request)
        {
            var neighbors = new List<GetLeaderboardResponse>();
            if (!_customerScores.TryGetValue(customerId, out ScoreCustomerKey? customerScoreKey))
            {
                return neighbors;
            }

            if (_customerScores.TryGetValue(customerId, out ScoreCustomerKey? scoreKey))
            {
                // if customers exist, get their scores and rankings
                if (_leaderboard.TryGetValue(scoreKey, out var leaderboardEntry))
                {
                    var leaderboard = _leaderboard.Keys.Reverse().ToList();

                    var index = leaderboard.IndexOf(scoreKey);
                    if (index != -1)
                    {
                        var topItems = new List<ScoreCustomerKey>();
                        var bottomItems = new List<ScoreCustomerKey>();

                        // Calculates where in the array you need to start fetching elements
                        int startIdx = Math.Max(0, index - request.High);
                        // Calculate how many elements you need to take
                        int countToTake = Math.Min(index, request.High);

                        topItems.AddRange(leaderboard.Skip(startIdx).Take(countToTake));

                        //Calculates the lowest index of the element to be extracted, ensuring that it does not exceed the lower bound of the array
                        int lowAdjustedForCount = Math.Min(_leaderboard.Count - index - 1, request.Low);
                        int lowCountToTake = Math.Min(lowAdjustedForCount + 1, _leaderboard.Count - index);

                        var bottomItemsSubset = leaderboard.Skip(index).Take(lowCountToTake);

                        bottomItems.AddRange(bottomItemsSubset);

                        foreach (var entry in topItems.Concat(bottomItems))
                        {
                            neighbors.Add(new GetLeaderboardResponse { CustomerId = entry.CustomerId, Score = entry.Score, Rank = leaderboard.IndexOf(entry) + 1 });
                        }
                    }
                }
            }
            return neighbors;
        }

        public async Task<GetLeaderboardResponse> GetCustomerRank(long customerID)
        {
            if (_customerScores.TryGetValue(customerID, out ScoreCustomerKey scoreKey))
            {
                // Customers exist, get their scores and rankings
                if (_leaderboard.TryGetValue(scoreKey, out var leaderboardEntry))
                {
                    return new GetLeaderboardResponse { CustomerId = leaderboardEntry.CustomerId, Score = scoreKey.Score, Rank = _leaderboard.Keys.ToList().IndexOf(scoreKey) + 1 };
                }
            }
            return new GetLeaderboardResponse();
        }


        public async Task<List<GetLeaderboardResponse>> GetLeaderboard()
        {
            var leaderboard = new List<GetLeaderboardResponse>();
            foreach (var item in _leaderboard)
            {
                leaderboard.Add(new GetLeaderboardResponse { CustomerId = item.Value.CustomerId, Score = item.Key.Score, Rank = _leaderboard.Keys.ToList().IndexOf(item.Key) + 1 });
            }
            return leaderboard;
        }
    }
}


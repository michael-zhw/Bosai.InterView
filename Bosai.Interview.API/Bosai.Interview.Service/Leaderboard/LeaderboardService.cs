using System;
using System.Collections.Generic;
using Bosai.Interview.Entity;
using Bosai.Interview.Service.Contracts.Leaderboard;
using Bosai.Interview.Service.Contracts.Leaderboard.Dto;

namespace Bosai.Interview.Service.Leaderboard
{
    public class LeaderboardService : ILeaderboardService
    {
        private readonly List<Customer> _customers = new();
        private readonly List<Customer> _leaderboard = new();
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
        public LeaderboardService() { }

        /// <summary>
        /// Use insertion sort
        /// </summary>
        /// <param name="customer"></param>
        /// <returns></returns>
        public async Task UpdateRank(Customer customer)
        {
            _lock.EnterWriteLock();
            try
            {
                var existCustomers = _leaderboard.FirstOrDefault(t => t.CustomerId == customer.CustomerId);
                if (existCustomers != null)
                {
                    //update score
                    existCustomers.Score = customer.Score;
                    _leaderboard.Remove(existCustomers);
                }

                int i = _leaderboard.Count - 1;
                //Adds to the end of the list
                _leaderboard.Add(customer);

                //Use insertion sort logic to move new customers to the correct location
                while (i >= 0 && (_leaderboard[i].Score < customer.Score || (_leaderboard[i].Score == customer.Score && _leaderboard[i].CustomerId > customer.CustomerId)))
                {
                    _leaderboard[i + 1] = _leaderboard[i];
                    i--;
                }
                _leaderboard[i + 1] = customer;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Update customer score
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<decimal> UpdateScore(UpdateScoreRequest request)
        {
            var customer = _customers.FirstOrDefault(t => t.CustomerId == request.customerid);
            if (customer == null)
            {
                customer = new Entity.Customer { CustomerId = request.customerid, Score = request.score };
                _customers.Add(customer);
            }
            else
            {
                customer.Score += request.score;
            }
            await UpdateRank(customer);
            return customer.Score;

        }

        /// <summary>
        /// Get customer by rank
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<List<Entity.Customer>> GetCustomersByRank(GetCustomersByRankRequest request)
        {
            if (request.Start.HasValue && request.End.HasValue)
            {
                _lock.EnterReadLock();
                try
                {
                    int startIndex = request.Start.Value - 1;
                    int endIndex = request.End > _leaderboard.Count ? _leaderboard.Count : request.End.Value - 1;
                    var customerInRange = _leaderboard.Skip(startIndex).Take(endIndex - startIndex + 1).ToList();
                    return customerInRange;
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }
            return new List<Customer>();
        }

        /// <summary>
        /// Get customer by customerid and high/low
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<List<Entity.Customer>> GetCustomersByCustomerId(long customerId, GetCustomersRequest request)
        {
            var customers = new List<Customer>();
            _lock.EnterReadLock();
            try
            {
                int index = _leaderboard.FindIndex(c => c.CustomerId == customerId);
                if (index <= -1)
                {
                    return new List<Customer>();
                }

                var targetCustomer = _leaderboard[index];
                var highRankingNeighborsStartIndex = Math.Max(0, index - request.High);
                var highRankCustomers = _leaderboard.Skip(highRankingNeighborsStartIndex).Take(index - highRankingNeighborsStartIndex).ToList();
                var lowRankingNeighbors = _leaderboard.Skip(index + 1).Take(request.Low).ToList();

                customers.AddRange(highRankCustomers);
                customers.Add(targetCustomer);
                customers.AddRange(lowRankingNeighbors);

                return customers;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public async Task<List<Customer>> GetAllCustomers()
        {
            return _leaderboard.ToList();
        }
    }
}


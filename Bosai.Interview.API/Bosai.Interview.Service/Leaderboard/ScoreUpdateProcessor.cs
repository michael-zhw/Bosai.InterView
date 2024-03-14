using System;
using System.Collections.Concurrent;
using Bosai.Interview.Entity;
using Bosai.Interview.Service.Contracts.Leaderboard;
using Bosai.Interview.Service.Contracts.Leaderboard.Dto;

namespace Bosai.Interview.Service.Leaderboard
{
    /// <summary>
    /// Consumer queue handler
    /// </summary>
    public class ScoreUpdateProcessor
    {
        private CancellationTokenSource _cancellationTokenSource;
        private readonly ScoreUpdateQueue _updateQueue;
        private readonly ILeaderboardService _leaderboard;

        public ScoreUpdateProcessor(ScoreUpdateQueue updateQueue,ILeaderboardService leaderboardService)
        {
            _updateQueue = updateQueue;
            _leaderboard = leaderboardService;
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public void StartProcessing()
        {
            CancellationToken token = _cancellationTokenSource.Token;
            Task.Run(() =>
            {
                while (!token.IsCancellationRequested)
                {
                    if (_updateQueue.GetPendingUpdateCount() > 0)
                    {
                        ScoreUpdate? update = null;
                        if (_updateQueue.TryDequeueUpdate(out update))
                        {
                            if (update != null)
                            {
                                ProcessScoreUpdate(update);
                            }
                        }
                        else
                        {
                            Thread.Sleep(1000);
                        }
                    }
                }
            });
        }

        public void StopProcessing()
        {
            _cancellationTokenSource.Cancel();
        }

        public void RestartProcessing()
        {
            StopProcessing();
            _cancellationTokenSource = new CancellationTokenSource();
            StartProcessing();
        }

        private async void ProcessScoreUpdate(ScoreUpdate update)
        {
            var scoreKey = new ScoreCustomerKey(update.ScoreChange, update.CustomerId);
            await _leaderboard.UpdateLeaderboard(update.CustomerId, scoreKey);
        }
    }
}


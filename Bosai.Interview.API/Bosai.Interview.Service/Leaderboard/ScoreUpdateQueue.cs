using System;
using System.Collections.Concurrent;

namespace Bosai.Interview.Service.Leaderboard
{
    /// <summary>
    /// ConcurrentQueue handler
    /// </summary>
    public class ScoreUpdateQueue
    {
        private ConcurrentQueue<ScoreUpdate> _updateQueue;
        public ScoreUpdateQueue()
        {
            _updateQueue = new ConcurrentQueue<ScoreUpdate>();
        }

        public void EnqueueUpdate(long customerId, double scoreChange)
        {
            _updateQueue.Enqueue(new ScoreUpdate { CustomerId = customerId, ScoreChange = scoreChange, Timestamp = DateTime.Now });
        }

        public bool TryDequeueUpdate(out ScoreUpdate? update)
        {
            return _updateQueue.TryDequeue(out update);
        }

        public int GetPendingUpdateCount()
        {
            return _updateQueue.Count;
        }
    }

    public class ScoreUpdate
    {
        public long CustomerId { get; set; }
        public double ScoreChange { get; set; }
        public DateTime Timestamp { get; set; }
    }
}


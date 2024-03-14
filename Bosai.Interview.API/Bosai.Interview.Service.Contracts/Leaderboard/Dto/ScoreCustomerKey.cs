using System;
using Bosai.Interview.Entity;

namespace Bosai.Interview.Service.Contracts.Leaderboard.Dto
{
    /// <summary>
    /// SortedDictionary handler
    /// </summary>
    public class ScoreCustomerKey : IComparable<ScoreCustomerKey>
    {
        public double Score { get; }
        public long CustomerId { get; }
        public ScoreCustomerKey(double score, long customerId)
        {
            Score = score;
            CustomerId = customerId;
        }

        public int CompareTo(ScoreCustomerKey? other)
        {
            //compare score
            if (Score != other?.Score)
            {
                return Score.CompareTo(other?.Score);
            }
            //if score same then compare customerid
            return other.CustomerId.CompareTo(CustomerId);
        }

        public override bool Equals(object? obj)
        {
            return obj is ScoreCustomerKey other && CompareTo(other) == 0;
        }

        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                int hash = 17;
                hash = hash * 23 + Score.GetHashCode();
                hash = hash * 23 + CustomerId.GetHashCode();
                return hash;
            }
        }

        public static bool operator ==(ScoreCustomerKey left, ScoreCustomerKey right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ScoreCustomerKey left, ScoreCustomerKey right)
        {
            return !left.Equals(right);
        }
    }
}


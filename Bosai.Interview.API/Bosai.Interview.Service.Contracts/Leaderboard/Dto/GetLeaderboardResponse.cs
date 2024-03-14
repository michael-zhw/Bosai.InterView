using System;
namespace Bosai.Interview.Service.Contracts.Leaderboard.Dto
{
	public class GetLeaderboardResponse
	{
        public long CustomerId { get; set; }
        public double Score { get; set; }
        public int Rank { get; set; }
    }
}


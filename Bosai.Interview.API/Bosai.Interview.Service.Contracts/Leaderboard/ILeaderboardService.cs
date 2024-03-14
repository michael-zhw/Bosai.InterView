using System;
using Bosai.Interview.Entity;
using Bosai.Interview.Service.Contracts.Leaderboard.Dto;

namespace Bosai.Interview.Service.Contracts.Leaderboard
{
    public interface ILeaderboardService
    {
        Task<double> UpdateScore(UpdateScoreRequest request);
        Task<List<GetLeaderboardResponse>> GetCustomersByRank(GetCustomersByRankRequest request);
        Task<List<GetLeaderboardResponse>> GetCustomersByCustomerId(long customerId, GetCustomersRequest request);
        //Task<List<Customer>> GetAllCustomers();
        Task UpdateLeaderboard(long customerId, ScoreCustomerKey scoreKey);
        Task<List<GetLeaderboardResponse>> GetLeaderboard();
    }
}


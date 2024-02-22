using System;
using Bosai.Interview.Entity;
using Bosai.Interview.Service.Contracts.Leaderboard.Dto;

namespace Bosai.Interview.Service.Contracts.Leaderboard
{
    public interface ILeaderboardService
    {
        Task<decimal> UpdateScore(UpdateScoreRequest request);
        Task<List<Entity.Customer>> GetCustomersByRank(GetCustomersByRankRequest request);
        Task<List<Entity.Customer>> GetCustomersByCustomerId(long customerId, GetCustomersRequest request);
        Task<List<Customer>> GetAllCustomers();
    }
}


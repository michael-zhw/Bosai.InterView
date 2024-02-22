using System;
using System.ComponentModel.DataAnnotations;

namespace Bosai.Interview.Service.Contracts.Leaderboard.Dto
{
    public class GetCustomersByRankRequest
    {
        [Range(1, int.MaxValue, ErrorMessage = "The start value is invalid ")]
        public int? Start { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "The end value is invalid")]
        public int? End { get; set; }
    }
}


using System;
using System.ComponentModel.DataAnnotations;

namespace Bosai.Interview.Service.Contracts.Leaderboard.Dto
{
    public class GetCustomersRequest
    {
        [Range(0, int.MaxValue, ErrorMessage = "The high value is invalid ")]
        public int High { get; set; } = 0;
        [Range(0, int.MaxValue, ErrorMessage = "The low value is invalid ")]
        public int Low { get; set; } = 0;
    }
}


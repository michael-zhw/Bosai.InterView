using System;
using System.ComponentModel.DataAnnotations;

namespace Bosai.Interview.Service.Contracts.Leaderboard.Dto
{
    public class UpdateScoreRequest
    {
        [Required]
        public long customerid { get; set; }

        [Required]
        [Range(-1000, 1000, ErrorMessage = "Score must be between -1000 and 1000")]
        public double score { get; set; }
    }
}
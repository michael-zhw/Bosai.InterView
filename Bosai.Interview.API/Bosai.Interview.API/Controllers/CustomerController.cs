using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bosai.Interview.Service.Contracts.Leaderboard;
using Bosai.Interview.Service.Contracts.Leaderboard.Dto;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Bosai.Interview.API.Controllers
{
    [ApiController]
    [Produces("application/json")]
    [Route("customer")]
    public class CustomerController : ControllerBase
    {
        private readonly ILogger<CustomerController> _logger;
        private readonly ILeaderboardService _leaderboardService;
        public CustomerController(ILogger<CustomerController> logger, ILeaderboardService leaderboardService)
        {
            _logger = logger;
            _leaderboardService = leaderboardService;
        }


        [HttpPost("{customerid}/score/{score}")]
        public async Task<ActionResult> UpdateScore([FromRoute] UpdateScoreRequest request)
        {
            if (ModelState.IsValid)
            {
                var result = await _leaderboardService.UpdateScore(request);
                return Ok(result);
            }
            else
            {
                return BadRequest(ModelState);
            }
        }

        [HttpGet("all")]
        public async Task<ActionResult> GetLeaderboard()
        {
            var result = await _leaderboardService.GetLeaderboard();
            return Ok(result);
        }
    }
}


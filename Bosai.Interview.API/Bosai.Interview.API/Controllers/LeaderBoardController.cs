using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bosai.Interview.Service.Contracts.Leaderboard;
using Bosai.Interview.Service.Contracts.Leaderboard.Dto;
using Bosai.Interview.Service.Leaderboard;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Bosai.Interview.API.Controllers
{
    [ApiController]
    [Produces("application/json")]
    [Route("leaderboard")]
    public class LeaderBoardController : ControllerBase
    {
        private readonly ILogger<LeaderBoardController> _logger;
        private readonly ILeaderboardService _leaderboardService;
        private readonly ScoreUpdateProcessor _processor;
        public LeaderBoardController(ILogger<LeaderBoardController> logger, ILeaderboardService leaderboardService,ScoreUpdateProcessor processor)
        {
            _logger = logger;
            _leaderboardService = leaderboardService;
            _processor = processor;
        }

        [HttpGet]
        public async Task<ActionResult> GetCustomersByRank([FromQuery] GetCustomersByRankRequest request)
        {
            var result = await _leaderboardService.GetCustomersByRank(request);
            return Ok(result);
        }

        [HttpGet("{customerid}")]
        public async Task<ActionResult> GetCustomersByCustomerId(long? customerid, [FromQuery] GetCustomersRequest request)
        {
            if (customerid.HasValue)
            {
                if (ModelState.IsValid)
                {
                    var result = await _leaderboardService.GetCustomersByCustomerId(customerid.Value, request);
                    return Ok(result);
                }
                else
                {
                    return BadRequest(ModelState);
                }
            }
            else
            {
                return new BadRequestResult();
            }
        }

        [HttpPost("open/queue")]
        public IActionResult StartUpdateProcessing()
        {
            _processor.StartProcessing();
            return Ok();
        }
    }
}


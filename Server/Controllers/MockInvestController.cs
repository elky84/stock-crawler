using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Server.Services;

namespace Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MockInvestController : ControllerBase
    {
        private readonly ILogger<MockInvestController> _logger;

        private readonly MockInvestService _mockInvestService;

        public MockInvestController(ILogger<MockInvestController> logger,
            MockInvestService mockInvestService)
        {
            _logger = logger;
            _mockInvestService = mockInvestService;
        }

        [HttpGet]
        public async Task<Protocols.Response.MockInvests> Get([FromQuery] string userId, [FromQuery] DateTime? date)
        {
            return await _mockInvestService.Get(userId, date);
        }

        [HttpPost("Refresh")]
        public async Task<Protocols.Response.MockInvestRefresh> Refresh([FromBody] Protocols.Request.MockInvestRefresh mockInvestRefresh)
        {
            return await _mockInvestService.Refresh(mockInvestRefresh.UserId);
        }

        [HttpPost("Analysis-Buy")]
        public async Task<Protocols.Response.MockInvestAnalysisBuy> AnalysisBuy([FromBody] Protocols.Request.MockInvestAnalysisBuy mockInvestAnalysisBuy)
        {
            return await _mockInvestService.AnalysisBuy(mockInvestAnalysisBuy);
        }

        [HttpPost("Buy")]
        public async Task<Protocols.Response.MockInvestBuy> Buy([FromBody] Protocols.Request.MockInvestBuy mockInvestBuy)
        {
            return await _mockInvestService.Buy(mockInvestBuy);
        }

        [HttpPost("Sell")]
        public async Task<Protocols.Response.MockInvestSell> Sell([FromBody] Protocols.Request.MockInvestSell mockInvestSell)
        {
            return await _mockInvestService.Sell(mockInvestSell);
        }
    }
}

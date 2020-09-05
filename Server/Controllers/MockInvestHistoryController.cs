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
    public class MockInvestHistoryController : ControllerBase
    {
        private readonly ILogger<MockInvestHistoryController> _logger;

        private readonly MockInvestHistoryService _mockInvestHistoryService;

        public MockInvestHistoryController(ILogger<MockInvestHistoryController> logger,
            MockInvestHistoryService mockInvestHistoryService)
        {
            _logger = logger;
            _mockInvestHistoryService = mockInvestHistoryService;
        }

        [HttpGet]
        public async Task<Protocols.Response.MockInvestHistories> Get([FromQuery] string userId)
        {
            return await _mockInvestHistoryService.Get(userId);
        }
    }
}

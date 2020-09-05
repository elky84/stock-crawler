using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Server.Code;
using Server.Protocols.Response;
using Server.Services;

namespace Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CodeController : ControllerBase
    {
        private readonly ILogger<CodeController> _logger;

        private readonly CodeService _codeService;

        public CodeController(ILogger<CodeController> logger,
            CodeService codeService)
        {
            _logger = logger;
            _codeService = codeService;
        }


        [HttpPost("Load")]
        public async Task<Header> Load([FromQuery] StockType stockType)
        {
            return await _codeService.Load(stockType);
        }
    }
}

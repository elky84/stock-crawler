﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Server.Code;
using Server.Protocols.Common;
using Server.Protocols.Response;
using Server.Services;

namespace Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CompanyController : ControllerBase
    {
        private readonly ILogger<CompanyController> _logger;

        private readonly CompanyService _companyService;

        public CompanyController(ILogger<CompanyController> logger,
            CompanyService companyService)
        {
            _logger = logger;
            _companyService = companyService;
        }


        [HttpPost("Crawling/Code")]
        public async Task<EzAspDotNet.Protocols.ResponseHeader> CrawlingCode([FromQuery] StockType stockType)
        {
            return await _companyService.CrawlingCode(stockType);
        }

        [HttpPost("Crawling/Alerts")]
        public async Task<EzAspDotNet.Protocols.ResponseHeader> CrawlingAlerts()
        {
            return await _companyService.CrawlingAlerts();
        }
    }
}

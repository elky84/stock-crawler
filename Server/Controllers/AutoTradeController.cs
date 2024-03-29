﻿using EzAspDotNet.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Server.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AutoTradeController : ControllerBase
    {
        private readonly ILogger<AutoTradeController> _logger;

        private readonly AutoTradeService _autoTradeService;

        public AutoTradeController(ILogger<AutoTradeController> logger,
            AutoTradeService autoTradeService)
        {
            _logger = logger;
            _autoTradeService = autoTradeService;
        }

        [HttpPost]
        public async Task<Protocols.Response.AutoTrade> Create([FromBody] Protocols.Request.AutoTrade autoTrade)
        {
            return await _autoTradeService.Create(autoTrade);
        }


        [HttpGet("UserId/{id}")]
        public async Task<Protocols.Response.AutoTrade> GetByUserId(string id)
        {
            return new Protocols.Response.AutoTrade
            {
                Datas = MapperUtil.Map<List<Models.AutoTrade>,
                                       List<Protocols.Common.AutoTrade>>
                                       (await _autoTradeService.GetByUserId(id))
            };
        }

        [HttpGet("{id}")]
        public async Task<Protocols.Response.AutoTrade> Get(string id)
        {
            return await _autoTradeService.Get(id);
        }

        [HttpPut("{id}")]
        public async Task<Protocols.Response.AutoTrade> Update(string id, [FromBody] Protocols.Request.AutoTrade autoTrade)
        {
            return await _autoTradeService.Update(id, autoTrade);
        }

        [HttpDelete("{id}")]
        public async Task<Protocols.Response.AutoTrade> Delete(string id)
        {
            return await _autoTradeService.Delete(id);
        }

        [HttpDelete("UserId/{id}")]
        public async Task<Protocols.Response.AutoTrade> DeleteByUserId(string id)
        {
            return await _autoTradeService.DeleteByUserId(id);
        }

    }
}

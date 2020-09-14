using Server.Models;
using WebUtil.Util;
using System.Linq;
using System.Threading.Tasks;
using WebUtil.Services;
using StockCrawler;
using System.Collections.Generic;
using MongoDB.Driver;
using System.Threading;
using Serilog;
using System;
using Microsoft.Extensions.Hosting;

namespace Server.Services
{
    public class CrawlingRepeatedService : RepeatedService
    {
        private readonly CrawlingService _crawlingService;

        public CrawlingRepeatedService(CrawlingService crawlingService
            )
            : base(new TimeSpan(0, 5, 0))
        {
            _crawlingService = crawlingService;
        }

        protected override void DoWork(object state)
        {
            var now = DateTime.Now;

            var openTime = now.Date.AddHours(9);
            var closeTime = now.Date.AddHours(15);

            if (openTime <= now && closeTime >= now)
            {
                _ = _crawlingService.ExecuteBackground();
            }
        }
    }
}

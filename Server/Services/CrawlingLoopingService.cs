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
    public class CrawlingLoopingService : LoopingService
    {
        private readonly CrawlingService _crawlingService;

        public CrawlingLoopingService(CrawlingService crawlingService
            )
        {
            _crawlingService = crawlingService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    DoWork();
                }
                catch (System.Exception e)
                {
                    Log.Logger.Error($"Implement Task Exception. Reason:{e.Message}");

                }

                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }


        protected void DoWork()
        {
            var now = DateTime.Now;
            // 토요일 일요일은 크롤링 안함
            if (now.DayOfWeek == DayOfWeek.Saturday || now.DayOfWeek == DayOfWeek.Sunday)
            {
                return;
            }

            var openTime = now.Date.AddHours(9);
            var closeTime = now.Date.AddHours(15);

            if (openTime <= now && closeTime >= now)
            {
                _ = _crawlingService.ExecuteBackground();
            }
        }
    }
}

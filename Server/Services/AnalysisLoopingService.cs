using Server.Models;
using MongoDbWebUtil.Util;
using System.Linq;
using System.Threading.Tasks;
using MongoDbWebUtil.Services;
using StockCrawler;
using System.Collections.Generic;
using MongoDB.Driver;
using System.Threading;
using Serilog;
using System;
using Microsoft.Extensions.Hosting;

namespace Server.Services
{
    public class AnalysisLoopingService : LoopingService
    {
        private readonly AnalysisService _analysisService;

        public AnalysisLoopingService(AnalysisService analysisService
            )
        {
            _analysisService = analysisService;
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

                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }


        protected void DoWork()
        {
            var now = DateTime.Now;
            // 토요일 일요일은 분석 안함
            if (now.DayOfWeek == DayOfWeek.Saturday || now.DayOfWeek == DayOfWeek.Sunday)
            {
                return;
            }

            // 오전 8시부터 오후 4시까지 실행.
            if (now.Hour >= 8 &&
                now.Hour <= 16)
            {
                _ = _analysisService.ExecuteBackground();
            }
        }
    }
}

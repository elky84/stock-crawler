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

            // 한시간에 한번 분석
            _ = _analysisService.ExecuteBackground();
        }
    }
}

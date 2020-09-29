using Server.Models;
using WebUtil.Util;
using System.Threading.Tasks;
using WebUtil.Services;
using MongoDB.Driver;
using System;
using System.Linq;
using System.Collections.Generic;
using Server.Exception;
using Microsoft.Extensions.Hosting;
using System.Threading;
using Serilog;

namespace Server.Services
{
    public class MockInvestLoopingService : LoopingService
    {
        private readonly MockInvestService _mockInvestService;

        private readonly UserService _userService;

        public MockInvestLoopingService(MockInvestService mockInvestService,
            UserService userService)
        {
            _mockInvestService = mockInvestService;
            _userService = userService;
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

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }


        protected void DoWork()
        {
            var now = DateTime.Now;
            // 토요일 일요일은 모의 투자 안함
            if (now.DayOfWeek == DayOfWeek.Saturday || now.DayOfWeek == DayOfWeek.Sunday)
            {
                return;
            }

            var buyTime = now.Date.AddHours(9);
            var sellTime = now.Date.AddHours(14).AddMinutes(30);

            // 매수
            {
                var diff = buyTime - now;
                if (Math.Abs(diff.TotalMinutes) <= 1)
                {
                    foreach (var user in _userService.GetAutoTradeUsers().Result)
                    {
                        _ = _mockInvestService.AnalysisBuy(new Protocols.Request.MockInvestAnalysisBuy
                        {
                            UserId = user.UserId,
                            Type = user.AnalysisType.GetValueOrDefault(Code.AnalysisType.GoldenCrossTransactionPrice),
                            Count = user.AutoTradeCount
                        });
                    }
                }
            }

            // 매도
            {
                var diff = sellTime - now;
                if (Math.Abs(diff.TotalMinutes) <= 1)
                {
                    foreach (var user in _userService.GetAutoTradeUsers().Result)
                    {
                        _ = _mockInvestService.Sell(new Protocols.Request.MockInvestSell
                        {
                            UserId = user.UserId,
                            All = true
                        });
                    }
                }
            }
        }
    }
}

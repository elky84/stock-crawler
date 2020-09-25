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
    public class MockInvestRepeatedService : RepeatedService
    {
        private readonly MockInvestService _mockInvestService;

        private readonly UserService _userService;

        public MockInvestRepeatedService(MockInvestService mockInvestService,
            UserService userService)
            : base(new TimeSpan(0, 0, 5))
        {
            _mockInvestService = mockInvestService;
            _userService = userService;
        }

        protected override void DoWork(object state)
        {
            var now = DateTime.Now;

            var buyTime = now.Date.AddHours(9);
            var sellTime = now.Date.AddHours(14).AddMinutes(30);

            // 매수
            {
                var diff = buyTime - now;
                if (Math.Abs(diff.TotalSeconds) <= 5)
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
                if (Math.Abs(diff.TotalSeconds) <= 5)
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

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

        public MockInvestRepeatedService(MockInvestService mockInvestService)
            : base(new TimeSpan(0, 5, 0))
        {
            _mockInvestService = mockInvestService;
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
                    // 하드 코딩 제거 필요. elky id, type, total price

                    _ = _mockInvestService.AnalysisBuy(new Protocols.Request.MockInvestAnalysisBuy
                    {
                        UserId = "elky",
                        Type = Code.AnalysisType.GoldenCrossTradeCount,
                        Count = 10,
                        TotalPrice = 10000000
                    });
                }
            }

            // 매도
            {
                var diff = sellTime - now;
                if (Math.Abs(diff.TotalSeconds) <= 5)
                {
                    // 하드 코딩 제거 필요. elky id
                    _ = _mockInvestService.Sell(new Protocols.Request.MockInvestSell
                    {
                        UserId = "elky",
                        All = true
                    });
                }
            }
        }

    }
}

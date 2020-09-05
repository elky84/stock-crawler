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
    public class MockInvestService : RepeatedService
    {
        private readonly UserService _userService;

        private readonly MongoDbUtil<MockInvest> _mongoDbMockInvest;

        private readonly StockDataService _stockDataService;

        private readonly AnalysisService _analysisService;

        private readonly MockInvestHistoryService _mockInvestHistoryService;

        public MockInvestService(MongoDbService mongoDbService,
            StockDataService stockDataService,
            AnalysisService analysisService,
            UserService userService,
            MockInvestHistoryService mockInvestHistoryService)
            : base(new TimeSpan(0, 5, 0))
        {
            _userService = userService;
            _stockDataService = stockDataService;
            _analysisService = analysisService;
            _mockInvestHistoryService = mockInvestHistoryService;

            _mongoDbMockInvest = new MongoDbUtil<MockInvest>(mongoDbService.Database);
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
                    _ = AnalysisBuy(new Protocols.Request.MockInvestAnalysisBuy
                    {
                        UserId = "elky",
                        Type = Code.AnalysisType.GoldenCross,
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
                    var mockInvests = Get("elky").Result;
                    foreach (var mockInvest in mockInvests.InvestList)
                    {
                        _ = Sell(new Protocols.Request.MockInvestSell
                        {
                            UserId = mockInvest.UserId,
                            Id = mockInvest.Id,
                            Amount = mockInvest.Amount
                        });
                    }
                }
            }
        }

        public async Task<Protocols.Response.MockInvests> Get(string userId)
        {
            var invests = (await _mongoDbMockInvest.FindAsync(Builders<MockInvest>.Filter.Eq(x => x.UserId, userId)));
            foreach (var invest in invests)
            {
                var latest = await _stockDataService.Latest(7, invest.Code);
                invest.Price = latest.Latest;
            }

            return new Protocols.Response.MockInvests
            {
                ResultCode = Code.ResultCode.Success,
                User = (await _userService.GetByUserId(userId))?.ToProtocol(),
                InvestList = invests.ConvertAll(x => x.ToProtocol())
            };
        }


        public async Task<Protocols.Response.MockInvestAnalysisBuy> AnalysisBuy(Protocols.Request.MockInvestAnalysisBuy mockInvestAnalysisBuy)
        {
            var user = await _userService.GetByUserId(mockInvestAnalysisBuy.UserId);
            var analysisDatas = await _analysisService.Get(mockInvestAnalysisBuy.Type, mockInvestAnalysisBuy.Count);
            var investDatas = new List<MockInvest>();

            foreach (var analysisData in analysisDatas)
            {
                var latest = await _stockDataService.Latest(7, analysisData.Code);

                var amount = mockInvestAnalysisBuy.TotalPrice / latest.Latest;

                user.Balance -= latest.Latest * amount;

                if (user.Balance < 0)
                {
                    throw new DeveloperException(Code.ResultCode.NotEnoughBalance);
                }

                await _userService.Update(user);

                var mockInvest = await _mongoDbMockInvest.CreateAsync(new MockInvest
                {
                    UserId = mockInvestAnalysisBuy.UserId,
                    Code = analysisData.Code,
                    Amount = amount,
                    BuyPrice = latest.Latest,
                    Price = latest.Latest,
                });

                investDatas.Add(mockInvest);

                await _mockInvestHistoryService.Write(Code.HistoryType.Buy, mockInvest);
            }

            return new Protocols.Response.MockInvestAnalysisBuy
            {
                ResultCode = Code.ResultCode.Success,
                User = user.ToProtocol(),
                InvestDatas = investDatas.ConvertAll(x => x.ToProtocol())
            };
        }

        public async Task<Protocols.Response.MockInvestBuy> Buy(Protocols.Request.MockInvestBuy mockInvestBuy)
        {
            var user = await _userService.GetByUserId(mockInvestBuy.UserId);

            var latest = await _stockDataService.Latest(7, mockInvestBuy.Code);

            user.Balance -= latest.Latest * mockInvestBuy.Amount;
            if (user.Balance < 0)
            {
                throw new DeveloperException(Code.ResultCode.NotEnoughBalance);
            }

            await _userService.Update(user);

            var mockInvest = await _mongoDbMockInvest.CreateAsync(new MockInvest
            {
                UserId = mockInvestBuy.UserId,
                Code = mockInvestBuy.Code,
                Amount = mockInvestBuy.Amount,
                BuyPrice = latest.Latest,
                Price = latest.Latest,
            });

            await _mockInvestHistoryService.Write(Code.HistoryType.Buy, mockInvest);

            return new Protocols.Response.MockInvestBuy
            {
                ResultCode = Code.ResultCode.Success,
                User = user.ToProtocol(),
                InvestData = mockInvest.ToProtocol()
            };
        }


        public async Task<Protocols.Response.MockInvestSell> Sell(Protocols.Request.MockInvestSell mockInvestSell)
        {
            var user = await _userService.GetByUserId(mockInvestSell.UserId);
            var mockInvest = await _mongoDbMockInvest.FindOneAsync(mockInvestSell.Id);

            var latest = await _stockDataService.Latest(7, mockInvest.Code);
            user.Balance += latest.Latest * mockInvestSell.Amount;
            mockInvest.Price = latest.Latest;

            if (mockInvest.Amount < mockInvestSell.Amount)
            {
                throw new DeveloperException(Code.ResultCode.CannotOverHaveStockAmount);
            }
            else if (mockInvest.Amount == mockInvestSell.Amount)
            {
                await _mongoDbMockInvest.RemoveAsync(Builders<MockInvest>.Filter.Eq(x => x.UserId, mockInvestSell.UserId) & Builders<MockInvest>.Filter.Eq(x => x.Code, mockInvest.Code));
            }
            else
            {
                mockInvest.Amount -= mockInvestSell.Amount;
                await _mongoDbMockInvest.UpdateAsync(mockInvest.Id, mockInvest);
            }

            await _mockInvestHistoryService.Write(Code.HistoryType.Sell, mockInvest, mockInvestSell.Amount);

            await _userService.Update(user);

            return new Protocols.Response.MockInvestSell
            {
                ResultCode = Code.ResultCode.Success,
                User = (await _userService.GetByUserId(mockInvestSell.UserId))?.ToProtocol(),
                InvestData = mockInvest.ToProtocol()
            };
        }

    }
}

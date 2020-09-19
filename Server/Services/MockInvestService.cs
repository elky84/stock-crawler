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
    public class MockInvestService
    {
        private readonly UserService _userService;

        private readonly MongoDbUtil<MockInvest> _mongoDbMockInvest;

        private readonly StockDataService _stockDataService;

        private readonly AnalysisService _analysisService;

        private readonly MockInvestHistoryService _mockInvestHistoryService;

        private readonly CrawlingService _crawlingService;

        public MockInvestService(MongoDbService mongoDbService,
            StockDataService stockDataService,
            AnalysisService analysisService,
            UserService userService,
            MockInvestHistoryService mockInvestHistoryService,
            CrawlingService crawlingService)
        {
            _userService = userService;
            _stockDataService = stockDataService;
            _analysisService = analysisService;
            _mockInvestHistoryService = mockInvestHistoryService;
            _crawlingService = crawlingService;

            _mongoDbMockInvest = new MongoDbUtil<MockInvest>(mongoDbService.Database);
        }

        public async Task<Protocols.Response.MockInvests> Get(string userId, DateTime? date)
        {
            var user = await _userService.GetByUserId(userId);

            var invests = (await _mongoDbMockInvest.FindAsync(Builders<MockInvest>.Filter.Eq(x => x.UserId, userId)));
            foreach (var invest in invests)
            {
                var latest = await _stockDataService.Latest(7, invest.Code, date);
                invest.Price = latest.Latest;
            }

            var valuationBalance = user.Balance + invests.Sum(x => x.TotalPrice.GetValueOrDefault(0));
            return new Protocols.Response.MockInvests
            {
                ResultCode = Code.ResultCode.Success,
                User = user.ToProtocol(),
                InvestList = invests.ConvertAll(x => x.ToProtocol()),
                ValuationBalance = valuationBalance,
                ValuationIncome = 100.0 - (double)user.OriginBalance / (double)valuationBalance * 100,
                Date = date
            };
        }


        public async Task<Protocols.Response.MockInvestRefresh> Refresh(string userId)
        {
            var user = await _userService.GetByUserId(userId);

            var invests = (await _mongoDbMockInvest.FindAsync(Builders<MockInvest>.Filter.Eq(x => x.UserId, userId)));
            await _crawlingService.Execute(new Protocols.Request.Crawling { Codes = invests.ConvertAll(x => x.Code).ToList(), Page = 1 });

            foreach (var invest in invests)
            {
                var latest = await _stockDataService.Latest(7, invest.Code);
                invest.Price = latest.Latest;
            }

            var valuationBalance = user.Balance + invests.Sum(x => x.TotalPrice.GetValueOrDefault(0));
            return new Protocols.Response.MockInvestRefresh
            {
                ResultCode = Code.ResultCode.Success,
                User = user.ToProtocol(),
                InvestList = invests.ConvertAll(x => x.ToProtocol()),
                ValuationBalance = valuationBalance,
                ValuationIncome = 100.0 - (double)user.OriginBalance / (double)valuationBalance * 100
            };
        }

        public async Task<Protocols.Response.MockInvestAnalysisBuy> AnalysisBuy(Protocols.Request.MockInvestAnalysisBuy mockInvestAnalysisBuy)
        {
            var user = await _userService.GetByUserId(mockInvestAnalysisBuy.UserId);
            var investDatas = new List<MockInvest>();

            int page = 0;
            while (investDatas.Count < mockInvestAnalysisBuy.Count)
            {
                var analysisDatas = await _analysisService.Get(mockInvestAnalysisBuy.Date.GetValueOrDefault(), mockInvestAnalysisBuy.Type, mockInvestAnalysisBuy.Count, page);
                if (!analysisDatas.Any())
                {
                    break;
                }

                foreach (var analysisData in analysisDatas)
                {
                    var latest = await _stockDataService.Latest(7, analysisData.Code, mockInvestAnalysisBuy.Date);
                    if (latest.Latest > mockInvestAnalysisBuy.TotalPrice)
                    {
                        continue;
                    }

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

                    if (investDatas.Count >= mockInvestAnalysisBuy.Count)
                    {
                        break;
                    }
                }
                ++page;
            }

            return new Protocols.Response.MockInvestAnalysisBuy
            {
                ResultCode = Code.ResultCode.Success,
                User = user.ToProtocol(),
                InvestDatas = investDatas.ConvertAll(x => x.ToProtocol()),
                Date = mockInvestAnalysisBuy.Date
            };
        }

        public async Task<Protocols.Response.MockInvestBuy> Buy(Protocols.Request.MockInvestBuy mockInvestBuy)
        {
            var user = await _userService.GetByUserId(mockInvestBuy.UserId);

            var latest = await _stockDataService.Latest(7, mockInvestBuy.Code, mockInvestBuy.Date);

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
                InvestData = mockInvest.ToProtocol(),
                Date = mockInvestBuy.Date
            };
        }


        public async Task<Protocols.Response.MockInvestSell> Sell(Protocols.Request.MockInvestSell mockInvestSell)
        {
            var user = await _userService.GetByUserId(mockInvestSell.UserId);
            var sellList = mockInvestSell.All ?
                (await _mongoDbMockInvest.FindAsync(Builders<MockInvest>.Filter.Eq(x => x.UserId, mockInvestSell.UserId))).ConvertAll(x => new Protocols.Common.MockInvestSell { Id = x.Id, Amount = x.Amount }) :
                mockInvestSell.SellList;

            var investDatas = new List<MockInvest>();

            foreach (var sell in sellList)
            {
                var mockInvest = await _mongoDbMockInvest.FindOneAsyncById(sell.Id);

                var latest = await _stockDataService.Latest(7, mockInvest.Code, mockInvestSell.Date);
                user.Balance += latest.Latest * sell.Amount;
                mockInvest.Price = latest.Latest;

                if (mockInvest.Amount < sell.Amount)
                {
                    throw new DeveloperException(Code.ResultCode.CannotOverHaveStockAmount);
                }
                else if (mockInvest.Amount == sell.Amount)
                {
                    await _mongoDbMockInvest.RemoveAsync(Builders<MockInvest>.Filter.Eq(x => x.UserId, mockInvestSell.UserId) & Builders<MockInvest>.Filter.Eq(x => x.Code, mockInvest.Code));
                }
                else
                {
                    mockInvest.Amount -= sell.Amount;
                    await _mongoDbMockInvest.UpdateAsync(mockInvest.Id, mockInvest);
                }

                await _mockInvestHistoryService.Write(Code.HistoryType.Sell, mockInvest, sell.Amount);

                investDatas.Add(mockInvest);
            }

            await _userService.Update(user);

            return new Protocols.Response.MockInvestSell
            {
                ResultCode = Code.ResultCode.Success,
                User = (await _userService.GetByUserId(mockInvestSell.UserId))?.ToProtocol(),
                InvestDatas = investDatas.ConvertAll(x => x.ToProtocol()),
                Date = mockInvestSell.Date
            };
        }
    }
}

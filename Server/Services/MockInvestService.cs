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

        private readonly AutoTradeService _autoTradeService;

        public MockInvestService(MongoDbService mongoDbService,
            StockDataService stockDataService,
            AnalysisService analysisService,
            UserService userService,
            MockInvestHistoryService mockInvestHistoryService,
            AutoTradeService autoTradeService)
        {
            _userService = userService;
            _stockDataService = stockDataService;
            _analysisService = analysisService;
            _mockInvestHistoryService = mockInvestHistoryService;
            _autoTradeService = autoTradeService;

            _mongoDbMockInvest = new MongoDbUtil<MockInvest>(mongoDbService.Database);

            _mongoDbMockInvest.Collection.Indexes.CreateOne(new CreateIndexModel<MockInvest>(
                    Builders<MockInvest>.IndexKeys.Ascending(x => x.UserId)));

            _mongoDbMockInvest.Collection.Indexes.CreateOne(new CreateIndexModel<MockInvest>(
                    Builders<MockInvest>.IndexKeys.Ascending(x => x.Date)));
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

            var buyPriceSum = invests.Sum(x => x.BuyPrice);
            var currentPriceSum = invests.Sum(x => x.Price);
            var valuationBalance = user.Balance + invests.Sum(x => x.TotalPrice.GetValueOrDefault(0));
            return new Protocols.Response.MockInvests
            {
                ResultCode = Code.ResultCode.Success,
                User = user.ToProtocol(),
                InvestList = invests.ConvertAll(x => x.ToProtocol()),
                ValuationBalance = valuationBalance,
                ValuationIncome = (double)valuationBalance / (double)user.OriginBalance * 100,
                InvestedIncome = (double)currentPriceSum / (double)buyPriceSum * 100,
                Date = date.GetValueOrDefault(DateTime.Now).Date
            };
        }


        public async Task<MockInvest> Get(string userId, string code, DateTime? date)
        {
            return (await _mongoDbMockInvest.FindOneAsync(Builders<MockInvest>.Filter.Eq(x => x.UserId, userId) &
                Builders<MockInvest>.Filter.Eq(x => x.Code, code)));
        }


        public async Task<Protocols.Response.MockInvestAnalysisBuy> AnalysisBuy(Protocols.Request.MockInvestAnalysisBuy mockInvestAnalysisBuy)
        {
            var user = await _userService.GetByUserId(mockInvestAnalysisBuy.UserId);
            var investDatas = new List<MockInvest>();

            int page = 0;
            var codePerUsablePrice = user.Balance / mockInvestAnalysisBuy.Count;
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
                    if (latest.Latest > codePerUsablePrice)
                    {
                        continue;
                    }

                    var amount = (int)(codePerUsablePrice / latest.Latest);

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
                        Date = mockInvestAnalysisBuy.Date.GetValueOrDefault(DateTime.Now).Date
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
                Type = mockInvestAnalysisBuy.Type,
                InvestDatas = investDatas.ConvertAll(x => x.ToProtocol()),
                Date = mockInvestAnalysisBuy.Date
            };
        }


        public async Task<Protocols.Response.MockInvestAnalysisAutoTrade> AnalysisAutoTrade(Protocols.Request.MockInvestAnalysisAutoTrade mockInvestAnalysisAutoTrade)
        {
            var user = await _userService.GetByUserId(mockInvestAnalysisAutoTrade.UserId);
            var autoTrades = new List<AutoTrade>();

            int page = 0;
            var codePerUsablePrice = user.Balance / mockInvestAnalysisAutoTrade.Count;
            while (autoTrades.Count < mockInvestAnalysisAutoTrade.Count)
            {
                var analysisDatas = await _analysisService.Get(mockInvestAnalysisAutoTrade.Date.GetValueOrDefault(), mockInvestAnalysisAutoTrade.Type, mockInvestAnalysisAutoTrade.Count, page);
                if (!analysisDatas.Any())
                {
                    break;
                }

                foreach (var analysisData in analysisDatas)
                {
                    var autoTrade = await _autoTradeService.CreateAsync(new Protocols.Request.AutoTrade
                    {
                        UserId = mockInvestAnalysisAutoTrade.UserId,
                        Code = analysisData.Code,
                        Balance = codePerUsablePrice,
                        BuyCondition = mockInvestAnalysisAutoTrade.BuyCondition,
                        SellCondition = mockInvestAnalysisAutoTrade.SellCondition
                    });

                    autoTrades.Add(autoTrade);

                    if (autoTrades.Count >= mockInvestAnalysisAutoTrade.Count)
                    {
                        break;
                    }
                }
                ++page;
            }

            return new Protocols.Response.MockInvestAnalysisAutoTrade
            {
                ResultCode = Code.ResultCode.Success,
                Type = mockInvestAnalysisAutoTrade.Type,
                User = user.ToProtocol(),
                AutoTrades = autoTrades.ConvertAll(x => x.ToProtocol()),
                Date = mockInvestAnalysisAutoTrade.Date
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
                Date = mockInvestBuy.Date.GetValueOrDefault(DateTime.Now).Date
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

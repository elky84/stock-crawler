﻿using Server.Models;
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

        private readonly NotificationService _notificationService;

        private readonly CompanyService _companyService;

        public MockInvestService(MongoDbService mongoDbService,
            StockDataService stockDataService,
            AnalysisService analysisService,
            UserService userService,
            MockInvestHistoryService mockInvestHistoryService,
            AutoTradeService autoTradeService,
            NotificationService notificationService,
            CompanyService companyService)
        {
            _userService = userService;
            _stockDataService = stockDataService;
            _analysisService = analysisService;
            _mockInvestHistoryService = mockInvestHistoryService;
            _autoTradeService = autoTradeService;
            _notificationService = notificationService;
            _companyService = companyService;

            _mongoDbMockInvest = new MongoDbUtil<MockInvest>(mongoDbService.Database);

            _mongoDbMockInvest.Collection.Indexes.CreateOne(new CreateIndexModel<MockInvest>(
                    Builders<MockInvest>.IndexKeys.Ascending(x => x.UserId)));

            _mongoDbMockInvest.Collection.Indexes.CreateOne(new CreateIndexModel<MockInvest>(
                    Builders<MockInvest>.IndexKeys.Ascending(x => x.Date)));
        }

        public async Task<Protocols.Response.MockInvestList> Get(string userId, DateTime? date)
        {
            var response = new Protocols.Response.MockInvestList();
            var users = string.IsNullOrEmpty(userId) ? await _userService.Get() : new List<User> { await _userService.GetByUserId(userId) };
            foreach (var user in users)
            {
                var invests = (await _mongoDbMockInvest.FindAsync(Builders<MockInvest>.Filter.Eq(x => x.UserId, user.UserId)));

                foreach (var invest in invests)
                {
                    var latest = await _stockDataService.Latest(7, invest.Code, date);
                    invest.Price = latest.Latest;
                }

                var buyPriceSum = invests.Sum(x => x.BuyPrice);
                var currentPriceSum = invests.Sum(x => x.Price);
                var valuationBalance = user.Balance + invests.Sum(x => x.TotalPrice.GetValueOrDefault(0));

                response.Datas.Add(new Protocols.Common.MockInvestList
                {
                    User = user.ToProtocol(),
                    MockInvests = invests.ConvertAll(x => x.ToProtocol()),
                    ValuationBalance = valuationBalance,
                    ValuationIncome = (double)valuationBalance / (double)user.OriginBalance * 100,
                    InvestedIncome = (double)currentPriceSum / (double)buyPriceSum * 100,
                    Date = date.GetValueOrDefault(DateTime.Now).Date
                });
            }
            return response;
        }


        public async Task<MockInvest> Get(string userId, string code, DateTime? date)
        {
            var filter = Builders<MockInvest>.Filter.Eq(x => x.UserId, userId) &
                Builders<MockInvest>.Filter.Eq(x => x.Code, code);
            if (date.HasValue)
            {
                filter &= Builders<MockInvest>.Filter.Eq(x => x.Date, date.Value.Date);
            }

            return await _mongoDbMockInvest.FindOneAsync(filter);
        }


        public async Task<Protocols.Response.MockInvestAnalysisBuy> AnalysisBuy(Protocols.Request.MockInvestAnalysisBuy mockInvestAnalysisBuy)
        {
            var user = await _userService.GetByUserId(mockInvestAnalysisBuy.UserId);
            var investDatas = new List<MockInvest>();

            int page = 0;
            var codePerUsablePrice = user.Balance / mockInvestAnalysisBuy.Count;
            while (investDatas.Count < mockInvestAnalysisBuy.Count)
            {
                var Datas = await _analysisService.Get(mockInvestAnalysisBuy.Date.GetValueOrDefault(), mockInvestAnalysisBuy.Type, mockInvestAnalysisBuy.Count, page);
                if (!Datas.Any())
                {
                    break;
                }

                foreach (var analysisData in Datas)
                {
                    var latest = await _stockDataService.Latest(7, analysisData.Code, mockInvestAnalysisBuy.Date);
                    if (latest.Latest > codePerUsablePrice)
                    {
                        continue;
                    }

                    if (await _companyService.NotAlert(analysisData.Code) == null)
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

                    await Notification(user, mockInvest);

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
                Datas = investDatas.ConvertAll(x => x.ToProtocol()),
                Date = mockInvestAnalysisBuy.Date
            };
        }

        private async Task Notification(User user, MockInvest mockInvest, string additional = null)
        {
            var code = await _companyService.Get(mockInvest.Code);
            var message = $"**[종목:{code?.Name}/{mockInvest.Code}]**\n" +
                $"`[매수] 주당가:{mockInvest.BuyPrice}, 수량:{mockInvest.Amount}, 총액:{mockInvest.TotalBuyPrice}`\n";

            if (!string.IsNullOrEmpty(additional))
            {
                message += additional;
            }

            message += $"`[유저] 아이디:{user.UserId}, 잔액:{user.Balance}`\n";

            await _notificationService.Execute(Code.InvestType.MockInvest, message);
        }


        public async Task<Protocols.Response.MockInvestAnalysisAutoTrade> AnalysisAutoTrade(Protocols.Request.MockInvestAnalysisAutoTrade mockInvestAnalysisAutoTrade)
        {
            var user = await _userService.GetByUserId(mockInvestAnalysisAutoTrade.UserId);
            var autoTrades = new List<AutoTrade>();

            int page = 0;
            var codePerUsablePrice = user.Balance / mockInvestAnalysisAutoTrade.Count;
            while (autoTrades.Count < mockInvestAnalysisAutoTrade.Count)
            {
                var datas = await _analysisService.Get(mockInvestAnalysisAutoTrade.Date.GetValueOrDefault(), mockInvestAnalysisAutoTrade.Type, mockInvestAnalysisAutoTrade.Count, page);
                if (!datas.Any())
                {
                    break;
                }

                foreach (var analysisData in datas)
                {
                    var autoTrade = await _autoTradeService.CreateAsync(new Protocols.Request.AutoTrade
                    {
                        UserId = mockInvestAnalysisAutoTrade.UserId,
                        Code = analysisData.Code,
                        Balance = codePerUsablePrice,
                        AnalysisType = mockInvestAnalysisAutoTrade.Type,
                        BuyTradeType = mockInvestAnalysisAutoTrade.BuyTradeType,
                        BuyCondition = mockInvestAnalysisAutoTrade.BuyCondition,
                        SellTradeType = mockInvestAnalysisAutoTrade.SellTradeType,
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
                Type = mockInvestAnalysisAutoTrade.Type,
                User = user.ToProtocol(),
                Datas = autoTrades.ConvertAll(x => x.ToProtocol()),
                Date = mockInvestAnalysisAutoTrade.Date
            };
        }

        public async Task<Protocols.Response.MockInvestAutoTradeRefresh> AutoTradeRefresh(Protocols.Request.MockInvestAutoTradeRefresh mockInvestAutoTradeRefresh)
        {
            var user = await _userService.GetByUserId(mockInvestAutoTradeRefresh.UserId);
            var autoTrades = await _autoTradeService.GetByUserId(mockInvestAutoTradeRefresh.UserId);
            foreach (var autoTrade in autoTrades)
            {
                var analysis = await NextAnalysis(autoTrade, autoTrades, mockInvestAutoTradeRefresh.Date.GetValueOrDefault(DateTime.Now));
                if (analysis != null)
                {
                    autoTrade.Code = analysis.Code;
                    await _autoTradeService.Update(autoTrade);
                }
            }

            return new Protocols.Response.MockInvestAutoTradeRefresh
            {
                Datas = autoTrades.ConvertAll(x => x.ToProtocol()),
                Date = mockInvestAutoTradeRefresh.Date
            };
        }


        public async Task<Analysis> NextAnalysis(AutoTrade autoTrade, List<AutoTrade> allAutoTrades, DateTime date)
        {
            var count = await _analysisService.CountAsync(DateTime.Now);
            var mockInvestHistories = await _mockInvestHistoryService.Get(autoTrade.UserId, date.Date);

            for (int page = 0; count > page * allAutoTrades.Count; ++page)
            {
                var datas = await _analysisService.Get(DateTime.Now.Date, autoTrade.AnalysisType, allAutoTrades.Count, page);
                if (!datas.Any())
                {
                    break;
                }

                foreach (var analysisData in datas)
                {
                    if (allAutoTrades.Any(x => x.Code == analysisData.Code) ||
                        mockInvestHistories.Any(x => x.Code == analysisData.Code))
                    {
                        continue;
                    }

                    if (await _companyService.NotAlert(analysisData.Code) == null)
                    {
                        continue;
                    }

                    return analysisData;
                }
            }
            return null;
        }

        public async Task<Protocols.Response.MockInvestBuy> Buy(Protocols.Request.MockInvestBuy mockInvestBuy)
        {
            if (mockInvestBuy.Amount <= 0)
            {
                throw new DeveloperException(Code.ResultCode.BuyAmountGreaterThanZero);
            }

            var user = await _userService.GetByUserId(mockInvestBuy.UserId);

            var latest = await _stockDataService.Latest(7, mockInvestBuy.Code, mockInvestBuy.Date);

            if (await _companyService.NotAlert(mockInvestBuy.Code) == null)
            {
                throw new DeveloperException(Code.ResultCode.InvestAlertCompany);
            }

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

            await Notification(user, mockInvest);

            return new Protocols.Response.MockInvestBuy
            {
                ResultCode = Code.ResultCode.Success,
                User = user.ToProtocol(),
                Data = mockInvest.ToProtocol(),
                Date = mockInvestBuy.Date
            };
        }


        public async Task<Protocols.Response.MockInvestSell> Sell(Protocols.Request.MockInvestSell mockInvestSell)
        {
            var user = await _userService.GetByUserId(mockInvestSell.UserId);
            var sellList = mockInvestSell.All ?
                (await _mongoDbMockInvest.FindAsync(Builders<MockInvest>.Filter.Eq(x => x.UserId, mockInvestSell.UserId))).ConvertAll(x => new Protocols.Common.MockInvestSell { 
                    Id = x.Id, 
                    Amount = x.Amount
                }) : mockInvestSell.SellList;

            var investDatas = new List<MockInvest>();

            foreach (var sell in sellList)
            {
                var mockInvest = await _mongoDbMockInvest.FindOneAsyncById(sell.Id);

                var latest = await _stockDataService.Latest(7, mockInvest.Code, mockInvestSell.Date);
                if(latest == null)
                {
                    Log.Information($"Not found latest.");
                    continue;
                }

                var sellTotalPrice = latest.Latest * sell.Amount;
                user.Balance += sellTotalPrice;
                mockInvest.Price = latest.Latest;

                if (mockInvest.Amount < sell.Amount)
                {
                    throw new DeveloperException(Code.ResultCode.CannotOverHaveStockAmount);
                }
                else if (mockInvest.Amount == sell.Amount)
                {
                    await _mongoDbMockInvest.RemoveGetAsync(Builders<MockInvest>.Filter.Eq(x => x.UserId, mockInvestSell.UserId) & Builders<MockInvest>.Filter.Eq(x => x.Code, mockInvest.Code));
                }
                else
                {
                    mockInvest.Amount -= sell.Amount;
                    await _mongoDbMockInvest.UpdateAsync(mockInvest.Id, mockInvest);
                }

                await _mockInvestHistoryService.Write(Code.HistoryType.Sell, mockInvest, sell.Amount);

                var sellMessage = $"`[매도] 주당가:{latest.Latest}, 수량:{sell.Amount}, 총액:{sellTotalPrice}`\n" +
                    $"`[손익] {sellTotalPrice - mockInvest.TotalBuyPrice} 손익률 {(double)sellTotalPrice / mockInvest.TotalBuyPrice}`\n";

                await Notification(user, mockInvest, sellMessage);

                investDatas.Add(mockInvest);
            }

            await _userService.Update(user);

            return new Protocols.Response.MockInvestSell
            {
                ResultCode = Code.ResultCode.Success,
                User = (await _userService.GetByUserId(mockInvestSell.UserId))?.ToProtocol(),
                Datas = investDatas.ConvertAll(x => x.ToProtocol()),
                Date = mockInvestSell.Date
            };
        }
    }
}

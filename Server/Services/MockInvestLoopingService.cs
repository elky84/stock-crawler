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
    public class MockInvestLoopingService : LoopingService
    {
        private readonly MockInvestService _mockInvestService;

        private readonly AutoTradeService _autoTradeService;

        private readonly StockDataService _stockDataService;

        public MockInvestLoopingService(MockInvestService mockInvestService,
            AutoTradeService autoTradeService,
            StockDataService stockDataService)
        {
            _mockInvestService = mockInvestService;
            _autoTradeService = autoTradeService;
            _stockDataService = stockDataService;
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

                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }


        protected void DoWork()
        {
            var now = DateTime.Now;
#if !DEBUG
            // 토요일 일요일은 모의 투자 안함
            if (now.DayOfWeek == DayOfWeek.Saturday || now.DayOfWeek == DayOfWeek.Sunday)
            {
                return;
            }
#endif

            var allAutoTrades = _autoTradeService.GetAutoTrades().Result;
            foreach (var autoTrade in allAutoTrades)
            {
                var latest = _stockDataService.Latest(7, autoTrade.Code).Result;
                if (latest == null)
                {
                    continue;
                }
#if !DEBUG
                if (latest.Date != DateTime.Now.Date) // 오늘 데이터가 없으면 패스
                {
                    continue;
                }
#endif

                var mockInvest = _mockInvestService.Get(autoTrade.UserId, autoTrade.Code, null).Result;
                if (mockInvest != null) // 이미 샀으면 유지 혹은 매도만
                {
                    // enum behaviour로 함수 추가해서 리팩토링
                    switch (autoTrade.SellTradeType)
                    {
                        case Code.AutoTradeType.PriceUnder:
                            if (Math.Abs(100 - ((double)mockInvest.BuyPrice / latest.Latest * 100)) > autoTrade.SellCondition)
                            {
                                continue;
                            }
                            break;
                        case Code.AutoTradeType.PriceOver:
                            if (Math.Abs(100 - ((double)mockInvest.BuyPrice / latest.Latest * 100)) < autoTrade.SellCondition)
                            {
                                continue;
                            }
                            break;
                        case Code.AutoTradeType.Time:
                            {
                                var target = mockInvest.Created.Add(TimeSpan.FromHours(autoTrade.SellCondition));
                                if (DateTime.Now < target)
                                {
                                    continue;
                                }
                            }
                            break;
                        default:
                            throw new DeveloperException(Code.ResultCode.NotImplementedYet);
                    }

                    // 매도
                    try
                    {
                        var sell = _mockInvestService.Sell(new Protocols.Request.MockInvestSell
                        {
                            UserId = autoTrade.UserId,
                            SellList = new List<Protocols.Common.MockInvestSell> {
                                            new Protocols.Common.MockInvestSell {
                                                Id = mockInvest.Id,
                                                Amount = mockInvest.Amount
                                            }
                                        }
                        }).Result;

                        var next = _mockInvestService.NextAnalysis(autoTrade, allAutoTrades.Where(x => x.UserId == autoTrade.UserId).ToList()).Result;
                        if (next != null)
                        {
                            autoTrade.Code = next.Code;
                        }

                        autoTrade.Balance += sell.Datas.Sum(x => x.TotalCurrentPrice.Value);
                        _ = _autoTradeService.Update(autoTrade);
                    }
                    catch (DeveloperException e)
                    {
                        Log.Logger.Error(e.Message);
                        Log.Logger.Error(e.Detail);
                        continue;
                    }
                }
                else
                {
                    // enum behaviour로 함수 추가해서 리팩토링
                    switch (autoTrade.BuyTradeType)
                    {
                        case Code.AutoTradeType.PriceUnder:
                            if (Math.Abs(100 - ((double)latest.Start / latest.Latest * 100)) > autoTrade.BuyCondition)
                            {
                                continue;
                            }
                            break;
                        case Code.AutoTradeType.PriceOver:
                            if (Math.Abs(100 - ((double)latest.Start / latest.Latest * 100)) < autoTrade.BuyCondition)
                            {
                                continue;
                            }
                            break;
                        case Code.AutoTradeType.Time:
                            {
                                var target = DateTime.Now.Date.Add(TimeSpan.FromHours(autoTrade.BuyCondition));
                                if (DateTime.Now < target)
                                {
                                    continue;
                                }
                            }
                            break;
                        default:
                            throw new DeveloperException(Code.ResultCode.NotImplementedYet);
                    }

                    // 매수
                    try
                    {
                        var buy = _mockInvestService.Buy(new Protocols.Request.MockInvestBuy
                        {
                            UserId = autoTrade.UserId,
                            Code = autoTrade.Code,
                            Amount = (int)(autoTrade.Balance / latest.Latest)
                        }).Result;

                        autoTrade.Balance -= buy.Data.TotalBuyPrice;
                        _ = _autoTradeService.Update(autoTrade);
                    }
                    catch (DeveloperException e)
                    {
                        Log.Logger.Error(e.Message);
                        Log.Logger.Error(e.Detail);
                        continue;
                    }
                }
            }
        }
    }
}

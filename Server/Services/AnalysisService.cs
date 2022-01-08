﻿using Server.Models;
using MongoDbWebUtil.Util;
using System;
using System.Threading.Tasks;
using MongoDbWebUtil.Services;
using System.Collections.Generic;
using MongoDB.Driver;
using System.Linq;
using Server.Code;
using StockCrawler.Models;
using EzAspDotNet.Exception;
using EnumExtend;

namespace Server.Services
{
    public class AnalysisService
    {
        private readonly MongoDbUtil<NaverStock> _mongoDbNaverStock;

        private readonly MongoDbUtil<Analysis> _mongoDbAnalysis;

        private readonly CompanyService _companyService;

        private readonly StockDataService _stockDataService;

        public AnalysisService(MongoDbService mongoDbService,
            CompanyService companyService,
            StockDataService stockDataService)
        {
            _mongoDbNaverStock = new MongoDbUtil<NaverStock>(mongoDbService.Database);
            _mongoDbAnalysis = new MongoDbUtil<Analysis>(mongoDbService.Database);
            _companyService = companyService;
            _stockDataService = stockDataService;

            _mongoDbAnalysis.Collection.Indexes.CreateOne(new CreateIndexModel<Analysis>(
                Builders<Analysis>.IndexKeys.Descending(x => x.Date).Ascending(x => x.Code)));
        }

        public async Task<long> CountAsync(DateTime? dateTime)
        {
            var date = dateTime.GetValueOrDefault(DateTime.Now).Date;
            return await _mongoDbAnalysis.CountAsync(Builders<Analysis>.Filter.Eq(x => x.Date, date));
        }

        public async Task<Protocols.Response.Analysis> Execute(Protocols.Request.Analysis analysis)
        {
            var Datas = new List<Analysis>();
            if (analysis.Days == null || analysis.Days.Count < 2)
            {
                throw new DeveloperException(ResultCode.AnalysisNeedComparable2DaysData);
            }

            analysis.Days = analysis.Days.OrderBy(x => x).ToList();

            var codes = analysis.All ? (await _companyService.All()).ConvertAll(x => x.Code) : analysis.Codes;

            var date = analysis.Date.GetValueOrDefault(DateTime.Now).Date;

#if DEBUG //TODO 운용중인 PC 성능 이슈로 DEBUG가 더 빠른컴퓨터인지라 의도적으로 이렇게 처리했다.
            Parallel.ForEach(codes, new ParallelOptions { MaxDegreeOfParallelism = 256 }, code =>
#else
            Parallel.ForEach(codes, new ParallelOptions { MaxDegreeOfParallelism = 4 }, code =>
#endif
            {
                var stockEvaluate = new StockEvaluate();

                var builder = Builders<NaverStock>.Filter;
                var filter = builder.Lt(x => x.Date, date) & builder.Gte(x => x.Date, date.AddDays(-(analysis.Days.Last() / 5) * 7 + analysis.Days.Last() % 5)) & builder.Eq(x => x.Code, code);
                var stockDatas = _mongoDbNaverStock.Find(filter);

                foreach (var day in analysis.Days)
                {
                    var stocks = stockDatas.Where(x => x.Date < date && x.Date >= date.AddDays(-(day / 5) * 7 + day % 5)).ToList();
                    if (stocks.Any())
                    {
                        stockEvaluate.MovingAverageLines.Add(day, stocks.Sum(x => x.Latest) / stocks.Count);
                        stockEvaluate.TransactionPrice = stocks.Last().Latest * stocks.Last().TradeCount;
                        stockEvaluate.TradeCount = stocks.Sum(x => (long)x.TradeCount) / stocks.Count;
                        stockEvaluate.Fluctuation = stocks.Sum(x => (long)x.Change) / stocks.Count; // 현재는 하루단위라서, 하루 내에 변동이 컸어도 인지가 안됨. 추후 데이터 보강시 변경해야 함.
                    }
                }

                if (stockEvaluate.MovingAverageLines.Any())
                {
                    stockEvaluate.BuyStockValue = (double)stockEvaluate.MovingAverageLines.First().Value - (double)stockEvaluate.MovingAverageLines.Last().Value;

                    var analysisData = new Analysis
                    {
                        Code = code,
                        Date = date,
                        StockEvaluate = stockEvaluate
                    };

                    _ = _mongoDbAnalysis.UpsertAsync(Builders<Analysis>.Filter.Eq(x => x.Date, date) & Builders<Analysis>.Filter.Eq(x => x.Code, code), analysisData);
                    Datas.Add(analysisData);
                }
            });

            return new Protocols.Response.Analysis
            {
                ResultCode = ResultCode.Success,
                Types = analysis.Types,
                Datas = Datas.ConvertAll(x => x.ToProtocol())
            };
        }

        public async Task<List<Analysis>> Get(DateTime date, AnalysisType type, int count, int page)
        {
            return await _mongoDbAnalysis.Page(Builders<Analysis>.Filter.Eq(x => x.Date, date)
                & Builders<Analysis>.Filter.Gt(x => x.StockEvaluate.BuyStockValue, 0),
                count, page * count, ToSortKeyword(type), false);
        }

        private string ToSortKeyword(AnalysisType type)
        {
            switch (type)
            {
                case AnalysisType.GoldenCrossTradeCount:
                    return ClassUtil.GetMemberNameWithDeclaringType((Analysis x) => x.StockEvaluate.TradeCount);
                case AnalysisType.GoldenCrossTransactionPrice:
                    return ClassUtil.GetMemberNameWithDeclaringType((Analysis x) => x.StockEvaluate.TransactionPrice);
                case AnalysisType.FluctuationRate:
                    return ClassUtil.GetMemberNameWithDeclaringType((Analysis x) => x.StockEvaluate.Fluctuation);
                default:
                    throw new DeveloperException(ResultCode.NotImplementedYet);
            }
        }

        public async Task ExecuteBackground()
        {
            await Execute(new Protocols.Request.Analysis
            {
                All = true,
                Date = DateTime.Now.Date,
                Types = EnumUtil.ToEnumList<AnalysisType>(),
                Days = new List<int> { 5, 20 }
            });
        }
    }
}

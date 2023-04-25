using EnumExtend;
using EzAspDotNet.Exception;
using EzAspDotNet.Models;
using EzAspDotNet.Services;
using EzAspDotNet.Util;
using EzMongoDb.Util;
using MongoDB.Driver;
using Server.Code;
using Server.Models;
using StockCrawler.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
            var analyses = new List<Analysis>();
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
                var naverStocks = _mongoDbNaverStock.Find(filter);

                foreach (var day in analysis.Days)
                {
                    var stocks = naverStocks.Where(x => x.Date < date && x.Date >= date.AddDays(-(day / 5) * 7 + day % 5)).ToList();
                    if (!stocks.Any()) continue;

                    stockEvaluate.MovingAverageLines.Add(day, stocks.Sum(x => x.Latest) / stocks.Count);
                    stockEvaluate.TransactionPrice = stocks.Last().Latest * stocks.Last().TradeCount;
                    stockEvaluate.TradeCount = stocks.Sum(x => (double)x.TradeCount) / stocks.Count;
                    stockEvaluate.Fluctuation = stocks.Sum(x => (double)x.Change) / stocks.Count; // 현재는 하루단위라서, 하루 내에 변동이 컸어도 인지가 안됨. 추후 데이터 보강시 변경해야 함.
                }

                if (!stockEvaluate.MovingAverageLines.Any()) return;

                stockEvaluate.BuyStockValue = (double)stockEvaluate.MovingAverageLines.First().Value - (double)stockEvaluate.MovingAverageLines.Last().Value;

                var analysisData = new Analysis
                {
                    Code = code,
                    Date = date,
                    StockEvaluate = stockEvaluate
                };

                _ = _mongoDbAnalysis.UpsertAsync(Builders<Analysis>.Filter.Eq(x => x.Date, date) & Builders<Analysis>.Filter.Eq(x => x.Code, code), analysisData);
                analyses.Add(analysisData);
            });

            return new Protocols.Response.Analysis
            {
                Types = analysis.Types,
                Datas = MapperUtil.Map<List<Analysis>, List<Protocols.Common.Analysis>>(analyses)
            };
        }

        public async Task<List<Analysis>> Get(DateTime date, AnalysisType type, int count, int page)
        {
            return await _mongoDbAnalysis.Page(Builders<Analysis>.Filter.Eq(x => x.Date, date)
                & Builders<Analysis>.Filter.Gt(x => x.StockEvaluate.BuyStockValue, 0),
                count, page * count, ToSortKeyword(type), false);
        }

        private static string ToSortKeyword(AnalysisType type)
        {
            return type switch
            {
                AnalysisType.GoldenCrossTradeCount => ClassUtil.GetMemberNameWithDeclaringType((Analysis x) =>
                    x.StockEvaluate.TradeCount),
                AnalysisType.GoldenCrossTransactionPrice => ClassUtil.GetMemberNameWithDeclaringType((Analysis x) =>
                    x.StockEvaluate.TransactionPrice),
                AnalysisType.FluctuationRate => ClassUtil.GetMemberNameWithDeclaringType((Analysis x) =>
                    x.StockEvaluate.Fluctuation),
                _ => throw new DeveloperException(EzAspDotNet.Protocols.Code.ResultCode.NotImplementedYet)
            };
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

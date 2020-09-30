using Server.Models;
using WebUtil.Util;
using System;
using System.Threading.Tasks;
using WebUtil.Services;
using System.Collections.Generic;
using MongoDB.Driver;
using System.Linq;
using Server.Code;
using Server.Exception;
using StockCrawler.Models;

namespace Server.Services
{
    public class AnalysisService
    {
        private readonly MongoDbUtil<NaverStock> _mongoDbNaverStock;

        private readonly MongoDbUtil<Analysis> _mongoDbAnalysis;

        private readonly CodeService _codeService;

        private readonly StockDataService _stockDataService;

        public AnalysisService(MongoDbService mongoDbService,
            CodeService codeService,
            StockDataService stockDataService)
        {
            _mongoDbNaverStock = new MongoDbUtil<NaverStock>(mongoDbService.Database);
            _mongoDbAnalysis = new MongoDbUtil<Analysis>(mongoDbService.Database);
            _codeService = codeService;
            _stockDataService = stockDataService;

            _mongoDbAnalysis.Collection.Indexes.CreateOne(new CreateIndexModel<Analysis>(
                Builders<Analysis>.IndexKeys.Descending(x => x.Date).Ascending(x => x.Code)));
        }

        public async Task<Protocols.Response.Analysis> Execute(Protocols.Request.Analysis analysis)
        {
            var analysisDatas = new List<Analysis>();
            if (analysis.Days == null || analysis.Days.Count < 2)
            {
                throw new DeveloperException(ResultCode.AnalysisNeedComparable2DaysData);
            }

            analysis.Days = analysis.Days.OrderBy(x => x).ToList();

            var codes = analysis.All ? (await _codeService.All()).ConvertAll(x => x.Value) : analysis.Codes;

            var date = analysis.Date.GetValueOrDefault(DateTime.Now).Date;

            Parallel.ForEach(codes, new ParallelOptions { MaxDegreeOfParallelism = 256 }, code =>
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
                    }
                }

                if (stockEvaluate.MovingAverageLines.Any())
                {
                    stockEvaluate.BuyStockValue = (double)stockEvaluate.MovingAverageLines.First().Value - (double)stockEvaluate.MovingAverageLines.Last().Value;

                    foreach (var analysisType in analysis.Types)
                    {
                        var analysisData = new Analysis
                        {
                            Type = analysisType,
                            Code = code,
                            Date = date,
                            StockEvaluate = stockEvaluate
                        };

                        _ = _mongoDbAnalysis.UpsertAsync(Builders<Analysis>.Filter.Eq(x => x.Date, date) & Builders<Analysis>.Filter.Eq(x => x.Type, analysisType) & Builders<Analysis>.Filter.Eq(x => x.Code, code), analysisData);
                        analysisDatas.Add(analysisData);
                    }
                }
            });

            return new Protocols.Response.Analysis
            {
                ResultCode = ResultCode.Success,
                Types = analysis.Types,
                AnalysisDatas = analysisDatas.ConvertAll(x => x.ToProtocol())
            };
        }

        public async Task<List<Analysis>> Get(DateTime date, AnalysisType type, int count, int page)
        {
            return await _mongoDbAnalysis.Page(Builders<Analysis>.Filter.Eq(x => x.Date, date) & Builders<Analysis>.Filter.Eq(x => x.Type, type) & Builders<Analysis>.Filter.Gt(x => x.StockEvaluate.BuyStockValue, 0),
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

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

namespace Server.Services
{
    public class AnalysisService
    {
        private readonly MongoDbUtil<NaverStock> _mongoDbNaverStock;

        private readonly MongoDbUtil<Models.Analysis> _mongoDbAnalysis;

        private readonly CodeService _codeService;

        private readonly StockDataService _stockDataService;

        public AnalysisService(MongoDbService mongoDbService,
            CodeService codeService,
            StockDataService stockDataService)
        {
            _mongoDbNaverStock = new MongoDbUtil<NaverStock>(mongoDbService.Database);
            _mongoDbAnalysis = new MongoDbUtil<Models.Analysis>(mongoDbService.Database);
            _codeService = codeService;
            _stockDataService = stockDataService;
        }

        public async Task<Protocols.Response.Analysis> Execute(Protocols.Request.Analysis analysis)
        {
            var analysisDatas = new List<Models.Analysis>();
            analysis.Days = analysis.Days.OrderBy(x => x).ToList();

            var codes = analysis.All ? (await _codeService.All()).ConvertAll(x => x.Value) : analysis.Codes;

            var date = analysis.Date != null ? analysis.Date.Date : DateTime.Now.Date;

            Parallel.ForEach(codes, new ParallelOptions { MaxDegreeOfParallelism = 256 }, code =>
            {
                var stockEvaluate = new Models.StockEvaluate();

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
                        stockEvaluate.TradeCount = stocks.Sum(x => x.TradeCount) / stocks.Count;
                    }
                }

                if (stockEvaluate.MovingAverageLines.Any())
                {
                    stockEvaluate.BuyStockValue = (double)stockEvaluate.MovingAverageLines.First().Value - (double)stockEvaluate.MovingAverageLines.Last().Value;

                    var analysisData = new Models.Analysis
                    {
                        Type = analysis.Type,
                        Code = code,
                        Date = date,
                        StockEvaluate = stockEvaluate
                    };

                    var origin = _mongoDbAnalysis.FindOne(Builders<Models.Analysis>.Filter.Eq(x => x.Date, date) & Builders<Models.Analysis>.Filter.Eq(x => x.Type, analysis.Type) & Builders<Models.Analysis>.Filter.Eq(x => x.Code, code));
                    if (origin != null)
                    {
                        analysisData.Id = origin.Id;
                        _mongoDbAnalysis.Update(analysisData.Id, analysisData);
                    }
                    else
                    {
                        _mongoDbAnalysis.Create(analysisData);
                    }

                    analysisDatas.Add(analysisData);
                }
            });

            return new Protocols.Response.Analysis
            {
                ResultCode = ResultCode.Success,
                Type = analysis.Type,
                AnalysisDatas = analysisDatas.ConvertAll(x => x.ToProtocol())
            };
        }

        public async Task<List<Models.Analysis>> Get(DateTime date, AnalysisType type, int count, int page)
        {
            return await _mongoDbAnalysis.Page(Builders<Models.Analysis>.Filter.Eq(x => x.Date, date) & Builders<Models.Analysis>.Filter.Eq(x => x.Type, type) & Builders<Models.Analysis>.Filter.Gt(x => x.StockEvaluate.BuyStockValue, 0),
                page * count, count, ToSortKeyword(type), false);
        }

        private string ToSortKeyword(AnalysisType type)
        {
            switch (type)
            {
                case AnalysisType.GoldenCrossTradeCount:
                    return ClassUtil.GetMemberNameWithDeclaringType((Models.Analysis x) => x.StockEvaluate.TradeCount);
                case AnalysisType.GoldenCrossTransactionPrice:
                    return ClassUtil.GetMemberNameWithDeclaringType((Models.Analysis x) => x.StockEvaluate.TransactionPrice);
                default:
                    throw new DeveloperException(ResultCode.NotImplementedYet);
            }
        }
    }
}

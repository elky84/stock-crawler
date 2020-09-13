using Server.Models;
using WebUtil.Util;
using System;
using System.Threading.Tasks;
using WebUtil.Services;
using System.Collections.Generic;
using MongoDB.Driver;
using Server.Protocols.Common;
using System.Linq;
using Server.Code;

namespace Server.Services
{
    public class AnalysisService
    {
        private readonly MongoDbUtil<NaverStock> _mongoDbNaverStock;

        private readonly MongoDbUtil<Models.Analysis> _mongoDbAnalysis;

        private readonly CodeService _codeService;

        public AnalysisService(MongoDbService mongoDbService,
            CodeService codeService)
        {
            _mongoDbNaverStock = new MongoDbUtil<NaverStock>(mongoDbService.Database);
            _mongoDbAnalysis = new MongoDbUtil<Models.Analysis>(mongoDbService.Database);
            _codeService = codeService;
        }

        public async Task<Protocols.Response.Analysis> Execute(Protocols.Request.Analysis analysis)
        {
            var analysisDatas = new List<Models.Analysis>();
            analysis.Days = analysis.Days.OrderBy(x => x).ToList();

            var codes = analysis.All ? (await _codeService.All()).ConvertAll(x => x.Value) : analysis.Codes;

            Parallel.ForEach(codes, new ParallelOptions { MaxDegreeOfParallelism = 256 }, code =>
            {
                var today = DateTime.Now.Date;
                var stockEvaluate = new Models.StockEvaluate();

                var builder = Builders<NaverStock>.Filter;
                var filter = builder.Lt(x => x.Date, today) & builder.Gte(x => x.Date, today.AddDays(-(analysis.Days.Last() / 5) * 7 + analysis.Days.Last() % 5)) & builder.Eq(x => x.Code, code);
                var stockDatas = _mongoDbNaverStock.Find(filter);

                foreach (var day in analysis.Days)
                {
                    var stocks = stockDatas.Where(x => x.Date < today && x.Date >= today.AddDays(-(day / 5) * 7 + day % 5)).ToList();
                    if (stocks.Any())
                    {
                        stockEvaluate.MovingAverageLines.Add(day, stocks.Sum(x => x.Latest) / stocks.Count);
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
                        Date = today,
                        StockEvaluate = stockEvaluate
                    };

                    _mongoDbAnalysis.CreateAsync(analysisData).ConfigureAwait(false);

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

        public async Task<List<Models.Analysis>> Get(AnalysisType type, int count)
        {
            return (await _mongoDbAnalysis.FindAsync(Builders<Models.Analysis>.Filter.Eq(x => x.Type, type) & Builders<Models.Analysis>.Filter.Gt(x => x.StockEvaluate.BuyStockValue, 0))).OrderByDescending(x => x.StockEvaluate.TradeCount).Take(count).ToList();
        }
    }
}

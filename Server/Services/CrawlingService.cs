using Server.Models;
using WebUtil.Util;
using System.Linq;
using System.Threading.Tasks;
using WebUtil.Services;
using StockCrawler;
using System.Collections.Generic;
using MongoDB.Driver;
using System.Threading;
using Serilog;
using System;
using Microsoft.Extensions.Hosting;

namespace Server.Services
{
    public class NaverStockCrawlerMongoDb : NaverStockCrawler
    {
        private readonly MongoDbUtil<NaverStock> _mongoDbNaverStock;

        public NaverStockCrawlerMongoDb(MongoDbUtil<NaverStock> mongoDbNaverStock, int page, string code)
            : base(page, code)
        {
            _mongoDbNaverStock = mongoDbNaverStock;
        }

        protected override async Task OnCrawlData(NaverStock naverStock)
        {
            var origin = await _mongoDbNaverStock.FindOneAsync(Builders<NaverStock>.Filter.Eq(x => x.Code, naverStock.Code) & Builders<NaverStock>.Filter.Eq(x => x.Date, naverStock.Date));
            if (origin != null)
            {
                naverStock.Id = origin.Id;
                await _mongoDbNaverStock.UpdateAsync(origin.Id, naverStock);
            }
            else
            {
                await _mongoDbNaverStock.CreateAsync(naverStock);
            }
        }
    }

    public class CrawlingService
    {
        private readonly MongoDbUtil<NaverStock> _mongoDbNaverStock;

        private readonly CodeService _codeService;

        public CrawlingService(MongoDbService mongoDbService,
            CodeService codeService)
        {
            _mongoDbNaverStock = new MongoDbUtil<NaverStock>(mongoDbService.Database);
            _codeService = codeService;
        }

        public async Task<Protocols.Response.Crawling> Execute(Protocols.Request.Crawling crawling)
        {
            var codes = crawling.All ? (await _codeService.All()).Select(x => x.Value) : crawling.Codes;
            Parallel.ForEach(codes, new ParallelOptions { MaxDegreeOfParallelism = 32 },
                code =>
                {
                    Task.WaitAll(Enumerable.Range(1, crawling.Page).ToList().ConvertAll(y => new NaverStockCrawlerMongoDb(_mongoDbNaverStock, y, code).RunAsync()).ToArray());
                }
            );

            return new Protocols.Response.Crawling
            {
                ResultCode = Code.ResultCode.Success
            };
        }


        public async Task ExecuteBackground()
        {
            var codes = (await _codeService.All()).Select(x => x.Value);
            foreach (var code in codes)
            {
                await new NaverStockCrawlerMongoDb(_mongoDbNaverStock, 1, code).RunAsync();
            }
        }
    }
}
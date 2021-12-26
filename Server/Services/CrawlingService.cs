using MongoDbWebUtil.Util;
using System.Linq;
using System.Threading.Tasks;
using MongoDbWebUtil.Services;
using MongoDB.Driver;
using StockCrawler.Models;
using StockCrawler.Crawler;

namespace Server.Services
{
    public class NaverStockDailyCrawlerMongoDb : NaverStockDailyCrawler
    {
        private readonly MongoDbUtil<NaverStock> _mongoDbNaverStock;

        public NaverStockDailyCrawlerMongoDb(MongoDbUtil<NaverStock> mongoDbNaverStock, int page, string code)
            : base(page, code)
        {
            _mongoDbNaverStock = mongoDbNaverStock;
        }

        protected override async Task OnCrawlData(NaverStock naverStock)
        {
            await _mongoDbNaverStock.UpsertAsync(Builders<NaverStock>.Filter.Eq(x => x.Code, naverStock.Code) & Builders<NaverStock>.Filter.Eq(x => x.Date, naverStock.Date), naverStock);
        }
    }

    public class CrawlingService
    {
        private readonly MongoDbUtil<NaverStock> _mongoDbNaverStock;

        private readonly CompanyService _companyService;

        public CrawlingService(MongoDbService mongoDbService,
            CompanyService companyService)
        {
            _mongoDbNaverStock = new MongoDbUtil<NaverStock>(mongoDbService.Database);
            _companyService = companyService;
        }

        public async Task<Protocols.Response.Crawling> Execute(Protocols.Request.Crawling crawling)
        {
            var codes = crawling.All ? (await _companyService.All()).Select(x => x.Code) : crawling.Codes;
#if DEBUG //TODO 운용중인 PC 성능 이슈로 DEBUG가 더 빠른컴퓨터인지라 의도적으로 이렇게 처리했다.
            Parallel.ForEach(codes, new ParallelOptions { MaxDegreeOfParallelism = 32 },
                code =>
                {
                    Task.WaitAll(Enumerable.Range(1, crawling.Page).ToList().ConvertAll(y => new NaverStockDailyCrawlerMongoDb(_mongoDbNaverStock, y, code).RunAsync()).ToArray());
                }
            );
#else
            Parallel.ForEach(codes, new ParallelOptions { MaxDegreeOfParallelism = 4 },
                code =>
                {
                    Task.WaitAll(Enumerable.Range(1, crawling.Page).ToList().ConvertAll(y => new NaverStockDailyCrawlerMongoDb(_mongoDbNaverStock, y, code).RunAsync()).ToArray());
                }
            );
#endif

            return new Protocols.Response.Crawling
            {
                ResultCode = Code.ResultCode.Success
            };
        }


        public async Task ExecuteBackground()
        {
            var codes = (await _companyService.All()).Select(x => x.Code);
            foreach (var code in codes)
            {
                await new NaverStockDailyCrawlerMongoDb(_mongoDbNaverStock, 1, code).RunAsync();
            }
        }
    }
}
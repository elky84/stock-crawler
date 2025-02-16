using EzAspDotNet.Services;
using EzMongoDb.Util;
using MongoDB.Driver;
using StockCrawler.Crawler;
using StockCrawler.Models;
using System.Linq;
using System.Threading.Tasks;

namespace Server.Services
{
    public class NaverStockDailyCrawlerMongoDb(MongoDbUtil<NaverStock> mongoDbNaverStock, int page, string code) : NaverStockDailyCrawler(page, code)
    {

        protected override async Task OnCrawlData(NaverStock naverStock)
        {
            await mongoDbNaverStock.UpsertAsync(Builders<NaverStock>.Filter.Eq(x => x.Code, naverStock.Code) & Builders<NaverStock>.Filter.Eq(x => x.Date, naverStock.Date), naverStock);
        }
    }

    public class CrawlingService(
        MongoDbService mongoDbService,
        CompanyService companyService)
    {
        private readonly MongoDbUtil<NaverStock> _mongoDbNaverStock = new(mongoDbService.Database);

        public async Task<Protocols.Response.Crawling> Execute(Protocols.Request.Crawling crawling)
        {
            var codes = crawling.All ? (await companyService.All()).Select(x => x.Code) : crawling.Codes;

            foreach(var code in codes)
            {
                foreach (var _ in Enumerable.Range(1, crawling.Page).Select(y => new NaverStockDailyCrawlerMongoDb(_mongoDbNaverStock, y, code).RunAsync()))
                {
                }
            }

            return new Protocols.Response.Crawling();
        }


        public async Task ExecuteBackground()
        {
            var codes = (await companyService.All()).Select(x => x.Code);
            foreach (var code in codes)
            {
                await new NaverStockDailyCrawlerMongoDb(_mongoDbNaverStock, 1, code).RunAsync();
            }
        }
    }
}
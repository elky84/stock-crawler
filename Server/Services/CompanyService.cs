using WebUtil.Util;
using System;
using System.Threading.Tasks;
using WebUtil.Services;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Data;
using Server.Protocols.Response;
using AngleSharp.Html.Parser;
using Server.Code;
using Server.Exception;
using MongoDB.Driver;
using System.Text;
using Serilog;
using System.Threading;

namespace Server.Services
{
    public class CompanyService
    {
        private readonly MongoDbUtil<Models.Company> _mongoDbCode;


        public CompanyService(MongoDbService mongoDbService)
        {
            _mongoDbCode = new MongoDbUtil<Models.Company>(mongoDbService.Database);

            _mongoDbCode.Collection.Indexes.CreateOne(new CreateIndexModel<Models.Company>(
                Builders<Models.Company>.IndexKeys.Ascending(x => x.Code)));

            _mongoDbCode.Collection.Indexes.CreateOne(new CreateIndexModel<Models.Company>(
                Builders<Models.Company>.IndexKeys.Ascending(x => x.Type)));
        }

        public async Task<List<Models.Company>> All()
        {
            return await _mongoDbCode.All();
        }

        public async Task<Models.Company> Get(string code)
        {
            return await _mongoDbCode.FindOneAsync(Builders<Models.Company>.Filter.Eq(x => x.Code, code));
        }

        public async Task<Header> CrawlingCode(StockType stockType)
        {
            int executionCount = 0;

            WebClient client = new WebClient();
            var queryMarketType = string.IsNullOrEmpty(stockType.GetDescription()) ? string.Empty : "marketType=" + stockType.GetDescription();
            var byteArr = await client.DownloadDataTaskAsync(new Uri($"http://kind.krx.co.kr/corpgeneral/corpList.do?method=download&searchType=13&{queryMarketType}"));
            var str = Encoding.GetEncoding("ksc_5601").GetString(byteArr);

            var parser = new HtmlParser();
            var document = await parser.ParseDocumentAsync(str);

            var thContent = document.QuerySelectorAll("tbody tr th").Select(x => x.TextContent.Trim()).ToArray();
            var tdContent = document.QuerySelectorAll("tbody tr td").Select(x => x.TextContent.Trim()).ToArray();
            if (!thContent.Any() || !tdContent.Any())
            {
                throw new DeveloperException(ResultCode.NotFoundStockCodeData);
            }

            Parallel.For(0, tdContent.Length / thContent.Length, n =>
            {
                Interlocked.Increment(ref executionCount);

                var cursor = n * thContent.Length;
                _ = OnCrawlingCodeData(new Models.Company
                {
                    Name = tdContent[cursor + 0],
                    Code = tdContent[cursor + 1],
                    Type = stockType
                });
            });

            Log.Information($"Pararell.For Count ({tdContent.Length / thContent.Length}) Executing Count ({executionCount})");
            return new Header { ResultCode = ResultCode.Success };
        }

        public async Task OnCrawlingCodeData(Models.Company code)
        {
            var origin = await _mongoDbCode.FindOneAsync(Builders<Models.Company>.Filter.Eq(x => x.Code, code.Code));
            if (origin == null)
            {
                await _mongoDbCode.CreateAsync(code);
            }
        }

        public async Task ExecuteBackground()
        {
            await CrawlingCode(StockType.All);
        }
    }
}

using EzAspDotNet.Util;
using System;
using System.Threading.Tasks;
using EzAspDotNet.Services;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Data;
using AngleSharp.Html.Parser;
using Server.Code;
using MongoDB.Driver;
using System.Text;
using Serilog;
using System.Threading;
using StockCrawler.Entity;
using StockCrawler.Crawler;
using EzAspDotNet.Exception;
using EnumExtend;
using System.Net.Http;

namespace Server.Services
{
    public class CompanyService
    {
        private readonly MongoDbUtil<Models.Company> _mongoDbCompany;


        public CompanyService(MongoDbService mongoDbService)
        {
            _mongoDbCompany = new MongoDbUtil<Models.Company>(mongoDbService.Database);

            _mongoDbCompany.Collection.Indexes.CreateOne(new CreateIndexModel<Models.Company>(
                Builders<Models.Company>.IndexKeys.Ascending(x => x.Code)));

            _mongoDbCompany.Collection.Indexes.CreateOne(new CreateIndexModel<Models.Company>(
                Builders<Models.Company>.IndexKeys.Ascending(x => x.Type)));
        }

        public async Task<List<Models.Company>> All()
        {
            return await _mongoDbCompany.All();
        }

        public async Task<Models.Company> Get(string code)
        {
            return await _mongoDbCompany.FindOneAsync(Builders<Models.Company>.Filter.Eq(x => x.Code, code));
        }


        public async Task<Models.Company> NotAlert(string code)
        {
            var company = await Get(code);
            return company == null || company.AlertType != AlertType.Normal ? null : company;
        }

        public async Task<EzAspDotNet.Protocols.ResponseHeader> CrawlingCode(StockType stockType)
        {
            int executionCount = 0;

            var queryMarketType = string.IsNullOrEmpty(stockType.GetDescription()) ? string.Empty : "marketType=" + stockType.GetDescription();
            var responseMessage = await (new HttpClient()).GetAsync(new Uri($"http://kind.krx.co.kr/corpgeneral/corpList.do?method=download&searchType=13&{queryMarketType}"));
            var str = await responseMessage.Content.ReadAsStringAsync();

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
            return new EzAspDotNet.Protocols.ResponseHeader { ResultCode = ResultCode.Success };
        }


        public async Task<EzAspDotNet.Protocols.ResponseHeader> CrawlingAlerts()
        {
            // 기본적으로 모든 변수를 초기화하고 시작
            await _mongoDbCompany.UpdateManyAsync(Builders<Models.Company>.Filter.Empty,
                Builders<Models.Company>.Update.Set(x => x.AlertType, AlertType.Normal));

            await new NaverStockHaltCrawler(OnCrawlAlertHalt).RunAsync();
            await new NaverStockManagementCrawler(OnCrawlAlertManagement).RunAsync();
            return new EzAspDotNet.Protocols.ResponseHeader { ResultCode = ResultCode.Success };
        }

        public void OnCrawlAlertHalt(NaverStockHalt naverStockHalt)
        {
            var origin = _mongoDbCompany.FindOne(Builders<Models.Company>.Filter.Eq(x => x.Code, naverStockHalt.Code));
            if (origin != null)
            {
                origin.AlertType = AlertType.Halt;
                _mongoDbCompany.Update(origin.Id, origin);
            }
        }

        public void OnCrawlAlertManagement(NaverStockManagement naverStockManagement)
        {
            var origin = _mongoDbCompany.FindOne(Builders<Models.Company>.Filter.Eq(x => x.Code, naverStockManagement.Code));
            if (origin != null)
            {
                origin.AlertType = AlertType.Management;
                _mongoDbCompany.Update(origin.Id, origin);
            }
        }

        public async Task OnCrawlingCodeData(Models.Company code)
        {
            var origin = await _mongoDbCompany.FindOneAsync(Builders<Models.Company>.Filter.Eq(x => x.Code, code.Code));
            if (origin == null)
            {
                await _mongoDbCompany.CreateAsync(code);
            }
        }

        public async Task ExecuteBackground()
        {
            await CrawlingCode(StockType.All);
            await CrawlingAlerts();
        }
    }
}

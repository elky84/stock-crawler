﻿using AngleSharp.Html.Parser;
using EnumExtend;
using EzAspDotNet.Exception;
using EzAspDotNet.Services;
using EzMongoDb.Util;
using MongoDB.Driver;
using Serilog;
using Server.Code;
using StockCrawler.Crawler;
using StockCrawler.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

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
            var executionCount = 0;

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

            Log.Information("Parallel.For Count ({TdContentLength}) Executing Count ({ExecutionCount})", tdContent.Length / thContent.Length, executionCount);
            return new EzAspDotNet.Protocols.ResponseHeader();
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

        private void OnCrawlAlertHalt(NaverStockHalt naverStockHalt)
        {
            var origin = _mongoDbCompany.FindOne(Builders<Models.Company>.Filter.Eq(x => x.Code, naverStockHalt.Code));
            if (origin == null) return;
            
            origin.AlertType = AlertType.Halt;
            _mongoDbCompany.Update(origin.Id, origin);
        }

        private void OnCrawlAlertManagement(NaverStockManagement naverStockManagement)
        {
            var origin = _mongoDbCompany.FindOne(Builders<Models.Company>.Filter.Eq(x => x.Code, naverStockManagement.Code));
            if (origin == null) return;
            
            origin.AlertType = AlertType.Management;
            _mongoDbCompany.Update(origin.Id, origin);
        }

        private async Task OnCrawlingCodeData(Models.Company code)
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

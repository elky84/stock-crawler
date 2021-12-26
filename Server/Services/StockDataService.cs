using Server.Models;
using MongoDbWebUtil.Util;
using System.Threading.Tasks;
using MongoDbWebUtil.Services;
using MongoDB.Driver;
using System;
using System.Linq;
using System.Collections.Generic;
using Server.Exception;
using StockCrawler.Models;

namespace Server.Services
{
    public class StockDataService
    {
        private readonly MongoDbUtil<NaverStock> _mongoDbNaverStock;

        public StockDataService(MongoDbService mongoDbService)
        {
            _mongoDbNaverStock = new MongoDbUtil<NaverStock>(mongoDbService.Database);

            _mongoDbNaverStock.Collection.Indexes.CreateOne(new CreateIndexModel<NaverStock>(
                Builders<NaverStock>.IndexKeys.Descending(x => x.Date).Ascending(x => x.Code)));
        }

        public async Task<List<NaverStock>> Get(int day, string code, DateTime? date = null)
        {
            date = date.HasValue ? date.Value.Date : DateTime.Now.Date;

            var builder = Builders<NaverStock>.Filter;
            var filter = builder.Lte(x => x.Date, date.Value) & builder.Gte(x => x.Date, date.Value.AddDays(-day)) & builder.Eq(x => x.Code, code);
            return await _mongoDbNaverStock.FindAsync(filter);
        }


        public async Task<NaverStock> Latest(int day, string code, DateTime? date = null)
        {
            date = date.HasValue ? date.Value.Date : DateTime.Now.Date;
            var builder = Builders<NaverStock>.Filter;
            var filter = builder.Lte(x => x.Date, date.Value) & builder.Gte(x => x.Date, date.Value.AddDays(-day)) & builder.Eq(x => x.Code, code);
            var stockDatas = await _mongoDbNaverStock.FindAsync(filter);
            return stockDatas.OrderByDescending(x => x.Date).FirstOrDefault();
        }
    }
}

using Server.Models;
using WebUtil.Util;
using System.Threading.Tasks;
using WebUtil.Services;
using MongoDB.Driver;
using System;
using System.Linq;
using System.Collections.Generic;

namespace Server.Services
{
    public class StockDataService
    {
        private readonly MongoDbUtil<NaverStock> _mongoDbNaverStock;

        public StockDataService(MongoDbService mongoDbService)
        {
            _mongoDbNaverStock = new MongoDbUtil<NaverStock>(mongoDbService.Database);
        }

        public async Task<List<NaverStock>> Get(int day, string code)
        {
            var builder = Builders<NaverStock>.Filter;
            var filter = builder.Gte(x => x.Date, DateTime.Now.Date.AddDays(-day)) & builder.Eq(x => x.Code, code);
            return await _mongoDbNaverStock.FindAsync(filter);
        }


        public async Task<NaverStock> Latest(int day, string code)
        {
            var builder = Builders<NaverStock>.Filter;
            var filter = builder.Gte(x => x.Date, DateTime.Now.Date.AddDays(-day)) & builder.Eq(x => x.Code, code);
            var stockDatas = await _mongoDbNaverStock.FindAsync(filter);
            return stockDatas.OrderByDescending(x => x.Date).First();
        }
    }
}

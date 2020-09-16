using Server.Models;
using WebUtil.Util;
using System.Threading.Tasks;
using WebUtil.Services;
using MongoDB.Driver;
using System;
using System.Linq;
using System.Collections.Generic;
using Server.Exception;

namespace Server.Services
{
    public class StockDataService
    {
        private readonly MongoDbUtil<NaverStock> _mongoDbNaverStock;

        public StockDataService(MongoDbService mongoDbService)
        {
            _mongoDbNaverStock = new MongoDbUtil<NaverStock>(mongoDbService.Database);
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
            var first = stockDatas.OrderByDescending(x => x.Date).FirstOrDefault();
            if (first == null)
            {
                throw new DeveloperException(Code.ResultCode.NotFoundStockData);
            }
            return first;
        }
    }
}

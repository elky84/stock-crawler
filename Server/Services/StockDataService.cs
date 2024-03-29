﻿using EzAspDotNet.Services;
using EzMongoDb.Util;
using MongoDB.Driver;
using StockCrawler.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
            date = date?.Date ?? DateTime.Now.Date;

            var builder = Builders<NaverStock>.Filter;
            var filter = builder.Lte(x => x.Date, date.Value) & builder.Gte(x => x.Date, date.Value.AddDays(-day)) & builder.Eq(x => x.Code, code);
            return await _mongoDbNaverStock.FindAsync(filter);
        }


        public async Task<NaverStock> Latest(int day, string code, DateTime? date = null)
        {
            date = date?.Date ?? DateTime.Now.Date;
            var builder = Builders<NaverStock>.Filter;
            var filter = builder.Lte(x => x.Date, date.Value) & builder.Gte(x => x.Date, date.Value.AddDays(-day)) & builder.Eq(x => x.Code, code);
            var naverStocks = await _mongoDbNaverStock.FindAsync(filter);
            return naverStocks.MaxBy(x => x.Date);
        }
    }
}

using Server.Models;
using WebUtil.Util;
using System.Threading.Tasks;
using WebUtil.Services;
using MongoDB.Driver;
using System;
using System.Linq;
using System.Collections.Generic;
using Server.Exception;
using Server.Code;

namespace Server.Services
{
    public class MockInvestHistoryService
    {
        private readonly MongoDbUtil<MockInvestHistory> _mongoDbMockInvestHistory;

        private readonly UserService _userService;

        public MockInvestHistoryService(MongoDbService mongoDbService,
            UserService userService)
        {
            _mongoDbMockInvestHistory = new MongoDbUtil<MockInvestHistory>(mongoDbService.Database);
            _userService = userService;

            _mongoDbMockInvestHistory.Collection.Indexes.CreateOne(new CreateIndexModel<MockInvestHistory>(
                Builders<MockInvestHistory>.IndexKeys.Ascending(x => x.UserId)));

            _mongoDbMockInvestHistory.Collection.Indexes.CreateOne(new CreateIndexModel<MockInvestHistory>(
                Builders<MockInvestHistory>.IndexKeys.Descending(x => x.Created)));
        }

        public async Task<MockInvestHistory> Write(HistoryType type, MockInvest mockInvest, int? amount = null)
        {
            return await _mongoDbMockInvestHistory.CreateAsync(new MockInvestHistory(type, mockInvest, amount));
        }

        public async Task<Protocols.Response.MockInvestHistories> Get(string userId)
        {
            return new Protocols.Response.MockInvestHistories
            {
                User = (await _userService.GetByUserId(userId)).ToProtocol(),
                Datas = (await _mongoDbMockInvestHistory.FindAsync(Builders<MockInvestHistory>.Filter.Eq(x => x.UserId, userId))).ConvertAll(x => x.ToProtocol())
            };
        }
    }
}

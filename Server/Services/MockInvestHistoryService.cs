using EzAspDotNet.Exception;
using EzAspDotNet.Models;
using EzAspDotNet.Services;
using EzAspDotNet.Util;
using MongoDB.Driver;
using Server.Code;
using Server.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
            var user = await _userService.GetByUserId(userId);
            if (user == null)
            {
                throw new DeveloperException(Code.ResultCode.NotFoundData);
            }

            return new Protocols.Response.MockInvestHistories
            {
                User = MapperUtil.Map<Protocols.Common.User>(user),
                Datas = MapperUtil.Map<List<MockInvestHistory>, List<Protocols.Common.MockInvestHistory>>
                                      (await _mongoDbMockInvestHistory.FindAsync(Builders<MockInvestHistory>.Filter.Eq(x => x.UserId, userId)))
            };
        }

        public async Task<List<MockInvestHistory>> Get(string userId, DateTime date)
        {
            return await _mongoDbMockInvestHistory.FindAsync(Builders<MockInvestHistory>.Filter.Eq(x => x.UserId, userId) &
                Builders<MockInvestHistory>.Filter.Eq(x => x.Date, date));
        }
    }
}

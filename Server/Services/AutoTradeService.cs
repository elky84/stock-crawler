using Server.Models;
using WebUtil.Util;
using System.Threading.Tasks;
using WebUtil.Services;
using MongoDB.Driver;
using Server.Exception;
using System.Collections.Generic;
using WebUtil.Models;

namespace Server.Services
{
    public class AutoTradeService
    {
        private readonly MongoDbUtil<AutoTrade> _mongoDbAutoTrade;

        public AutoTradeService(MongoDbService mongoDbService)
        {
            _mongoDbAutoTrade = new MongoDbUtil<AutoTrade>(mongoDbService.Database);

            _mongoDbAutoTrade.Collection.Indexes.CreateOne(new CreateIndexModel<AutoTrade>(
                Builders<AutoTrade>.IndexKeys.Ascending(x => x.UserId)));
        }

        public async Task<Protocols.Response.AutoTrade> Create(Protocols.Request.AutoTrade autoTrade)
        {
            try
            {
                var created = await CreateAsync(autoTrade);
                return new Protocols.Response.AutoTrade
                {
                    ResultCode = Code.ResultCode.Success,
                    Datas = new List<Protocols.Common.AutoTrade> { created?.ToProtocol() }
                };
            }
            catch (MongoWriteException)
            {
                throw new DeveloperException(Code.ResultCode.UsingAutoTradeId);
            }
        }

        public async Task<AutoTrade> CreateAsync(Protocols.Request.AutoTrade autoTrade)
        {
            try
            {
                return await _mongoDbAutoTrade.CreateAsync(autoTrade.ToModel());
            }
            catch (MongoWriteException)
            {
                throw new DeveloperException(Code.ResultCode.UsingAutoTradeId);
            }
        }

        public async Task<Protocols.Response.AutoTrade> Get(string id)
        {
            return new Protocols.Response.AutoTrade
            {
                ResultCode = Code.ResultCode.Success,
                Datas = new List<Protocols.Common.AutoTrade> { (await _mongoDbAutoTrade.FindOneAsyncById(id))?.ToProtocol() }
            };
        }

        public async Task<List<AutoTrade>> GetByUserId(string userId)
        {
            return await _mongoDbAutoTrade.FindAsync(Builders<AutoTrade>.Filter.Eq(x => x.UserId, userId));
        }

        public async Task<List<AutoTrade>> Get(FilterDefinition<AutoTrade> filter)
        {
            return await _mongoDbAutoTrade.FindAsync(filter);
        }

        public async Task<List<AutoTrade>> GetAutoTrades()
        {
            return await _mongoDbAutoTrade.All();
        }

        public async Task<Protocols.Response.AutoTrade> Update(string id, Protocols.Request.AutoTrade autoTrade)
        {
            autoTrade.Id = id;
            var update = autoTrade.ToModel();

            var updated = await _mongoDbAutoTrade.UpdateAsync(id, update);
            return new Protocols.Response.AutoTrade
            {
                ResultCode = Code.ResultCode.Success,
                Datas = new List<Protocols.Common.AutoTrade> { (updated ?? update).ToProtocol() }
            };
        }

        public async Task<AutoTrade> Update(AutoTrade autoTrade)
        {
            await _mongoDbAutoTrade.UpdateAsync(autoTrade.Id, autoTrade);
            return autoTrade;
        }


        public async Task<Protocols.Response.AutoTrade> Delete(string id)
        {
            return new Protocols.Response.AutoTrade
            {
                ResultCode = Code.ResultCode.Success,
                Datas = new List<Protocols.Common.AutoTrade> {
                    (await _mongoDbAutoTrade.RemoveAsync(id))?.ToProtocol()
                }
            };
        }

        public async Task<Protocols.Response.AutoTrade> DeleteByUserId(string userId)
        {
            await _mongoDbAutoTrade.RemoveManyAsync(Builders<AutoTrade>.Filter.Eq(x => x.UserId, userId));
            return new Protocols.Response.AutoTrade
            {
                ResultCode = Code.ResultCode.Success
            };
        }
    }
}

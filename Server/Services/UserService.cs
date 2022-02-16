using Server.Models;
using EzAspDotNet.Util;
using System.Threading.Tasks;
using EzAspDotNet.Services;
using MongoDB.Driver;
using System.Collections.Generic;
using EzAspDotNet.Models;
using EzAspDotNet.Exception;

namespace Server.Services
{
    public class UserService
    {
        private readonly MongoDbUtil<User> _mongoDbUser;

        public UserService(MongoDbService mongoDbService)
        {
            _mongoDbUser = new MongoDbUtil<User>(mongoDbService.Database);

            _mongoDbUser.Collection.Indexes.CreateOne(new CreateIndexModel<User>(Builders<User>.IndexKeys.Ascending(x => x.UserId), new CreateIndexOptions { Unique = true }));
        }

        public async Task<Protocols.Response.User> Create(Protocols.Request.User user)
        {
            try
            {
                var created = await _mongoDbUser.CreateAsync(user.ToModel());

                return new Protocols.Response.User
                {
                    ResultCode = Code.ResultCode.Success,
                    Data = created?.ToProtocol()
                };
            }
            catch (MongoWriteException)
            {
                throw new DeveloperException(Code.ResultCode.UsingUserId);
            }
        }

        public async Task<List<User>> Get()
        {
            return await _mongoDbUser.All();
        }


        public async Task<Protocols.Response.User> Get(string id)
        {
            return new Protocols.Response.User
            {
                ResultCode = Code.ResultCode.Success,
                Data = (await _mongoDbUser.FindOneAsyncById(id))?.ToProtocol()
            };
        }

        public async Task<User> GetByUserId(string userId)
        {
            return await _mongoDbUser.FindOneAsync(Builders<User>.Filter.Eq(x => x.UserId, userId));
        }

        public async Task<List<User>> Get(FilterDefinition<User> filter)
        {
            return await _mongoDbUser.FindAsync(filter);
        }

        public async Task<Protocols.Response.User> Update(string id, Protocols.Request.User user)
        {
            user.Id = id;
            var update = user.ToModel();

            var updated = await _mongoDbUser.UpdateAsync(id, update);
            return new Protocols.Response.User
            {
                ResultCode = Code.ResultCode.Success,
                Data = (updated ?? update).ToProtocol()
            };
        }

        public async Task<Protocols.Response.User> UpdateByUserId(string userId, Protocols.Request.User user)
        {
            var origin = await GetByUserId(userId);

            user.UserId = userId;
            var update = user.ToModel();
            origin.CopyHeader(update);

            var updated = await _mongoDbUser.UpdateAsync(Builders<User>.Filter.Eq(x => x.UserId, userId), update);
            return new Protocols.Response.User
            {
                ResultCode = Code.ResultCode.Success,
                Data = (updated ?? update).ToProtocol()
            };
        }

        public async Task<User> Update(User user)
        {
            await _mongoDbUser.UpdateAsync(Builders<User>.Filter.Eq(x => x.UserId, user.UserId), user);
            return user;
        }


        public async Task<Protocols.Response.User> Delete(string id)
        {
            return new Protocols.Response.User
            {
                ResultCode = Code.ResultCode.Success,
                Data = (await _mongoDbUser.RemoveGetAsync(id))?.ToProtocol()
            };
        }

        public async Task<Protocols.Response.User> DeleteByUserId(string userId)
        {
            return new Protocols.Response.User
            {
                ResultCode = Code.ResultCode.Success,
                Data = (await _mongoDbUser.RemoveGetAsync(Builders<User>.Filter.Eq(x => x.UserId, userId)))?.ToProtocol()
            };
        }
    }
}

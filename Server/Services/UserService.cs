using EzAspDotNet.Exception;
using EzAspDotNet.Models;
using EzAspDotNet.Services;
using EzAspDotNet.Util;
using MongoDB.Driver;
using Server.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

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
                var created = await _mongoDbUser.CreateAsync(MapperUtil.Map<User>(user));

                return new Protocols.Response.User
                {
                    ResultCode = Code.ResultCode.Success,
                    Data = MapperUtil.Map<Protocols.Common.User>(created)
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
            var user = await _mongoDbUser.FindOneAsyncById(id);
            if (user == null)
            {
                throw new DeveloperException(Code.ResultCode.NotFoundData);
            }

            return new Protocols.Response.User
            {
                ResultCode = Code.ResultCode.Success,
                Data = MapperUtil.Map<Protocols.Common.User>(user)
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
            var update = MapperUtil.Map<User>(user);

            var updated = await _mongoDbUser.UpdateAsync(id, update);
            if (updated == null)
            {
                throw new DeveloperException(Code.ResultCode.NotFoundData);
            }

            return new Protocols.Response.User
            {
                ResultCode = Code.ResultCode.Success,
                Data = MapperUtil.Map<Protocols.Common.User>(updated)
            };
        }

        public async Task<Protocols.Response.User> UpdateByUserId(string userId, Protocols.Request.User user)
        {
            var origin = await GetByUserId(userId);

            user.UserId = userId;
            var update = MapperUtil.Map<User>(user);
            origin.CopyHeader(update);

            var updated = await _mongoDbUser.UpdateAsync(Builders<User>.Filter.Eq(x => x.UserId, userId), update);
            if (updated == null)
            {
                throw new DeveloperException(Code.ResultCode.NotFoundData);
            }

            return new Protocols.Response.User
            {
                ResultCode = Code.ResultCode.Success,
                Data = MapperUtil.Map<Protocols.Common.User>(updated)
            };
        }

        public async Task<User> Update(User user)
        {
            await _mongoDbUser.UpdateAsync(Builders<User>.Filter.Eq(x => x.UserId, user.UserId), user);
            return user;
        }


        public async Task<Protocols.Response.User> Delete(string id)
        {
            var deleted = await _mongoDbUser.RemoveGetAsync(id);
            if (deleted == null)
            {
                throw new DeveloperException(Code.ResultCode.NotFoundData);
            }
            return new Protocols.Response.User
            {
                ResultCode = Code.ResultCode.Success,
                Data = MapperUtil.Map<Protocols.Common.User>(deleted)
            };
        }

        public async Task<Protocols.Response.User> DeleteByUserId(string userId)
        {
            var deleted = await _mongoDbUser.RemoveGetAsync(Builders<User>.Filter.Eq(x => x.UserId, userId));
            if (deleted == null)
            {
                throw new DeveloperException(Code.ResultCode.NotFoundData);
            }

            return new Protocols.Response.User
            {
                ResultCode = Code.ResultCode.Success,
                Data = MapperUtil.Map<Protocols.Common.User>(deleted)
            };
        }
    }
}

﻿using EzAspDotNet.Exception;
using EzAspDotNet.Models;
using EzAspDotNet.Services;
using EzAspDotNet.Util;
using MongoDB.Driver;
using Server.Code;
using Server.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Server.Services
{
    public class NotificationService
    {
        private readonly MongoDbUtil<Notification> _mongoDbNotification;

        public NotificationService(MongoDbService mongoDbService)
        {
            _mongoDbNotification = new MongoDbUtil<Notification>(mongoDbService.Database);

            _mongoDbNotification.Collection.Indexes.CreateOne(new CreateIndexModel<Notification>(
                Builders<Notification>.IndexKeys.Ascending(x => x.InvestType)));
        }

        public async Task<List<Notification>> All()
        {
            return await _mongoDbNotification.All();
        }

        public async Task<List<Notification>> Get(FilterDefinition<Notification> filter)
        {
            return await _mongoDbNotification.FindAsync(filter);
        }

        public async Task<Protocols.Response.Notification> Create(Protocols.Request.NotificationCreate notification)
        {
            var created = await Create(notification.Data);
            if (created == null)
            {
                throw new DeveloperException(Code.ResultCode.NotFoundData);
            }

            return new Protocols.Response.Notification
            {
                ResultCode = EzAspDotNet.Protocols.Code.ResultCode.Success,
                Data = MapperUtil.Map<Protocols.Common.Notification>(created)
            };

        }

        private async Task<Notification> Create(Protocols.Common.NotificationCreate notification)
        {
            try
            {
                return await _mongoDbNotification.UpsertAsync(Builders<Notification>.Filter.Eq(x => x.InvestType, notification.InvestType),
                    MapperUtil.Map<Notification>(notification));
            }
            catch (MongoWriteException)
            {
                throw new DeveloperException(ResultCode.UsingNotificationId);
            }
        }

        public async Task<Protocols.Response.NotificationMulti> CreateMulti(Protocols.Request.NotificationMulti notificationMulti)
        {
            var notifications = new List<Notification>();
            foreach (var notification in notificationMulti.Datas)
            {
                notifications.Add(await Create(notification));
            }

            return new Protocols.Response.NotificationMulti
            {
                Datas = MapperUtil.Map<List<Notification>, List<Protocols.Common.Notification>>(notifications)
            };
        }

        public async Task<Protocols.Response.Notification> Get(string id)
        {
            var notification = await _mongoDbNotification.FindOneAsyncById(id);
            if (notification == null)
            {
                throw new DeveloperException(Code.ResultCode.NotFoundData);
            }

            return new Protocols.Response.Notification
            {
                ResultCode = EzAspDotNet.Protocols.Code.ResultCode.Success,
                Data = MapperUtil.Map<Protocols.Common.Notification>(notification)
            };
        }


        public async Task<Protocols.Response.Notification> Update(string id, Protocols.Request.NotificationUpdate notificationUpdate)
        {
            var update = MapperUtil.Map<Notification>(notificationUpdate.Data);

            var updated = await _mongoDbNotification.UpdateAsync(id, update);
            if (updated == null)
            {
                throw new DeveloperException(Code.ResultCode.NotFoundData);
            }

            return new Protocols.Response.Notification
            {
                ResultCode = EzAspDotNet.Protocols.Code.ResultCode.Success,
                Data = MapperUtil.Map<Protocols.Common.Notification>(updated)
            };
        }

        public async Task<Protocols.Response.Notification> Delete(string id)
        {
            var deleted = await _mongoDbNotification.RemoveGetAsync(id);
            if (deleted == null)
            {
                throw new DeveloperException(Code.ResultCode.NotFoundData);
            }

            return new Protocols.Response.Notification
            {
                ResultCode = ResultCode.Success,
                Data = MapperUtil.Map<Protocols.Common.Notification>(deleted)
            };
        }
    }
}

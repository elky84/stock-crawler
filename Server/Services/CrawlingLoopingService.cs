using System.Threading.Tasks;
using MongoDbWebUtil.Services;
using System.Threading;
using System;
using EzAspDotNet.Exception;

namespace Server.Services
{
    public class CrawlingLoopingService : LoopingService
    {
        private readonly CrawlingService _crawlingService;

        public CrawlingLoopingService(CrawlingService crawlingService
            )
        {
            _crawlingService = crawlingService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    DoWork();
                }
                catch (System.Exception e)
                {
                    e.ExceptionLog();
                }

                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }


        protected void DoWork()
        {
            var now = DateTime.Now;
            // 토요일 일요일은 크롤링 안함
            if (now.DayOfWeek == DayOfWeek.Saturday || now.DayOfWeek == DayOfWeek.Sunday)
            {
                return;
            }

            var openTime = now.Date.AddHours(8).AddMinutes(30); // 30분 일찍 크롤링SDS시작!
            var closeTime = now.Date.AddHours(15).AddMinutes(30);

            if (openTime <= now && closeTime >= now)
            {
                _ = _crawlingService.ExecuteBackground();
            }
        }
    }
}

using System.Threading.Tasks;
using System.Threading;
using System;
using EzAspDotNet.Exception;
using EzAspDotNet.Services;

namespace Server.Services
{
    public class CrawlingLoopingService(CrawlingService crawlingService) : LoopingService
    {

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


        private void DoWork()
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
                _ = crawlingService.ExecuteBackground();
            }
        }
    }
}

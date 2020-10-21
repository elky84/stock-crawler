using System.Threading.Tasks;
using WebUtil.Services;
using System.Threading;
using Serilog;
using System;

namespace Server.Services
{
    public class CompanyLoopingService : LoopingService
    {
        private readonly CompanyService _companyService;

        public CompanyLoopingService(CompanyService companyService
            )
        {
            _companyService = companyService;
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
                    Log.Logger.Error($"Implement Task Exception. Reason:{e.Message}");

                }

                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }


        protected void DoWork()
        {
            var now = DateTime.Now;
            // 토요일 일요일은 안가져옴 안함
            if (now.DayOfWeek == DayOfWeek.Saturday || now.DayOfWeek == DayOfWeek.Sunday)
            {
                return;
            }

            // 오전 8시~9시에만 실행
            if (now.Hour >= 8 &&
                now.Hour <= 9)
            {
                _ = _companyService.ExecuteBackground();
            }
        }
    }
}

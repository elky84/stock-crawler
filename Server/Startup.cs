using AutoMapper;
using EzAspDotNet.Exception;
using EzAspDotNet.Services;
using EzAspDotNet.StartUp;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Serilog;
using Server.Services;
using System.Text;

namespace Server
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            System.Text.EncodingProvider provider = System.Text.CodePagesEncodingProvider.Instance;
            Encoding.RegisterProvider(provider);

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Error()
                .WriteTo.Console()
                .CreateLogger();

            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            EzAspDotNet.Models.MapperUtil.Initialize(
                new MapperConfiguration(cfg =>
                {
                    cfg.CreateMap<EzAspDotNet.Notification.Models.Notification, Protocols.Common.Notification>();
                    cfg.CreateMap<Protocols.Common.Notification, EzAspDotNet.Notification.Models.Notification>();

                    cfg.CreateMap<Models.Notification, Protocols.Common.Notification>();
                    cfg.CreateMap<Protocols.Common.Notification, Models.Notification>();

                    cfg.CreateMap<Models.AutoTrade, Protocols.Common.AutoTrade>();
                    cfg.CreateMap<Protocols.Common.AutoTrade, Models.AutoTrade>();

                    cfg.CreateMap<Models.Analysis, Protocols.Common.Analysis>();
                    cfg.CreateMap<Protocols.Common.Analysis, Models.Analysis>();

                    cfg.CreateMap<Models.Company, Protocols.Common.Company>();
                    cfg.CreateMap<Protocols.Common.Company, Models.Company>();

                    cfg.CreateMap<Models.MockInvest, Protocols.Common.MockInvest>();
                    cfg.CreateMap<Protocols.Common.MockInvest, Models.MockInvest>();

                    cfg.CreateMap<Models.MockInvestHistory, Protocols.Common.MockInvestHistory>();
                    cfg.CreateMap<Protocols.Common.MockInvestHistory, Models.MockInvestHistory>();

                    cfg.CreateMap<Models.StockEvaluate, Protocols.Common.StockEvaluate>();
                    cfg.CreateMap<Protocols.Common.StockEvaluate, Models.StockEvaluate>();

                    cfg.CreateMap<Models.User, Protocols.Common.User>();
                    cfg.CreateMap<Protocols.Common.User, Models.User>();

                })
            );

            services.CommonConfigureServices();

            services.AddHttpClient();

            services.AddControllers().AddNewtonsoftJson();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "API", Version = "v1" });
            });

            services.ConfigureSwaggerGen(options =>
            {
                options.CustomSchemaIds(x => x.FullName);
            });

            services.AddSingleton<IHostedService, CrawlingLoopingService>();
            services.AddSingleton<IHostedService, AnalysisLoopingService>();
            services.AddSingleton<IHostedService, MockInvestLoopingService>();
            services.AddSingleton<IHostedService, WebHookLoopingService>();
            services.AddSingleton<IHostedService, CompanyLoopingService>();

            services.AddSingleton<CrawlingService>();
            services.AddSingleton<MockInvestService>();
            services.AddSingleton<AnalysisService>();
            services.AddSingleton<UserService>();
            services.AddSingleton<StockDataService>();
            services.AddSingleton<MockInvestHistoryService>();
            services.AddSingleton<CompanyService>();
            services.AddSingleton<AutoTradeService>();
            services.AddSingleton<NotificationService>();

            services.AddSingleton<WebHookService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.ConfigureExceptionHandler();

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.UseSwagger(c =>
            {
                c.SerializeAsV2 = true;
            });

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("v1/swagger.json", "My API V1");
            });
        }
    }
}

﻿using System.Text;
using System.Threading.Tasks;
using Serilog;
using StockCrawler.Crawler;

namespace Cli
{
    class Program
    {
        static async Task Main(string[] args)
        {
            System.Text.EncodingProvider provider = System.Text.CodePagesEncodingProvider.Instance;
            Encoding.RegisterProvider(provider);

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console()
                .CreateLogger();

            Log.Logger.Information("starting up!");

            var naverStockCrawler = new NaverStockCrawler(1);
            await naverStockCrawler.RunAsync();
        }
    }
}
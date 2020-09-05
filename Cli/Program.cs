using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Abot2.Core;
using Abot2.Crawler;
using Abot2.Poco;
using AngleSharp;
using Serilog;
using StockCrawler;

namespace cli
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
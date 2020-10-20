using Serilog;
using StockCrawler.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using WebUtil.Util;

namespace StockCrawler.Crawler
{
    public class NaverStockDailyCrawler : CrawlerBase
    {
        private string Code { get; set; }

        public NaverStockDailyCrawler(int page = 1, string code = "263750" /* 펄 어비스 */) :
            base("https://finance.naver.com/item/sise_day.nhn", page)
        {
            Code = code;
        }

        protected override string UrlComposite(int? page)
        {
            return $"{UrlBase}?page={page.GetValueOrDefault(1)}&code={Code}";
        }

        protected override void OnPageCrawl(AngleSharp.Html.Dom.IHtmlDocument document)
        {
            var thContent = document.QuerySelectorAll("tbody tr th").Select(x => x.TextContent.Trim()).ToArray();
            var tdContent = document.QuerySelectorAll("tbody tr td span").Select(x => x.TextContent.Trim()).Where(x => !string.IsNullOrEmpty(x)).ToArray();
            if (!thContent.Any() || !tdContent.Any())
            {
                Log.Logger.Error($"Not Found Data {Code}");
                return;
            }

            Parallel.For(0, tdContent.Length / thContent.Length, n =>
            {
                var cursor = n * thContent.Length;
                var date = DateTime.Parse(tdContent[cursor + 0]);
                var latest = tdContent[cursor + 1].ToInt();
                var start = tdContent[cursor + 3].ToInt();
                var change = latest - start;
                var high = tdContent[cursor + 4].ToInt();
                var low = tdContent[cursor + 5].ToInt();
                var tradeCount = tdContent[cursor + 6].ToInt();

                _ = OnCrawlData(new NaverStock
                {
                    Date = date,
                    Latest = latest,
                    Change = change,
                    Start = start,
                    Low = low,
                    High = high,
                    TradeCount = tradeCount,
                    Code = Code
                });
            });
        }

        protected virtual async Task OnCrawlData(NaverStock naverStock)
        {
            await Task.Run(() => Log.Logger.Information(naverStock.ToString()));
        }
    }
}

using Serilog;
using Server.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using StockCrawler.Util;
using WebUtil.Util;

namespace StockCrawler
{
    public class NaverStockCrawler : CrawlerBase
    {
        private string Code { get; set; }

        public NaverStockCrawler(int page = 1, string code = "263750" /* 펄 어비스 */) :
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
            var thContent = document.QuerySelectorAll("tbody tr th").Select(x => x.TextContent.Trim()).ToList();
            var tdContent = document.QuerySelectorAll("tbody tr td span").Select(x => x.TextContent.Trim()).ToArray();
            if (!thContent.Any() || !tdContent.Any())
            {
                Log.Logger.Error($"Not Found Data {Code}");
                return;
            }

            Parallel.For(0, tdContent.Length / thContent.Count, n =>
            {
                var cursor = n * thContent.Count;
                var date = DateTime.Parse(tdContent[thContent.IndexOf("날짜")]);
                var latest = tdContent[thContent.IndexOf("종가")].ToInt();
                var start = tdContent[thContent.IndexOf("시가")].ToInt();
                var change = latest - start;
                var high = tdContent[thContent.IndexOf("고가")].ToInt();
                var low = tdContent[thContent.IndexOf("저가")].ToInt();
                var tradeCount = tdContent[thContent.IndexOf("거래량")].ToInt();

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

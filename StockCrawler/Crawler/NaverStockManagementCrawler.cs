using Serilog;
using StockCrawler.Entity;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using EzAspDotNet.Util;

namespace StockCrawler.Crawler
{
    public class NaverStockManagementCrawler : CrawlerBase
    {
        public Action<NaverStockManagement> OnCrawlAction;

        public NaverStockManagementCrawler(Action<NaverStockManagement> onCrawlAction) :
            base("https://finance.naver.com/sise/management.nhn", 1)
        {
            OnCrawlAction = onCrawlAction;
        }

        protected override string UrlComposite(int? page)
        {
            return $"{UrlBase}";
        }

        protected override void OnPageCrawl(AngleSharp.Html.Dom.IHtmlDocument document)
        {
            var thContent = document.QuerySelectorAll("tbody tr th").Select(x => x.TextContent.Trim()).ToArray();
            var tdContent = document.QuerySelectorAll("tbody tr td")
                .Where(x => string.IsNullOrEmpty(x.ClassName) || (!x.ClassName.StartsWith("blank_") && !x.ClassName.StartsWith("division_line")))
                .Select(x => x.TextContent.Trim())
                .ToArray();
            var tdHref = document.QuerySelectorAll("tbody tr td a")
                .Select(x => x.GetAttribute("href"))
                .Where(x => x != "#")
                .ToArray();
            if (!thContent.Any() || !tdContent.Any())
            {
                Log.Logger.Error($"Not Found Data");
                return;
            }

            Parallel.For(0, tdContent.Length / thContent.Length, n =>
            {
                var cursor = n * thContent.Length;

                var name = tdContent[cursor + 1];
                var href = tdHref[n];
                var uri = new Uri(UrlBase.CutAndComposite("/", 0, 3, href));
                string code = HttpUtility.ParseQueryString(uri.Query).Get("code");

                var latest = tdContent[cursor + 2].ToInt();

                var change = tdContent[cursor + 3].ToInt();

                var changeRate = tdContent[cursor + 4].ToDouble();

                var tradeCount = tdContent[cursor + 5].ToInt();

                var date = DateTime.Parse(tdContent[cursor + 6]);
                var reason = tdContent[cursor + 7];

                OnCrawlAction?.Invoke(new NaverStockManagement
                {
                    Date = date,
                    Code = code,
                    Reason = reason,
                    Latest = latest,
                    Change = change,
                    ChangeRate = changeRate,
                    TradeCount = tradeCount
                });
            });
        }
    }
}

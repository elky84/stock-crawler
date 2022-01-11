using Serilog;
using StockCrawler.Entity;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using EzAspDotNet.Util;

namespace StockCrawler.Crawler
{
    public class NaverStockHaltCrawler : CrawlerBase
    {
        public Action<NaverStockHalt> OnCrawlAction;

        public NaverStockHaltCrawler(Action<NaverStockHalt> onCrawlAction) :
            base("https://finance.naver.com/sise/trading_halt.nhn", 1)
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
            var tdHref = document.QuerySelectorAll("tbody tr td a").Select(x => x.GetAttribute("href")).ToArray();
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

                var date = DateTime.Parse(tdContent[cursor + 2]);
                var reason = tdContent[cursor + 3];

                OnCrawlAction?.Invoke(new NaverStockHalt
                {
                    Date = date,
                    Code = code,
                    Reason = reason
                });
            });
        }
    }
}

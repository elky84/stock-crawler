﻿using Abot2.Core;
using Abot2.Crawler;
using Abot2.Poco;
using AngleSharp;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StockCrawler.Crawler
{
    public abstract class CrawlerBase
    {
        protected string UrlBase { get; set; }

        protected int? Page { get; set; }

        protected PoliteWebCrawler CrawlerInstance { get; set; }

        public CrawlerBase(string urlBase, int? page = 1)
        {
            UrlBase = urlBase;
            Page = page;
        }

        public PoliteWebCrawler Create()
        {
            var config = new CrawlConfiguration
            {
                MaxConcurrentThreads = 10,
                MaxPagesToCrawl = 1,
                MaxPagesToCrawlPerDomain = 10,
                MinRetryDelayInMilliseconds = 1000,
                MinCrawlDelayPerDomainMilliSeconds = 1000,
                IsSendingCookiesEnabled = true
            };

            CrawlerInstance = new PoliteWebCrawler(config, null, null, null, new PageRequester(config, new WebContentExtractor()), null, null, null, null);
            CrawlerInstance.PageCrawlStarting += ProcessPageCrawlStarting;
            CrawlerInstance.PageCrawlCompleted += ProcessPageCrawlCompleted;
            CrawlerInstance.PageCrawlDisallowed += PageCrawlDisallowed;
            CrawlerInstance.PageLinksCrawlDisallowed += PageLinksCrawlDisallowed;
            return CrawlerInstance;
        }

        public async Task RunAsync()
        {
            if (CrawlerInstance == null)
            {
                Create();
            }

            for (int n = 0; n < 5; ++n)
            {
                if (await ExecuteAsync(Page))
                {
                    break;
                }

                Thread.Sleep(1);
            }
        }

        private async Task<bool> ExecuteAsync(int? page)
        {
            var builder = new UriBuilder(UrlComposite(page));

            var crawlResult = await CrawlerInstance.CrawlAsync(builder.Uri);

            return !crawlResult.ErrorOccurred;
        }

        protected abstract string UrlComposite(int? page);

        void ProcessPageCrawlStarting(object sender, PageCrawlStartingArgs e)
        {
            PageToCrawl pageToCrawl = e.PageToCrawl;
            Log.Logger.Debug($"About to crawl link {pageToCrawl.Uri.AbsoluteUri} which was found on page {pageToCrawl.ParentUri.AbsoluteUri}");
        }

        void ProcessPageCrawlCompleted(object sender, PageCrawlCompletedArgs e)
        {
            CrawledPage crawledPage = e.CrawledPage;
            if (crawledPage.HttpRequestException != null || crawledPage.HttpResponseMessage.StatusCode != HttpStatusCode.OK)
                Log.Logger.Error($"Crawl of page failed {crawledPage.Uri.AbsoluteUri}");
            else
                Log.Logger.Information($"Crawl of page succeeded {crawledPage.Uri.AbsoluteUri}");

            if (string.IsNullOrEmpty(crawledPage.Content.Text))
                Log.Logger.Debug($"Page had no content {crawledPage.Uri.AbsoluteUri}");

            OnPageCrawl(crawledPage.AngleSharpHtmlDocument);
        }

        protected abstract void OnPageCrawl(AngleSharp.Html.Dom.IHtmlDocument document);

        void PageLinksCrawlDisallowed(object sender, PageLinksCrawlDisallowedArgs e)
        {
            CrawledPage crawledPage = e.CrawledPage;
            Log.Logger.Error($"Did not crawl the links on page {crawledPage.Uri.AbsoluteUri} due to {e.DisallowedReason}");
        }

        void PageCrawlDisallowed(object sender, PageCrawlDisallowedArgs e)
        {
            PageToCrawl pageToCrawl = e.PageToCrawl;
            Log.Logger.Error($"Did not crawl page {pageToCrawl.Uri.AbsoluteUri} due to {e.DisallowedReason}");
        }
    }
}

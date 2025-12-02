using AngleSharp;
using AngleSharp.Dom;
using PromptEnhancer.Models.Examples;
using PromptEnhancer.Search.Interfaces;
using System.Collections.Concurrent;
using System.Text;
using System.Text.RegularExpressions;

namespace PromptEnhancer.Search
{
    internal partial class SearchWebScraper : ISearchWebScraper
    {
        private const int MinHtmlTextPartLength = 50;

        [GeneratedRegex(@"\s{2,}", RegexOptions.Compiled)]
        private static partial Regex NormalizeWhitespaceRegex();

        private static readonly HttpClient _httpClient = new HttpClient();

        public async Task<IEnumerable<UrlRecord>> ScrapeDataFromUrlsAsync(IEnumerable<string> usedUrls, string selectors = "body")
        {
            if (!usedUrls.Any())
            {
                return [];
            }
            var config = Configuration.Default.WithDefaultLoader();
            var context = BrowsingContext.New(config);
            var cb = new ConcurrentBag<UrlRecord>();

            await Parallel.ForEachAsync(usedUrls, async (url, _) =>
            {
                var scrapedContent = await ScrapeUrlContent(context, url, selectors);
                cb.Add(scrapedContent);
            });

            var result = cb.ToList();
            return result;
        }

        private async Task<UrlRecord> ScrapeUrlContent(IBrowsingContext context, string url, string selectors)
        {
            var resp = await _httpClient.GetAsync(url);
            if (!resp.IsSuccessStatusCode)
            {
                return new UrlRecord { Content = "Scraping not successfull", Url = url };
            }

            var html = await resp.Content.ReadAsStringAsync();

            var document = await context.OpenAsync(req => req.Content(html));
            var sb = new StringBuilder();

            //var seen = new HashSet<string>();
            foreach (var node in document.QuerySelectorAll("style, script"))
            {
                node.Remove();
            }
            //"p, li, h1, h2, h3, h4, span, section, [class*='description']"
            var relevantParts = document.QuerySelectorAll(selectors);
            var testtext = document.DocumentElement.Text();
            foreach (var part in relevantParts)
            {
                var text = Normalize(part.TextContent);
                if (text is not null) // && seen.Add(text)
                {
                    sb.AppendLine(text);
                }
            }
            return new UrlRecord { Content = sb.ToString(), Url = url };
        }

        private string? Normalize(string text)
        {
            text = NormalizeWhitespaceRegex().Replace(text.Replace("\r", " ").Replace("\n", " "), " ");
            text = text.Trim();
            return text.Length > MinHtmlTextPartLength ? text : null;
        }
    }
}

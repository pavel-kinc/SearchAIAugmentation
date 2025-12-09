using AngleSharp;
using AngleSharp.Dom;
using PromptEnhancer.Models.Examples;
using PromptEnhancer.Search.Interfaces;
using System.Collections.Concurrent;
using System.Text;
using System.Text.RegularExpressions;

namespace PromptEnhancer.Search
{
    /// <summary>
    /// Provides functionality to scrape and extract data from web pages based on specified HTML selectors.
    /// </summary>
    /// <remarks>This class is designed to process a collection of URLs, retrieve their HTML content, and
    /// extract relevant text based on the provided HTML selectors. It uses parallel processing to improve performance
    /// when handling multiple URLs. The extracted data is returned as a collection of <see cref="UrlRecord"/> objects,
    /// each containing the URL and the corresponding scraped content.</remarks>
    internal partial class SearchWebScraper : ISearchWebScraper
    {
        private const int MinHtmlTextPartLength = 50;

        [GeneratedRegex(@"\s{2,}", RegexOptions.Compiled)]
        private static partial Regex NormalizeWhitespaceRegex();

        private static readonly HttpClient _httpClient = new HttpClient();

        /// <inheritdoc/>
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

        /// <summary>
        /// Scrapes the content of a specified URL and extracts relevant text based on the provided HTML selectors.
        /// </summary>
        /// <remarks>This method removes all <c>style</c> and <c>script</c> elements from the HTML
        /// document before extracting content. The extracted content is normalized and concatenated into a single
        /// string.</remarks>
        /// <param name="context">The browsing context used to parse and manipulate the HTML content.</param>
        /// <param name="url">The URL of the web page to scrape. Must be a valid, accessible URL.</param>
        /// <param name="selectors">A HTML selector string used to identify the relevant parts of the HTML document to extract.</param>
        /// <returns>A <see cref="UrlRecord"/> containing the extracted content and the URL of the web page. If the HTTP request
        /// is unsuccessful, the <see cref="UrlRecord.Content"/> will indicate that scraping was not successful.</returns>
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

        /// <summary>
        /// Normalizes the specified text by replacing excessive whitespace and trimming leading and trailing spaces.
        /// </summary>
        /// <returns>The normalized text with excessive whitespace removed and trimmed, or <see langword="null"/> if the
        /// resulting text length is less than the minimum required length.</returns>
        private string? Normalize(string text)
        {
            text = NormalizeWhitespaceRegex().Replace(text.Replace("\r", " ").Replace("\n", " "), " ");
            text = text.Trim();
            return text.Length > MinHtmlTextPartLength ? text : null;
        }
    }
}

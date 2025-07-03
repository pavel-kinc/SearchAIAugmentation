using AngleSharp;
using AngleSharp.Dom;
using System.Collections.Concurrent;
using System.Text;
using System.Text.RegularExpressions;

namespace PromptEnhancer.Search
{
    internal partial class SearchWebScraper
    {
        [GeneratedRegex(@"\s{2,}", RegexOptions.Compiled)]
        private static partial Regex NormalizeWhitespaceRegex();

        private const int MinHtmlTextPartLength = 50;
        internal static async Task<string> ScrapeDataFromUrlsAsync(IEnumerable<string> usedUrls)
        {
            if (!usedUrls.Any())
            {
                return string.Empty;
            }
            var config = Configuration.Default.WithDefaultLoader();
            var context = BrowsingContext.New(config);
            var cb = new ConcurrentBag<string>();

            await Parallel.ForEachAsync(usedUrls, async (url, _) =>
            {
                var scrapedContent = await ScrapeUrlContent(context, url);
                cb.Add(scrapedContent);
            });
            
            var result = cb.ToList();
            var res = string.Join(Environment.NewLine, result);
            return res;
        }

        private static async Task<string> ScrapeUrlContent(IBrowsingContext context, string url)
        {
            var document = await context.OpenAsync(url);
            var sb = new StringBuilder();

            //var seen = new HashSet<string>();
            foreach (var node in document.QuerySelectorAll("style, script"))
            {
                node.Remove();
            }
            var relevantParts = document.QuerySelectorAll("p, li, h1, h2, h3, h4, span, section, [class*='description']");
            var testtext = document.DocumentElement.Text();
            foreach (var part in relevantParts)
            {
                var text = Normalize(part.TextContent);
                if (text is not null) // && seen.Add(text)
                {
                    sb.AppendLine(text);
                }
            }
            return sb.ToString();
        }

        private static string? Normalize(string text)
        {
            text = NormalizeWhitespaceRegex().Replace(text.Replace("\r", " ").Replace("\n", " "), " ");
            text = text.Trim();
            return text.Length > MinHtmlTextPartLength ? text : null;
        }
    }
}

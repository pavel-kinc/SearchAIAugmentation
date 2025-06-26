using AngleSharp;
using AngleSharp.Dom;
using Microsoft.AspNetCore.Mvc.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

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
            var result = new StringBuilder();
            var config = Configuration.Default.WithDefaultLoader();
            var context = BrowsingContext.New(config);

            foreach (var url in usedUrls)
            {
                // maybe make this per url so that it can be parallelized (and then joined per url - do not send SB)
                await ScrapeUrlContent(result, context, url);
            }

            var res =  result.ToString();
            return res;
        }

        private static async Task ScrapeUrlContent(StringBuilder result, IBrowsingContext context, string url)
        {
            var document = await context.OpenAsync(url);

            var seen = new HashSet<string>();
            foreach (var node in document.QuerySelectorAll("style, script"))
            {
                node.Remove();
            }
            var relevantParts = document.QuerySelectorAll("p, li, h1, h2, h3, h4, span, section");
            var testtext = document.DocumentElement.Text();
            foreach (var part in relevantParts)
            {
                var text = Normalize(part.TextContent);
                if (text is not null && seen.Add(text))
                {
                    result.AppendLine(text);
                }
            }
        }

        private static string? Normalize(string text)
        {
            text = NormalizeWhitespaceRegex().Replace(text.Replace("\r", " ").Replace("\n", " "), " ");
            text = text.Trim();
            return text.Length > MinHtmlTextPartLength ? text : null;
        }
    }
}

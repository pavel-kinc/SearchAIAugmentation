using PromptEnhancer.Models.Examples;

namespace PromptEnhancer.Search.Interfaces
{
    public interface ISearchWebScraper
    {
        public Task<IEnumerable<UrlRecord>> ScrapeDataFromUrlsAsync(IEnumerable<string> usedUrls, string selectors = "body");
    }
}

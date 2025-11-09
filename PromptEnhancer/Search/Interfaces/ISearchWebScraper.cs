using PromptEnhancer.KnowledgeRecord;
using PromptEnhancer.Models;

namespace PromptEnhancer.Search.Interfaces
{
    public interface ISearchWebScraper
    {
        public Task<IEnumerable<UrlData>> ScrapeDataFromUrlsAsync(IEnumerable<string> usedUrls, string selectors = "body");
    }
}

namespace PromptEnhancer.Search.Interfaces
{
    public interface ISearchWebScraper
    {
        public Task<string> ScrapeDataFromUrlsAsync(IEnumerable<string> usedUrls);
    }
}

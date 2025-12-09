using PromptEnhancer.Models.Examples;

namespace PromptEnhancer.Search.Interfaces
{
    /// <summary>
    /// Defines a contract for web scraping operations that extract data from a collection of URLs.
    /// </summary>
    /// <remarks>Implementations of this interface are expected to perform web scraping tasks by navigating to
    /// the specified URLs and extracting data based on the provided HTML selectors. The extracted data is returned as a
    /// collection of  <see cref="UrlRecord"/> objects.</remarks>
    public interface ISearchWebScraper
    {
        /// <summary>
        /// Asynchronously scrapes data from the specified URLs using the provided HTML selectors.
        /// </summary>
        /// <param name="usedUrls">A collection of URLs to scrape data from. Each URL must be a valid, well-formed URI.</param>
        /// <param name="selectors">The HTML selectors used to extract specific elements from the HTML content of each URL. Defaults to "body" if
        /// not specified.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a collection of <see
        /// cref="UrlRecord"/> objects, each representing the data extracted from a URL.</returns>
        public Task<IEnumerable<UrlRecord>> ScrapeDataFromUrlsAsync(IEnumerable<string> usedUrls, string selectors = "body");
    }
}

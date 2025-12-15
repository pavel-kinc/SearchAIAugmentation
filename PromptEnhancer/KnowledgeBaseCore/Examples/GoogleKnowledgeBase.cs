using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Data;
using PromptEnhancer.KnowledgeRecord;
using PromptEnhancer.KnowledgeSearchRequest.Examples;
using PromptEnhancer.KnowledgeSearchRequest.Interfaces;
using PromptEnhancer.Models.Examples;
using PromptEnhancer.Search;
using PromptEnhancer.Search.Interfaces;
using PromptEnhancer.Services.ChunkService;
using System.Collections.Concurrent;

namespace PromptEnhancer.KnowledgeBaseCore.Examples
{

    /// <summary>
    /// Represents a knowledge base that utilizes Google Search to retrieve up-to-date data. This class can be used for
    /// various queries that require data from web searches.
    /// </summary>
    /// <remarks>The <see cref="GoogleKnowledgeBase"/> class extends the functionality of <see
    /// cref="KnowledgeBaseUrl{TRecord, TFilterModel, TSettings, TFilter, TUrlRecord}"/> by integrating with Google
    /// Search through a search provider manager. It supports asynchronous search operations and can handle multiple
    /// queries concurrently.</remarks>
    public class GoogleKnowledgeBase : KnowledgeBaseUrl<KnowledgeUrlRecord, GoogleSearchFilterModel, GoogleSettings, UrlRecordFilter, UrlRecord>
    {
        private readonly ISearchProviderManager _searchProviderManager;
        private readonly ILogger<GoogleKnowledgeBase> _logger;

        public GoogleKnowledgeBase(ISearchWebScraper searchWebScraper, IChunkGeneratorService chunkGenerator, ISearchProviderManager searchProviderManager, ILogger<GoogleKnowledgeBase> logger)
            : base(searchWebScraper, chunkGenerator)
        {
            _searchProviderManager = searchProviderManager;
            _logger = logger;
        }

        /// <inheritdoc/>
        public override string Description => $"This knowledge base uses Google Search to retrieve up-to-date data. Can be used for various queries that could use data from web search.";

        /// <inheritdoc/>
        public async override Task<IEnumerable<KnowledgeUrlRecord>> SearchAsync(
            IKnowledgeSearchRequest<GoogleSearchFilterModel, GoogleSettings> request, IEnumerable<string> queriesToSearch, UrlRecordFilter? filter = null, CancellationToken ct = default)
        {
            if (!queriesToSearch.Any())
            {
                _logger.LogWarning("GoogleKnowledgeBase received no queries to search.");
                return [];
            }
            var settings = request.Settings;
            var textSearch = _searchProviderManager.CreateTextSearch(new SearchProviderSettings
            {
                SearchApiKey = settings.SearchApiKey,
                Engine = settings.Engine,
                Provider = Models.Enums.SearchProviderEnum.Google
            });

            TextSearchOptions? options = null;

            if (request.Filter is not null)
            {
                options = request.Filter.BuildParameters();
            }
            if (textSearch is null)
            {
                _logger.LogError("GoogleKnowledgeBase could not create a text search instance.");
                return [];
            }
            _logger.LogInformation("GoogleKnowledgeBase starting search for {QueryCount} queries.", queriesToSearch.Count());
            var cb = new ConcurrentBag<KnowledgeUrlRecord>();
            await Parallel.ForEachAsync(queriesToSearch, async (queryString, _) =>
            {
                IEnumerable<UrlRecord> data = await GetDataFromGoogle(queryString, textSearch, options, settings);
                var results = GetKnowledgeRecords(data, filter, queryString, settings.AllowChunking, settings.ChunkTokenSize ?? KnowledgeUrlRecord.DefaultChunkTokenSize, settings.ChunkLimitPerUrl, ct);
                foreach (var record in results)
                {
                    cb.Add(record);
                }
            });
            _logger.LogInformation("GoogleKnowledgeBase returned {Count} records for {QueryCount} queries.", cb.Count, queriesToSearch.Count());
            return [.. cb];
        }

        /// <summary>
        /// Retrieves data from Google based on the specified query string and search options.
        /// </summary>
        /// <remarks>If <paramref name="settings"/> specifies to use a web scraper, the method scrapes
        /// data from the URLs obtained from the search results. Otherwise, it constructs <see cref="UrlRecord"/>
        /// objects using the search result snippets.</remarks>
        /// <param name="queryString">The search query string used to retrieve data from Google. Cannot be null or empty.</param>
        /// <param name="textSearch">The text search service used to perform the search. Cannot be null.</param>
        /// <param name="options">Optional search options that can modify the search behavior. Can be null.</param>
        /// <param name="settings">The settings that configure the search, including the number of top results to retrieve and whether to use a
        /// web scraper.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains an enumerable collection of <see
        /// cref="UrlRecord"/> objects representing the search results.</returns>
        protected async Task<IEnumerable<UrlRecord>> GetDataFromGoogle(string queryString, ITextSearch textSearch, TextSearchOptions? options, GoogleSettings settings)
        {
            var res = await _searchProviderManager.GetSearchResults(textSearch!, queryString!, settings.TopN, options);

            var searchResults = await res.Results.ToListAsync();
            var usedUrls = searchResults.Where(x => !string.IsNullOrEmpty(x.Link)).Select(x => x.Link!);
            IEnumerable<UrlRecord>? data = null;
            if (settings.UseScraper)
            {
                //will be needed some specifications from config what to search for maybe? (selector)
                _logger.LogInformation("GoogleKnowledgeBase is scraping data from {UrlCount} URLs for query: {Query}", usedUrls.Count(), queryString);
                data = await _searchWebScraper.ScrapeDataFromUrlsAsync(usedUrls);
            }
            else
            {
                //this uses snippets from search only
                data = searchResults.Where(x => !string.IsNullOrEmpty(x.Link)).Select(x => new UrlRecord
                {
                    Url = x.Link!,
                    Content = ((x.Name ?? string.Empty) + " " + x.Value).Trim()
                });
            }

            return data;
        }
    }
}

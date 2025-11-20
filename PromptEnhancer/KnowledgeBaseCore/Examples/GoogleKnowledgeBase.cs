using Microsoft.SemanticKernel.Data;
using PromptEnhancer.ChunkService;
using PromptEnhancer.KnowledgeRecord;
using PromptEnhancer.KnowledgeSearchRequest.Examples;
using PromptEnhancer.KnowledgeSearchRequest.Interfaces;
using PromptEnhancer.Models.Examples;
using PromptEnhancer.Search;
using PromptEnhancer.Search.Interfaces;
using System.Collections.Concurrent;

namespace PromptEnhancer.KnowledgeBaseCore.Examples
{
    public class GoogleKnowledgeBase : KnowledgeBaseUrl<KnowledgeUrlRecord, GoogleSearchFilterModel, GoogleSettings, UrlRecordFilter, UrlRecord>
    {
        private readonly ISearchProviderManager _searchProviderManager;
        public GoogleKnowledgeBase(ISearchWebScraper searchWebScraper, IChunkGeneratorService chunkGenerator, ISearchProviderManager searchProviderManager)
            : base(searchWebScraper, chunkGenerator)
        {
            _searchProviderManager = searchProviderManager;
        }

        public override string Description => $"This knowledge base uses Google Search to retrieve up-to-date data. Can be used for various queries that could use data from web search.";

        public async override Task<IEnumerable<KnowledgeUrlRecord>> SearchAsync(
            IKnowledgeSearchRequest<GoogleSearchFilterModel, GoogleSettings> request, IEnumerable<string> queriesToSearch, UrlRecordFilter? filter = null, CancellationToken ct = default)
        {
            //TODO more queires
            if (!queriesToSearch.Any())
            {
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
                //TODO maybe exception?
                return [];
            }

            var cb = new ConcurrentBag<KnowledgeUrlRecord>();
            await Parallel.ForEachAsync(queriesToSearch, async (queryString, _) =>
            {
                IEnumerable<UrlRecord> data = await GetDataFromGoogle(queryString, textSearch, options, settings);

                var results = GetKnowledgeRecords(data, filter, queryString, settings.AllowChunking, KnowledgeUrlRecord.ChunkSelector, KnowledgeUrlRecord.AssignChunkToProperty, settings.ChunkSize ?? KnowledgeUrlRecord.DefaultChunkSize, settings.ChunkLimitPerUrl, ct);
                foreach (var record in results)
                {
                    cb.Add(record);
                }
            });

            return [.. cb];
        }

        private async Task<IEnumerable<UrlRecord>> GetDataFromGoogle(string queryString, ITextSearch textSearch, TextSearchOptions? options, GoogleSettings settings)
        {
            var res = await _searchProviderManager.GetSearchResults(textSearch!, queryString!, settings.TopN, options);

            var searchResults = await res.Results.ToListAsync();
            var usedUrls = searchResults.Where(x => !string.IsNullOrEmpty(x.Link)).Select(x => x.Link!);
            IEnumerable<UrlRecord>? data = null;
            if (settings.UseScraper)
            {
                //will be needed some specifications from config what to search for maybe? (selector)
                data = await _searchWebScraper.ScrapeDataFromUrlsAsync(usedUrls);
            }
            else
            {
                //this uses snippets from search only
                data = searchResults.Where(x => !string.IsNullOrEmpty(x.Link)).Select(x => new UrlRecord
                {
                    Url = x.Link!,
                    Content = x.Value
                });
            }

            return data;
        }
    }
}

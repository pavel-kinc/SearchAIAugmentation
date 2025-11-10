using HtmlAgilityPack;
using Microsoft.SemanticKernel.Data;
using PromptEnhancer.ChunkUtilities.Interfaces;
using PromptEnhancer.KnowledgeRecord;
using PromptEnhancer.KnowledgeSearchRequest.Examples;
using PromptEnhancer.KnowledgeSearchRequest.Interfaces;
using PromptEnhancer.Models;
using PromptEnhancer.Models.Examples;
using PromptEnhancer.Models.Pipeline;
using PromptEnhancer.Search.Interfaces;

namespace PromptEnhancer.KnowledgeBase.Examples
{
    public class GoogleKnowledgeBase : KnowledgeBaseUrlCore<KnowledgeUrlRecord, GoogleSearchFilterModel, GoogleSettings, UrlRecordFilter, UrlRecord>
    {
        private readonly ISearchProviderManager _searchProviderManager;
        public GoogleKnowledgeBase(ISearchWebScraper searchWebScraper, IChunkGeneratorService chunkGenerator, ISearchProviderManager searchProviderManager) 
            : base(searchWebScraper, chunkGenerator)
        {
            _searchProviderManager = searchProviderManager;
        }

        public async override Task<IEnumerable<KnowledgeUrlRecord>> SearchAsync(
            IKnowledgeSearchRequest<GoogleSearchFilterModel, GoogleSettings> request, IEnumerable<string> queryToSearch, UrlRecordFilter? filter = null, CancellationToken ct = default)
        {
            //TODO more queires
            if (!queryToSearch.Any())
            {
                return [];
            }
            var settings = request.Settings;
            IEnumerable<UrlRecord> data = await GetDataFromGoogle(queryToSearch.First(), request);

            var results = GetKnowledgeRecords(data, filter, settings.AllowChunking, KnowledgeUrlRecord.ChunkSelector, KnowledgeUrlRecord.AssignChunkToProperty, settings.ChunkSize ?? KnowledgeUrlRecord.DefaultChunkSize, settings.ChunkLimitPerUrl, ct);
            return results;
        }

        private async Task<IEnumerable<UrlRecord>> GetDataFromGoogle(string queryString, IKnowledgeSearchRequest<GoogleSearchFilterModel, GoogleSettings> request)
        {
            var settings = request.Settings;
            var textSearch = _searchProviderManager.CreateTextSearch(new SearchProviderData
            {
                SearchApiKey = settings.SearchApiKey,
                Engine = settings.Engine,
                Provider = Models.Enums.SearchProviderEnum.Google
            });

            TextSearchOptions? options = null;

            if(request.Filter is not null)
            {
                options = request.Filter.BuildParameters();
            }

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

using PromptEnhancer.ChunkUtilities.Interfaces;
using PromptEnhancer.KnowledgeBase.Interfaces;
using PromptEnhancer.KnowledgeRecord;
using PromptEnhancer.KnowledgeSearchRequest.Examples;
using PromptEnhancer.KnowledgeSearchRequest.Interfaces;
using PromptEnhancer.Models;
using PromptEnhancer.Models.Pipeline;
using PromptEnhancer.Search.Interfaces;

namespace PromptEnhancer.KnowledgeBase.Examples
{
    public class GoogleKnowledgeBase : KnowledgeBaseUrlCore, IKnowledgeBase<KnowledgeRecord<UrlRecord>, GoogleSearchFilterModel, GoogleSettings, UrlRecordFilter>
    {
        private readonly ISearchProviderManager _searchProviderManager;
        public GoogleKnowledgeBase(ISearchWebScraper searchWebScraper, IChunkGeneratorService chunkGenerator, ISearchProviderManager searchProviderManager) 
            : base(searchWebScraper, chunkGenerator)
        {
            _searchProviderManager = searchProviderManager;
        }

        public async virtual Task<IEnumerable<KnowledgeRecord<UrlRecord>>> SearchAsync(
            IKnowledgeSearchRequest<GoogleSearchFilterModel, GoogleSettings> request, PipelineContext context, UrlRecordFilter? filter = null, CancellationToken ct = default)
        {
            var settings = request.Settings;
            var textSearch = _searchProviderManager.CreateTextSearch(new SearchProviderData
            {
                SearchApiKey = settings.SearchApiKey,
                Engine = settings.Engine,
                Provider = Models.Enums.SearchProviderEnum.Google
            });

            var res = await _searchProviderManager.GetSearchResults(textSearch!, context.QueryString!, settings.TopN);

            var searchResults = await res.Results.ToListAsync();
            var usedUrls = searchResults.Where(x => !string.IsNullOrEmpty(x.Link)).Select(x => x.Link!);
            IEnumerable<UrlData>? data = null;
            if (settings.UseScraper)
            {
                //will be needed some specifications from config what to search for maybe? (selector)
                data = await _searchWebScraper.ScrapeDataFromUrlsAsync(usedUrls);
            }
            else
            {
                //this uses snippets from search only
                data = searchResults.Where(x => !string.IsNullOrEmpty(x.Link)).Select(x => new UrlData{
                    Url = x.Link!,
                    Content = x.Value
                });
            }

            if (filter != null)
            {
                data = Filter(data, filter);
            }

            var results = data.Select(x => new KnowledgeUrlRecord
            {
                Id = Guid.NewGuid().ToString(),
                SourceObject = new UrlRecord
                {
                    Url = x.Url,
                    Content = x.Content
                },
                Source = GetType().Name,
            });

            return results;
        }

        private IEnumerable<UrlData> Filter(IEnumerable<UrlData> records, UrlRecordFilter filter)
        {
            return [.. records.Where(x => x.Url.Contains(filter.UrlMustContain))];
        }
    }
}

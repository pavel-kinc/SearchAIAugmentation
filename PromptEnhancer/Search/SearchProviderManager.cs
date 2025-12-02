using Microsoft.SemanticKernel.Data;
using Microsoft.SemanticKernel.Plugins.Web.Google;
using PromptEnhancer.Models.Enums;
using PromptEnhancer.Search.Interfaces;

namespace PromptEnhancer.Search
{
#pragma warning disable SKEXP0050
    //TODO what to do with this? maybe just rename it to google?
    public class SearchProviderManager : ISearchProviderManager
    {
        public virtual ITextSearch? CreateTextSearch(SearchProviderSettings searchProviderData)
        {
            if (searchProviderData.Provider == SearchProviderEnum.Google)
            {
                var textSearch = new GoogleTextSearch(
                    searchEngineId: searchProviderData.Engine!,
                    apiKey: searchProviderData.SearchApiKey!);
                return textSearch;
            }
            return null;
        }

        public virtual async Task<KernelSearchResults<TextSearchResult>> GetSearchResults(ITextSearch textSearch, string query, int topSearchCount = 3, TextSearchOptions? options = null)
        {
            return await textSearch.GetTextSearchResultsAsync(query, options ?? new() { Top = topSearchCount });
        }
    }
}

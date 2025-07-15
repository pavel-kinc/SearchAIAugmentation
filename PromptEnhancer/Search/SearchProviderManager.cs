using Microsoft.SemanticKernel.Data;
using Microsoft.SemanticKernel.Plugins.Web.Google;

using PromptEnhancer.Models;
using PromptEnhancer.Models.Enums;
using PromptEnhancer.Search.Interfaces;

namespace PromptEnhancer.Search
{
    public class SearchProviderManager : ISearchProviderManager
    {
        public ITextSearch? CreateTextSearch(SearchProviderData searchProviderData)
        {
            if (searchProviderData.Provider == SearchProviderEnum.Google)
            {
#pragma warning disable SKEXP0050 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
                var textSearch = new GoogleTextSearch(
                    searchEngineId: searchProviderData.Engine!,
                    apiKey: searchProviderData.SearchApiKey!);
#pragma warning restore SKEXP0050 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
                return textSearch;
            }
            return null;
        }

        public async Task<KernelSearchResults<TextSearchResult>> GetSearchResults(ITextSearch textSearch, string query, int topSearchCount = 3)
        {
            return await textSearch.GetTextSearchResultsAsync(query, new() { Top = topSearchCount });
        }
    }
}

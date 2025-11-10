using Microsoft.SemanticKernel.Data;
using Microsoft.SemanticKernel.Plugins.Web.Google;

using PromptEnhancer.Models;
using PromptEnhancer.Models.Enums;
using PromptEnhancer.Search.Interfaces;

namespace PromptEnhancer.Search
{
    public class SearchProviderManager : ISearchProviderManager
    {
        public virtual ITextSearch? CreateTextSearch(SearchProviderData searchProviderData)
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

#pragma warning disable SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
        public virtual async Task<KernelSearchResults<TextSearchResult>> GetSearchResults(ITextSearch textSearch, string query, int topSearchCount = 3, TextSearchOptions? options = null)
#pragma warning restore SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
        {
            return await textSearch.GetTextSearchResultsAsync(query, options ?? new() { Top = topSearchCount });
        }
    }
}

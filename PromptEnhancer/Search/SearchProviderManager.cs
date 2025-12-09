using Microsoft.SemanticKernel.Data;
using Microsoft.SemanticKernel.Plugins.Web.Google;
using PromptEnhancer.Models.Enums;
using PromptEnhancer.Search.Interfaces;

namespace PromptEnhancer.Search
{
#pragma warning disable SKEXP0050
    /// <summary>
    /// Manages the creation and execution of text search operations using various search providers.
    /// </summary>
    /// <remarks>This class provides methods to create text search instances based on the specified search
    /// provider and to retrieve search results using the created text search instance. It supports extensibility by
    /// allowing different search providers to be used.</remarks>
    public class SearchProviderManager : ISearchProviderManager
    {
        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public virtual async Task<KernelSearchResults<TextSearchResult>> GetSearchResults(ITextSearch textSearch, string query, int topSearchCount = 3, TextSearchOptions? options = null)
        {
            return await textSearch.GetTextSearchResultsAsync(query, options ?? new() { Top = topSearchCount });
        }
    }
}

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Data;
using PromptEnhancer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SemanticKernel.Plugins.Web.Google;

namespace PromptEnhancer.Search
{
    public static class SearchProviderManager
    {
        public static ITextSearch? CreateTextSearch(SearchProviderData searchProviderData)
        {
            if (searchProviderData.Provider == SearchProviderEnum.Google)
            {
#pragma warning disable SKEXP0050 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
                var textSearch = new GoogleTextSearch(
                    searchEngineId: searchProviderData.Engine!,
                    apiKey: searchProviderData.SearchApiKey);
#pragma warning restore SKEXP0050 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
                return textSearch;
            }
            return null;
        }

        public async static Task<KernelSearchResults<TextSearchResult>> GetSearchResults(ITextSearch textSearch, string query, int topSearchCount = 3)
        {
            return await textSearch.GetTextSearchResultsAsync(query, new() { Top = topSearchCount });
        }
    }
}

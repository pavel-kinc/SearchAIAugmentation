using Microsoft.SemanticKernel.Data;
using PromptEnhancer.Models;

namespace PromptEnhancer.Search.Interfaces
{
    public interface ISearchProviderManager
    {
        public ITextSearch? CreateTextSearch(SearchProviderData searchProviderData);
        public Task<KernelSearchResults<TextSearchResult>> GetSearchResults(ITextSearch textSearch, string query, int topSearchCount = 3);
    }
}

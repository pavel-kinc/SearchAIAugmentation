using Microsoft.SemanticKernel.Data;

namespace PromptEnhancer.Search.Interfaces
{
    public interface ISearchProviderManager
    {
        public ITextSearch? CreateTextSearch(SearchProviderSettings searchProviderData);
        public Task<KernelSearchResults<TextSearchResult>> GetSearchResults(ITextSearch textSearch, string query, int topSearchCount = 3, TextSearchOptions? options = null);
    }
}

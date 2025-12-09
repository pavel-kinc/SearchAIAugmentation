using Microsoft.SemanticKernel.Data;

namespace PromptEnhancer.Search.Interfaces
{
    /// <summary>
    /// Provides functionality to create and manage text search operations using configurable search providers.
    /// </summary>
    /// <remarks>This interface defines methods for creating text search instances and retrieving search
    /// results  based on a specified query. Implementations of this interface are responsible for managing the 
    /// lifecycle and behavior of the underlying search providers.</remarks>
    public interface ISearchProviderManager
    {
        /// <summary>
        /// Creates an instance of a text search provider based on the specified settings.
        /// </summary>
        /// <param name="searchProviderData">The settings used to configure the text search provider. Cannot be null.</param>
        /// <returns>An instance of <see cref="ITextSearch"/> configured with the specified settings,  or <see langword="null"/>
        /// if the creation fails due to invalid settings.</returns>
        public ITextSearch? CreateTextSearch(SearchProviderSettings searchProviderData);

        /// <summary>
        /// Executes a text search query and retrieves the top matching results.
        /// </summary>
        /// <remarks>This method performs a search using the specified text search engine and query,
        /// returning  the most relevant results based on the provided criteria. The results are limited to the 
        /// specified <paramref name="topSearchCount"/>.</remarks>
        /// <param name="textSearch">The text search engine to use for executing the query.</param>
        /// <param name="query">The search query string to evaluate.</param>
        /// <param name="topSearchCount">The maximum number of top results to return. The default value is 3.</param>
        /// <param name="options">Optional search configuration settings. If null, default options are used.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a  <see
        /// cref="KernelSearchResults{TextSearchResult}"/> object with the top matching results.</returns>
        public Task<KernelSearchResults<TextSearchResult>> GetSearchResults(ITextSearch textSearch, string query, int topSearchCount = 3, TextSearchOptions? options = null);
    }
}

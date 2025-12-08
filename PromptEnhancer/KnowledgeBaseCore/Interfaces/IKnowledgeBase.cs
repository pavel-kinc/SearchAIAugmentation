using PromptEnhancer.KnowledgeRecord.Interfaces;
using PromptEnhancer.KnowledgeSearchRequest.Interfaces;

namespace PromptEnhancer.KnowledgeBaseCore.Interfaces
{
    /// <summary>
    /// Represents a knowledge base that supports searching for records based on specified queries, filters, and
    /// settings.
    /// </summary>
    /// <remarks>This interface extends <see cref="IKnowledgeBaseCore"/> and provides functionality for
    /// performing asynchronous searches within a knowledge base. The search operation can be customized using search
    /// filters, settings, and optional model filters.</remarks>
    /// <typeparam name="TRecord">The type of the knowledge record to be retrieved in the knowledge base. Wrapper for TModel. Must implement <see cref="IKnowledgeRecord"/>.</typeparam>
    /// <typeparam name="SearchFilter">The type of the search filter used to refine search results. Must implement <see
    /// cref="IKnowledgeBaseSearchFilter"/>.</typeparam>
    /// <typeparam name="SearchSettings">The type of the search settings used to configure search behavior. Must implement <see
    /// cref="IKnowledgeBaseSearchSettings"/>.</typeparam>
    /// <typeparam name="TFilter">The type of the model filter applied to the search results. Must implement <see cref="IModelFilter{TModel}"/>.</typeparam>
    /// <typeparam name="TModel">The type of the model associated with the retrieved data. Must be a reference type.</typeparam>
    public interface IKnowledgeBase<TRecord, SearchFilter, SearchSettings, TFilter, TModel> : IKnowledgeBaseCore
        where TRecord : class, IKnowledgeRecord
        where SearchFilter : class, IKnowledgeBaseSearchFilter
        where SearchSettings : class, IKnowledgeBaseSearchSettings
        where TFilter : class, IModelFilter<TModel>
        where TModel : class

    {
        /// <summary>
        /// Asynchronously searches for records based on the specified search request, queries, and optional filter.
        /// </summary>
        /// <remarks>The search operation is configured using the provided <paramref name="request"/> and
        /// applies the specified <paramref name="queriesToSearch"/>. If a <paramref name="filter"/> is provided, it is
        /// used to further narrow the results. The operation respects the cancellation token <paramref name="ct"/> to
        /// allow for graceful cancellation.</remarks>
        /// <param name="request">The search request containing the filter and settings to configure the search operation.</param>
        /// <param name="queriesToSearch">A collection of query strings to be used in the search operation. Each query is applied to the search
        /// request.</param>
        /// <param name="filter">An optional filter to further refine the search results. If <see langword="null"/>, no additional filtering
        /// is applied.</param>
        /// <param name="ct">A cancellation token that can be used to cancel the search operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains an enumerable collection of
        /// records that match the search criteria.</returns>
        Task<IEnumerable<TRecord>> SearchAsync(IKnowledgeSearchRequest<SearchFilter, SearchSettings> request, IEnumerable<string> queriesToSearch, TFilter? filter = null, CancellationToken ct = default);
    }
}

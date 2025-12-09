using PromptEnhancer.KnowledgeBaseCore.Interfaces;
using PromptEnhancer.KnowledgeRecord.Interfaces;
using PromptEnhancer.KnowledgeSearchRequest.Interfaces;

namespace PromptEnhancer.KnowledgeBaseCore
{
    /// <summary>
    /// Contains knowledge base along with search request and optional filter to perform searches on the knowledge base. <br/>
    /// Represents a container for managing and interacting with a knowledge base, including performing searches and
    /// applying filters to the search results.
    /// </summary>
    /// <remarks>This class provides a high-level abstraction for interacting with a knowledge base, including
    /// the ability to perform asynchronous searches with specified queries and optional filters. It encapsulates the
    /// underlying knowledge base and search request objects, simplifying their usage.</remarks>
    /// <typeparam name="TRecord">The type of the knowledge records stored in the knowledge base. Must implement <see cref="IKnowledgeRecord"/>.</typeparam>
    /// <typeparam name="TSearchFilter">The type of the search filter used to refine search queries. Must implement <see
    /// cref="IKnowledgeBaseSearchFilter"/>.</typeparam>
    /// <typeparam name="TSearchSettings">The type of the search settings used to configure search behavior. Must implement <see
    /// cref="IKnowledgeBaseSearchSettings"/>.</typeparam>
    /// <typeparam name="TFilter">The type of the model filter applied to the search results. Must implement <see cref="IModelFilter{T}"/>.</typeparam>
    /// <typeparam name="T">The type of the model being filtered. Must be a reference type.</typeparam>
    public class KnowledgeBaseContainer<TRecord, TSearchFilter, TSearchSettings, TFilter, T> : IKnowledgeBaseContainer
        where TRecord : class, IKnowledgeRecord
        where TSearchFilter : class, IKnowledgeBaseSearchFilter
        where TSearchSettings : class, IKnowledgeBaseSearchSettings
        where TFilter : class, IModelFilter<T>
        where T : class
    {
        private readonly IKnowledgeBase<TRecord, TSearchFilter, TSearchSettings, TFilter, T> _knowledgeBase;
        private readonly IKnowledgeSearchRequest<TSearchFilter, TSearchSettings> _knowledgeSearchRequest;
        private readonly TFilter? _filter;

        public KnowledgeBaseContainer(IKnowledgeBase<TRecord, TSearchFilter, TSearchSettings, TFilter, T> knowledgeBase,
            IKnowledgeSearchRequest<TSearchFilter, TSearchSettings> knowledgeSearchRequest,
            TFilter? filter)
        {
            _knowledgeBase = knowledgeBase;
            _knowledgeSearchRequest = knowledgeSearchRequest;
            _filter = filter;
        }

        /// <summary>
        /// Gets the description of the knowledge base to decide, if it is relevant to given user query (in automatic picking).
        /// </summary>
        public virtual string Description => _knowledgeBase.Description;

        /// <inheritdoc/>
        public async virtual Task<IEnumerable<IKnowledgeRecord>> SearchAsync(IEnumerable<string> queriesToSearch, CancellationToken ct = default)
        {
            return await _knowledgeBase.SearchAsync(_knowledgeSearchRequest, queriesToSearch, _filter, ct);
        }
    }
}

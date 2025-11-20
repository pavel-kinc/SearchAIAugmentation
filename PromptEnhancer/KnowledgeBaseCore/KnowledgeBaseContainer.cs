using PromptEnhancer.KnowledgeBaseCore.Interfaces;
using PromptEnhancer.KnowledgeRecord.Interfaces;
using PromptEnhancer.KnowledgeSearchRequest.Interfaces;

namespace PromptEnhancer.KnowledgeBaseCore
{
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

        public virtual string Description => _knowledgeBase.Description;

        public async virtual Task<IEnumerable<IKnowledgeRecord>> SearchAsync(IEnumerable<string> queriesToSearch, CancellationToken ct = default)
        {
            return await _knowledgeBase.SearchAsync(_knowledgeSearchRequest, queriesToSearch, _filter, ct);
        }
    }
}

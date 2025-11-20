using PromptEnhancer.KnowledgeRecord.Interfaces;
using PromptEnhancer.KnowledgeSearchRequest.Interfaces;

namespace PromptEnhancer.KnowledgeBaseCore.Interfaces
{
    public interface IKnowledgeBase<TRecord, SearchFilter, SearchSettings, TFilter, TModel> : IKnowledgeBaseCore
        where TRecord : class, IKnowledgeRecord
        where SearchFilter : class, IKnowledgeBaseSearchFilter
        where SearchSettings : class, IKnowledgeBaseSearchSettings
        where TFilter : class, IModelFilter<TModel>
        where TModel : class

    {
        Task<IEnumerable<TRecord>> SearchAsync(IKnowledgeSearchRequest<SearchFilter, SearchSettings> request, IEnumerable<string> queriesToSearch, TFilter? filter = null, CancellationToken ct = default);
    }
}

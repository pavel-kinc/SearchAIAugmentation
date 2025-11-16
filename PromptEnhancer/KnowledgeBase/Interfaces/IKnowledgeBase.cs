using PromptEnhancer.KnowledgeRecord.Interfaces;
using PromptEnhancer.KnowledgeSearchRequest.Interfaces;

namespace PromptEnhancer.KnowledgeBase.Interfaces
{
    public interface IKnowledgeBase<T, SearchFilter, SearchSettings, TFilter, TModel> : IKnowledgeBaseCore
        where T : class, IKnowledgeRecord
        where SearchFilter : class, IKnowledgeBaseSearchFilter
        where SearchSettings : class, IKnowledgeBaseSearchSettings
        where TFilter : class, IRecordFilter<TModel>
        where TModel : class

    {
        Task<IEnumerable<T>> SearchAsync(IKnowledgeSearchRequest<SearchFilter, SearchSettings> request, IEnumerable<string> queriesToSearch, TFilter? filter = null, CancellationToken ct = default);
    }
}

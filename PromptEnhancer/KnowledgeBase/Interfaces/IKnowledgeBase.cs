using PromptEnhancer.KnowledgeRecord.Interfaces;
using PromptEnhancer.KnowledgeSearchRequest.Interfaces;
using PromptEnhancer.Models.Pipeline;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PromptEnhancer.KnowledgeBase.Interfaces
{
    public interface IKnowledgeBase<T, SearchFilter, SearchSettings, TFilter, TModel>
        where T : class, IKnowledgeRecord
        where SearchFilter : class, IKnowledgeBaseSearchFilter
        where SearchSettings : class, IKnowledgeBaseSearchSettings
        where TFilter : class, IRecordFilter<TModel>
        where TModel : class

    {
        Task<IEnumerable<T>> SearchAsync(IKnowledgeSearchRequest<SearchFilter, SearchSettings> request, IEnumerable<string> queryToSearch, TFilter? filter = null, CancellationToken ct = default);
    }
}

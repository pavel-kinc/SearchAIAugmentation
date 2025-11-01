using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PromptEnhancer.KnowledgeBase.Interfaces
{
    public interface IKnowledgeBase
    {
        Task<IEnumerable<KnowledgeRecord>> SearchAsync(KnowledgeSearchRequest request, CancellationToken ct = default);
    }
}

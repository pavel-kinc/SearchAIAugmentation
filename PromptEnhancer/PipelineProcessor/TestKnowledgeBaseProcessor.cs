using PromptEnhancer.KnowledgeBase;
using PromptEnhancer.KnowledgeBase.Interfaces;
using PromptEnhancer.Models.Pipeline;
using PromptEnhancer.Pipeline.Interfaces;
using PromptEnhancer.PipelineProcessor.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PromptEnhancer.PipelineProcessor
{
    public class TestKnowledgeBaseProcessor : IKnowledgeBaseProcessor
    {
        public async Task<IEnumerable<IKnowledgeRecord>> SearchAsync(KnowledgeSearchRequest request, PipelineContext context, CancellationToken ct = default)
        {
            context.QueryString += " [Queried TestKnowledgeBasePlugin]";
            var records = new List<KnowledgeRecord>
            {
                new KnowledgeRecord
                {
                    Id = "1",
                    Content = "1 + 1 = 3"
                }
            };
            return records;
        }
    }
}

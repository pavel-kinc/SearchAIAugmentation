using PromptEnhancer.KnowledgeRecord.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PromptEnhancer.Models
{
    public class PipelineEmbeddingsModel
    {
        public string EmbeddingSource { get; set; } = "unknown";
        public string EmbeddingModel { get; set; } = "unknown";
        public required IKnowledgeRecord AssociatedRecord { get; set; }
        // this is float[] for compatibility with most embedding models, otherwise could be Embedding type ReadOnlyMemory
        public ReadOnlyMemory<float>? EmbeddingVector { get; set; }
        public string UsedQuery => AssociatedRecord.UsedSearchQuery;
    }
}

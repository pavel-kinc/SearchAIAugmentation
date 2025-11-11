using PromptEnhancer.KnowledgeRecord.Interfaces;

namespace PromptEnhancer.Models
{
    public class PipelineEmbeddingsModel
    {
        public string EmbeddingSource { get; init; } = "unknown";
        public string EmbeddingModel { get; init; } = "unknown";
        // this is float[] for compatibility with most embedding models, otherwise could be Embedding type ReadOnlyMemory
        public ReadOnlyMemory<float> EmbeddingVector { get; init; }
    }
}

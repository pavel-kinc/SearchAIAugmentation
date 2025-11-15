namespace PromptEnhancer.Models
{
    public class PipelineEmbeddingsModel
    {
        public string EmbeddingSource { get; init; } = "unknown";
        public string EmbeddingModel { get; init; } = "unknown";
        // this is float[] for compatibility with most embedding models, otherwise could be Embedding type
        public ReadOnlyMemory<float> EmbeddingVector { get; init; }
        public int VectorDimension { get; init; }
    }
}

namespace PromptEnhancer.Models
{
    /// <summary>
    /// Represents a model containing information about an embedding, including its source, model, and vector
    /// representation.
    /// </summary>
    /// <remarks>This class is designed to store metadata and the vector representation of an embedding, which
    /// can be used in machine learning or natural language processing tasks.</remarks>
    public class PipelineEmbeddingsModel
    {
        public string? EmbeddingSource { get; init; }
        public string? EmbeddingModel { get; init; }
        // this is float[] for compatibility with most embedding models, otherwise could be Embedding type
        public ReadOnlyMemory<float> EmbeddingVector { get; init; }
    }
}

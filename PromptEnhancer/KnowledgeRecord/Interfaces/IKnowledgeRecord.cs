using PromptEnhancer.Models;

namespace PromptEnhancer.KnowledgeRecord.Interfaces
{
    /// <summary>
    /// Represents a knowledge record containing metadata, source information, embeddings, and other related data.
    /// </summary>
    /// <remarks>This interface is designed to encapsulate the structure and properties of a knowledge record,
    /// which may include metadata, embeddings for machine learning models, similarity scores, and  representations for
    /// use in natural language processing or other AI-related tasks.  It provides a flexible structure for working with
    /// knowledge-based data.</remarks>
    public interface IKnowledgeRecord
    {
        public string? Id { get; set; }
        public IDictionary<string, string>? Metadata { get; set; }

        /// <summary>
        /// Source of the given Knowledge Record.
        /// </summary>
        public string Source { get; set; }
        // optional precomputed embeddings, use only with same model!
        public PipelineEmbeddingsModel? Embeddings { get; set; }

        public double? SimilarityScore { get; set; }

        public string UsedSearchQuery { get; set; }

        /// <summary>
        /// Gets the string representation of the object suitable for use in large language model (LLM) interactions.
        /// </summary>
        public string LLMRepresentationString { get; }

        /// <summary>
        /// Gets the string representation of the object used for Embedding generation.
        /// </summary>
        public string EmbeddingRepresentationString { get; }

        /// <summary>
        /// Gets a value indicating whether the object contains embedding related data.
        /// </summary>
        public bool HasEmbeddingData => Embeddings is not null || SimilarityScore is not null;
        public object SourceObject { get; }
    }
}

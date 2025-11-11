using PromptEnhancer.Models;
using System.ComponentModel.DataAnnotations;

namespace PromptEnhancer.KnowledgeRecord.Interfaces
{
    public interface IKnowledgeRecord
    {
        public string? Id { get; set; }
        public IDictionary<string, string>? Metadata { get; set; }
        public string Source { get; set; }
        // optional precomputed embeddings, use only with same model!
        // maybe just getter to underlying object field?
        public PipelineEmbeddingsModel? Embeddings { get; set; }

        [Range(0, 1)]
        public float? SimilarityScore { get; set; }

        public string UsedSearchQuery { get; set; }

        //static abstract (string property, int chunkSize)? ChunkableProperty { get; }
        public string LLMRepresentationString { get; }
        public string EmbeddingRepresentationString { get; }
        public bool HasEmbeddingData => Embeddings is not null || SimilarityScore is not null;
        // optional property weights for embedding generation
        public IDictionary<string, int>? PropertyWeights { get; }
        public object SourceObject { get; }
    }
}

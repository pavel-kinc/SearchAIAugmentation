using PromptEnhancer.KnowledgeRecord.Interfaces;
using PromptEnhancer.Models;
using PromptEnhancer.Models.Examples;
using System.Text.Json;

namespace PromptEnhancer.KnowledgeRecord
{
    public class KnowledgeRecord<T> : IKnowledgeRecord
        where T : class
    {
        public static Func<T, string>? ChunkSelector => null;
        public static Action<T, string>? AssignChunkToProperty => null;
        public string? Id { get; set; }
        //TODO required here makes the base knowledge to fail in T creation - then there is error in concrete implementations, same with other properties
        public T SourceObject { get; set; }
        public IDictionary<string, string>? Metadata { get; set; }

        public string Source { get; set; }
        // optional precomputed embeddings, use only with same model!
        public PipelineEmbeddingsModel? Embeddings { get; set; }
        public float? SimilarityScore { get; set; }

        public string UsedSearchQuery { get; set; }

        //public virtual (string property, int chunkSize)? ChunkableProperty => null;
        public virtual string LLMRepresentationString => JsonSerializer.Serialize(SourceObject);
        public virtual string EmbeddingRepresentationString => JsonSerializer.Serialize(SourceObject);
        // optional property weights for embedding generation
        public virtual IDictionary<string, int>? PropertyWeights => null;

        object IKnowledgeRecord.SourceObject => SourceObject;
    }
}

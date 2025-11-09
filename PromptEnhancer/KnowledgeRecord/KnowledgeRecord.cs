using PromptEnhancer.KnowledgeBase.Interfaces;
using PromptEnhancer.KnowledgeRecord.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PromptEnhancer.KnowledgeRecord
{
    public class KnowledgeRecord<T> : IKnowledgeRecord
        where T : class
    {
        private float? _rankSimilarity = null;

        public string? Id { get; set; }
        public required T SourceObject { get; set; }
        public IDictionary<string, string>? Metadata { get; set; }

        public required string Source { get; set; }
        // optional precomputed embeddings, use only with same model!
        public float[]? GivenEmbeddings { get; set; } = null;

        public float? RankSimilarity
        {
            get => _rankSimilarity;
            set
            {
                var v = value;
                if (v is not null && v > 1)
                {
                    v = 1;
                }
                _rankSimilarity = v;
            }
        }

        public virtual (string property, int chunkSize)? ChunkableProperty => null;
        public virtual string? EmbeddingRepresentationString => JsonSerializer.Serialize(SourceObject);
        // optional property weights for embedding generation
        public virtual IDictionary<string, int>? PropertyWeights => null;

        object? IKnowledgeRecord.SourceObject => SourceObject;
    }
}

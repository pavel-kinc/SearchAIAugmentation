using PromptEnhancer.KnowledgeRecord.Interfaces;
using PromptEnhancer.Models;
using System.Linq.Expressions;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace PromptEnhancer.KnowledgeRecord
{
    /// <summary>
    /// Represents a record of knowledge that encapsulates a source object, metadata, and optional embeddings for use in
    /// knowledge-based systems.
    /// </summary>
    /// <remarks>This class is designed to store and manage knowledge-related data, including the source
    /// object, metadata, and optional embeddings. It supports serialization of the source object into string
    /// representations for use in various contexts, such as large language models (LLMs) or embedding-based
    /// searches.</remarks>
    /// <typeparam name="T">The type of the source object contained in the record. Must be a reference type.</typeparam>
    public class KnowledgeRecord<T> : IKnowledgeRecord
        where T : class
    {
        // Default JsonSerializerOptions with specific Unicode ranges
        public static readonly JsonSerializerOptions Default = new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.LatinExtendedA, UnicodeRanges.Latin1Supplement),
        };

        public string? Id { get; set; }
        public T SourceObject { get; set; }

        // optional metadata
        public IDictionary<string, string>? Metadata { get; set; }

        /// <inheritdoc/>
        public string Source { get; set; }

        // optional embeddings, use only with the same model!
        public PipelineEmbeddingsModel? Embeddings { get; set; }
        public double? SimilarityScore { get; set; }

        // this is now used also for ranking the embeddings, maybe use combination of more things?
        public string UsedSearchQuery { get; set; }

        /// <summary>
        /// Gets an expression that selects a chunk identifier for an entity of type <typeparamref name="T"/>.
        /// </summary>
        public virtual Expression<Func<T, string?>>? ChunkSelector => null;

        /// <inheritdoc/>
        public virtual string LLMRepresentationString => JsonSerializer.Serialize(SourceObject, Default);

        /// <inheritdoc/>
        public virtual string EmbeddingRepresentationString => JsonSerializer.Serialize(SourceObject, Default);

        object IKnowledgeRecord.SourceObject => SourceObject;
    }
}

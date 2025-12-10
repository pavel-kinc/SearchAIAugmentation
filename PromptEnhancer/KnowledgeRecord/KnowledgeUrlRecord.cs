using PromptEnhancer.Models.Examples;
using System.Linq.Expressions;

namespace PromptEnhancer.KnowledgeRecord
{
    /// <summary>
    /// Represents a knowledge record that encapsulates a URL-based source object.
    /// </summary>
    /// <remarks>This class provides functionality for working with URL-based knowledge records,  including
    /// default token chunking size, content selection, and embedding representation.</remarks>
    public class KnowledgeUrlRecord : KnowledgeRecord<UrlRecord>
    {
        // optional
        public static int DefaultChunkTokenSize => 300;
        public override Expression<Func<UrlRecord, string?>>? ChunkSelector => x => x.Content;
        public override string EmbeddingRepresentationString => SourceObject!.Content;
    }
}

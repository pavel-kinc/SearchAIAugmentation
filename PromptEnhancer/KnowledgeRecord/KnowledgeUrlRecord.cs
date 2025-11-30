using PromptEnhancer.Models.Examples;
using System.Linq.Expressions;

namespace PromptEnhancer.KnowledgeRecord
{
    public class KnowledgeUrlRecord : KnowledgeRecord<UrlRecord>
    {
        public static int DefaultChunkTokenSize => 300;
        public override Expression<Func<UrlRecord, string?>>? ChunkSelector => x => x.Content;
        public override string EmbeddingRepresentationString => SourceObject!.Content;
    }
}

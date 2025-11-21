using PromptEnhancer.Models.Examples;
using System.Linq.Expressions;

namespace PromptEnhancer.KnowledgeRecord
{
    public class KnowledgeUrlRecord : KnowledgeRecord<UrlRecord>
    {
        //public static (string, int)? ChunkableProperty => (UrlRecord.Content, 300);
        //TODO maybe new generic interface that has this as static abstract and then also change in KBCore
        //TODO expression, reflexe
        public override Expression<Func<UrlRecord, string?>>? ChunkSelector => x => x.Content;
        //TODO this does not work, because i generate chunks from UrlRecord, aka before KnowledgeRecord even exists (with this i create more underlying objects)
        //public override string? ChunkProperty
        //{
        //    get => SourceObject.Content;
        //    set => SourceObject.Content = value!;
        //}
        public static int DefaultChunkSize => 500;
        public override string EmbeddingRepresentationString => SourceObject!.Content;
    }
}

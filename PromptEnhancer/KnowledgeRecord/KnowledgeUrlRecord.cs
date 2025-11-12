using Microsoft.Extensions.AI;
using Microsoft.ML.OnnxRuntimeGenAI;
using PromptEnhancer.Models;
using PromptEnhancer.Models.Examples;

namespace PromptEnhancer.KnowledgeRecord
{
    public class KnowledgeUrlRecord : KnowledgeRecord<UrlRecord>
    {
        //public static (string, int)? ChunkableProperty => (UrlRecord.Content, 300);
        //TODO maybe put this in seperate interface, it is no longer in KnowledgeRecord base class
        public static new Func<UrlRecord, string>? ChunkSelector => x => x.Content;
        public static new Action<UrlRecord, string>? AssignChunkToProperty => (x, chunk) => x.Content = chunk;
        //TODO this does not work, because i generate chunks from UrlRecord, aka before KnowledgeRecord even exists (with this i create more underlying objects)
        //public override string? ChunkProperty
        //{
        //    get => SourceObject.Content;
        //    set => SourceObject.Content = value!;
        //}
        public static int DefaultChunkSize => 300;
        public override string EmbeddingRepresentationString => SourceObject!.Content;
    }
}

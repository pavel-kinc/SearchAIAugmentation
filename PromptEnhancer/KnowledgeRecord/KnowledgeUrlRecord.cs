using PromptEnhancer.Models.Examples;

namespace PromptEnhancer.KnowledgeRecord
{
    public class KnowledgeUrlRecord : KnowledgeRecord<UrlRecord>
    {
        //public static (string, int)? ChunkableProperty => (UrlRecord.Content, 300);
        //TODO maybe put this in seperate interface, it is no longer in KnowledgeRecord base class
        public static Func<UrlRecord, string>? ChunkSelector => x => x.Content;
        public static Action<UrlRecord, string>? AssignChunkToProperty => (x, chunk) => x.Content = chunk;
        public static int DefaultChunkSize => 300;

    }
}

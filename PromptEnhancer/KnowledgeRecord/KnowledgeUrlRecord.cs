using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PromptEnhancer.KnowledgeBase
{
    public class KnowledgeUrlRecord : KnowledgeRecord<UrlRecord>
    {
        public override (string, int)? ChunkableProperty => (SourceObject.Content, 300);

        public override string 

    }
}

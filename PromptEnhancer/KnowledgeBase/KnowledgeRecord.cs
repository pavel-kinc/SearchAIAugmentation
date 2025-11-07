using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PromptEnhancer.KnowledgeBase
{
    public class KnowledgeRecord
    {
        public string? Id { get; set; }
        public string? Content { get; set; }
        public IDictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
    }
}

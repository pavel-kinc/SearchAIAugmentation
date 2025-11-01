using PromptEnhancer.KnowledgeBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PromptEnhancer.Models.Pipeline
{
    public class PipelineContext
    {
        public string? QueryString { get; set; }

        public IEnumerable<KnowledgeRecord>? RetrievedRecords { get; set; }

        public IDictionary<string, object>? Metadata { get; set; }
    }
}

using Microsoft.SemanticKernel;
using PromptEnhancer.KnowledgeBase.Interfaces;
using PromptEnhancer.KnowledgeRecord.Interfaces;
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

        public List<IKnowledgeRecord> RetrievedRecords { get; set; } = [];

        public IDictionary<string, object>? Metadata { get; set; }
    }
}

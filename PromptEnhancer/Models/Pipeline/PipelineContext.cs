using PromptEnhancer.KnowledgeRecord.Interfaces;

namespace PromptEnhancer.Models.Pipeline
{
    //TODO maybe implement locks for this class?
    public class PipelineContext
    {
        public string? QueryString { get; set; }

        public List<IKnowledgeRecord> RetrievedRecords { get; set; } = [];

        public IDictionary<string, object>? Metadata { get; set; }
    }
}

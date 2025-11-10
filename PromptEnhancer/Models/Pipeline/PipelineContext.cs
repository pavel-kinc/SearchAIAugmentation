using PromptEnhancer.KnowledgeRecord.Interfaces;

namespace PromptEnhancer.Models.Pipeline
{
    //TODO maybe implement locks for this class?
    public class PipelineContext
    {
        public string? QueryString { get; set; }

        public IEnumerable<string> QueryStrings { get; private set; } = [];

        public List<IKnowledgeRecord> RetrievedRecords { get; private set; } = [];

        public IEnumerable<PipelineEmbeddingsModel> PipelineEmbeddingsModels { get; set; } = [];

        public IDictionary<string, object>? Metadata { get; private set; }
    }
}

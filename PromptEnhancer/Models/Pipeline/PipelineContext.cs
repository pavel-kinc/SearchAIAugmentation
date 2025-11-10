using PromptEnhancer.KnowledgeRecord.Interfaces;

namespace PromptEnhancer.Models.Pipeline
{
    //TODO maybe implement locks for this class?
    public class PipelineContext
    {
        public string? QueryString { get; set; }

        public IEnumerable<string> QueryStrings { get; init; } = [];

        public List<IKnowledgeRecord> RetrievedRecords { get; init; } = [];

        public IEnumerable<PipelineEmbeddingsModel> PipelineEmbeddingsModels { get; set; } = [];

        public IEnumerable<PipelineRankedRecord> PipelineRankedRecords { get; set; } = [];

        public IEnumerable<IKnowledgeRecord> PickedRecords { get; init; } = [];

        public IDictionary<string, object>? Metadata { get; init; }
    }
}

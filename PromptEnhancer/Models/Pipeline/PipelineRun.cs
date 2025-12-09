using Microsoft.Extensions.AI;
using PromptEnhancer.KnowledgeRecord.Interfaces;

namespace PromptEnhancer.Models.Pipeline
{
    /// <summary>
    /// Represents the state and data associated with a single pipeline execution, including input, output, and
    /// intermediate processing details.
    /// </summary>
    /// <remarks>The <see cref="PipelineRun"/> class is designed to encapsulate the data flow and context for
    /// a pipeline execution.  It provides properties to track input data, intermediate results, metadata, and final
    /// outputs.  This class is flexible and can be used to process various steps of a pipeline as data becomes
    /// available.</remarks>
    public class PipelineRun
    {
        public PipelineRun(Entry? entry = null)
        {
            Entry = entry;
            QueryString = entry?.QueryString;
        }
        public string? QueryString { get; set; }

        public IEnumerable<string> QueryStrings { get; set; } = [];

        public List<IKnowledgeRecord> RetrievedRecords { get; init; } = [];

        public IEnumerable<IKnowledgeRecord> PickedRecords { get; set; } = [];

        public List<string> AdditionalContext { get; init; } = [];

        public string? SystemPromptToLLM { get; set; }

        public string? UserPromptToLLM { get; set; }
        public IEnumerable<ChatMessage>? ChatHistory { get; set; }
        public List<string> PipelineLog { get; set; } = [];

        public IDictionary<string, object> Metadata { get; init; } = new Dictionary<string, object>();

        public ChatResponse? FinalResponse { get; set; }

        public Entry? Entry { get; init; }

        public long InputTokenUsage { get; set; } = 0;

        public long OutputTokenUsage { get; set; } = 0;

        public long TokenUsage => InputTokenUsage + OutputTokenUsage;
    }
}

using Microsoft.Extensions.AI;
using PromptEnhancer.KnowledgeRecord.Interfaces;

namespace PromptEnhancer.Models.Pipeline
{
    //TODO maybe implement locks for this class? (now there is no concurrency)
    public class PipelineRun
    {
        //TODO not required (pipeline can be used from any point to do any step, when the needed data is there)
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

        public IDictionary<string, object> Metadata { get; init; } = new Dictionary<string, object>();

        public ChatResponse? FinalResponse { get; set; }

        // TODO maybe make this required and take its ID/Name as the main identifier? (since pipeline can parallelly process multiple contexts/entries)
        public Entry? Entry { get; init; }

        public long InputTokenUsage { get; set; } = 0;

        public long OutputTokenUsage { get; set; } = 0;

        public long TokenUsage => InputTokenUsage + OutputTokenUsage;
    }
}

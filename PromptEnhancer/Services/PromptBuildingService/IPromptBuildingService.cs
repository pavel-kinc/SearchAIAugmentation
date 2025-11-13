using PromptEnhancer.KnowledgeRecord.Interfaces;
using PromptEnhancer.Models;
using PromptEnhancer.Models.Configurations;

namespace PromptEnhancer.Services.PromptBuildingService
{
    public interface IPromptBuildingService
    {
        public string BuildSystemPrompt(PromptConfiguration? promptConfiguration);

        public string BuildUserPrompt(string queryString, IEnumerable<IKnowledgeRecord> pickedRecords, IEnumerable<string> additionalContext, Entry? entry);
    }
}

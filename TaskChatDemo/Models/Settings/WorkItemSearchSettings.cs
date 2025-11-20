using PromptEnhancer.KnowledgeBaseCore.Interfaces;

namespace TaskChatDemo.Models.Settings
{
    public class WorkItemSearchSettings : IKnowledgeBaseSearchSettings
    {
        public string ApiUrl { get; set; } = "https://localhost:7189";
    }
}

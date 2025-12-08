using PromptEnhancer.KnowledgeBaseCore.Interfaces;

namespace TaskChatDemo.Models.Settings
{
    /// <summary>
    /// Represents the settings used for configuring a work item search operation.
    /// </summary>
    /// <remarks>This class provides configuration options for connecting to a work item search API.</remarks>
    public class WorkItemSearchSettings : IKnowledgeBaseSearchSettings
    {
        public string ApiUrl { get; set; } = "https://localhost:7189";
    }
}

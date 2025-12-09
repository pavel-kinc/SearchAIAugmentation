using Microsoft.Extensions.AI;
using PromptEnhancer.KnowledgeBaseCore.Interfaces;

namespace TaskChatDemo.Models.Settings
{
    /// <summary>
    /// Represents the settings used for searching item data within a knowledge base.
    /// </summary>
    /// <remarks>This class is used to configure the search behavior by specifying the embedding
    /// generator.</remarks>
    public class ItemDataSearchSettings : IKnowledgeBaseSearchSettings
    {
        public required IEmbeddingGenerator<string, Embedding<float>> Generator { get; init; }
    }
}

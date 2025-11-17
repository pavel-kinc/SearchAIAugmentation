using Microsoft.Extensions.AI;
using PromptEnhancer.KnowledgeBaseCore.Interfaces;

namespace TaskChatDemo.Models.Settings
{
    public class ItemDataSearchSettings : IKnowledgeBaseSearchSettings
    {
        public required IEmbeddingGenerator<string,Embedding<float>> Generator { get; init; }
    }
}

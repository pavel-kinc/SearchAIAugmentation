using PromptEnhancer.KnowledgeBaseCore.Interfaces;

namespace TaskChatDemo.Models.SearchFilterModels
{
    public class SearchItemFilterModel : IKnowledgeBaseSearchFilter
    {
        public int Top { get; init; } = 5;
        public double MinSimilarityScore = 0.1;
    }
}

using PromptEnhancer.KnowledgeBaseCore.Interfaces;

namespace TaskChatDemo.Models.SearchFilterModels
{
    /// <summary>
    /// Represents a model for filtering task items in a knowledge base.
    /// </summary>
    /// <remarks>This model is used to specify criteria for filtering search results, such as the number of
    /// top results to return and the minimum similarity score required for an item to be included in the
    /// results.</remarks>
    public class SearchItemFilterModel : IKnowledgeBaseSearchFilter
    {
        public int Top { get; init; } = 5;
        public double MinSimilarityScore = 0.1;
    }
}

namespace PromptEnhancer.KnowledgeBaseCore.Interfaces
{
    /// <summary>
    /// Defines a contract for applying filters to knowledge base search queries.
    /// </summary>
    public interface IKnowledgeBaseSearchFilter
    {

    }

    /// <summary>
    /// Represents a search filter that applies no filtering criteria, effectively matching all results.
    /// </summary>
    /// <remarks>This filter can be used when no specific filtering is required, or to indicate that all items. (Useful in Knowledge Base generic with no filter)</remarks>
    public class EmptySearchFilter : IKnowledgeBaseSearchFilter
    {

    }
}

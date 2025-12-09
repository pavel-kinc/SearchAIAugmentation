namespace PromptEnhancer.KnowledgeBaseCore.Interfaces
{

    /// <summary>
    /// Defines the configuration settings for performing searches within a knowledge base.
    /// </summary>
    /// <remarks>This interface provides a contract for specifying search-related settings, such as filters,
    /// limits, or other parameters that influence the behavior of knowledge base queries. Implementations of this
    /// interface should define the specific settings required for their search functionality.</remarks>
    public interface IKnowledgeBaseSearchSettings
    {
    }

    /// <summary>
    /// Represents a placeholder implementation of <see cref="IKnowledgeBaseSearchSettings"/> with no configurable
    /// settings.
    /// </summary>
    /// <remarks>This class can be used in scenarios where no specific search settings are required, or as a
    /// default implementation of the <see cref="IKnowledgeBaseSearchSettings"/> interface.</remarks>
    public class EmptySearchSettings : IKnowledgeBaseSearchSettings
    {

    }
}

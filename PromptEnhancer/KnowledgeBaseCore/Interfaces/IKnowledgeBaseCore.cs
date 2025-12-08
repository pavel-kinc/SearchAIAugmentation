namespace PromptEnhancer.KnowledgeBaseCore.Interfaces
{
    /// <summary>
    /// Represents the core interface for a knowledge base, providing access to its description.
    /// </summary>
    public interface IKnowledgeBaseCore
    {
        /// <summary>
        /// Gets the description of the knowledge base to decide, if it is relevant to given user query (in automatic picking).
        /// </summary>
        public string Description { get; }
    }
}

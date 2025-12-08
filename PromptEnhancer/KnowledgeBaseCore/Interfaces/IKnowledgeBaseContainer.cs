using PromptEnhancer.KnowledgeRecord.Interfaces;

namespace PromptEnhancer.KnowledgeBaseCore.Interfaces
{
    /// <summary>
    /// Represents a container for managing and searching knowledge base records.
    /// </summary>
    /// <remarks>This interface provides functionality to retrieve a description of the knowledge base  and
    /// perform asynchronous searches for records based on specified queries.</remarks>
    public interface IKnowledgeBaseContainer
    {
        /// <summary>
        /// Gets the description of the knowledge base to decide, if it is relevant to given user query (in automatic picking).
        /// </summary>
        public string Description { get; }
        /// <summary>
        /// Searches for data that match the specified queries and converts them to knowledge records.
        /// </summary>
        /// <param name="queriesToSearch">A collection of query strings to search for. Each query is used to find contextual data.</param>
        /// <param name="ct">An optional <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains an enumerable collection of 
        /// <see cref="IKnowledgeRecord"/> objects that match the specified queries. If no matches are found, the
        /// collection will be empty.</returns>
        public Task<IEnumerable<IKnowledgeRecord>> SearchAsync(IEnumerable<string> queriesToSearch, CancellationToken ct = default);
    }
}

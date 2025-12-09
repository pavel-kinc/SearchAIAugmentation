using Microsoft.SemanticKernel;
using PromptEnhancer.KnowledgeRecord.Interfaces;

namespace PromptEnhancer.Services.RecordRankerService
{
    /// <summary>
    /// Provides functionality to assign similarity scores to a collection of knowledge records based on a query string.
    /// </summary>
    public interface IRecordRankerService
    {
        /// <summary>
        /// Assigns a similarity score to a collection of knowledge records based on their relevance to a specified
        /// query string.
        /// </summary>
        /// <remarks>This method evaluates the relevance of each record in the provided collection based
        /// on the specified query string and assigns a similarity score accordingly. If <paramref name="queryString"/>
        /// is <see langword="null"/>, the method may use a default or alternative mechanism to calculate similarity
        /// scores.</remarks>
        /// <param name="kernel">The <see cref="Kernel"/> instance used to process the records and calculate similarity scores.</param>
        /// <param name="records">A collection of knowledge records to which similarity scores will be assigned.</param>
        /// <param name="queryString">The query string used to evaluate the relevance of the records. Can be <see langword="null"/> if no query is
        /// provided.</param>
        /// <param name="generatorKey">An optional key used to identify the generator responsible for processing the records. Can be <see
        /// langword="null"/>.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is <see langword="true"/> if the
        /// similarity scores were successfully assigned; otherwise, <see langword="false"/>.</returns>
        Task<bool> AssignSimilarityScoreToRecordsAsync(Kernel kernel, IEnumerable<IKnowledgeRecord> records, string? queryString, string? generatorKey);
    }
}

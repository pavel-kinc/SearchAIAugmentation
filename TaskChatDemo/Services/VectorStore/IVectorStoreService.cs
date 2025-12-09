using TaskChatDemo.Models.SearchFilterModels;
using TaskChatDemo.Models.TaskItem;

namespace TaskChatDemo.Services.VectorStore
{
    /// <summary>
    /// Provides an interface for retrieving Task data items with associated scores based on a query vector.
    /// </summary>
    /// <remarks>Implementations of this interface are expected to perform a search operation using the
    /// provided query vector and return a collection of data items along with their relevance scores.</remarks>
    public interface IVectorStoreService
    {
        /// <summary>
        /// Asynchronously retrieves a collection of task items with associated scores based on the provided query
        /// vector.
        /// </summary>
        /// <param name="queryVector">A read-only memory segment representing the query vector used to calculate scores.</param>
        /// <param name="model">An optional filter model to refine the search results. Can be <see langword="null"/> to apply no additional
        /// filtering.</param>
        /// <returns>A task representing the asynchronous operation. The task result contains an enumerable of <see
        /// cref="TaskItemData"/> objects, each with a calculated score.</returns>
        public Task<IEnumerable<TaskItemData>> GetDataWithScoresAsync(ReadOnlyMemory<float> queryVector, SearchItemFilterModel? model);
    }
}

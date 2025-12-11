using Microsoft.Extensions.VectorData;
using TaskChatDemo.Models.SearchFilterModels;
using TaskChatDemo.Models.TaskItem;

namespace TaskChatDemo.Services.VectorStore
{
    /// <summary>
    /// Provides services for managing and retrieving task items from a vector store.
    /// </summary>
    /// <remarks>This service interacts with a vector store collection to perform operations such as searching
    /// for task items based on a query vector and optional filtering criteria.</remarks>
    public class VectorStoreService : IVectorStoreService
    {
        private readonly VectorStoreCollection<Guid, TaskItemModel> _taskCollection;

        public VectorStoreService(VectorStoreCollection<Guid, TaskItemModel> taskCollection)
        {
            _taskCollection = taskCollection;
        }

        /// <summary>
        /// Asynchronously retrieves a collection of task items with similarity scores based on the provided query
        /// vector.
        /// </summary>
        /// <remarks>The method filters out results with a similarity score below the specified minimum
        /// threshold in the <paramref name="model"/>. If <paramref name="model"/> is null, a default of 5 top results
        /// is used, and no minimum similarity score is applied.</remarks>
        /// <param name="queryVector">The vector used to query and calculate similarity scores for task items.</param>
        /// <param name="model">An optional filter model that specifies the maximum number of results to return and the minimum similarity
        /// score threshold.</param>
        /// <returns>An enumerable collection of <see cref="TaskItemData"/> objects that meet the specified similarity criteria.
        /// Each item includes its similarity score.</returns>
        public async Task<IEnumerable<TaskItemData>> GetDataWithScoresAsync(ReadOnlyMemory<float> queryVector, SearchItemFilterModel? model)
        {
            var results = await _taskCollection.SearchAsync(queryVector, model?.Top ?? 5).ToListAsync();
            IEnumerable<TaskItemData> data = results
                .Where(r => r.Record is not null)
                .Where(r => (r.Score ?? 0) > (model?.MinSimilarityScore ?? 0))
                .Select(r =>
                {
                    var m = r.Record!;
                    return new TaskItemData
                    {
                        Id = m.Id,
                        Title = m.Title,
                        Description = m.Description,
                        Status = m.Status,
                        Assignee = m.Assignee,
                        CreatedDate = m.CreatedDate,
                        SimilarityScore = r.Score ?? 0.0
                    };
                });
            return data;
        }
    }
}

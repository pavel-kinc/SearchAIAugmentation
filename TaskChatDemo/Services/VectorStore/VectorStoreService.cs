using Microsoft.Extensions.VectorData;
using TaskChatDemo.Models.SearchFilterModels;
using TaskChatDemo.Models.TaskItem;

namespace TaskChatDemo.Services.VectorStore
{
    public class VectorStoreService : IVectorStoreService
    {
        private readonly VectorStoreCollection<Guid, TaskItemModel> _taskCollection;
        public VectorStoreService(VectorStoreCollection<Guid, TaskItemModel> taskCollection)
        {
            _taskCollection = taskCollection;
        }

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

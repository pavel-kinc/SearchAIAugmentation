using TaskChatDemo.Models.SearchFilterModels;
using TaskChatDemo.Models.TaskItem;

namespace TaskChatDemo.Services.VectorStore
{
    public interface IVectorStoreService
    {
        public Task<IEnumerable<TaskItemData>> GetDataWithScoresAsync(ReadOnlyMemory<float> queryVector, SearchItemFilterModel? model);
    }
}

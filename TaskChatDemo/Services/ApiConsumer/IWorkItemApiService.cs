using TaskChatDemo.Models;
using TaskChatDemo.Models.SearchFilterModels;

namespace TaskChatDemo.Services.ApiConsumer
{
    public interface IWorkItemApiService
    {
        Task<List<WorkItem>> GetWorkItemsAsync(SearchWorkItemFilterModel? filter, string apiUrl);
    }
}

using TaskChatDemo.Models;
using TaskChatDemo.Models.SearchFilterModels;

namespace TaskChatDemo.Services.ApiConsumer
{
    /// <summary>
    /// Provides methods for interacting with work items through an API.
    /// </summary>
    public interface IWorkItemApiService
    {
        /// <summary>
        /// Asynchronously retrieves a list of work items based on the specified filter criteria.
        /// </summary>
        /// <param name="filter">The filter criteria to apply when searching for work items. Can be null to retrieve all work items.</param>
        /// <param name="apiUrl">The base URL of the API endpoint to query for work items. Cannot be null or empty.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of <see cref="WorkItem"/>
        /// objects that match the filter criteria.</returns>
        Task<List<WorkItem>> GetWorkItemsAsync(SearchWorkItemFilterModel? filter, string apiUrl);
    }
}

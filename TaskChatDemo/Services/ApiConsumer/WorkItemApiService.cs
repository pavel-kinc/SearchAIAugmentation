using TaskChatDemo.Models;
using TaskChatDemo.Models.SearchFilterModels;

namespace TaskChatDemo.Services.ApiConsumer
{
    /// <summary>
    /// Provides methods for interacting with work items through an API.
    /// </summary>
    /// <remarks>This service uses an <see cref="IHttpClientFactory"/> to create HTTP clients for making
    /// requests to the specified API endpoint. It supports filtering work items based on the provided
    /// criteria.</remarks>
    public class WorkItemApiService : IWorkItemApiService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public WorkItemApiService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        /// <inheritdoc/>
        public async Task<List<WorkItem>> GetWorkItemsAsync(SearchWorkItemFilterModel? filter, string apiUrl)
        {
            var client = _httpClientFactory.CreateClient();

            client.BaseAddress = new Uri(apiUrl);

            string qs = filter?.BuildQuery() ?? string.Empty;
            var response = await client.GetAsync($"work-items?{qs}");
            response.EnsureSuccessStatusCode();

            var items = await response.Content.ReadFromJsonAsync<List<WorkItem>>();
            return items ?? new List<WorkItem>();
        }
    }
}

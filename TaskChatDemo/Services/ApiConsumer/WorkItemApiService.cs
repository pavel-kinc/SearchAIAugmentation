using TaskChatDemo.Models;
using TaskChatDemo.Models.SearchFilterModels;

namespace TaskChatDemo.Services.ApiConsumer
{
    public class WorkItemApiService : IWorkItemApiService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public WorkItemApiService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

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

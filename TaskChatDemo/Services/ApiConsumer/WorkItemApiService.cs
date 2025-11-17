using TaskChatDemo.Models;
using TaskChatDemo.Models.SearchFilterModels;

namespace TaskChatDemo.Services.ApiConsumer
{
    public class WorkItemApiService : IWorkItemApiService
    {
        private readonly HttpClient _http;

        public WorkItemApiService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<WorkItem>> GetWorkItemsAsync(SearchWorkItemFilterModel? filter, string apiUrl)
        {
            _http.BaseAddress = new Uri(apiUrl);
            string qs = filter?.BuildQuery() ?? string.Empty;
            var response = await _http.GetAsync("work-items?" + qs);
            response.EnsureSuccessStatusCode();

            var items = await response.Content.ReadFromJsonAsync<List<WorkItem>>();
            return items ?? [];
        }
    }
}

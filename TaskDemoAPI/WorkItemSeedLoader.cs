using System.Collections.Immutable;
using System.Text.Json;
using System.Text.Json.Serialization;
using TaskDemoAPI.Models;

namespace TaskDemoAPI
{
    public static class WorkItemSeedLoader
    {
        public static ImmutableArray<WorkItem> LoadFromJson(string path)
        {
            var json = File.ReadAllText(path);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
            };

            var list = JsonSerializer.Deserialize<List<WorkItem>>(json, options)
                       ?? new List<WorkItem>();

            return list.ToImmutableArray();
        }
    }
}

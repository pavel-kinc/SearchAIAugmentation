using System.Collections.Immutable;
using System.Text.Json;
using System.Text.Json.Serialization;
using TaskDemoAPI.Models;

namespace TaskDemoAPI
{
    /// <summary>
    /// Loads a collection of <see cref="WorkItem"/> objects from a JSON file.
    /// </summary>
    /// <remarks>The JSON file is expected to contain an array of work items. Property names in the JSON are
    /// matched      to the <see cref="WorkItem"/> properties in a case-insensitive manner. Enum values are deserialized
    /// using camelCase naming.</remarks>
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

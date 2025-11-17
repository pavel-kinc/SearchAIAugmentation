using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel;
using System.Collections.Immutable;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using TaskChatDemo.Models.TaskItem;

namespace TaskChatDemo
{
    public static class TaskItemModelUtility
    {
        public static ImmutableArray<TaskItemModel> LoadFromJson(string path)
        {
            var json = File.ReadAllText(path);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
            };

            var list = JsonSerializer.Deserialize<List<TaskItemModel>>(json, options)
                       ?? new List<TaskItemModel>();

            return list.ToImmutableArray();
        }

        public async static Task SaveAsync(IEnumerable<TaskItemModel> workItems, string path)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            string newJson = JsonSerializer.Serialize(workItems, options);
            await File.WriteAllTextAsync(path, newJson);
        }

        public static async Task AssignEmbeddingsToTasks(Kernel kernel, IEnumerable<TaskItemModel> workItems)
        {
            var embedder = kernel.GetRequiredService<IEmbeddingGenerator<string, Embedding<float>>>();
            var chunks = workItems
            .Where(item => item.Embedding is null)
            .Chunk(1000).ToList();

            foreach (var chunk in chunks)
            {
                var embeddings = await embedder.GenerateAsync(chunk.Select(item => JsonSerializer.Serialize(item)));

                foreach (var (item, embedding) in chunk.Zip(embeddings))
                {
                    item.Embedding = embedding.Vector.ToArray();
                }
            }
            await SaveAsync(workItems, Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, "Data", "task_models.json"));
        }
    }
}

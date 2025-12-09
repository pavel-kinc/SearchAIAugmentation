using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel;
using System.Collections.Immutable;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using TaskChatDemo.Models.TaskItem;

namespace TaskChatDemo
{
    /// <summary>
    /// Provides utility methods for loading, saving, and processing task item models.
    /// </summary>
    /// <remarks>This class includes methods to load task items from a JSON file, save task items to a JSON
    /// file, and assign embeddings to task items using a specified kernel.</remarks>
    public static class TaskItemModelUtility
    {
        /// <summary>
        /// Loads a collection of <see cref="TaskItemModel"/> objects from a JSON file.
        /// </summary>
        /// <remarks>The JSON deserialization is case-insensitive with respect to property names and
        /// supports enum values in camel case.</remarks>
        /// <param name="path">The file path to the JSON file containing the task items.</param>
        /// <returns>An immutable array of <see cref="TaskItemModel"/> objects. Returns an empty array if the file is empty or
        /// the JSON is invalid.</returns>
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

        /// <summary>
        /// Asynchronously saves a collection of work items to a specified file path in JSON format.
        /// </summary>
        /// <remarks>The method serializes the provided work items into a JSON string with indented
        /// formatting and writes it to the specified file path.</remarks>
        /// <param name="workItems">The collection of work items to be serialized and saved. Cannot be null.</param>
        /// <param name="path">The file path where the JSON data will be written. Cannot be null or empty.</param>
        /// <returns>A task that represents the asynchronous save operation.</returns>
        public async static Task SaveAsync(IEnumerable<TaskItemModel> workItems, string path)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            string newJson = JsonSerializer.Serialize(workItems, options);
            await File.WriteAllTextAsync(path, newJson);
        }

        /// <summary>
        /// Asynchronously assigns embeddings to a collection of task items that lack them.
        /// </summary>
        /// <remarks>This method processes the task items in chunks of 1000 to generate embeddings
        /// efficiently. The generated embeddings are assigned to the <c>Embedding</c> property of each task item. The
        /// updated task items are then saved to a JSON file located in the "Data" directory of the executing
        /// assembly.</remarks>
        /// <param name="kernel">The kernel instance used to retrieve the embedding generator service.</param>
        /// <param name="workItems">A collection of <see cref="TaskItemModel"/> objects representing the tasks to which embeddings will be
        /// assigned. Only tasks with a null <c>Embedding</c> property will be processed.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
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

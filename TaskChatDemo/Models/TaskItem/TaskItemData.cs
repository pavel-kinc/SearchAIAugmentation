using System.Text.Json.Serialization;

namespace TaskChatDemo.Models.TaskItem
{
    /// <summary>
    /// Represents a task item with details such as title, description, status, and assignee.
    /// </summary>
    public class TaskItemData
    {
        public Guid Id { get; set; }
        public required string Title { get; set; }
        public string? Description { get; set; }
        public string Status { get; set; } = "TODO";
        public string? Assignee { get; set; }
        public DateTime? CreatedDate { get; set; }
        [JsonIgnore]
        public double SimilarityScore { get; set; }
    }
}

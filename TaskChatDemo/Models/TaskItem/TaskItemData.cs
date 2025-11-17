using System.Text.Json.Serialization;

namespace TaskChatDemo.Models.TaskItem
{
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

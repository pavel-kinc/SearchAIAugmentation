namespace TaskChatDemo.Models
{
    /// <summary>
    /// Represents a work item with details such as title, description, type, and associated metadata.
    /// </summary>
    public class WorkItem
    {
        public required string Id { get; init; }
        public string Title { get; init; } = string.Empty;
        public string? Description { get; init; }
        public string Type { get; init; } = "Unknown";
        public string? AssignedTo { get; init; }
        public DateTime? CreatedDate { get; init; }
        public IList<string> Tags { get; init; } = new List<string>();
        public double? OriginalEstimateHours { get; init; }
        public string? Url { get; init; }
    }
}

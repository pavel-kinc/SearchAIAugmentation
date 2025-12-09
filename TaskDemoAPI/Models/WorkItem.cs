namespace TaskDemoAPI.Models
{
    /// <summary>
    /// Represents a unit of work or task, typically used in project management or tracking systems.
    /// </summary>
    /// <remarks>A <see cref="WorkItem"/> contains details such as its unique identifier, title, description,
    /// type,  assigned user, creation date, associated tags, estimated effort, and an optional URL for additional
    /// context. This class is immutable after initialization, with all properties being set at object
    /// creation.</remarks>
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

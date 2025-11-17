namespace TaskDemoAPI.Models
{
    public class WorkItem
    {
        public required string Id { get; init; }
        public string Title { get; init; } = string.Empty;
        public string? Description { get; init; }
        public WorkItemType Type { get; init; } = WorkItemType.Unknown;
        public string? AssignedTo { get; init; }
        public DateTime? CreatedDate { get; init; }
        public IList<string> Tags { get; init; } = new List<string>();
        public double? OriginalEstimateHours { get; init; }
        public string? Url { get; init; }
    }
}

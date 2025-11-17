namespace TaskDemoAPI.Models
{
    public class WorkItemFilter
    {
        public string? Title { get; set; }
        public WorkItemType? Type { get; set; }
        public string? Description { get; init; }
        public string? AssignedTo { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
    }
}

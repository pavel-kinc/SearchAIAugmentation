namespace TaskDemoAPI.Models
{
    /// <summary>
    /// Represents a set of criteria used to filter work items.
    /// </summary>
    /// <remarks>This class provides properties to specify filtering criteria such as title, type,
    /// description,  assigned user, and date range. Use this class to define the conditions for querying or filtering 
    /// work items in a collection.</remarks>
    public class WorkItemFilter
    {
        public string? Title { get; set; }
        public string? Type { get; set; }
        public string? Description { get; init; }
        public string? AssignedTo { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
    }
}

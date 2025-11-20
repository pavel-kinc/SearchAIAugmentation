using TaskDemoAPI.Models;

namespace TaskDemoAPI.WorkItemRepository
{
    public class WorkItemRepository : IWorkItemRepository
    {
        private readonly IReadOnlyList<WorkItem> _workItems;

        public WorkItemRepository(IReadOnlyList<WorkItem> workItems) => _workItems = workItems;

        public IEnumerable<WorkItem> GetTasks(WorkItemFilter filter)
        {
            IEnumerable<WorkItem> query = _workItems;

            if (!string.IsNullOrWhiteSpace(filter.Title))
            {
                query = query.Where(x =>
                    x.Title.Contains(filter.Title, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(filter.Type))
            {
                query = query.Where(x =>
                    x.Type.Contains(filter.Type, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(filter.Description))
                query = query.Where(x => x.Description?.Contains(filter.Description) == true);

            if (!string.IsNullOrWhiteSpace(filter.AssignedTo))
                query = query.Where(x => string.Equals(x.AssignedTo, filter.AssignedTo, StringComparison.OrdinalIgnoreCase));

            if (filter.From.HasValue)
                query = query.Where(x => x.CreatedDate >= filter.From.Value);

            if (filter.To.HasValue)
                query = query.Where(x => x.CreatedDate <= filter.To.Value);

            return query;
        }
    }
}

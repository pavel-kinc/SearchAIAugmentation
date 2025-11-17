using TaskDemoAPI.Models;

namespace TaskDemoAPI.WorkItemRepository
{
    public interface IWorkItemRepository
    {
        IEnumerable<WorkItem> GetTasks(WorkItemFilter filter);
    }
}

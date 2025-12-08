using TaskDemoAPI.Models;

namespace TaskDemoAPI.WorkItemRepository
{
    /// <summary>
    /// Defines a repository for retrieving work items based on specified criteria.
    /// </summary>
    /// <remarks>This interface provides a method to query and retrieve work items, such as tasks,  based on a
    /// given filter. Implementations of this interface are responsible for  defining the data source and retrieval
    /// logic.</remarks>
    public interface IWorkItemRepository
    {
        /// <summary>
        /// Retrieves a collection of work items that match the specified filter criteria.
        /// </summary>
        /// <param name="filter">The filter criteria used to determine which work items to include in the result. Cannot be null.</param>
        /// <returns>An enumerable collection of <see cref="WorkItem"/> objects that satisfy the specified filter.  Returns an
        /// empty collection if no work items match the filter.</returns>
        IEnumerable<WorkItem> GetTasks(WorkItemFilter filter);
    }
}

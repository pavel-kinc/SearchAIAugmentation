using Microsoft.AspNetCore.Mvc;
using TaskDemoAPI.Models;
using TaskDemoAPI.WorkItemRepository;

namespace TaskDemoAPI.Controllers;

/// <summary>
/// Provides endpoints for managing and retrieving work items.
[ApiController]
public class WorkItemController : ControllerBase
{
    private readonly IWorkItemRepository _workItemRepository;

    public WorkItemController(IWorkItemRepository workItemRepo)
    {
        _workItemRepository = workItemRepo;
    }

    /// <summary>
    /// Retrieves a collection of work items based on the specified filter criteria.
    /// </summary>
    /// <remarks>The filter criteria are applied to query the work items from the repository. Ensure that the
    /// filter is properly configured to achieve the desired results.</remarks>
    /// <param name="filter">The filter criteria used to narrow down the work items. This parameter must not be null.</param>
    /// <returns>An <see cref="IActionResult"/> containing the filtered collection of work items. Returns an empty collection if
    /// no work items match the filter.</returns>
    [HttpGet("work-items")]
    public IActionResult GetWorkItems([FromQuery] WorkItemFilter filter)
    {
        var workItems = _workItemRepository.GetTasks(filter);
        return Ok(workItems);
    }
}

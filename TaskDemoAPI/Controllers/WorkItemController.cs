using Microsoft.AspNetCore.Mvc;
using TaskDemoAPI.Models;
using TaskDemoAPI.WorkItemRepository;

namespace TaskDemoAPI.Controllers;

[ApiController]
public class WorkItemController : ControllerBase
{
    private readonly IWorkItemRepository _workItemRepository;

    public WorkItemController(IWorkItemRepository workItemRepo)
    {
        _workItemRepository = workItemRepo;
    }

    [HttpGet("work-items")]
    public IActionResult GetWorkItems([FromQuery] WorkItemFilter filter)
    {
        var workItems = _workItemRepository.GetTasks(filter);
        return Ok(workItems);
    }
}

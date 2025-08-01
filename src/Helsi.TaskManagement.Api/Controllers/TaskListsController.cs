using Helsi.TaskManagement.Api.Infrastructure.Extensions;
using Microsoft.AspNetCore.Mvc;
using Helsi.TaskManagement.Application.Services.Interfaces;
using Helsi.TaskManagement.Application.DTOs.Requests;
using Helsi.TaskManagement.Application.DTOs.Responses;
using Helsi.TaskManagement.Application.Common.Models;

namespace Helsi.TaskManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TaskListsController(ITaskListService taskListService) : BaseController
{
    private readonly ITaskListService _taskListService = taskListService ?? throw new ArgumentNullException(nameof(taskListService));

    /// <summary>
    /// Create a new task list
    /// </summary>
    /// <param name="request">Task list creation request</param>
    /// <returns>Created task list</returns>
    [HttpPost]
    [ProducesResponseType(typeof(TaskListResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<TaskListResponse>> CreateTaskList([FromBody] CreateAndUpdateTaskListRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        
        var result = await _taskListService.CreateAsync(request, CurrentUserId);
        return CreatedAtAction(nameof(GetTaskList), new { id = result.Id }, result);
    }

    /// <summary>
    /// Get a specific task list by ID
    /// </summary>
    /// <param name="id">Task list ID</param>
    /// <returns>Task list details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(TaskListResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskListResponse>> GetTaskList(string id)
    {
        var result = await _taskListService.GetByIdAsync(id, CurrentUserId);
        return Ok(result);
    }

    /// <summary>
    /// Update an existing task list
    /// </summary>
    /// <param name="id">Task list ID</param>
    /// <param name="request">Task list update request</param>
    /// <returns>Updated task list</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(TaskListResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskListResponse>> UpdateTaskList(string id, [FromBody] CreateAndUpdateTaskListRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _taskListService.UpdateAsync(id, request, CurrentUserId);
        return Ok(result);
    }

    /// <summary>
    /// Delete a task list
    /// </summary>
    /// <param name="id">Task list ID</param>
    /// <returns>Success status</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTaskList(string id)
    {
        var userId = HttpContext.GetUserId();
        
        var deleted = await _taskListService.DeleteAsync(id, userId);
        if (!deleted)
            return NotFound($"Task list with ID '{id}' not found");

        return NoContent();
    }

    /// <summary>
    /// Get paginated list of task lists accessible to the user
    /// </summary>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10, max: 100)</param>
    /// <returns>Paginated list of task lists</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResult<TaskListSummaryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PaginatedResult<TaskListSummaryResponse>>> GetTaskLists(
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 10)
    {
        var result = await _taskListService.GetPaginatedAsync(CurrentUserId, page, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// Add a user connection to a task list
    /// </summary>
    /// <param name="id">Task list ID</param>
    /// <param name="request">Connection request</param>
    /// <returns>Success status</returns>
    [HttpPost("{id}/connections")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddConnection(string id, [FromBody] AddConnectionRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        await _taskListService.AddConnectionAsync(id, request, CurrentUserId);
        return NoContent();
    }

    /// <summary>
    /// Get all user connections for a task list
    /// </summary>
    /// <param name="id">Task list ID</param>
    /// <returns>List of connected user IDs</returns>
    [HttpGet("{id}/connections")]
    [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<string>>> GetConnections(string id)
    {
        var result = await _taskListService.GetConnectionsAsync(id, CurrentUserId);
        return Ok(result);
    }

    /// <summary>
    /// Remove a user connection from a task list
    /// </summary>
    /// <param name="id">Task list ID</param>
    /// <param name="userId">User ID to remove</param>
    /// <returns>Success status</returns>
    [HttpDelete("{id}/connections/{userId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveConnection(string id, string userId)
    {
        await _taskListService.RemoveConnectionAsync(id, userId, CurrentUserId);
        return NoContent();
    }
}
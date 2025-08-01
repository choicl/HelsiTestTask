using Helsi.TaskManagement.Application.Common.Exceptions;
using Helsi.TaskManagement.Application.Common.Models;
using Helsi.TaskManagement.Application.DTOs.Requests;
using Helsi.TaskManagement.Application.DTOs.Responses;
using Helsi.TaskManagement.Application.Services.Interfaces;
using Helsi.TaskManagement.Domain.Entities;
using Helsi.TaskManagement.Domain.Interfaces;
using UnauthorizedAccessException = Helsi.TaskManagement.Application.Common.Exceptions.UnauthorizedAccessException;

namespace Helsi.TaskManagement.Application.Services.Implementations;

public class TaskListService(ITaskListRepository repository) : ITaskListService
{
    private readonly ITaskListRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));

    public async Task<TaskListResponse> CreateAsync(CreateAndUpdateTaskListRequest request, string userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("UserId cannot be empty", nameof(userId));

        var taskList = new TaskList(request.Name, userId);
        var createdTaskList = await _repository.CreateAsync(taskList, cancellationToken);

        return MapToResponse(createdTaskList);
    }

    public async Task<TaskListResponse> GetByIdAsync(string id, string userId, CancellationToken cancellationToken = default)
    {
        var taskList = await GetTaskListWithAccessCheck(id, userId, cancellationToken);
        return MapToResponse(taskList);
    }

    public async Task<TaskListResponse> UpdateAsync(string id, CreateAndUpdateTaskListRequest request, string userId, CancellationToken cancellationToken = default)
    {
        var taskList = await GetTaskListWithAccessCheck(id, userId, cancellationToken);
        
        taskList.SetName(request.Name);
        var updatedTaskList = await _repository.UpdateAsync(taskList, cancellationToken);

        return MapToResponse(updatedTaskList);
    }

    public async Task<bool> DeleteAsync(string id, string userId, CancellationToken cancellationToken = default)
    {
        await GetTaskListWithOwnerCheck(id, userId, cancellationToken);
        return await _repository.DeleteAsync(id, cancellationToken);
    }

    public async Task<PaginatedResult<TaskListSummaryResponse>> GetPaginatedAsync(string userId, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("UserId cannot be empty", nameof(userId));

        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 10;

        var taskLists = await _repository.GetByUserAccessAsync(userId, page, pageSize, cancellationToken);
        var totalCount = await _repository.GetCountByUserAccessAsync(userId, cancellationToken);

        var summaryResponses = taskLists.Select(MapToSummaryResponse);

        return new PaginatedResult<TaskListSummaryResponse>(summaryResponses, totalCount, page, pageSize);
    }

    public async Task AddConnectionAsync(string id, AddConnectionRequest request, string userId, CancellationToken cancellationToken = default)
    {
        var taskList = await GetTaskListWithAccessCheck(id, userId, cancellationToken);
        
        taskList.AddConnection(request.UserId);
        await _repository.UpdateAsync(taskList, cancellationToken);
    }

    public async Task<IEnumerable<string>> GetConnectionsAsync(string id, string userId, CancellationToken cancellationToken = default)
    {
        var taskList = await GetTaskListWithAccessCheck(id, userId, cancellationToken);
        return taskList.ConnectedUserIds;
    }

    public async Task RemoveConnectionAsync(string id, string connectionUserId, string userId, CancellationToken cancellationToken = default)
    {
        var taskList = await GetTaskListWithAccessCheck(id, userId, cancellationToken);
        
        taskList.RemoveConnection(connectionUserId);
        await _repository.UpdateAsync(taskList, cancellationToken);
    }

    #region Private Helper Methods

    private async Task<TaskList> GetTaskListWithAccessCheck(string id, string userId, CancellationToken cancellationToken)
    {
        var taskList = await _repository.GetByIdAsync(id, cancellationToken);
        
        if (taskList == null)
            throw new TaskListNotFoundException(id);

        if (!taskList.HasAccess(userId))
            throw new UnauthorizedAccessException(userId, "access task list");

        return taskList;
    }

    private async Task GetTaskListWithOwnerCheck(string id, string userId, CancellationToken cancellationToken)
    {
        var taskList = await _repository.GetByIdAsync(id, cancellationToken);
        
        if (taskList == null)
            throw new TaskListNotFoundException(id);

        if (!taskList.IsOwner(userId))
            throw new UnauthorizedAccessException(userId, "delete task list");
    }

    private static TaskListResponse MapToResponse(TaskList taskList)
    {
        return new TaskListResponse
        {
            Id = taskList.Id,
            Name = taskList.Name,
            OwnerId = taskList.OwnerId,
            ConnectedUserIds = taskList.ConnectedUserIds.ToList(),
            CreatedAt = taskList.CreatedAt,
            UpdatedAt = taskList.UpdatedAt
        };
    }

    private static TaskListSummaryResponse MapToSummaryResponse(TaskList taskList)
    {
        return new TaskListSummaryResponse
        {
            Id = taskList.Id,
            Name = taskList.Name,
            OwnerId = taskList.OwnerId,
            CreatedAt = taskList.CreatedAt
        };
    }

    #endregion
}
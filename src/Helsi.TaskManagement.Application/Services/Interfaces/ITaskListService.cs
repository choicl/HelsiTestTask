using Helsi.TaskManagement.Application.Common.Models;
using Helsi.TaskManagement.Application.DTOs.Requests;
using Helsi.TaskManagement.Application.DTOs.Responses;

namespace Helsi.TaskManagement.Application.Services.Interfaces;

public interface ITaskListService
{
    Task<TaskListResponse> CreateAsync(CreateAndUpdateTaskListRequest request, string userId, CancellationToken cancellationToken = default);
    Task<TaskListResponse> GetByIdAsync(string id, string userId, CancellationToken cancellationToken = default);
    Task<TaskListResponse> UpdateAsync(string id, CreateAndUpdateTaskListRequest request, string userId, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(string id, string userId, CancellationToken cancellationToken = default);
    Task<PaginatedResult<TaskListSummaryResponse>> GetPaginatedAsync(string userId, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default);
    Task AddConnectionAsync(string id, AddConnectionRequest request, string userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<string>> GetConnectionsAsync(string id, string userId, CancellationToken cancellationToken = default);
    Task RemoveConnectionAsync(string id, string connectionUserId, string userId, CancellationToken cancellationToken = default);
}
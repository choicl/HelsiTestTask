using Helsi.TaskManagement.Domain.Entities;

namespace Helsi.TaskManagement.Domain.Interfaces;

public interface ITaskListRepository
{
    Task<TaskList?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<IEnumerable<TaskList>> GetByUserAccessAsync(string userId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> GetCountByUserAccessAsync(string userId, CancellationToken cancellationToken = default);
    Task<TaskList> CreateAsync(TaskList taskList, CancellationToken cancellationToken = default);
    Task<TaskList> UpdateAsync(TaskList taskList, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default); 
}
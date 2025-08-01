using MongoDB.Driver;
using Helsi.TaskManagement.Domain.Entities;
using Helsi.TaskManagement.Domain.Interfaces;
using Helsi.TaskManagement.Infrastructure.Persistence.Context;

namespace Helsi.TaskManagement.Infrastructure.Persistence.Repositories;

public class TaskListRepository(MongoDbContext context) : ITaskListRepository
{
    private readonly MongoDbContext _context = context ?? throw new ArgumentNullException(nameof(context));

    public async Task<TaskList?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(id))
            return null;

        return await _context.TaskLists
            .Find(tl => tl.Id == id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IEnumerable<TaskList>> GetByUserAccessAsync(string userId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return [];

        var filter = Builders<TaskList>.Filter.Or(Builders<TaskList>.Filter.Eq(tl => tl.OwnerId, userId),
            Builders<TaskList>.Filter.AnyEq(tl => tl.ConnectedUserIds, userId));

        var sort = Builders<TaskList>.Sort.Descending(tl => tl.CreatedAt);

        return await _context.TaskLists
            .Find(filter)
            .Sort(sort)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetCountByUserAccessAsync(string userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return 0;

        var filter = Builders<TaskList>.Filter.Or(Builders<TaskList>.Filter.Eq(tl => tl.OwnerId, userId),
            Builders<TaskList>.Filter.AnyEq(tl => tl.ConnectedUserIds, userId));

        return (int)await _context.TaskLists.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
    }

    public async Task<TaskList> CreateAsync(TaskList taskList, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(taskList);

        if (string.IsNullOrWhiteSpace(taskList.Id)) 
            taskList.Id = MongoDB.Bson.ObjectId.GenerateNewId().ToString();

        await _context.TaskLists.InsertOneAsync(taskList, cancellationToken: cancellationToken);
        return taskList;
    }

    public async Task<TaskList> UpdateAsync(TaskList taskList, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(taskList);

        taskList.UpdateTimestamp();

        var result = await _context.TaskLists.ReplaceOneAsync(tl => tl.Id == taskList.Id, taskList,
            cancellationToken: cancellationToken);

        if (result.MatchedCount == 0)
            throw new InvalidOperationException($"TaskList with id {taskList.Id} not found");

        return taskList;
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(id))
            return false;

        var result = await _context.TaskLists.DeleteOneAsync(tl => tl.Id == id, cancellationToken);

        return result.DeletedCount > 0;
    }
}
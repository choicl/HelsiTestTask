using MongoDB.Driver;
using Helsi.TaskManagement.Domain.Entities;
using Helsi.TaskManagement.Infrastructure.Common;

namespace Helsi.TaskManagement.Infrastructure.Persistence.Context;

public class MongoDbContext(IMongoDatabase database, MongoDbSettings settings)
{
    private readonly IMongoDatabase _database = database ?? throw new ArgumentNullException(nameof(database));
    private readonly MongoDbSettings _settings = settings ?? throw new ArgumentNullException(nameof(settings));

    public IMongoCollection<TaskList> TaskLists => 
        _database.GetCollection<TaskList>(_settings.TaskListsCollectionName);
}
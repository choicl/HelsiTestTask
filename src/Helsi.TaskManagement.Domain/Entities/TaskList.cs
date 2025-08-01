using Helsi.TaskManagement.Domain.Common;

namespace Helsi.TaskManagement.Domain.Entities;

public class TaskList : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string OwnerId { get; private set; }
    public List<string> ConnectedUserIds { get; private set; } = new();
    
    public TaskList(string name, string ownerId)
    {
        SetName(name);
        OwnerId = ownerId ?? throw new ArgumentNullException(nameof(ownerId));
    }

    public void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Task list name cannot be empty", nameof(name));
        
        if (name.Length > 255)
            throw new ArgumentException("Task list name cannot exceed 255 characters", nameof(name));

        Name = name.Trim();
        UpdateTimestamp();
    }

    public void AddConnection(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User ID cannot be empty", nameof(userId));

        if (userId == OwnerId)
            throw new InvalidOperationException("Owner cannot be added as a connection");

        if (!ConnectedUserIds.Contains(userId))
        {
            ConnectedUserIds.Add(userId);
            UpdateTimestamp();
        }
    }

    public void RemoveConnection(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User ID cannot be empty", nameof(userId));

        if (ConnectedUserIds.Remove(userId))
        {
            UpdateTimestamp();
        }
    }

    public bool HasAccess(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return false;

        return userId == OwnerId || ConnectedUserIds.Contains(userId);
    }

    public bool IsOwner(string userId)
    {
        return !string.IsNullOrWhiteSpace(userId) && userId == OwnerId;
    }
}
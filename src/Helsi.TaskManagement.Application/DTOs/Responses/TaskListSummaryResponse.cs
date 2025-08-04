namespace Helsi.TaskManagement.Application.DTOs.Responses;

public class TaskListSummaryResponse
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string OwnerId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
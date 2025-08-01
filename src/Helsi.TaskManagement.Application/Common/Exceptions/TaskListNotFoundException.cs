namespace Helsi.TaskManagement.Application.Common.Exceptions;

public class TaskListNotFoundException(string taskListId) : Exception($"Task list with ID '{taskListId}' was not found.");
namespace Helsi.TaskManagement.Application.Common.Exceptions;

public class UnauthorizedAccessException(string userId, string operation)
    : Exception($"User '{userId}' is not authorized to perform '{operation}' operation.");
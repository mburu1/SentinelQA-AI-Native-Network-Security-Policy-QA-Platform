namespace SentinelQA.Application.Common;

public sealed class NotFoundException(string message) : Exception(message);
public sealed class ConflictException(string message) : Exception(message);
public sealed class ForbiddenException(string message = "You are not authorized to perform this action.") : Exception(message);
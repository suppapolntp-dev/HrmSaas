namespace HrmSaas.Application.Common.Exceptions;

public class AppException : Exception
{
    public int StatusCode { get; }
    public string Title { get; }

    public AppException(int statusCode, string title, string message) : base(message)
    {
        StatusCode = statusCode;
        Title = title;
    }
}

public sealed class UnauthorizedException : AppException
{
    public UnauthorizedException(string message = "Authentication is required.")
        : base(401, "Unauthorized", message) { }
}

public sealed class ForbiddenException : AppException
{
    public ForbiddenException(string message = "You do not have permission to perform this action.")
        : base(403, "Forbidden", message) { }
}

public sealed class NotFoundException : AppException
{
    public NotFoundException(string message)
        : base(404, "Not Found", message) { }
}

public sealed class ConflictException : AppException
{
    public ConflictException(string message)
        : base(409, "Conflict", message) { }
}

public sealed class ValidationAppException : AppException
{
    public IDictionary<string, string[]> Errors { get; }

    public ValidationAppException(IDictionary<string, string[]> errors, string message = "Validation failed.")
        : base(400, "Bad Request", message)
    {
        Errors = errors;
    }
}

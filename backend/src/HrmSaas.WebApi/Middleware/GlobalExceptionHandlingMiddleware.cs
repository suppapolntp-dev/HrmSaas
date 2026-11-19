using System.Text.Json;
using HrmSaas.Application.Common.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HrmSaas.WebApi.Middleware;

public sealed class GlobalExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public GlobalExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlingMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        int statusCode;
        string title;
        string detail;
        IDictionary<string, string[]>? validationErrors = null;

        if (exception is ValidationAppException validationEx)
        {
            statusCode = validationEx.StatusCode;
            title = validationEx.Title;
            detail = validationEx.Message;
            validationErrors = validationEx.Errors;
        }
        else if (exception is AppException appEx)
        {
            statusCode = appEx.StatusCode;
            title = appEx.Title;
            detail = appEx.Message;
        }
        else if (exception is DbUpdateConcurrencyException)
        {
            statusCode = StatusCodes.Status409Conflict;
            title = "Conflict";
            detail = "The record was modified by another process. Please reload and try again.";
        }
        else
        {
            _logger.LogError(exception, "Unhandled exception on {Method} {Path}", context.Request.Method, context.Request.Path);
            statusCode = StatusCodes.Status500InternalServerError;
            title = "Internal Server Error";
            detail = _environment.IsDevelopment()
                ? exception.Message
                : "An unexpected error occurred. Please contact support with the traceId.";
        }

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path
        };

        problem.Extensions["traceId"] = context.TraceIdentifier;

        if (validationErrors is not null)
        {
            problem.Extensions["errors"] = validationErrors;
        }

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        string json = JsonSerializer.Serialize(problem);
        await context.Response.WriteAsync(json);
    }
}

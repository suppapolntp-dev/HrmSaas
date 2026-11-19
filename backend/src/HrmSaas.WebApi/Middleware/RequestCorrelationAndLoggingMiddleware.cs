using System.Diagnostics;
using System.Net;
using HrmSaas.Infrastructure.Security;

namespace HrmSaas.WebApi.Middleware;

public sealed class RequestCorrelationAndLoggingMiddleware
{
    private const string CorrelationHeader = "X-Correlation-Id";

    private readonly RequestDelegate _next;
    private readonly ILogger<RequestCorrelationAndLoggingMiddleware> _logger;

    public RequestCorrelationAndLoggingMiddleware(
        RequestDelegate next,
        ILogger<RequestCorrelationAndLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, TenantContext tenantContext)
    {
        string correlationId = context.Request.Headers.TryGetValue(CorrelationHeader, out var headerVal) &&
                               !string.IsNullOrWhiteSpace(headerVal.ToString())
            ? headerVal.ToString().Trim()
            : Guid.NewGuid().ToString("N");

        context.TraceIdentifier = correlationId;
        context.Response.Headers[CorrelationHeader] = correlationId;

        IPAddress? clientIp = context.Connection.RemoteIpAddress;
        tenantContext.SetCorrelation(clientIp, correlationId);

        var sw = Stopwatch.StartNew();
        try
        {
            await _next(context);
        }
        finally
        {
            sw.Stop();
            _logger.LogInformation(
                "HTTP {Method} {Path} responded {StatusCode} in {ElapsedMs}ms (CorrelationId: {CorrelationId})",
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                sw.ElapsedMilliseconds,
                correlationId);
        }
    }
}

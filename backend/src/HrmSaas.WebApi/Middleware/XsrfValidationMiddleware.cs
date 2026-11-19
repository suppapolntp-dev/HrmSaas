using System.Security.Cryptography;
using System.Text;
using HrmSaas.Application.Common.Exceptions;
using HrmSessionOptions = HrmSaas.Infrastructure.Options.SessionOptions;
using Microsoft.Extensions.Options;

namespace HrmSaas.WebApi.Middleware;

public sealed class XsrfValidationMiddleware
{
    private const string XsrfHeaderName = "X-XSRF-TOKEN";

    private readonly RequestDelegate _next;
    private readonly HrmSessionOptions _sessionOptions;

    public XsrfValidationMiddleware(RequestDelegate next, IOptions<HrmSessionOptions> sessionOptions)
    {
        _next = next;
        _sessionOptions = sessionOptions.Value;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        string method = context.Request.Method;
        bool isStateChanging =
            HttpMethods.IsPost(method) ||
            HttpMethods.IsPut(method) ||
            HttpMethods.IsPatch(method) ||
            HttpMethods.IsDelete(method);

        if (!isStateChanging || context.Request.Path.StartsWithSegments("/api/v1/auth/login"))
        {
            await _next(context);
            return;
        }

        if (context.User.Identity?.IsAuthenticated != true)
        {
            await _next(context);
            return;
        }

        if (!context.Request.Cookies.TryGetValue(_sessionOptions.XsrfCookieName, out string? cookieToken) ||
            !context.Request.Headers.TryGetValue(XsrfHeaderName, out var headerValues))
        {
            throw new ForbiddenException("Missing CSRF token.");
        }

        string headerToken = headerValues.ToString();
        if (string.IsNullOrWhiteSpace(cookieToken) ||
            string.IsNullOrWhiteSpace(headerToken) ||
            !FixedTimeEquals(cookieToken, headerToken))
        {
            throw new ForbiddenException("Invalid CSRF token.");
        }

        await _next(context);
    }

    private static bool FixedTimeEquals(string left, string right)
    {
        byte[] leftBytes = Encoding.UTF8.GetBytes(left);
        byte[] rightBytes = Encoding.UTF8.GetBytes(right);
        return CryptographicOperations.FixedTimeEquals(leftBytes, rightBytes);
    }
}

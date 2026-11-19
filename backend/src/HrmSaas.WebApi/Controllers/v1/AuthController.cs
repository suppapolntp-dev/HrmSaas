using System.Net;
using System.Security.Cryptography;
using HrmSaas.Application.Common.Exceptions;
using HrmSaas.Application.Common.Interfaces;
using HrmSaas.Application.Features.Auth;
using HrmSessionOptions = HrmSaas.Infrastructure.Options.SessionOptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;

namespace HrmSaas.WebApi.Controllers.v1;

[ApiController]
[Route("api/v1/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly AuthService _authService;
    private readonly ITenantContext _tenantContext;
    private readonly HrmSessionOptions _sessionOptions;

    public AuthController(
        AuthService authService,
        ITenantContext tenantContext,
        IOptions<HrmSessionOptions> sessionOptions)
    {
        _authService = authService;
        _tenantContext = tenantContext;
        _sessionOptions = sessionOptions.Value;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("login")]
    public async Task<ActionResult<AuthMeResponse>> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        IPAddress clientIp = HttpContext.Connection.RemoteIpAddress ?? IPAddress.Loopback;
        string userAgent = Request.Headers.UserAgent.ToString();

        LoginOutcome outcome = await _authService.LoginAsync(request, clientIp, userAgent, cancellationToken);

        AppendSessionCookies(outcome.RawSessionToken, outcome.ExpiresAt);
        return Ok(outcome.User);
    }

    [HttpPost("register")]
    [AllowAnonymous]
    [EnableRateLimiting("login")]
    public async Task<ActionResult<AuthMeResponse>> Register(
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken)
    {
        IPAddress clientIp = HttpContext.Connection.RemoteIpAddress ?? IPAddress.Loopback;
        string userAgent = Request.Headers.UserAgent.ToString();

        LoginOutcome outcome = await _authService.RegisterAsync(request, clientIp, userAgent, cancellationToken);

        AppendSessionCookies(outcome.RawSessionToken, outcome.ExpiresAt);
        return StatusCode(StatusCodes.Status201Created, outcome.User);
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        if (Request.Cookies.TryGetValue(_sessionOptions.CookieName, out string? rawToken) &&
            !string.IsNullOrWhiteSpace(rawToken))
        {
            await _authService.LogoutAsync(rawToken, cancellationToken);
        }

        DeleteSessionCookies();
        return NoContent();
    }

    [HttpPost("switch-context")]
    [Authorize]
    public async Task<ActionResult<AuthMeResponse>> SwitchContext(
        [FromBody] SwitchContextRequest request,
        CancellationToken cancellationToken)
    {
        string rawToken = GetRequiredSessionCookie();
        (Guid userId, Guid tenantId) = GetRequiredUserAndTenant();

        AuthMeResponse updated = await _authService.SwitchContextAsync(
            rawToken,
            userId,
            tenantId,
            request,
            cancellationToken);

        return Ok(updated);
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<AuthMeResponse>> Me(CancellationToken cancellationToken)
    {
        (Guid userId, Guid tenantId) = GetRequiredUserAndTenant();

        if (!Request.Cookies.ContainsKey(_sessionOptions.XsrfCookieName))
        {
            AppendXsrfCookie(DateTimeOffset.UtcNow.AddHours(_sessionOptions.AbsoluteExpirationHours));
        }

        AuthMeResponse me = await _authService.GetMeAsync(
            userId,
            tenantId,
            _tenantContext.ActiveCompanyId,
            _tenantContext.ActiveBranchId,
            cancellationToken);

        return Ok(me);
    }

    [HttpPost("refresh-session")]
    [Authorize]
    public async Task<IActionResult> RefreshSession(CancellationToken cancellationToken)
    {
        string rawToken = GetRequiredSessionCookie();
        DateTimeOffset nextExpiry = await _authService.RefreshSessionAsync(rawToken, cancellationToken);

        AppendSessionCookies(rawToken, nextExpiry);
        return Ok(new { expiresAt = nextExpiry });
    }

    private string GetRequiredSessionCookie()
    {
        if (!Request.Cookies.TryGetValue(_sessionOptions.CookieName, out string? rawToken) ||
            string.IsNullOrWhiteSpace(rawToken))
        {
            throw new UnauthorizedException("Session cookie is missing.");
        }

        return rawToken;
    }

    private (Guid UserId, Guid TenantId) GetRequiredUserAndTenant()
    {
        if (!_tenantContext.CurrentUserId.HasValue || !_tenantContext.TenantId.HasValue)
        {
            throw new UnauthorizedException("Authenticated user context is missing.");
        }

        return (_tenantContext.CurrentUserId.Value, _tenantContext.TenantId.Value);
    }

    private void AppendSessionCookies(string rawSessionToken, DateTimeOffset expiresAt)
    {
        bool isSecure = _sessionOptions.RequireSecureCookie || Request.IsHttps;

        Response.Cookies.Append(_sessionOptions.CookieName, rawSessionToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = isSecure,
            SameSite = SameSiteMode.Strict,
            Path = "/",
            Expires = expiresAt
        });

        AppendXsrfCookie(expiresAt);
    }

    private void AppendXsrfCookie(DateTimeOffset expiresAt)
    {
        bool isSecure = _sessionOptions.RequireSecureCookie || Request.IsHttps;
        string xsrfToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');

        Response.Cookies.Append(_sessionOptions.XsrfCookieName, xsrfToken, new CookieOptions
        {
            HttpOnly = false,
            Secure = isSecure,
            SameSite = SameSiteMode.Strict,
            Path = "/",
            Expires = expiresAt
        });
    }

    private void DeleteSessionCookies()
    {
        bool isSecure = _sessionOptions.RequireSecureCookie || Request.IsHttps;
        var expiredOptions = new CookieOptions
        {
            Secure = isSecure,
            SameSite = SameSiteMode.Strict,
            Path = "/"
        };

        Response.Cookies.Delete(_sessionOptions.CookieName, expiredOptions);
        Response.Cookies.Delete(_sessionOptions.XsrfCookieName, expiredOptions);
    }
}

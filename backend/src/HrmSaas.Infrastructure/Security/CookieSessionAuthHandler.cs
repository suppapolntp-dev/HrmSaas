using System.Security.Claims;
using System.Text.Encodings.Web;
using HrmSaas.Application.Common.Interfaces;
using HrmSaas.Application.Features.Auth;
using HrmSaas.Infrastructure.Options;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HrmSaas.Infrastructure.Security;

public static class SessionAuthDefaults
{
    public const string AuthenticationScheme = "HrmCookieSession";
}

public sealed class CookieSessionAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly ISessionStore _sessionStore;
    private readonly SessionOptions _sessionOptions;

    public CookieSessionAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISessionStore sessionStore,
        IOptions<SessionOptions> sessionOptions)
        : base(options, logger, encoder)
    {
        _sessionStore = sessionStore;
        _sessionOptions = sessionOptions.Value;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Cookies.TryGetValue(_sessionOptions.CookieName, out string? rawToken) ||
            string.IsNullOrWhiteSpace(rawToken))
        {
            return AuthenticateResult.NoResult();
        }

        string tokenHash = AuthService.ComputeSha256Hex(rawToken);
        SessionData? session = await _sessionStore.GetByTokenHashAsync(tokenHash, Context.RequestAborted);
        if (session is null || session.IsRevoked || session.ExpiresAt <= DateTimeOffset.UtcNow)
        {
            return AuthenticateResult.Fail("Invalid or expired session.");
        }

        await _sessionStore.TouchIfNeededAsync(tokenHash, session, Context.RequestAborted);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, session.UserId.ToString()),
            new("tenant_id", session.TenantId.ToString()),
            new("session_id", session.SessionId.ToString())
        };

        if (session.ActiveCompanyId.HasValue)
        {
            claims.Add(new Claim("company_id", session.ActiveCompanyId.Value.ToString()));
        }

        if (session.ActiveBranchId.HasValue)
        {
            claims.Add(new Claim("branch_id", session.ActiveBranchId.Value.ToString()));
        }

        foreach (string permission in session.Permissions)
        {
            claims.Add(new Claim("permission", permission));
        }

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return AuthenticateResult.Success(ticket);
    }
}

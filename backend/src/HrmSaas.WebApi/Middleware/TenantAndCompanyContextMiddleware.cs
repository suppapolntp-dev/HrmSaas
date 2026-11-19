using System.Security.Claims;
using HrmSaas.Infrastructure.Security;

namespace HrmSaas.WebApi.Middleware;

public sealed class TenantAndCompanyContextMiddleware
{
    private readonly RequestDelegate _next;

    public TenantAndCompanyContextMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, TenantContext tenantContext)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            Guid? tenantId = TryParseGuidClaim(context.User, "tenant_id");
            Guid? userId = TryParseGuidClaim(context.User, ClaimTypes.NameIdentifier);
            Guid? companyId = TryParseGuidClaim(context.User, "company_id");
            Guid? branchId = TryParseGuidClaim(context.User, "branch_id");

            if (tenantId.HasValue)
            {
                tenantContext.SetContext(
                    tenantId.Value,
                    companyId,
                    branchId,
                    userId,
                    context.Connection.RemoteIpAddress,
                    tenantContext.CorrelationId);
            }
        }

        await _next(context);
    }

    private static Guid? TryParseGuidClaim(ClaimsPrincipal user, string claimType)
    {
        string? raw = user.FindFirstValue(claimType);
        if (Guid.TryParse(raw, out Guid parsed))
        {
            return parsed;
        }

        return null;
    }
}

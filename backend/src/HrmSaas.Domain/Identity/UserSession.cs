using System.Net;
using HrmSaas.Domain.Common;

namespace HrmSaas.Domain.Identity;

public sealed class UserSession : BaseEntity, ITenantScoped
{
    private UserSession()
    {
    }

    public Guid UserId { get; private set; }
    public Guid TenantId { get; private set; }
    public string SessionTokenHash { get; private set; } = string.Empty;
    public Guid? ActiveCompanyId { get; private set; }
    public Guid? ActiveBranchId { get; private set; }
    public IPAddress IpAddress { get; private set; } = IPAddress.Loopback;
    public string UserAgent { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset LastAccessedAt { get; private set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset ExpiresAt { get; private set; }
    public bool IsRevoked { get; private set; }

    public static UserSession Create(
        Guid userId,
        Guid tenantId,
        string sessionTokenHash,
        Guid? activeCompanyId,
        Guid? activeBranchId,
        IPAddress ipAddress,
        string userAgent,
        DateTimeOffset utcNow,
        DateTimeOffset expiresAt,
        Guid? id = null)
    {
        if (userId == Guid.Empty || tenantId == Guid.Empty)
        {
            throw new ArgumentException("UserId and TenantId are required.");
        }

        if (string.IsNullOrWhiteSpace(sessionTokenHash))
        {
            throw new ArgumentException("SessionTokenHash is required.", nameof(sessionTokenHash));
        }

        var session = new UserSession
        {
            UserId = userId,
            TenantId = tenantId,
            SessionTokenHash = sessionTokenHash,
            ActiveCompanyId = activeCompanyId,
            ActiveBranchId = activeBranchId,
            IpAddress = ipAddress,
            UserAgent = userAgent ?? string.Empty,
            CreatedAt = utcNow,
            LastAccessedAt = utcNow,
            ExpiresAt = expiresAt,
            IsRevoked = false
        };

        if (id.HasValue)
        {
            session.Id = id.Value;
        }

        return session;
    }

    public void SwitchContext(Guid companyId, Guid? branchId, DateTimeOffset utcNow)
    {
        if (IsRevoked)
        {
            throw new InvalidOperationException("Cannot switch context on a revoked session.");
        }

        ActiveCompanyId = companyId;
        ActiveBranchId = branchId;
        LastAccessedAt = utcNow;
    }

    public void Touch(DateTimeOffset utcNow, DateTimeOffset newExpiresAt)
    {
        LastAccessedAt = utcNow;
        ExpiresAt = newExpiresAt;
    }

    public void Revoke()
    {
        IsRevoked = true;
    }
}

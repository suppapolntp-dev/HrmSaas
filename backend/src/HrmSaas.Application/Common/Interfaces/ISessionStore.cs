using System.Net;

namespace HrmSaas.Application.Common.Interfaces;

public sealed class SessionData
{
    public Guid SessionId { get; init; }
    public Guid UserId { get; init; }
    public Guid TenantId { get; init; }
    public Guid? ActiveCompanyId { get; init; }
    public Guid? ActiveBranchId { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset LastAccessedAt { get; init; }
    public DateTimeOffset ExpiresAt { get; init; }
    public bool IsRevoked { get; init; }
    public List<string> Permissions { get; init; } = [];
}

public interface ISessionStore
{
    Task<SessionData?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default);
    Task<SessionData> CreateAsync(
        Guid userId,
        Guid tenantId,
        string tokenHash,
        Guid? activeCompanyId,
        Guid? activeBranchId,
        IPAddress ipAddress,
        string userAgent,
        CancellationToken cancellationToken = default);
    Task TouchIfNeededAsync(string tokenHash, SessionData session, CancellationToken cancellationToken = default);
    Task<SessionData?> RefreshAsync(string tokenHash, CancellationToken cancellationToken = default);
    Task<SessionData?> SwitchContextAsync(
        string tokenHash,
        Guid companyId,
        Guid? branchId,
        CancellationToken cancellationToken = default);
    Task RevokeAsync(string tokenHash, CancellationToken cancellationToken = default);
}

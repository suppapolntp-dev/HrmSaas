using System.Net;
using System.Text.Json;
using HrmSaas.Application.Common.Interfaces;
using HrmSaas.Domain.Identity;
using HrmSaas.Infrastructure.Options;
using HrmSaas.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;

namespace HrmSaas.Infrastructure.Security;

public sealed class DistributedSessionStore : ISessionStore
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(5);

    private readonly AppDbContext _dbContext;
    private readonly IDistributedCache _cache;
    private readonly SessionOptions _sessionOptions;
    private readonly ITenantContext _tenantContext;

    public DistributedSessionStore(
        AppDbContext dbContext,
        IDistributedCache cache,
        IOptions<SessionOptions> sessionOptions,
        ITenantContext tenantContext)
    {
        _dbContext = dbContext;
        _cache = cache;
        _sessionOptions = sessionOptions.Value;
        _tenantContext = tenantContext;
    }

    public async Task<SessionData?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tokenHash))
        {
            return null;
        }

        string cacheKey = BuildCacheKey(tokenHash);
        string? cachedJson = await _cache.GetStringAsync(cacheKey, cancellationToken);
        if (!string.IsNullOrWhiteSpace(cachedJson))
        {
            SessionData? cached = JsonSerializer.Deserialize<SessionData>(cachedJson);
            if (cached is not null)
            {
                if (cached.IsRevoked || cached.ExpiresAt <= DateTimeOffset.UtcNow)
                {
                    await _cache.RemoveAsync(cacheKey, cancellationToken);
                    return null;
                }

                return cached;
            }
        }

        UserSession? entity = await FindSessionByTokenHashAsync(tokenHash, asNoTracking: true, cancellationToken);

        if (entity is null || entity.IsRevoked || entity.ExpiresAt <= DateTimeOffset.UtcNow)
        {
            return null;
        }

        EnsureTenantContext(entity.TenantId, entity.UserId);

        List<string> permissions = await LoadPermissionsAsync(
            entity.TenantId,
            entity.UserId,
            entity.ActiveCompanyId,
            cancellationToken);

        SessionData data = MapToSessionData(entity, permissions);
        await CacheSessionAsync(cacheKey, data, cancellationToken);
        return data;
    }

    public async Task<SessionData> CreateAsync(
        Guid userId,
        Guid tenantId,
        string tokenHash,
        Guid? activeCompanyId,
        Guid? activeBranchId,
        IPAddress ipAddress,
        string userAgent,
        CancellationToken cancellationToken = default)
    {
        DateTimeOffset utcNow = DateTimeOffset.UtcNow;
        DateTimeOffset expiresAt = utcNow.AddMinutes(_sessionOptions.SlidingExpirationMinutes);

        var session = UserSession.Create(
            userId,
            tenantId,
            tokenHash,
            activeCompanyId,
            activeBranchId,
            ipAddress,
            userAgent,
            utcNow,
            expiresAt);

        _dbContext.UserSessions.Add(session);
        await _dbContext.SaveChangesAsync(cancellationToken);

        List<string> permissions = await LoadPermissionsAsync(
            tenantId,
            userId,
            activeCompanyId,
            cancellationToken);

        SessionData data = MapToSessionData(session, permissions);
        await CacheSessionAsync(BuildCacheKey(tokenHash), data, cancellationToken);
        return data;
    }

    public async Task TouchIfNeededAsync(
        string tokenHash,
        SessionData session,
        CancellationToken cancellationToken = default)
    {
        DateTimeOffset utcNow = DateTimeOffset.UtcNow;
        TimeSpan elapsed = utcNow - session.LastAccessedAt;
        if (elapsed.TotalSeconds < _sessionOptions.LastAccessedUpdateThresholdSeconds)
        {
            return;
        }

        DateTimeOffset absoluteLimit = session.CreatedAt.AddHours(_sessionOptions.AbsoluteExpirationHours);
        if (utcNow >= absoluteLimit)
        {
            return;
        }

        DateTimeOffset slidingExpiry = utcNow.AddMinutes(_sessionOptions.SlidingExpirationMinutes);
        DateTimeOffset nextExpiry = slidingExpiry < absoluteLimit ? slidingExpiry : absoluteLimit;

        EnsureTenantContext(session.TenantId, session.UserId);

        UserSession? entity = await _dbContext.UserSessions
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == session.SessionId, cancellationToken);

        if (entity is null || entity.IsRevoked)
        {
            return;
        }

        entity.Touch(utcNow, nextExpiry);
        await _dbContext.SaveChangesAsync(cancellationToken);

        SessionData updated = new()
        {
            SessionId = session.SessionId,
            UserId = session.UserId,
            TenantId = session.TenantId,
            ActiveCompanyId = session.ActiveCompanyId,
            ActiveBranchId = session.ActiveBranchId,
            CreatedAt = session.CreatedAt,
            LastAccessedAt = utcNow,
            ExpiresAt = nextExpiry,
            IsRevoked = false,
            Permissions = session.Permissions
        };

        await CacheSessionAsync(BuildCacheKey(tokenHash), updated, cancellationToken);
    }

    public async Task<SessionData?> RefreshAsync(string tokenHash, CancellationToken cancellationToken = default)
    {
        UserSession? entity = await FindSessionByTokenHashAsync(tokenHash, asNoTracking: false, cancellationToken);

        DateTimeOffset utcNow = DateTimeOffset.UtcNow;
        if (entity is null || entity.IsRevoked || entity.ExpiresAt <= utcNow)
        {
            return null;
        }

        EnsureTenantContext(entity.TenantId, entity.UserId);

        DateTimeOffset absoluteLimit = entity.CreatedAt.AddHours(_sessionOptions.AbsoluteExpirationHours);
        if (utcNow >= absoluteLimit)
        {
            entity.Revoke();
            await _dbContext.SaveChangesAsync(cancellationToken);
            await _cache.RemoveAsync(BuildCacheKey(tokenHash), cancellationToken);
            return null;
        }

        DateTimeOffset slidingExpiry = utcNow.AddMinutes(_sessionOptions.SlidingExpirationMinutes);
        DateTimeOffset nextExpiry = slidingExpiry < absoluteLimit ? slidingExpiry : absoluteLimit;

        entity.Touch(utcNow, nextExpiry);
        await _dbContext.SaveChangesAsync(cancellationToken);

        List<string> permissions = await LoadPermissionsAsync(
            entity.TenantId,
            entity.UserId,
            entity.ActiveCompanyId,
            cancellationToken);

        SessionData data = MapToSessionData(entity, permissions);
        await CacheSessionAsync(BuildCacheKey(tokenHash), data, cancellationToken);
        return data;
    }

    public async Task<SessionData?> SwitchContextAsync(
        string tokenHash,
        Guid companyId,
        Guid? branchId,
        CancellationToken cancellationToken = default)
    {
        UserSession? entity = await FindSessionByTokenHashAsync(tokenHash, asNoTracking: false, cancellationToken);

        if (entity is null || entity.IsRevoked || entity.ExpiresAt <= DateTimeOffset.UtcNow)
        {
            return null;
        }

        EnsureTenantContext(entity.TenantId, entity.UserId);

        entity.SwitchContext(companyId, branchId, DateTimeOffset.UtcNow);
        await _dbContext.SaveChangesAsync(cancellationToken);

        List<string> permissions = await LoadPermissionsAsync(
            entity.TenantId,
            entity.UserId,
            companyId,
            cancellationToken);

        SessionData data = MapToSessionData(entity, permissions);
        await CacheSessionAsync(BuildCacheKey(tokenHash), data, cancellationToken);
        return data;
    }

    public async Task RevokeAsync(string tokenHash, CancellationToken cancellationToken = default)
    {
        await _cache.RemoveAsync(BuildCacheKey(tokenHash), cancellationToken);

        UserSession? entity = await FindSessionByTokenHashAsync(tokenHash, asNoTracking: false, cancellationToken);

        if (entity is null || entity.IsRevoked)
        {
            return;
        }

        EnsureTenantContext(entity.TenantId, entity.UserId);

        entity.Revoke();
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<UserSession?> FindSessionByTokenHashAsync(
        string tokenHash,
        bool asNoTracking,
        CancellationToken cancellationToken)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        await _dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"SELECT set_config('app.auth_lookup_token_hash', {tokenHash}, true);",
            cancellationToken);

        IQueryable<UserSession> query = _dbContext.UserSessions.IgnoreQueryFilters();
        if (asNoTracking)
        {
            query = query.AsNoTracking();
        }

        UserSession? entity = await query.FirstOrDefaultAsync(x => x.SessionTokenHash == tokenHash, cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return entity;
    }

    private void EnsureTenantContext(Guid tenantId, Guid userId)
    {
        if (!_tenantContext.TenantId.HasValue)
        {
            _tenantContext.SetTenant(tenantId, userId);
        }
    }

    private async Task<List<string>> LoadPermissionsAsync(
        Guid tenantId,
        Guid userId,
        Guid? activeCompanyId,
        CancellationToken cancellationToken)
    {
        if (!activeCompanyId.HasValue)
        {
            return [];
        }

        List<Guid> roleIds = await _dbContext.UserCompanyRoles
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.UserId == userId && x.CompanyId == activeCompanyId.Value)
            .Select(x => x.RoleId)
            .ToListAsync(cancellationToken);

        if (roleIds.Count == 0)
        {
            return [];
        }

        return await _dbContext.RolePermissions
            .AsNoTracking()
            .Where(rp => roleIds.Contains(rp.RoleId))
            .Join(
                _dbContext.Permissions.AsNoTracking(),
                rp => rp.PermissionId,
                p => p.Id,
                (rp, p) => p.Code)
            .Distinct()
            .ToListAsync(cancellationToken);
    }

    private static SessionData MapToSessionData(UserSession entity, List<string> permissions)
    {
        return new SessionData
        {
            SessionId = entity.Id,
            UserId = entity.UserId,
            TenantId = entity.TenantId,
            ActiveCompanyId = entity.ActiveCompanyId,
            ActiveBranchId = entity.ActiveBranchId,
            CreatedAt = entity.CreatedAt,
            LastAccessedAt = entity.LastAccessedAt,
            ExpiresAt = entity.ExpiresAt,
            IsRevoked = entity.IsRevoked,
            Permissions = permissions
        };
    }

    private static async Task CacheSessionAsync(
        IDistributedCache cache,
        string cacheKey,
        SessionData data,
        CancellationToken cancellationToken)
    {
        string json = JsonSerializer.Serialize(data);
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = CacheTtl
        };

        await cache.SetStringAsync(cacheKey, json, options, cancellationToken);
    }

    private Task CacheSessionAsync(string cacheKey, SessionData data, CancellationToken cancellationToken)
        => CacheSessionAsync(_cache, cacheKey, data, cancellationToken);

    private static string BuildCacheKey(string tokenHash) => $"hrm:session:{tokenHash}";
}

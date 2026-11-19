using HrmSaas.Application.Common.Exceptions;
using HrmSaas.Application.Common.Interfaces;
using HrmSaas.Application.Common.Models;
using HrmSaas.Domain.Audit;
using HrmSaas.Domain.Identity;
using Microsoft.EntityFrameworkCore;

namespace HrmSaas.Application.Features.Settings;

public sealed class RbacAndAuditService
{
    private readonly IAppDbContext _dbContext;
    private readonly ITenantContext _tenantContext;

    public RbacAndAuditService(IAppDbContext dbContext, ITenantContext tenantContext)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
    }

    public async Task<IReadOnlyList<PermissionDto>> GetPermissionsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Permissions
            .AsNoTracking()
            .OrderBy(p => p.Module)
            .ThenBy(p => p.Code)
            .Select(p => new PermissionDto(p.Id, p.Code, p.Module, p.Description))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<RoleDto>> GetRolesAsync(CancellationToken cancellationToken = default)
    {
        List<Role> roles = await _dbContext.Roles
            .AsNoTracking()
            .OrderBy(r => r.Code)
            .ToListAsync(cancellationToken);

        List<Guid> roleIds = roles.Select(r => r.Id).ToList();
        var rolePerms = await _dbContext.RolePermissions
            .AsNoTracking()
            .Where(rp => roleIds.Contains(rp.RoleId))
            .Join(
                _dbContext.Permissions.AsNoTracking(),
                rp => rp.PermissionId,
                p => p.Id,
                (rp, p) => new { rp.RoleId, p.Code })
            .ToListAsync(cancellationToken);

        var result = new List<RoleDto>();
        foreach (Role role in roles)
        {
            var codes = new List<string>();
            foreach (var rp in rolePerms)
            {
                if (rp.RoleId == role.Id)
                {
                    codes.Add(rp.Code);
                }
            }

            result.Add(new RoleDto(
                role.Id,
                role.CompanyId,
                role.Code,
                role.Name,
                role.IsSystemRole,
                codes));
        }

        return result;
    }

    public async Task<RoleDto> CreateRoleAsync(CreateRoleRequest request, CancellationToken cancellationToken = default)
    {
        Guid tenantId = _tenantContext.TenantId
            ?? throw new UnauthorizedException("Tenant context is required.");
        Guid? companyId = _tenantContext.ActiveCompanyId;
        string normalizedCode = request.Code.Trim().ToUpperInvariant();

        bool duplicate = await _dbContext.Roles
            .AnyAsync(r => r.Code == normalizedCode && r.CompanyId == companyId, cancellationToken);

        if (duplicate)
        {
            throw new ConflictException($"Role code '{normalizedCode}' already exists.");
        }

        var role = Role.Create(tenantId, companyId, normalizedCode, request.Name, isSystemRole: false);
        _dbContext.Roles.Add(role);

        List<string> assignedCodes = await AssignPermissionsToRoleAsync(role.Id, request.PermissionCodes, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new RoleDto(role.Id, role.CompanyId, role.Code, role.Name, role.IsSystemRole, assignedCodes);
    }

    public async Task<RoleDto> UpdateRoleAsync(
        Guid roleId,
        UpdateRoleRequest request,
        CancellationToken cancellationToken = default)
    {
        Role role = await _dbContext.Roles
            .FirstOrDefaultAsync(r => r.Id == roleId, cancellationToken)
            ?? throw new NotFoundException("Role not found.");

        role.Rename(request.Name);

        List<RolePermission> existing = await _dbContext.RolePermissions
            .Where(rp => rp.RoleId == role.Id)
            .ToListAsync(cancellationToken);

        _dbContext.RolePermissions.RemoveRange(existing);

        List<string> assignedCodes = await AssignPermissionsToRoleAsync(role.Id, request.PermissionCodes, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new RoleDto(role.Id, role.CompanyId, role.Code, role.Name, role.IsSystemRole, assignedCodes);
    }

    public async Task<PagedResult<AuditLogDto>> GetAuditLogsAsync(
        AuditLogQuery query,
        CancellationToken cancellationToken = default)
    {
        int page = query.Page < 1 ? 1 : query.Page;
        int pageSize = query.PageSize is < 1 or > 100 ? 20 : query.PageSize;

        var baseQuery = _dbContext.AuditLogs.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.EntityName))
        {
            string entity = query.EntityName.Trim();
            baseQuery = baseQuery.Where(a => a.EntityName == entity);
        }

        if (!string.IsNullOrWhiteSpace(query.Action))
        {
            string action = query.Action.Trim().ToUpperInvariant();
            baseQuery = baseQuery.Where(a => a.Action == action);
        }

        if (query.ActorUserId.HasValue)
        {
            baseQuery = baseQuery.Where(a => a.ActorUserId == query.ActorUserId.Value);
        }

        if (query.FromDate.HasValue)
        {
            baseQuery = baseQuery.Where(a => a.OccurredAt >= query.FromDate.Value);
        }

        if (query.ToDate.HasValue)
        {
            baseQuery = baseQuery.Where(a => a.OccurredAt <= query.ToDate.Value);
        }

        int totalCount = await baseQuery.CountAsync(cancellationToken);

        List<AuditLog> rows = await baseQuery
            .OrderByDescending(a => a.OccurredAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var items = new List<AuditLogDto>();
        foreach (AuditLog row in rows)
        {
            items.Add(new AuditLogDto(
                row.Id,
                row.CompanyId,
                row.ActorUserId,
                row.EntityName,
                row.EntityId,
                row.Action,
                row.OldValues,
                row.NewValues,
                row.IpAddress?.ToString(),
                row.CorrelationId,
                row.OccurredAt));
        }

        return new PagedResult<AuditLogDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    private async Task<List<string>> AssignPermissionsToRoleAsync(
        Guid roleId,
        IReadOnlyList<string> permissionCodes,
        CancellationToken cancellationToken)
    {
        List<string> normalized = permissionCodes
            .Where(c => !string.IsNullOrWhiteSpace(c))
            .Select(c => c.Trim().ToLowerInvariant())
            .Distinct()
            .ToList();

        List<Permission> matching = await _dbContext.Permissions
            .AsNoTracking()
            .Where(p => normalized.Contains(p.Code.ToLower()))
            .ToListAsync(cancellationToken);

        var assignedCodes = new List<string>();
        foreach (Permission perm in matching)
        {
            _dbContext.RolePermissions.Add(RolePermission.Create(roleId, perm.Id));
            assignedCodes.Add(perm.Code);
        }

        return assignedCodes;
    }
}

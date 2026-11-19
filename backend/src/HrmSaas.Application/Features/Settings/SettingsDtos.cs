namespace HrmSaas.Application.Features.Settings;

public sealed record PermissionDto(
    Guid Id,
    string Code,
    string Module,
    string Description);

public sealed record RoleDto(
    Guid Id,
    Guid? CompanyId,
    string Code,
    string Name,
    bool IsSystemRole,
    IReadOnlyList<string> PermissionCodes);

public sealed record CreateRoleRequest(
    string Code,
    string Name,
    IReadOnlyList<string> PermissionCodes);

public sealed record UpdateRoleRequest(
    string Name,
    IReadOnlyList<string> PermissionCodes);

public sealed record AuditLogQuery(
    int Page = 1,
    int PageSize = 20,
    string? EntityName = null,
    string? Action = null,
    Guid? ActorUserId = null,
    DateTimeOffset? FromDate = null,
    DateTimeOffset? ToDate = null);

public sealed record AuditLogDto(
    Guid Id,
    Guid? CompanyId,
    Guid? ActorUserId,
    string EntityName,
    Guid EntityId,
    string Action,
    string? OldValues,
    string? NewValues,
    string? IpAddress,
    string CorrelationId,
    DateTimeOffset OccurredAt);

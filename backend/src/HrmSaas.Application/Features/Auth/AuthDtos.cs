namespace HrmSaas.Application.Features.Auth;

public sealed record LoginRequest(string Email, string Password);

public sealed record RegisterRequest(string Name, string Email, string Password);

public sealed record SwitchContextRequest(Guid CompanyId, Guid? BranchId);

public sealed record AccessibleCompanyDto(
    Guid CompanyId,
    string Code,
    string LegalName,
    IReadOnlyList<AccessibleBranchDto> Branches);

public sealed record AccessibleBranchDto(
    Guid BranchId,
    string Code,
    string Name);

public sealed record AuthMeResponse(
    Guid UserId,
    Guid TenantId,
    string Email,
    Guid? EmployeeId,
    string? EmployeeCode,
    string? FullName,
    Guid? ActiveCompanyId,
    Guid? ActiveBranchId,
    IReadOnlyList<string> Permissions,
    IReadOnlyList<AccessibleCompanyDto> Companies);

public sealed record LoginOutcome(
    string RawSessionToken,
    DateTimeOffset ExpiresAt,
    AuthMeResponse User);

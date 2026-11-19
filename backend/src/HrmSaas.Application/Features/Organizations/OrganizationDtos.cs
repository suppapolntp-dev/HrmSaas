namespace HrmSaas.Application.Features.Organizations;

public sealed record CompanyDto(
    Guid Id,
    string Code,
    string LegalName,
    string TaxId,
    string SsoEmployerCode,
    string BaseCurrency);

public sealed record CreateCompanyRequest(
    string Code,
    string LegalName,
    string TaxId,
    string SsoEmployerCode,
    string BaseCurrency = "THB");

public sealed record UpdateCompanyRequest(
    string LegalName,
    string TaxId,
    string SsoEmployerCode,
    string BaseCurrency);

public sealed record BranchDto(
    Guid Id,
    Guid CompanyId,
    string Code,
    string Name,
    string Timezone,
    decimal Latitude,
    decimal Longitude,
    int GeofenceRadiusMeters,
    string[] AllowedIps);

public sealed record CreateBranchRequest(
    string Code,
    string Name,
    string Timezone,
    decimal Latitude,
    decimal Longitude,
    int GeofenceRadiusMeters,
    string[]? AllowedIps);

public sealed record UpdateBranchGeofenceRequest(
    decimal Latitude,
    decimal Longitude,
    int GeofenceRadiusMeters,
    string[] AllowedIps);

public sealed record DepartmentDto(
    Guid Id,
    Guid CompanyId,
    Guid? ParentDepartmentId,
    string Code,
    string Name,
    Guid? HeadEmployeeId);

public sealed record DepartmentTreeNodeDto(
    Guid Id,
    string Code,
    string Name,
    Guid? HeadEmployeeId,
    List<DepartmentTreeNodeDto> Children);

public sealed record CreateDepartmentRequest(
    string Code,
    string Name,
    Guid? ParentDepartmentId,
    Guid? HeadEmployeeId);

public sealed record UpdateDepartmentRequest(
    string Name,
    Guid? ParentDepartmentId,
    Guid? HeadEmployeeId);

public sealed record PositionDto(
    Guid Id,
    Guid CompanyId,
    string Code,
    string Title);

public sealed record CreatePositionRequest(string Code, string Title);
public sealed record UpdatePositionRequest(string Title);

public sealed record JobGradeDto(
    Guid Id,
    Guid CompanyId,
    string Code,
    int LevelOrder,
    string Name);

public sealed record CreateJobGradeRequest(string Code, int LevelOrder, string Name);
public sealed record UpdateJobGradeRequest(int LevelOrder, string Name);

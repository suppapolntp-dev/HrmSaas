namespace HrmSaas.Application.Features.Employees;

public sealed record EmployeeListQuery(
    int Page = 1,
    int PageSize = 20,
    Guid? BranchId = null,
    Guid? DepartmentId = null,
    string? Status = null,
    string? Search = null,
    string? SortBy = null,
    bool SortDesc = false);

public sealed record EmployeeListItemDto(
    Guid Id,
    string EmployeeCode,
    string FirstName,
    string LastName,
    Guid BranchId,
    string BranchName,
    Guid DepartmentId,
    string DepartmentName,
    Guid PositionId,
    string PositionTitle,
    Guid JobGradeId,
    string JobGradeName,
    DateOnly HireDate,
    string EmploymentStatus);

public sealed record EmployeeContractDto(
    Guid Id,
    string ContractType,
    DateOnly EffectiveFrom,
    DateOnly? EffectiveTo,
    decimal BaseSalary,
    string PaymentCycle,
    bool IsActive);

public sealed record EmployeeReportingLineDto(
    Guid Id,
    Guid ManagerEmployeeId,
    string ManagerEmployeeCode,
    string ManagerFullName,
    string LineType,
    int PriorityOrder,
    DateOnly EffectiveFrom,
    DateOnly? EffectiveTo);

public sealed record EmployeeLeaveBalanceSummaryDto(
    Guid LeaveTypeId,
    string LeaveTypeCode,
    string LeaveTypeName,
    int QuotaYear,
    decimal CarriedOverDays,
    decimal EntitledDays,
    decimal UsedDays,
    decimal PendingDays,
    decimal RemainingDays);

public sealed record Employee360Dto(
    Guid Id,
    Guid CompanyId,
    Guid BranchId,
    string BranchName,
    Guid DepartmentId,
    string DepartmentName,
    Guid PositionId,
    string PositionTitle,
    Guid JobGradeId,
    string JobGradeName,
    Guid? UserId,
    string EmployeeCode,
    string FirstName,
    string LastName,
    DateOnly HireDate,
    DateOnly? ProbationEndDate,
    string EmploymentStatus,
    IReadOnlyList<EmployeeContractDto> Contracts,
    IReadOnlyList<EmployeeReportingLineDto> ReportingLines,
    IReadOnlyList<EmployeeLeaveBalanceSummaryDto> LeaveBalances);

public sealed record OnboardEmployeeRequest(
    string EmployeeCode,
    string FirstName,
    string LastName,
    Guid BranchId,
    Guid DepartmentId,
    Guid PositionId,
    Guid JobGradeId,
    DateOnly HireDate,
    DateOnly? ProbationEndDate,
    string EmploymentStatus,
    string ContractType,
    decimal BaseSalary,
    string PaymentCycle,
    string? Email,
    string? InitialPassword,
    Guid? RoleId);

public sealed record UpdateEmployeeRequest(
    Guid BranchId,
    Guid DepartmentId,
    Guid PositionId,
    Guid JobGradeId,
    string FirstName,
    string LastName,
    string EmploymentStatus);

public sealed record ReportingLineItemRequest(
    Guid ManagerEmployeeId,
    string LineType,
    int PriorityOrder,
    DateOnly EffectiveFrom,
    DateOnly? EffectiveTo);

public sealed record UpdateReportingLinesRequest(IReadOnlyList<ReportingLineItemRequest> Lines);

public sealed record CreateContractRequest(
    string ContractType,
    DateOnly EffectiveFrom,
    DateOnly? EffectiveTo,
    decimal BaseSalary,
    string PaymentCycle);

namespace HrmSaas.Application.Features.Workflows;

public sealed record LeavePolicyItemDto(
    Guid Id,
    Guid LeaveTypeId,
    string LeaveTypeCode,
    string LeaveTypeName,
    bool IsPaid,
    bool AllowHalfDay,
    bool AllowHourly,
    bool RequiresAttachment,
    Guid? JobGradeId,
    int MinTenureMonths,
    int? MaxTenureMonths,
    decimal AnnualQuotaDays,
    string ProrateRule,
    decimal MaxCarryOverDays);

public sealed record CreateLeavePolicyRequest(
    string LeaveTypeCode,
    string LeaveTypeName,
    bool IsPaid,
    bool AllowHalfDay,
    bool AllowHourly,
    bool RequiresAttachment,
    Guid? JobGradeId,
    int MinTenureMonths,
    int? MaxTenureMonths,
    decimal AnnualQuotaDays,
    string ProrateRule,
    decimal MaxCarryOverDays);

public sealed record UpdateLeavePolicyRequest(
    int MinTenureMonths,
    int? MaxTenureMonths,
    decimal AnnualQuotaDays,
    string ProrateRule,
    decimal MaxCarryOverDays);

public sealed record MyLeaveBalanceDto(
    Guid Id,
    Guid LeaveTypeId,
    string LeaveTypeCode,
    string LeaveTypeName,
    int QuotaYear,
    decimal CarriedOverDays,
    decimal EntitledDays,
    decimal UsedDays,
    decimal PendingDays,
    decimal RemainingDays);

public sealed record SubmitLeaveRequest(
    Guid LeaveTypeId,
    DateOnly StartDate,
    DateOnly EndDate,
    decimal DaysCount,
    string Reason);

public sealed record SubmitOvertimeRequest(
    DateOnly WorkDate,
    TimeOnly StartTime,
    TimeOnly EndTime,
    decimal Hours,
    string Reason);

public sealed record ApprovalRequestDto(
    Guid Id,
    Guid RequesterEmployeeId,
    string RequesterEmployeeCode,
    string RequesterFullName,
    string RequestType,
    Guid ReferenceId,
    int CurrentStepOrder,
    string Status,
    string PayloadSummary,
    DateTimeOffset SubmittedAt,
    DateTimeOffset? CompletedAt);

public sealed record DecideApprovalRequest(
    string Action,
    string? Comment);

public sealed record WorkflowStepDto(
    Guid Id,
    int StepOrder,
    string ApproverStrategy,
    Guid? SpecificRoleId,
    string? SpecificRoleName,
    Guid? SpecificEmployeeId,
    string? SpecificEmployeeName);

public sealed record WorkflowDefinitionDto(
    Guid Id,
    string RequestType,
    Guid? DepartmentId,
    string? DepartmentName,
    string Name,
    bool IsActive,
    IReadOnlyList<WorkflowStepDto> Steps);

public sealed record WorkflowStepInput(
    int StepOrder,
    string ApproverStrategy,
    Guid? SpecificRoleId,
    Guid? SpecificEmployeeId);

public sealed record CreateWorkflowDefinitionRequest(
    string RequestType,
    Guid? DepartmentId,
    string Name,
    bool IsActive,
    IReadOnlyList<WorkflowStepInput> Steps);

public sealed record UpdateWorkflowDefinitionRequest(
    Guid? DepartmentId,
    string Name,
    bool IsActive,
    IReadOnlyList<WorkflowStepInput> Steps);

namespace HrmSaas.Application.Features.Dashboard;

public sealed record MyDashboardRequestItemDto(
    Guid Id,
    string RequestType,
    int CurrentStepOrder,
    string Status,
    string PayloadSummary,
    DateTimeOffset SubmittedAt);

public sealed record LatestPayslipSnapshotDto(
    Guid PayslipId,
    Guid PayrollPeriodId,
    string PeriodCode,
    DateOnly PayDate,
    decimal GrossIncome,
    decimal TotalDeductions,
    decimal NetPay,
    bool IsLocked);

public sealed record MyDashboardSummaryDto(
    DateOnly? RecentMissingPunchDate,
    IReadOnlyList<MyDashboardRequestItemDto> RecentRequests,
    LatestPayslipSnapshotDto? LatestPayslip);

public sealed record TeamExceptionItemDto(
    Guid EmployeeId,
    string EmployeeCode,
    string EmployeeName,
    string AttendanceStatus,
    int LateMinutes,
    DateTimeOffset? ActualInUtc);

public sealed record WhoIsOutItemDto(
    Guid EmployeeId,
    string EmployeeCode,
    string EmployeeName,
    string DepartmentName);

public sealed record TeamPulseDto(
    DateOnly WorkDate,
    int TotalScheduledCount,
    int PresentCount,
    int LateCount,
    int MissingPunchCount,
    int AbsentCount,
    int OnLeaveCount,
    IReadOnlyList<TeamExceptionItemDto> Exceptions,
    IReadOnlyList<WhoIsOutItemDto> WhoIsOut);

public sealed record DepartmentHeadcountDto(
    Guid DepartmentId,
    string DepartmentName,
    int ActiveCount,
    int ProbationCount);

public sealed record LifecycleAlertDto(
    Guid EmployeeId,
    string EmployeeCode,
    string EmployeeName,
    string AlertType,
    DateOnly DueDate,
    int DaysRemaining);

public sealed record HrOverviewDto(
    int ActiveCount,
    int ProbationCount,
    int NewHiresThisMonthCount,
    IReadOnlyList<DepartmentHeadcountDto> DepartmentBreakdown,
    IReadOnlyList<LifecycleAlertDto> LifecycleAlerts);

public sealed record PayrollReadinessDto(
    Guid? PeriodId,
    string? PeriodCode,
    DateOnly? PeriodStart,
    DateOnly? PeriodEnd,
    DateOnly? AttendanceCutoffDate,
    DateOnly? PayDate,
    string? Status,
    int? DaysUntilCutoff,
    int PendingApprovalsCount,
    int MissingPunchCount,
    decimal? TotalGrossEstimate,
    decimal? TotalNetEstimate);

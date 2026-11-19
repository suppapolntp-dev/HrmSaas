namespace HrmSaas.Application.Features.Attendance;

public sealed record ShiftBreakRuleDto(
    Guid Id,
    TimeOnly BreakStart,
    TimeOnly BreakEnd,
    int DurationMinutes,
    bool IsPaid);

public sealed record ShiftDefinitionDto(
    Guid Id,
    string Code,
    string Name,
    string ShiftType,
    TimeOnly StartTime,
    TimeOnly EndTime,
    bool IsOvernight,
    int GracePeriodMinutes,
    int LateAfterMinutes,
    int StandardWorkMinutes,
    IReadOnlyList<ShiftBreakRuleDto> BreakRules);

public sealed record ShiftBreakRuleInput(
    TimeOnly BreakStart,
    TimeOnly BreakEnd,
    int DurationMinutes,
    bool IsPaid);

public sealed record CreateShiftRequest(
    string Code,
    string Name,
    string ShiftType,
    TimeOnly StartTime,
    TimeOnly EndTime,
    bool IsOvernight,
    int GracePeriodMinutes,
    int LateAfterMinutes,
    int StandardWorkMinutes,
    IReadOnlyList<ShiftBreakRuleInput> BreakRules);

public sealed record UpdateShiftRequest(
    string Name,
    string ShiftType,
    TimeOnly StartTime,
    TimeOnly EndTime,
    bool IsOvernight,
    int GracePeriodMinutes,
    int LateAfterMinutes,
    int StandardWorkMinutes,
    IReadOnlyList<ShiftBreakRuleInput> BreakRules);

public sealed record RosterItemDto(
    Guid Id,
    Guid EmployeeId,
    string EmployeeCode,
    string EmployeeName,
    DateOnly WorkDate,
    Guid? ShiftDefinitionId,
    string? ShiftCode,
    string? ShiftName,
    string DayType);

public sealed record BulkAssignRosterRequest(
    IReadOnlyList<Guid> EmployeeIds,
    DateOnly StartDate,
    DateOnly EndDate,
    Guid? ShiftDefinitionId,
    string DayType);

public sealed record CheckInOutRequest(
    decimal? Latitude,
    decimal? Longitude,
    string Source = "WEB");

public sealed record AttendanceDailySummaryDto(
    Guid Id,
    Guid EmployeeId,
    string EmployeeCode,
    string EmployeeName,
    Guid BranchId,
    Guid DepartmentId,
    DateOnly WorkDate,
    Guid? ShiftDefinitionId,
    string? ShiftName,
    DateTimeOffset? ActualInUtc,
    DateTimeOffset? ActualOutUtc,
    int WorkedMinutes,
    int LateMinutes,
    int EarlyOutMinutes,
    int OtMinutes,
    string AttendanceStatus,
    bool IsLockedByPayroll);

public sealed record TimeAdjustmentRequest(
    DateOnly WorkDate,
    DateTimeOffset RequestedInUtc,
    DateTimeOffset RequestedOutUtc,
    string Reason);

public sealed record TimeAdjustmentCreatedDto(
    Guid ApprovalRequestId,
    string Status,
    DateTimeOffset SubmittedAt);

public sealed record MyTodayAttendanceDto(
    DateOnly WorkDate,
    string BranchName,
    string BranchTimezone,
    string DayType,
    Guid? ShiftDefinitionId,
    string? ShiftCode,
    string? ShiftName,
    TimeOnly? ShiftStartTime,
    TimeOnly? ShiftEndTime,
    bool IsOvernight,
    DateTimeOffset? ActualInUtc,
    DateTimeOffset? ActualOutUtc,
    int WorkedMinutes,
    int LateMinutes,
    int EarlyOutMinutes,
    int OtMinutes,
    string AttendanceStatus,
    bool IsLockedByPayroll);

public sealed record MyUpcomingRosterDto(
    Guid Id,
    DateOnly WorkDate,
    string DayType,
    Guid? ShiftDefinitionId,
    string? ShiftCode,
    string? ShiftName,
    TimeOnly? StartTime,
    TimeOnly? EndTime,
    bool IsOvernight);

using HrmSaas.Domain.Common;

namespace HrmSaas.Domain.Attendance;

public static class AttendanceStatus
{
    public const string Present = "PRESENT";
    public const string Late = "LATE";
    public const string EarlyOut = "EARLY_OUT";
    public const string MissingPunch = "MISSING_PUNCH";
    public const string Absent = "ABSENT";
    public const string OnLeave = "ON_LEAVE";
    public const string DayOff = "DAY_OFF";
}

public sealed class AttendanceDailySummary : TenantCompanyEntity
{
    private AttendanceDailySummary()
    {
    }

    public Guid EmployeeId { get; private set; }
    public DateOnly WorkDate { get; private set; }
    public Guid? ShiftDefinitionId { get; private set; }
    public DateTimeOffset? ActualInUtc { get; private set; }
    public DateTimeOffset? ActualOutUtc { get; private set; }
    public int WorkedMinutes { get; private set; }
    public int LateMinutes { get; private set; }
    public int EarlyOutMinutes { get; private set; }
    public int OtMinutes { get; private set; }
    public string AttendanceStatus { get; private set; } = Attendance.AttendanceStatus.Present;
    public bool IsLockedByPayroll { get; private set; }

    public static AttendanceDailySummary Create(
        Guid tenantId,
        Guid companyId,
        Guid employeeId,
        DateOnly workDate,
        Guid? shiftDefinitionId,
        DateTimeOffset? actualInUtc,
        DateTimeOffset? actualOutUtc,
        int workedMinutes,
        int lateMinutes,
        int earlyOutMinutes,
        int otMinutes,
        string attendanceStatus,
        bool isLockedByPayroll = false,
        Guid? id = null)
    {
        var summary = new AttendanceDailySummary
        {
            TenantId = tenantId,
            CompanyId = companyId,
            EmployeeId = employeeId,
            WorkDate = workDate,
            ShiftDefinitionId = shiftDefinitionId,
            ActualInUtc = actualInUtc,
            ActualOutUtc = actualOutUtc,
            WorkedMinutes = Math.Max(0, workedMinutes),
            LateMinutes = Math.Max(0, lateMinutes),
            EarlyOutMinutes = Math.Max(0, earlyOutMinutes),
            OtMinutes = Math.Max(0, otMinutes),
            AttendanceStatus = attendanceStatus.Trim().ToUpperInvariant(),
            IsLockedByPayroll = isLockedByPayroll
        };

        if (id.HasValue)
        {
            summary.Id = id.Value;
        }

        return summary;
    }

    public void LockForPayroll()
    {
        IsLockedByPayroll = true;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}

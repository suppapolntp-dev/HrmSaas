using HrmSaas.Domain.Common;

namespace HrmSaas.Domain.Attendance;

public static class ShiftType
{
    public const string Normal = "NORMAL";
    public const string Overnight = "OVERNIGHT";
    public const string Flexible = "FLEXIBLE";
}

public sealed class ShiftDefinition : TenantCompanyEntity
{
    private ShiftDefinition()
    {
    }

    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string ShiftType { get; private set; } = Attendance.ShiftType.Normal;
    public TimeOnly StartTime { get; private set; }
    public TimeOnly EndTime { get; private set; }
    public bool IsOvernight { get; private set; }
    public int GracePeriodMinutes { get; private set; }
    public int LateAfterMinutes { get; private set; }
    public int StandardWorkMinutes { get; private set; }

    public static ShiftDefinition Create(
        Guid tenantId,
        Guid companyId,
        string code,
        string name,
        string shiftType,
        TimeOnly startTime,
        TimeOnly endTime,
        bool isOvernight,
        int gracePeriodMinutes,
        int lateAfterMinutes,
        int standardWorkMinutes,
        Guid? id = null)
    {
        if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Shift code and name are required.");
        }

        if (gracePeriodMinutes < 0 || lateAfterMinutes < 0 || standardWorkMinutes <= 0)
        {
            throw new ArgumentException("Shift durations must be valid non-negative/positive minutes.");
        }

        var shift = new ShiftDefinition
        {
            TenantId = tenantId,
            CompanyId = companyId,
            Code = code.Trim().ToUpperInvariant(),
            Name = name.Trim(),
            ShiftType = shiftType.Trim().ToUpperInvariant(),
            StartTime = startTime,
            EndTime = endTime,
            IsOvernight = isOvernight,
            GracePeriodMinutes = gracePeriodMinutes,
            LateAfterMinutes = lateAfterMinutes,
            StandardWorkMinutes = standardWorkMinutes
        };

        if (id.HasValue)
        {
            shift.Id = id.Value;
        }

        return shift;
    }

    public void Update(
        string name,
        string shiftType,
        TimeOnly startTime,
        TimeOnly endTime,
        bool isOvernight,
        int gracePeriodMinutes,
        int lateAfterMinutes,
        int standardWorkMinutes)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Shift name is required.", nameof(name));
        }

        if (gracePeriodMinutes < 0 || lateAfterMinutes < 0 || standardWorkMinutes <= 0)
        {
            throw new ArgumentException("Shift durations must be valid non-negative/positive minutes.");
        }

        Name = name.Trim();
        ShiftType = shiftType.Trim().ToUpperInvariant();
        StartTime = startTime;
        EndTime = endTime;
        IsOvernight = isOvernight;
        GracePeriodMinutes = gracePeriodMinutes;
        LateAfterMinutes = lateAfterMinutes;
        StandardWorkMinutes = standardWorkMinutes;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}

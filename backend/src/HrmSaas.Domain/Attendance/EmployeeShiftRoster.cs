using HrmSaas.Domain.Common;

namespace HrmSaas.Domain.Attendance;

public static class RosterDayType
{
    public const string Workday = "WORKDAY";
    public const string DayOff = "DAY_OFF";
    public const string PublicHoliday = "PUBLIC_HOLIDAY";
}

public sealed class EmployeeShiftRoster : TenantCompanyEntity
{
    private EmployeeShiftRoster()
    {
    }

    public Guid EmployeeId { get; private set; }
    public Guid? ShiftDefinitionId { get; private set; }
    public DateOnly WorkDate { get; private set; }
    public string DayType { get; private set; } = RosterDayType.Workday;

    public static EmployeeShiftRoster Create(
        Guid tenantId,
        Guid companyId,
        Guid employeeId,
        Guid? shiftDefinitionId,
        DateOnly workDate,
        string dayType = RosterDayType.Workday,
        Guid? id = null)
    {
        var normalizedDayType = dayType.Trim().ToUpperInvariant();
        if (normalizedDayType == RosterDayType.Workday && !shiftDefinitionId.HasValue)
        {
            throw new ArgumentException("Workday roster must specify a ShiftDefinitionId.");
        }

        var roster = new EmployeeShiftRoster
        {
            TenantId = tenantId,
            CompanyId = companyId,
            EmployeeId = employeeId,
            ShiftDefinitionId = shiftDefinitionId,
            WorkDate = workDate,
            DayType = normalizedDayType
        };

        if (id.HasValue)
        {
            roster.Id = id.Value;
        }

        return roster;
    }
}

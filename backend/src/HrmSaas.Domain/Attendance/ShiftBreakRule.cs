using HrmSaas.Domain.Common;

namespace HrmSaas.Domain.Attendance;

public sealed class ShiftBreakRule : BaseEntity, ITenantScoped
{
    private ShiftBreakRule()
    {
    }

    public Guid TenantId { get; private set; }
    public Guid ShiftDefinitionId { get; private set; }
    public TimeOnly BreakStart { get; private set; }
    public TimeOnly BreakEnd { get; private set; }
    public int DurationMinutes { get; private set; }
    public bool IsPaid { get; private set; }

    public static ShiftBreakRule Create(
        Guid tenantId,
        Guid shiftDefinitionId,
        TimeOnly breakStart,
        TimeOnly breakEnd,
        int durationMinutes,
        bool isPaid = false,
        Guid? id = null)
    {
        if (durationMinutes <= 0)
        {
            throw new ArgumentException("Break duration must be > 0 minutes.", nameof(durationMinutes));
        }

        var rule = new ShiftBreakRule
        {
            TenantId = tenantId,
            ShiftDefinitionId = shiftDefinitionId,
            BreakStart = breakStart,
            BreakEnd = breakEnd,
            DurationMinutes = durationMinutes,
            IsPaid = isPaid
        };

        if (id.HasValue)
        {
            rule.Id = id.Value;
        }

        return rule;
    }
}

using HrmSaas.Domain.Common;

namespace HrmSaas.Domain.Workflows;

public static class ProrateRule
{
    public const string None = "NONE";
    public const string MonthlyProrate = "MONTHLY_PRORATE";
    public const string DailyProrate = "DAILY_PRORATE";
}

public sealed class LeaveEntitlementPolicy : TenantCompanyEntity
{
    private LeaveEntitlementPolicy()
    {
    }

    public Guid LeaveTypeId { get; private set; }
    public Guid? JobGradeId { get; private set; }
    public int MinTenureMonths { get; private set; }
    public int? MaxTenureMonths { get; private set; }
    public decimal AnnualQuotaDays { get; private set; }
    public string ProrateRule { get; private set; } = Workflows.ProrateRule.MonthlyProrate;
    public decimal MaxCarryOverDays { get; private set; }

    public static LeaveEntitlementPolicy Create(
        Guid tenantId,
        Guid companyId,
        Guid leaveTypeId,
        Guid? jobGradeId,
        int minTenureMonths,
        int? maxTenureMonths,
        decimal annualQuotaDays,
        string prorateRule,
        decimal maxCarryOverDays,
        Guid? id = null)
    {
        if (minTenureMonths < 0 || annualQuotaDays < 0 || maxCarryOverDays < 0)
        {
            throw new ArgumentException("Policy tenure and quota values cannot be negative.");
        }

        if (maxTenureMonths.HasValue && maxTenureMonths.Value < minTenureMonths)
        {
            throw new ArgumentException("MaxTenureMonths cannot be less than MinTenureMonths.");
        }

        var policy = new LeaveEntitlementPolicy
        {
            TenantId = tenantId,
            CompanyId = companyId,
            LeaveTypeId = leaveTypeId,
            JobGradeId = jobGradeId,
            MinTenureMonths = minTenureMonths,
            MaxTenureMonths = maxTenureMonths,
            AnnualQuotaDays = annualQuotaDays,
            ProrateRule = prorateRule.Trim().ToUpperInvariant(),
            MaxCarryOverDays = maxCarryOverDays
        };

        if (id.HasValue)
        {
            policy.Id = id.Value;
        }

        return policy;
    }
}

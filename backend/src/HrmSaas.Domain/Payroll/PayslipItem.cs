using HrmSaas.Domain.Common;

namespace HrmSaas.Domain.Payroll;

public sealed class PayslipItem : BaseEntity, ITenantScoped
{
    private PayslipItem()
    {
    }

    public Guid TenantId { get; private set; }
    public Guid PayslipId { get; private set; }
    public Guid SalaryComponentId { get; private set; }
    public string ComponentNameSnapshot { get; private set; } = string.Empty;
    public decimal Amount { get; private set; }
    public decimal? Units { get; private set; }

    public static PayslipItem Create(
        Guid tenantId,
        Guid payslipId,
        Guid salaryComponentId,
        string componentNameSnapshot,
        decimal amount,
        decimal? units = null,
        Guid? id = null)
    {
        if (string.IsNullOrWhiteSpace(componentNameSnapshot))
        {
            throw new ArgumentException("ComponentNameSnapshot is required.", nameof(componentNameSnapshot));
        }

        var item = new PayslipItem
        {
            TenantId = tenantId,
            PayslipId = payslipId,
            SalaryComponentId = salaryComponentId,
            ComponentNameSnapshot = componentNameSnapshot.Trim(),
            Amount = amount,
            Units = units
        };

        if (id.HasValue)
        {
            item.Id = id.Value;
        }

        return item;
    }
}

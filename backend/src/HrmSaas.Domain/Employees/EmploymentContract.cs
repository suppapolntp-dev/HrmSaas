using HrmSaas.Domain.Common;

namespace HrmSaas.Domain.Employees;

public static class ContractType
{
    public const string FullTime = "FULL_TIME";
    public const string PartTime = "PART_TIME";
    public const string Contract = "CONTRACT";
    public const string Intern = "INTERN";
}

public static class PaymentCycle
{
    public const string Monthly = "MONTHLY";
    public const string BiWeekly = "BI_WEEKLY";
}

public sealed class EmploymentContract : TenantCompanyEntity
{
    private EmploymentContract()
    {
    }

    public Guid EmployeeId { get; private set; }
    public string ContractType { get; private set; } = Employees.ContractType.FullTime;
    public DateOnly EffectiveFrom { get; private set; }
    public DateOnly? EffectiveTo { get; private set; }
    public decimal BaseSalary { get; private set; }
    public string PaymentCycle { get; private set; } = Employees.PaymentCycle.Monthly;
    public bool IsActive { get; private set; }

    public static EmploymentContract Create(
        Guid tenantId,
        Guid companyId,
        Guid employeeId,
        string contractType,
        DateOnly effectiveFrom,
        DateOnly? effectiveTo,
        decimal baseSalary,
        string paymentCycle = Employees.PaymentCycle.Monthly,
        bool isActive = true,
        Guid? id = null)
    {
        if (baseSalary < 0)
        {
            throw new ArgumentException("Base salary cannot be negative.", nameof(baseSalary));
        }

        if (effectiveTo.HasValue && effectiveTo.Value < effectiveFrom)
        {
            throw new ArgumentException("EffectiveTo cannot be earlier than EffectiveFrom.");
        }

        var contract = new EmploymentContract
        {
            TenantId = tenantId,
            CompanyId = companyId,
            EmployeeId = employeeId,
            ContractType = contractType.Trim().ToUpperInvariant(),
            EffectiveFrom = effectiveFrom,
            EffectiveTo = effectiveTo,
            BaseSalary = baseSalary,
            PaymentCycle = paymentCycle.Trim().ToUpperInvariant(),
            IsActive = isActive
        };

        if (id.HasValue)
        {
            contract.Id = id.Value;
        }

        return contract;
    }

    public void Deactivate(DateOnly effectiveTo)
    {
        EffectiveTo = effectiveTo;
        IsActive = false;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}

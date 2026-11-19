using HrmSaas.Domain.Common;

namespace HrmSaas.Domain.Payroll;

public static class SalaryComponentType
{
    public const string Earning = "EARNING";
    public const string Deduction = "DEDUCTION";
}

public static class CalculationFormulaType
{
    public const string Fixed = "FIXED";
    public const string OvertimeHourly = "OVERTIME_HOURLY";
    public const string AttendanceDeduction = "ATTENDANCE_DEDUCTION";
    public const string CustomAmount = "CUSTOM_AMOUNT";
}

public sealed class SalaryComponent : TenantCompanyEntity
{
    private SalaryComponent()
    {
    }

    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string ComponentType { get; private set; } = SalaryComponentType.Earning;
    public bool IsTaxable { get; private set; }
    public bool IsSsoBase { get; private set; }
    public string CalculationFormulaType { get; private set; } = Payroll.CalculationFormulaType.Fixed;

    public static SalaryComponent Create(
        Guid tenantId,
        Guid companyId,
        string code,
        string name,
        string componentType,
        bool isTaxable,
        bool isSsoBase,
        string calculationFormulaType,
        Guid? id = null)
    {
        if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("SalaryComponent code and name are required.");
        }

        var component = new SalaryComponent
        {
            TenantId = tenantId,
            CompanyId = companyId,
            Code = code.Trim().ToUpperInvariant(),
            Name = name.Trim(),
            ComponentType = componentType.Trim().ToUpperInvariant(),
            IsTaxable = isTaxable,
            IsSsoBase = isSsoBase,
            CalculationFormulaType = calculationFormulaType.Trim().ToUpperInvariant()
        };

        if (id.HasValue)
        {
            component.Id = id.Value;
        }

        return component;
    }
}

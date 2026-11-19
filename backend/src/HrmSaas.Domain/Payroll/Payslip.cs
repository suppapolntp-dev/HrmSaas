using HrmSaas.Domain.Common;

namespace HrmSaas.Domain.Payroll;

public sealed class Payslip : TenantCompanyEntity
{
    private Payslip()
    {
    }

    public Guid PayrollPeriodId { get; private set; }
    public Guid EmployeeId { get; private set; }
    public decimal GrossIncome { get; private set; }
    public decimal TotalDeductions { get; private set; }
    public decimal SsoEmployeeAmount { get; private set; }
    public decimal WithholdingTaxAmount { get; private set; }
    public decimal NetPay { get; private set; }
    public bool IsLocked { get; private set; }

    public static Payslip Create(
        Guid tenantId,
        Guid companyId,
        Guid payrollPeriodId,
        Guid employeeId,
        decimal grossIncome,
        decimal totalDeductions,
        decimal ssoEmployeeAmount,
        decimal withholdingTaxAmount,
        decimal netPay,
        bool isLocked = false,
        Guid? id = null)
    {
        var payslip = new Payslip
        {
            TenantId = tenantId,
            CompanyId = companyId,
            PayrollPeriodId = payrollPeriodId,
            EmployeeId = employeeId,
            GrossIncome = grossIncome,
            TotalDeductions = totalDeductions,
            SsoEmployeeAmount = ssoEmployeeAmount,
            WithholdingTaxAmount = withholdingTaxAmount,
            NetPay = netPay,
            IsLocked = isLocked
        };

        if (id.HasValue)
        {
            payslip.Id = id.Value;
        }

        return payslip;
    }

    public void Lock()
    {
        IsLocked = true;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}

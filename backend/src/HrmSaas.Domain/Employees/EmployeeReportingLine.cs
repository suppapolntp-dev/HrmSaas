using HrmSaas.Domain.Common;

namespace HrmSaas.Domain.Employees;

public static class ReportingLineType
{
    public const string Direct = "DIRECT";
    public const string Secondary = "SECONDARY";
}

public sealed class EmployeeReportingLine : TenantCompanyEntity
{
    private EmployeeReportingLine()
    {
    }

    public Guid EmployeeId { get; private set; }
    public Guid ManagerEmployeeId { get; private set; }
    public string LineType { get; private set; } = ReportingLineType.Direct;
    public int PriorityOrder { get; private set; }
    public DateOnly EffectiveFrom { get; private set; }
    public DateOnly? EffectiveTo { get; private set; }

    public static EmployeeReportingLine Create(
        Guid tenantId,
        Guid companyId,
        Guid employeeId,
        Guid managerEmployeeId,
        string lineType,
        int priorityOrder,
        DateOnly effectiveFrom,
        DateOnly? effectiveTo = null,
        Guid? id = null)
    {
        if (employeeId == managerEmployeeId)
        {
            throw new InvalidOperationException("Employee cannot report to themselves.");
        }

        if (priorityOrder < 1)
        {
            throw new ArgumentException("Priority order must be >= 1.", nameof(priorityOrder));
        }

        var line = new EmployeeReportingLine
        {
            TenantId = tenantId,
            CompanyId = companyId,
            EmployeeId = employeeId,
            ManagerEmployeeId = managerEmployeeId,
            LineType = lineType.Trim().ToUpperInvariant(),
            PriorityOrder = priorityOrder,
            EffectiveFrom = effectiveFrom,
            EffectiveTo = effectiveTo
        };

        if (id.HasValue)
        {
            line.Id = id.Value;
        }

        return line;
    }
}

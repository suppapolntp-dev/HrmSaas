using HrmSaas.Domain.Common;

namespace HrmSaas.Domain.Workflows;

public sealed class LeaveType : TenantCompanyEntity
{
    private LeaveType()
    {
    }

    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public bool IsPaid { get; private set; }
    public bool AllowHalfDay { get; private set; }
    public bool AllowHourly { get; private set; }
    public bool RequiresAttachment { get; private set; }

    public static LeaveType Create(
        Guid tenantId,
        Guid companyId,
        string code,
        string name,
        bool isPaid,
        bool allowHalfDay,
        bool allowHourly,
        bool requiresAttachment,
        Guid? id = null)
    {
        if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("LeaveType code and name are required.");
        }

        var leaveType = new LeaveType
        {
            TenantId = tenantId,
            CompanyId = companyId,
            Code = code.Trim().ToUpperInvariant(),
            Name = name.Trim(),
            IsPaid = isPaid,
            AllowHalfDay = allowHalfDay,
            AllowHourly = allowHourly,
            RequiresAttachment = requiresAttachment
        };

        if (id.HasValue)
        {
            leaveType.Id = id.Value;
        }

        return leaveType;
    }
}

using HrmSaas.Domain.Common;

namespace HrmSaas.Domain.Organizations;

public sealed class Department : TenantCompanyEntity
{
    private Department()
    {
    }

    public Guid? ParentDepartmentId { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public Guid? HeadEmployeeId { get; private set; }

    public static Department Create(
        Guid tenantId,
        Guid companyId,
        string code,
        string name,
        Guid? parentDepartmentId = null,
        Guid? headEmployeeId = null,
        Guid? id = null)
    {
        if (tenantId == Guid.Empty || companyId == Guid.Empty)
        {
            throw new ArgumentException("TenantId and CompanyId are required.");
        }

        if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Department code and name are required.");
        }

        var department = new Department
        {
            TenantId = tenantId,
            CompanyId = companyId,
            ParentDepartmentId = parentDepartmentId,
            Code = code.Trim().ToUpperInvariant(),
            Name = name.Trim(),
            HeadEmployeeId = headEmployeeId
        };

        if (id.HasValue)
        {
            department.Id = id.Value;
        }

        return department;
    }

    public void Update(string name, Guid? parentDepartmentId, Guid? headEmployeeId)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Department name is required.", nameof(name));
        }

        if (parentDepartmentId == Id)
        {
            throw new InvalidOperationException("Department cannot be its own parent.");
        }

        Name = name.Trim();
        ParentDepartmentId = parentDepartmentId;
        HeadEmployeeId = headEmployeeId;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}

using HrmSaas.Domain.Common;

namespace HrmSaas.Domain.Identity;

public sealed class Role : TenantEntity
{
    private Role()
    {
    }

    public Guid? CompanyId { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public bool IsSystemRole { get; private set; }

    public static Role Create(
        Guid tenantId,
        Guid? companyId,
        string code,
        string name,
        bool isSystemRole = false,
        Guid? id = null)
    {
        if (tenantId == Guid.Empty)
        {
            throw new ArgumentException("TenantId is required.", nameof(tenantId));
        }

        if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Role code and name are required.");
        }

        var role = new Role
        {
            TenantId = tenantId,
            CompanyId = companyId,
            Code = code.Trim().ToUpperInvariant(),
            Name = name.Trim(),
            IsSystemRole = isSystemRole
        };

        if (id.HasValue)
        {
            role.Id = id.Value;
        }

        return role;
    }

    public void Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Role name is required.", nameof(name));
        }

        Name = name.Trim();
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}

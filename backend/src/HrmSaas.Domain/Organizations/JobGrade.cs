using HrmSaas.Domain.Common;

namespace HrmSaas.Domain.Organizations;

public sealed class JobGrade : TenantCompanyEntity
{
    private JobGrade()
    {
    }

    public string Code { get; private set; } = string.Empty;
    public int LevelOrder { get; private set; }
    public string Name { get; private set; } = string.Empty;

    public static JobGrade Create(
        Guid tenantId,
        Guid companyId,
        string code,
        int levelOrder,
        string name,
        Guid? id = null)
    {
        if (tenantId == Guid.Empty || companyId == Guid.Empty)
        {
            throw new ArgumentException("TenantId and CompanyId are required.");
        }

        if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("JobGrade code and name are required.");
        }

        if (levelOrder < 1)
        {
            throw new ArgumentException("Level order must be >= 1.", nameof(levelOrder));
        }

        var jobGrade = new JobGrade
        {
            TenantId = tenantId,
            CompanyId = companyId,
            Code = code.Trim().ToUpperInvariant(),
            LevelOrder = levelOrder,
            Name = name.Trim()
        };

        if (id.HasValue)
        {
            jobGrade.Id = id.Value;
        }

        return jobGrade;
    }

    public void Update(int levelOrder, string name)
    {
        if (levelOrder < 1)
        {
            throw new ArgumentException("Level order must be >= 1.", nameof(levelOrder));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("JobGrade name is required.", nameof(name));
        }

        LevelOrder = levelOrder;
        Name = name.Trim();
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}

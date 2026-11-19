using HrmSaas.Domain.Common;

namespace HrmSaas.Domain.Organizations;

public sealed class Position : TenantCompanyEntity
{
    private Position()
    {
    }

    public string Code { get; private set; } = string.Empty;
    public string Title { get; private set; } = string.Empty;

    public static Position Create(
        Guid tenantId,
        Guid companyId,
        string code,
        string title,
        Guid? id = null)
    {
        if (tenantId == Guid.Empty || companyId == Guid.Empty)
        {
            throw new ArgumentException("TenantId and CompanyId are required.");
        }

        if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Position code and title are required.");
        }

        var position = new Position
        {
            TenantId = tenantId,
            CompanyId = companyId,
            Code = code.Trim().ToUpperInvariant(),
            Title = title.Trim()
        };

        if (id.HasValue)
        {
            position.Id = id.Value;
        }

        return position;
    }

    public void Rename(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Position title is required.", nameof(title));
        }

        Title = title.Trim();
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}

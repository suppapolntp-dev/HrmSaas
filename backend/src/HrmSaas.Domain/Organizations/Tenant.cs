using HrmSaas.Domain.Common;

namespace HrmSaas.Domain.Organizations;

public static class TenantStatus
{
    public const string Active = "ACTIVE";
    public const string Suspended = "SUSPENDED";
    public const string Trial = "TRIAL";
}

public sealed class Tenant : BaseEntity
{
    private Tenant()
    {
    }

    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string SubscriptionPlan { get; private set; } = string.Empty;
    public string Status { get; private set; } = TenantStatus.Active;
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; private set; }

    public static Tenant Create(
        string code,
        string name,
        string subscriptionPlan,
        string status = TenantStatus.Active,
        Guid? id = null)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("Tenant code is required.", nameof(code));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Tenant name is required.", nameof(name));
        }

        var tenant = new Tenant
        {
            Code = code.Trim().ToUpperInvariant(),
            Name = name.Trim(),
            SubscriptionPlan = subscriptionPlan.Trim().ToUpperInvariant(),
            Status = status.Trim().ToUpperInvariant(),
            CreatedAt = DateTimeOffset.UtcNow
        };

        if (id.HasValue)
        {
            tenant.Id = id.Value;
        }

        return tenant;
    }

    public void UpdateProfile(string name, string subscriptionPlan)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Tenant name is required.", nameof(name));
        }

        Name = name.Trim();
        SubscriptionPlan = subscriptionPlan.Trim().ToUpperInvariant();
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void ChangeStatus(string status)
    {
        if (status != TenantStatus.Active &&
            status != TenantStatus.Suspended &&
            status != TenantStatus.Trial)
        {
            throw new ArgumentException("Invalid tenant status.", nameof(status));
        }

        Status = status;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}

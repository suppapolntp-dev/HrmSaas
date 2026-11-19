using HrmSaas.Domain.Common;

namespace HrmSaas.Domain.Identity;

public static class UserStatus
{
    public const string Active = "ACTIVE";
    public const string Locked = "LOCKED";
    public const string Inactive = "INACTIVE";
}

public sealed class User : TenantEntity
{
    private User()
    {
    }

    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public bool MfaEnabled { get; private set; }
    public string Status { get; private set; } = UserStatus.Active;
    public DateTimeOffset? LastLoginAt { get; private set; }

    public static User Create(
        Guid tenantId,
        string email,
        string passwordHash,
        bool mfaEnabled = false,
        string status = UserStatus.Active,
        Guid? id = null)
    {
        if (tenantId == Guid.Empty)
        {
            throw new ArgumentException("TenantId is required.", nameof(tenantId));
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Email is required.", nameof(email));
        }

        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            throw new ArgumentException("Password hash is required.", nameof(passwordHash));
        }

        var user = new User
        {
            TenantId = tenantId,
            Email = email.Trim().ToLowerInvariant(),
            PasswordHash = passwordHash,
            MfaEnabled = mfaEnabled,
            Status = status.Trim().ToUpperInvariant()
        };

        if (id.HasValue)
        {
            user.Id = id.Value;
        }

        return user;
    }

    public void RecordLogin(DateTimeOffset utcNow)
    {
        LastLoginAt = utcNow;
        UpdatedAt = utcNow;
    }

    public void ChangePassword(string newPasswordHash, DateTimeOffset utcNow)
    {
        if (string.IsNullOrWhiteSpace(newPasswordHash))
        {
            throw new ArgumentException("Password hash is required.", nameof(newPasswordHash));
        }

        PasswordHash = newPasswordHash;
        UpdatedAt = utcNow;
    }
}

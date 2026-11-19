namespace HrmSaas.Domain.Common;

public interface ITenantScoped
{
    Guid TenantId { get; }
}

public interface ICompanyScoped
{
    Guid CompanyId { get; }
}

public interface ISoftDeletable
{
    bool IsDeleted { get; }
    DateTimeOffset? DeletedAt { get; }
}

public abstract class BaseEntity
{
    public Guid Id { get; protected set; } = Guid.CreateVersion7();
}

public abstract class AuditableEntity : BaseEntity, ISoftDeletable
{
    public DateTimeOffset CreatedAt { get; protected set; } = DateTimeOffset.UtcNow;
    public Guid? CreatedBy { get; protected set; }
    public DateTimeOffset? UpdatedAt { get; protected set; }
    public Guid? UpdatedBy { get; protected set; }
    public bool IsDeleted { get; protected set; }
    public DateTimeOffset? DeletedAt { get; protected set; }

    public void SetCreatedAudit(Guid? actorUserId, DateTimeOffset utcNow)
    {
        CreatedAt = utcNow;
        CreatedBy = actorUserId;
    }

    public void SetUpdatedAudit(Guid? actorUserId, DateTimeOffset utcNow)
    {
        UpdatedAt = utcNow;
        UpdatedBy = actorUserId;
    }

    public void SoftDelete(Guid? actorUserId, DateTimeOffset utcNow)
    {
        if (IsDeleted)
        {
            return;
        }

        IsDeleted = true;
        DeletedAt = utcNow;
        UpdatedAt = utcNow;
        UpdatedBy = actorUserId;
    }
}

public abstract class TenantEntity : AuditableEntity, ITenantScoped
{
    public Guid TenantId { get; protected set; }
}

public abstract class TenantCompanyEntity : TenantEntity, ICompanyScoped
{
    public Guid CompanyId { get; protected set; }
}

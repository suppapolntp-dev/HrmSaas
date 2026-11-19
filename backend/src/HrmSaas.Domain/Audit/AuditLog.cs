using System.Net;
using HrmSaas.Domain.Common;

namespace HrmSaas.Domain.Audit;

public sealed class AuditLog : BaseEntity, ITenantScoped
{
    private AuditLog()
    {
    }

    public Guid TenantId { get; private set; }
    public Guid? CompanyId { get; private set; }
    public Guid? ActorUserId { get; private set; }
    public string EntityName { get; private set; } = string.Empty;
    public Guid EntityId { get; private set; }
    public string Action { get; private set; } = string.Empty;
    public string? OldValues { get; private set; }
    public string? NewValues { get; private set; }
    public IPAddress? IpAddress { get; private set; }
    public string CorrelationId { get; private set; } = string.Empty;
    public DateTimeOffset OccurredAt { get; private set; } = DateTimeOffset.UtcNow;

    public static AuditLog Create(
        Guid tenantId,
        Guid? companyId,
        Guid? actorUserId,
        string entityName,
        Guid entityId,
        string action,
        string? oldValuesJson,
        string? newValuesJson,
        IPAddress? ipAddress,
        string correlationId,
        DateTimeOffset occurredAt,
        Guid? id = null)
    {
        var audit = new AuditLog
        {
            TenantId = tenantId,
            CompanyId = companyId,
            ActorUserId = actorUserId,
            EntityName = entityName.Trim(),
            EntityId = entityId,
            Action = action.Trim().ToUpperInvariant(),
            OldValues = oldValuesJson,
            NewValues = newValuesJson,
            IpAddress = ipAddress,
            CorrelationId = correlationId,
            OccurredAt = occurredAt
        };

        if (id.HasValue)
        {
            audit.Id = id.Value;
        }

        return audit;
    }
}

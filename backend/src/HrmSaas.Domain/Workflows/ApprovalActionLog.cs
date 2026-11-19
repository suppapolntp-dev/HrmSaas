using HrmSaas.Domain.Common;

namespace HrmSaas.Domain.Workflows;

public static class ApprovalActionType
{
    public const string Approve = "APPROVE";
    public const string Reject = "REJECT";
    public const string Cancel = "CANCEL";
}

public sealed class ApprovalActionLog : BaseEntity, ITenantScoped
{
    private ApprovalActionLog()
    {
    }

    public Guid TenantId { get; private set; }
    public Guid ApprovalRequestId { get; private set; }
    public int StepOrder { get; private set; }
    public Guid ActorEmployeeId { get; private set; }
    public string Action { get; private set; } = ApprovalActionType.Approve;
    public string? Comment { get; private set; }
    public DateTimeOffset ActedAt { get; private set; } = DateTimeOffset.UtcNow;

    public static ApprovalActionLog Create(
        Guid tenantId,
        Guid approvalRequestId,
        int stepOrder,
        Guid actorEmployeeId,
        string action,
        string? comment,
        DateTimeOffset actedAt,
        Guid? id = null)
    {
        var log = new ApprovalActionLog
        {
            TenantId = tenantId,
            ApprovalRequestId = approvalRequestId,
            StepOrder = stepOrder,
            ActorEmployeeId = actorEmployeeId,
            Action = action.Trim().ToUpperInvariant(),
            Comment = string.IsNullOrWhiteSpace(comment) ? null : comment.Trim(),
            ActedAt = actedAt
        };

        if (id.HasValue)
        {
            log.Id = id.Value;
        }

        return log;
    }
}

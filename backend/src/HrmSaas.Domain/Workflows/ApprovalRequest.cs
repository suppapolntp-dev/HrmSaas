using HrmSaas.Domain.Common;

namespace HrmSaas.Domain.Workflows;

public static class ApprovalStatus
{
    public const string Pending = "PENDING";
    public const string Approved = "APPROVED";
    public const string Rejected = "REJECTED";
    public const string Cancelled = "CANCELLED";
}

public sealed class ApprovalRequest : TenantCompanyEntity
{
    private ApprovalRequest()
    {
    }

    public Guid RequesterEmployeeId { get; private set; }
    public string RequestType { get; private set; } = WorkflowRequestType.Leave;
    public Guid ReferenceId { get; private set; }
    public Guid WorkflowDefinitionId { get; private set; }
    public int CurrentStepOrder { get; private set; } = 1;
    public string Status { get; private set; } = ApprovalStatus.Pending;
    public string PayloadSummary { get; private set; } = "{}";
    public DateTimeOffset SubmittedAt { get; private set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? CompletedAt { get; private set; }
    public uint RowVersion { get; private set; }

    public static ApprovalRequest Create(
        Guid tenantId,
        Guid companyId,
        Guid requesterEmployeeId,
        string requestType,
        Guid referenceId,
        Guid workflowDefinitionId,
        string payloadSummaryJson,
        DateTimeOffset submittedAt,
        Guid? id = null)
    {
        var request = new ApprovalRequest
        {
            TenantId = tenantId,
            CompanyId = companyId,
            RequesterEmployeeId = requesterEmployeeId,
            RequestType = requestType.Trim().ToUpperInvariant(),
            ReferenceId = referenceId,
            WorkflowDefinitionId = workflowDefinitionId,
            CurrentStepOrder = 1,
            Status = ApprovalStatus.Pending,
            PayloadSummary = string.IsNullOrWhiteSpace(payloadSummaryJson) ? "{}" : payloadSummaryJson,
            SubmittedAt = submittedAt
        };

        if (id.HasValue)
        {
            request.Id = id.Value;
        }

        return request;
    }

    public Result AdvanceApproval(int totalSteps, DateTimeOffset utcNow)
    {
        if (Status != ApprovalStatus.Pending)
        {
            return Result.Failure(DomainErrors.Workflow.InvalidTransition);
        }

        if (totalSteps < 1)
        {
            return Result.Failure(DomainErrors.Workflow.InvalidStepOrder);
        }

        if (CurrentStepOrder >= totalSteps)
        {
            Status = ApprovalStatus.Approved;
            CompletedAt = utcNow;
        }
        else
        {
            CurrentStepOrder++;
        }

        UpdatedAt = utcNow;
        return Result.Success();
    }

    public Result Reject(DateTimeOffset utcNow)
    {
        if (Status != ApprovalStatus.Pending)
        {
            return Result.Failure(DomainErrors.Workflow.InvalidTransition);
        }

        Status = ApprovalStatus.Rejected;
        CompletedAt = utcNow;
        UpdatedAt = utcNow;
        return Result.Success();
    }

    public Result Cancel(DateTimeOffset utcNow)
    {
        if (Status != ApprovalStatus.Pending)
        {
            return Result.Failure(DomainErrors.Workflow.InvalidTransition);
        }

        Status = ApprovalStatus.Cancelled;
        CompletedAt = utcNow;
        UpdatedAt = utcNow;
        return Result.Success();
    }
}

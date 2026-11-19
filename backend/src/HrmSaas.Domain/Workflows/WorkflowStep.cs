using HrmSaas.Domain.Common;

namespace HrmSaas.Domain.Workflows;

public static class ApproverStrategy
{
    public const string DirectManager = "DIRECT_MANAGER";
    public const string SecondaryManager = "SECONDARY_MANAGER";
    public const string HrRole = "HR_ROLE";
    public const string SpecificRole = "SPECIFIC_ROLE";
    public const string SpecificEmployee = "SPECIFIC_EMPLOYEE";
}

public sealed class WorkflowStep : BaseEntity, ITenantScoped
{
    private WorkflowStep()
    {
    }

    public Guid TenantId { get; private set; }
    public Guid WorkflowDefinitionId { get; private set; }
    public int StepOrder { get; private set; }
    public string ApproverStrategy { get; private set; } = Workflows.ApproverStrategy.DirectManager;
    public Guid? SpecificRoleId { get; private set; }
    public Guid? SpecificEmployeeId { get; private set; }

    public static WorkflowStep Create(
        Guid tenantId,
        Guid workflowDefinitionId,
        int stepOrder,
        string approverStrategy,
        Guid? specificRoleId = null,
        Guid? specificEmployeeId = null,
        Guid? id = null)
    {
        if (stepOrder < 1)
        {
            throw new ArgumentException("StepOrder must be >= 1.", nameof(stepOrder));
        }

        var step = new WorkflowStep
        {
            TenantId = tenantId,
            WorkflowDefinitionId = workflowDefinitionId,
            StepOrder = stepOrder,
            ApproverStrategy = approverStrategy.Trim().ToUpperInvariant(),
            SpecificRoleId = specificRoleId,
            SpecificEmployeeId = specificEmployeeId
        };

        if (id.HasValue)
        {
            step.Id = id.Value;
        }

        return step;
    }
}

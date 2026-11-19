using HrmSaas.Domain.Common;

namespace HrmSaas.Domain.Workflows;

public static class WorkflowRequestType
{
    public const string Leave = "LEAVE";
    public const string Overtime = "OVERTIME";
    public const string TimeAdjustment = "TIME_ADJUSTMENT";
}

public sealed class WorkflowDefinition : TenantCompanyEntity
{
    private WorkflowDefinition()
    {
    }

    public string RequestType { get; private set; } = WorkflowRequestType.Leave;
    public Guid? DepartmentId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;

    public static WorkflowDefinition Create(
        Guid tenantId,
        Guid companyId,
        string requestType,
        Guid? departmentId,
        string name,
        bool isActive = true,
        Guid? id = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Workflow name is required.", nameof(name));
        }

        var definition = new WorkflowDefinition
        {
            TenantId = tenantId,
            CompanyId = companyId,
            RequestType = requestType.Trim().ToUpperInvariant(),
            DepartmentId = departmentId,
            Name = name.Trim(),
            IsActive = isActive
        };

        if (id.HasValue)
        {
            definition.Id = id.Value;
        }

        return definition;
    }

    public void Update(string name, Guid? departmentId, bool isActive)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Workflow name is required.", nameof(name));
        }

        Name = name.Trim();
        DepartmentId = departmentId;
        IsActive = isActive;
    }
}

using System.Text.Json;
using HrmSaas.Application.Common.Exceptions;
using HrmSaas.Application.Common.Interfaces;
using HrmSaas.Domain.Employees;
using HrmSaas.Domain.Identity;
using HrmSaas.Domain.Workflows;
using Microsoft.EntityFrameworkCore;

namespace HrmSaas.Application.Features.Workflows;

public sealed class LeaveAndWorkflowService
{
    private readonly IAppDbContext _dbContext;
    private readonly ITenantContext _tenantContext;

    public LeaveAndWorkflowService(IAppDbContext dbContext, ITenantContext tenantContext)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
    }

    public async Task<IReadOnlyList<LeavePolicyItemDto>> GetLeavePoliciesAsync(CancellationToken cancellationToken = default)
    {
        var leaveTypes = await _dbContext.LeaveTypes
            .AsNoTracking()
            .ToDictionaryAsync(lt => lt.Id, cancellationToken);

        List<LeaveEntitlementPolicy> policies = await _dbContext.LeaveEntitlementPolicies
            .AsNoTracking()
            .OrderBy(p => p.LeaveTypeId)
            .ThenBy(p => p.MinTenureMonths)
            .ToListAsync(cancellationToken);

        var result = new List<LeavePolicyItemDto>();
        foreach (LeaveEntitlementPolicy policy in policies)
        {
            if (!leaveTypes.TryGetValue(policy.LeaveTypeId, out LeaveType? lt))
            {
                continue;
            }

            result.Add(new LeavePolicyItemDto(
                policy.Id,
                lt.Id,
                lt.Code,
                lt.Name,
                lt.IsPaid,
                lt.AllowHalfDay,
                lt.AllowHourly,
                lt.RequiresAttachment,
                policy.JobGradeId,
                policy.MinTenureMonths,
                policy.MaxTenureMonths,
                policy.AnnualQuotaDays,
                policy.ProrateRule,
                policy.MaxCarryOverDays));
        }

        return result;
    }

    public async Task<LeavePolicyItemDto> CreateLeavePolicyAsync(
        CreateLeavePolicyRequest request,
        CancellationToken cancellationToken = default)
    {
        (Guid tenantId, Guid companyId) = RequireTenantAndCompany();
        string normalizedCode = request.LeaveTypeCode.Trim().ToUpperInvariant();

        LeaveType? leaveType = await _dbContext.LeaveTypes
            .FirstOrDefaultAsync(lt => lt.Code == normalizedCode, cancellationToken);

        if (leaveType is null)
        {
            leaveType = LeaveType.Create(
                tenantId,
                companyId,
                normalizedCode,
                request.LeaveTypeName,
                request.IsPaid,
                request.AllowHalfDay,
                request.AllowHourly,
                request.RequiresAttachment);
            _dbContext.LeaveTypes.Add(leaveType);
        }

        var policy = LeaveEntitlementPolicy.Create(
            tenantId,
            companyId,
            leaveType.Id,
            request.JobGradeId,
            request.MinTenureMonths,
            request.MaxTenureMonths,
            request.AnnualQuotaDays,
            request.ProrateRule,
            request.MaxCarryOverDays);

        _dbContext.LeaveEntitlementPolicies.Add(policy);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new LeavePolicyItemDto(
            policy.Id,
            leaveType.Id,
            leaveType.Code,
            leaveType.Name,
            leaveType.IsPaid,
            leaveType.AllowHalfDay,
            leaveType.AllowHourly,
            leaveType.RequiresAttachment,
            policy.JobGradeId,
            policy.MinTenureMonths,
            policy.MaxTenureMonths,
            policy.AnnualQuotaDays,
            policy.ProrateRule,
            policy.MaxCarryOverDays);
    }

    public async Task<LeavePolicyItemDto> UpdateLeavePolicyAsync(
        Guid policyId,
        UpdateLeavePolicyRequest request,
        CancellationToken cancellationToken = default)
    {
        (Guid tenantId, Guid companyId) = RequireTenantAndCompany();

        LeaveEntitlementPolicy existing = await _dbContext.LeaveEntitlementPolicies
            .FirstOrDefaultAsync(p => p.Id == policyId, cancellationToken)
            ?? throw new NotFoundException("Leave policy not found.");

        LeaveType leaveType = await _dbContext.LeaveTypes
            .AsNoTracking()
            .FirstOrDefaultAsync(lt => lt.Id == existing.LeaveTypeId, cancellationToken)
            ?? throw new NotFoundException("Leave type not found.");

        _dbContext.LeaveEntitlementPolicies.Remove(existing);

        var replacement = LeaveEntitlementPolicy.Create(
            tenantId,
            companyId,
            existing.LeaveTypeId,
            existing.JobGradeId,
            request.MinTenureMonths,
            request.MaxTenureMonths,
            request.AnnualQuotaDays,
            request.ProrateRule,
            request.MaxCarryOverDays,
            id: existing.Id);

        _dbContext.LeaveEntitlementPolicies.Add(replacement);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new LeavePolicyItemDto(
            replacement.Id,
            leaveType.Id,
            leaveType.Code,
            leaveType.Name,
            leaveType.IsPaid,
            leaveType.AllowHalfDay,
            leaveType.AllowHourly,
            leaveType.RequiresAttachment,
            replacement.JobGradeId,
            replacement.MinTenureMonths,
            replacement.MaxTenureMonths,
            replacement.AnnualQuotaDays,
            replacement.ProrateRule,
            replacement.MaxCarryOverDays);
    }

    public async Task<IReadOnlyList<MyLeaveBalanceDto>> GetMyLeaveBalancesAsync(
        int? year,
        CancellationToken cancellationToken = default)
    {
        Employee employee = await RequireCurrentEmployeeAsync(cancellationToken);
        int targetYear = year ?? DateTime.UtcNow.Year;

        var leaveTypes = await _dbContext.LeaveTypes
            .AsNoTracking()
            .ToDictionaryAsync(lt => lt.Id, cancellationToken);

        List<EmployeeLeaveBalance> balances = await _dbContext.EmployeeLeaveBalances
            .AsNoTracking()
            .Where(b => b.EmployeeId == employee.Id && b.QuotaYear == targetYear)
            .ToListAsync(cancellationToken);

        var result = new List<MyLeaveBalanceDto>();
        foreach (EmployeeLeaveBalance bal in balances)
        {
            leaveTypes.TryGetValue(bal.LeaveTypeId, out LeaveType? lt);
            result.Add(new MyLeaveBalanceDto(
                bal.Id,
                bal.LeaveTypeId,
                lt?.Code ?? string.Empty,
                lt?.Name ?? string.Empty,
                bal.QuotaYear,
                bal.CarriedOverDays,
                bal.EntitledDays,
                bal.UsedDays,
                bal.PendingDays,
                bal.RemainingDays));
        }

        return result;
    }

    public async Task<ApprovalRequestDto> SubmitLeaveRequestAsync(
        SubmitLeaveRequest request,
        CancellationToken cancellationToken = default)
    {
        (Guid tenantId, Guid companyId) = RequireTenantAndCompany();
        Employee employee = await RequireCurrentEmployeeAsync(cancellationToken);

        if (request.EndDate < request.StartDate || request.DaysCount <= 0)
        {
            throw new ValidationAppException(new Dictionary<string, string[]>
            {
                ["daysCount"] = ["Leave dates and days count must be valid."]
            });
        }

        int quotaYear = request.StartDate.Year;
        EmployeeLeaveBalance balance = await _dbContext.EmployeeLeaveBalances
            .FirstOrDefaultAsync(
                b => b.EmployeeId == employee.Id && b.LeaveTypeId == request.LeaveTypeId && b.QuotaYear == quotaYear,
                cancellationToken)
            ?? throw new NotFoundException("Leave balance not found for this leave type and year.");

        var reserveResult = balance.ReservePending(request.DaysCount);
        if (reserveResult.IsFailure)
        {
            throw new ConflictException(reserveResult.Error.Message);
        }

        WorkflowDefinition wf = await FindWorkflowDefinitionAsync(
            WorkflowRequestType.Leave,
            employee.DepartmentId,
            cancellationToken);

        string payloadJson = JsonSerializer.Serialize(new
        {
            leaveTypeId = request.LeaveTypeId,
            startDate = request.StartDate,
            endDate = request.EndDate,
            daysCount = request.DaysCount,
            quotaYear,
            reason = request.Reason
        });

        var approval = ApprovalRequest.Create(
            tenantId,
            companyId,
            employee.Id,
            WorkflowRequestType.Leave,
            Guid.CreateVersion7(),
            wf.Id,
            payloadJson,
            DateTimeOffset.UtcNow);

        _dbContext.ApprovalRequests.Add(approval);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapApprovalDto(approval, employee);
    }

    public async Task<ApprovalRequestDto> CancelLeaveRequestAsync(
        Guid approvalRequestId,
        CancellationToken cancellationToken = default)
    {
        Employee employee = await RequireCurrentEmployeeAsync(cancellationToken);

        ApprovalRequest approval = await _dbContext.ApprovalRequests
            .FirstOrDefaultAsync(a => a.Id == approvalRequestId, cancellationToken)
            ?? throw new NotFoundException("Approval request not found.");

        if (approval.RequesterEmployeeId != employee.Id)
        {
            throw new ForbiddenException("You can only cancel your own leave request.");
        }

        var cancelResult = approval.Cancel(DateTimeOffset.UtcNow);
        if (cancelResult.IsFailure)
        {
            throw new ConflictException(cancelResult.Error.Message);
        }

        if (approval.RequestType == WorkflowRequestType.Leave)
        {
            await ReleaseLeavePendingAsync(approval, cancellationToken);
        }

        _dbContext.ApprovalActionLogs.Add(
            ApprovalActionLog.Create(
                approval.TenantId,
                approval.Id,
                approval.CurrentStepOrder,
                employee.Id,
                ApprovalActionType.Cancel,
                "Cancelled by requester",
                DateTimeOffset.UtcNow));

        await _dbContext.SaveChangesAsync(cancellationToken);
        return MapApprovalDto(approval, employee);
    }

    public async Task<ApprovalRequestDto> SubmitOvertimeRequestAsync(
        SubmitOvertimeRequest request,
        CancellationToken cancellationToken = default)
    {
        (Guid tenantId, Guid companyId) = RequireTenantAndCompany();
        Employee employee = await RequireCurrentEmployeeAsync(cancellationToken);

        if (request.Hours <= 0)
        {
            throw new ValidationAppException(new Dictionary<string, string[]>
            {
                ["hours"] = ["Overtime hours must be greater than zero."]
            });
        }

        WorkflowDefinition wf = await FindWorkflowDefinitionAsync(
            WorkflowRequestType.Overtime,
            employee.DepartmentId,
            cancellationToken);

        string payloadJson = JsonSerializer.Serialize(new
        {
            workDate = request.WorkDate,
            startTime = request.StartTime,
            endTime = request.EndTime,
            hours = request.Hours,
            reason = request.Reason
        });

        var approval = ApprovalRequest.Create(
            tenantId,
            companyId,
            employee.Id,
            WorkflowRequestType.Overtime,
            Guid.CreateVersion7(),
            wf.Id,
            payloadJson,
            DateTimeOffset.UtcNow);

        _dbContext.ApprovalRequests.Add(approval);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapApprovalDto(approval, employee);
    }

    public async Task<IReadOnlyList<ApprovalRequestDto>> GetPendingApprovalsAsync(
        CancellationToken cancellationToken = default)
    {
        Employee currentEmployee = await RequireCurrentEmployeeAsync(cancellationToken);
        Guid currentUserId = _tenantContext.CurrentUserId!.Value;
        Guid companyId = _tenantContext.ActiveCompanyId!.Value;

        List<Guid> currentUserRoleIds = await _dbContext.UserCompanyRoles
            .AsNoTracking()
            .Where(r => r.UserId == currentUserId && r.CompanyId == companyId)
            .Select(r => r.RoleId)
            .ToListAsync(cancellationToken);

        List<ApprovalRequest> pending = await _dbContext.ApprovalRequests
            .AsNoTracking()
            .Where(a => a.Status == ApprovalStatus.Pending)
            .OrderByDescending(a => a.SubmittedAt)
            .ToListAsync(cancellationToken);

        List<Guid> wfIds = pending.Select(p => p.WorkflowDefinitionId).Distinct().ToList();
        List<WorkflowStep> steps = await _dbContext.WorkflowSteps
            .AsNoTracking()
            .Where(s => wfIds.Contains(s.WorkflowDefinitionId))
            .ToListAsync(cancellationToken);

        List<EmployeeReportingLine> reportingLines = await _dbContext.EmployeeReportingLines
            .AsNoTracking()
            .Where(r => r.ManagerEmployeeId == currentEmployee.Id)
            .ToListAsync(cancellationToken);

        var subordinateIds = new HashSet<Guid>(reportingLines.Select(r => r.EmployeeId));
        var employees = await _dbContext.Employees
            .AsNoTracking()
            .ToDictionaryAsync(e => e.Id, cancellationToken);

        var result = new List<ApprovalRequestDto>();
        foreach (ApprovalRequest req in pending)
        {
            WorkflowStep? currentStep = steps.FirstOrDefault(
                s => s.WorkflowDefinitionId == req.WorkflowDefinitionId && s.StepOrder == req.CurrentStepOrder);

            if (currentStep is null)
            {
                continue;
            }

            bool isApprover = IsCurrentUserStepApprover(
                currentStep,
                req.RequesterEmployeeId,
                currentEmployee.Id,
                subordinateIds,
                currentUserRoleIds);

            if (!isApprover)
            {
                continue;
            }

            employees.TryGetValue(req.RequesterEmployeeId, out Employee? requester);
            result.Add(MapApprovalDto(req, requester));
        }

        return result;
    }

    public async Task<IReadOnlyList<ApprovalRequestDto>> GetMyRequestsAsync(
        string requestType,
        CancellationToken cancellationToken = default)
    {
        Employee employee = await RequireCurrentEmployeeAsync(cancellationToken);
        string normalizedType = requestType.Trim().ToUpperInvariant();

        List<ApprovalRequest> requests = await _dbContext.ApprovalRequests
            .AsNoTracking()
            .Where(a => a.RequesterEmployeeId == employee.Id && a.RequestType == normalizedType)
            .OrderByDescending(a => a.SubmittedAt)
            .ToListAsync(cancellationToken);

        var result = new List<ApprovalRequestDto>();
        foreach (ApprovalRequest req in requests)
        {
            result.Add(MapApprovalDto(req, employee));
        }

        return result;
    }

    public async Task<IReadOnlyList<ApprovalRequestDto>> GetApprovalHistoryAsync(
        CancellationToken cancellationToken = default)
    {
        List<ApprovalRequest> requests = await _dbContext.ApprovalRequests
            .AsNoTracking()
            .Where(a => a.Status != ApprovalStatus.Pending)
            .OrderByDescending(a => a.CompletedAt ?? a.SubmittedAt)
            .Take(100)
            .ToListAsync(cancellationToken);

        var employees = await _dbContext.Employees
            .AsNoTracking()
            .ToDictionaryAsync(e => e.Id, cancellationToken);

        var result = new List<ApprovalRequestDto>();
        foreach (ApprovalRequest req in requests)
        {
            employees.TryGetValue(req.RequesterEmployeeId, out Employee? requester);
            result.Add(MapApprovalDto(req, requester));
        }

        return result;
    }

    public async Task<IReadOnlyList<WorkflowDefinitionDto>> GetWorkflowDefinitionsAsync(
        CancellationToken cancellationToken = default)
    {
        List<WorkflowDefinition> definitions = await _dbContext.WorkflowDefinitions
            .AsNoTracking()
            .OrderBy(w => w.RequestType)
            .ThenBy(w => w.Name)
            .ToListAsync(cancellationToken);

        List<Guid> definitionIds = definitions.Select(d => d.Id).ToList();
        List<WorkflowStep> allSteps = await _dbContext.WorkflowSteps
            .AsNoTracking()
            .Where(s => definitionIds.Contains(s.WorkflowDefinitionId))
            .OrderBy(s => s.StepOrder)
            .ToListAsync(cancellationToken);

        var departments = await _dbContext.Departments
            .AsNoTracking()
            .ToDictionaryAsync(d => d.Id, d => d.Name, cancellationToken);

        var roles = await _dbContext.Roles
            .AsNoTracking()
            .ToDictionaryAsync(r => r.Id, r => r.Name, cancellationToken);

        var employees = await _dbContext.Employees
            .AsNoTracking()
            .ToDictionaryAsync(e => e.Id, e => $"{e.FirstName} {e.LastName}".Trim(), cancellationToken);

        var result = new List<WorkflowDefinitionDto>();
        foreach (WorkflowDefinition def in definitions)
        {
            var stepDtos = new List<WorkflowStepDto>();
            foreach (WorkflowStep step in allSteps)
            {
                if (step.WorkflowDefinitionId != def.Id)
                {
                    continue;
                }

                string? roleName = step.SpecificRoleId.HasValue && roles.TryGetValue(step.SpecificRoleId.Value, out string? rn)
                    ? rn
                    : null;
                string? empName = step.SpecificEmployeeId.HasValue && employees.TryGetValue(step.SpecificEmployeeId.Value, out string? en)
                    ? en
                    : null;

                stepDtos.Add(new WorkflowStepDto(
                    step.Id,
                    step.StepOrder,
                    step.ApproverStrategy,
                    step.SpecificRoleId,
                    roleName,
                    step.SpecificEmployeeId,
                    empName));
            }

            string? deptName = def.DepartmentId.HasValue && departments.TryGetValue(def.DepartmentId.Value, out string? dn)
                ? dn
                : null;

            result.Add(new WorkflowDefinitionDto(
                def.Id,
                def.RequestType,
                def.DepartmentId,
                deptName,
                def.Name,
                def.IsActive,
                stepDtos));
        }

        return result;
    }

    public async Task<WorkflowDefinitionDto> CreateWorkflowDefinitionAsync(
        CreateWorkflowDefinitionRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.Steps.Count == 0)
        {
            throw new ValidationAppException(new Dictionary<string, string[]>
            {
                ["steps"] = ["At least one approval step is required."]
            });
        }

        (Guid tenantId, Guid companyId) = RequireTenantAndCompany();
        var definition = WorkflowDefinition.Create(
            tenantId,
            companyId,
            request.RequestType,
            request.DepartmentId,
            request.Name,
            request.IsActive);

        _dbContext.WorkflowDefinitions.Add(definition);

        foreach (WorkflowStepInput stepInput in request.Steps.OrderBy(s => s.StepOrder))
        {
            var step = WorkflowStep.Create(
                tenantId,
                definition.Id,
                stepInput.StepOrder,
                stepInput.ApproverStrategy,
                stepInput.SpecificRoleId,
                stepInput.SpecificEmployeeId);
            _dbContext.WorkflowSteps.Add(step);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        IReadOnlyList<WorkflowDefinitionDto> all = await GetWorkflowDefinitionsAsync(cancellationToken);
        return all.First(d => d.Id == definition.Id);
    }

    public async Task<WorkflowDefinitionDto> UpdateWorkflowDefinitionAsync(
        Guid id,
        UpdateWorkflowDefinitionRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.Steps.Count == 0)
        {
            throw new ValidationAppException(new Dictionary<string, string[]>
            {
                ["steps"] = ["At least one approval step is required."]
            });
        }

        WorkflowDefinition definition = await _dbContext.WorkflowDefinitions
            .FirstOrDefaultAsync(w => w.Id == id, cancellationToken)
            ?? throw new NotFoundException("Workflow definition not found.");

        definition.Update(request.Name, request.DepartmentId, request.IsActive);

        List<WorkflowStep> existingSteps = await _dbContext.WorkflowSteps
            .Where(s => s.WorkflowDefinitionId == definition.Id)
            .ToListAsync(cancellationToken);
        _dbContext.WorkflowSteps.RemoveRange(existingSteps);

        foreach (WorkflowStepInput stepInput in request.Steps.OrderBy(s => s.StepOrder))
        {
            var step = WorkflowStep.Create(
                definition.TenantId,
                definition.Id,
                stepInput.StepOrder,
                stepInput.ApproverStrategy,
                stepInput.SpecificRoleId,
                stepInput.SpecificEmployeeId);
            _dbContext.WorkflowSteps.Add(step);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        IReadOnlyList<WorkflowDefinitionDto> all = await GetWorkflowDefinitionsAsync(cancellationToken);
        return all.First(d => d.Id == definition.Id);
    }

    public async Task<ApprovalRequestDto> DecideApprovalAsync(
        Guid requestId,
        DecideApprovalRequest request,
        CancellationToken cancellationToken = default)
    {
        Employee actor = await RequireCurrentEmployeeAsync(cancellationToken);
        ApprovalRequest approval = await _dbContext.ApprovalRequests
            .FirstOrDefaultAsync(a => a.Id == requestId, cancellationToken)
            ?? throw new NotFoundException("Approval request not found.");

        int totalSteps = await _dbContext.WorkflowSteps
            .AsNoTracking()
            .CountAsync(s => s.WorkflowDefinitionId == approval.WorkflowDefinitionId, cancellationToken);

        int actedStepOrder = approval.CurrentStepOrder;
        DateTimeOffset utcNow = DateTimeOffset.UtcNow;
        string normalizedAction = request.Action.Trim().ToUpperInvariant();

        if (normalizedAction == ApprovalActionType.Approve)
        {
            var advanceResult = approval.AdvanceApproval(Math.Max(1, totalSteps), utcNow);
            if (advanceResult.IsFailure)
            {
                throw new ConflictException(advanceResult.Error.Message);
            }

            if (approval.Status == ApprovalStatus.Approved && approval.RequestType == WorkflowRequestType.Leave)
            {
                await ApproveLeavePendingAsync(approval, cancellationToken);
            }
        }
        else if (normalizedAction == ApprovalActionType.Reject)
        {
            var rejectResult = approval.Reject(utcNow);
            if (rejectResult.IsFailure)
            {
                throw new ConflictException(rejectResult.Error.Message);
            }

            if (approval.RequestType == WorkflowRequestType.Leave)
            {
                await ReleaseLeavePendingAsync(approval, cancellationToken);
            }
        }
        else
        {
            throw new ValidationAppException(new Dictionary<string, string[]>
            {
                ["action"] = ["Action must be APPROVE or REJECT."]
            });
        }

        _dbContext.ApprovalActionLogs.Add(
            ApprovalActionLog.Create(
                approval.TenantId,
                approval.Id,
                actedStepOrder,
                actor.Id,
                normalizedAction,
                request.Comment,
                utcNow));

        await _dbContext.SaveChangesAsync(cancellationToken);

        Employee? requester = await _dbContext.Employees
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == approval.RequesterEmployeeId, cancellationToken);

        return MapApprovalDto(approval, requester);
    }

    private async Task<WorkflowDefinition> FindWorkflowDefinitionAsync(
        string requestType,
        Guid departmentId,
        CancellationToken cancellationToken)
    {
        return await _dbContext.WorkflowDefinitions
            .AsNoTracking()
            .Where(w =>
                w.IsActive &&
                w.RequestType == requestType &&
                (w.DepartmentId == departmentId || w.DepartmentId == null))
            .OrderByDescending(w => w.DepartmentId.HasValue)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException($"No active workflow definition found for {requestType}.");
    }

    private async Task ApproveLeavePendingAsync(ApprovalRequest approval, CancellationToken cancellationToken)
    {
        (Guid leaveTypeId, int quotaYear, decimal daysCount) = ParseLeavePayload(approval.PayloadSummary);
        EmployeeLeaveBalance? balance = await _dbContext.EmployeeLeaveBalances
            .FirstOrDefaultAsync(
                b => b.EmployeeId == approval.RequesterEmployeeId && b.LeaveTypeId == leaveTypeId && b.QuotaYear == quotaYear,
                cancellationToken);

        balance?.ApprovePending(daysCount);
    }

    private async Task ReleaseLeavePendingAsync(ApprovalRequest approval, CancellationToken cancellationToken)
    {
        (Guid leaveTypeId, int quotaYear, decimal daysCount) = ParseLeavePayload(approval.PayloadSummary);
        EmployeeLeaveBalance? balance = await _dbContext.EmployeeLeaveBalances
            .FirstOrDefaultAsync(
                b => b.EmployeeId == approval.RequesterEmployeeId && b.LeaveTypeId == leaveTypeId && b.QuotaYear == quotaYear,
                cancellationToken);

        balance?.ReleasePending(daysCount);
    }

    private static (Guid LeaveTypeId, int QuotaYear, decimal DaysCount) ParseLeavePayload(string json)
    {
        using JsonDocument doc = JsonDocument.Parse(json);
        JsonElement root = doc.RootElement;
        Guid leaveTypeId = root.GetProperty("leaveTypeId").GetGuid();
        int quotaYear = root.GetProperty("quotaYear").GetInt32();
        decimal daysCount = root.GetProperty("daysCount").GetDecimal();
        return (leaveTypeId, quotaYear, daysCount);
    }

    private static bool IsCurrentUserStepApprover(
        WorkflowStep step,
        Guid requesterEmployeeId,
        Guid currentEmployeeId,
        HashSet<Guid> subordinateIds,
        List<Guid> currentUserRoleIds)
    {
        if (step.ApproverStrategy == ApproverStrategy.DirectManager ||
            step.ApproverStrategy == ApproverStrategy.SecondaryManager)
        {
            return subordinateIds.Contains(requesterEmployeeId);
        }

        if (step.ApproverStrategy == ApproverStrategy.SpecificEmployee)
        {
            return step.SpecificEmployeeId == currentEmployeeId;
        }

        if (step.ApproverStrategy == ApproverStrategy.SpecificRole && step.SpecificRoleId.HasValue)
        {
            return currentUserRoleIds.Contains(step.SpecificRoleId.Value);
        }

        return false;
    }

    private async Task<Employee> RequireCurrentEmployeeAsync(CancellationToken cancellationToken)
    {
        Guid userId = _tenantContext.CurrentUserId
            ?? throw new UnauthorizedException("Authenticated user context is required.");

        return await _dbContext.Employees
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.UserId == userId, cancellationToken)
            ?? throw new NotFoundException("Employee profile not found for the current user.");
    }

    private (Guid TenantId, Guid CompanyId) RequireTenantAndCompany()
    {
        Guid tenantId = _tenantContext.TenantId
            ?? throw new UnauthorizedException("Tenant context is required.");
        Guid companyId = _tenantContext.ActiveCompanyId
            ?? throw new ForbiddenException("Active company context is required.");
        return (tenantId, companyId);
    }

    private static ApprovalRequestDto MapApprovalDto(ApprovalRequest req, Employee? requester)
    {
        string fullName = requester is null ? string.Empty : $"{requester.FirstName} {requester.LastName}".Trim();
        return new ApprovalRequestDto(
            req.Id,
            req.RequesterEmployeeId,
            requester?.EmployeeCode ?? string.Empty,
            fullName,
            req.RequestType,
            req.ReferenceId,
            req.CurrentStepOrder,
            req.Status,
            req.PayloadSummary,
            req.SubmittedAt,
            req.CompletedAt);
    }
}

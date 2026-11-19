using HrmSaas.Application.Features.Workflows;
using HrmSaas.Infrastructure.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HrmSaas.WebApi.Controllers.v1;

[ApiController]
[Route("api/v1/leaves")]
[Authorize]
public sealed class LeavesController : ControllerBase
{
    private readonly LeaveAndWorkflowService _workflowService;

    public LeavesController(LeaveAndWorkflowService workflowService)
    {
        _workflowService = workflowService;
    }

    [HttpGet("policies")]
    public async Task<ActionResult<IReadOnlyList<LeavePolicyItemDto>>> GetPolicies(CancellationToken cancellationToken)
    {
        return Ok(await _workflowService.GetLeavePoliciesAsync(cancellationToken));
    }

    [HttpPost("policies")]
    [RequirePermission("workflow.policy.manage")]
    public async Task<ActionResult<LeavePolicyItemDto>> CreatePolicy(
        [FromBody] CreateLeavePolicyRequest request,
        CancellationToken cancellationToken)
    {
        LeavePolicyItemDto created = await _workflowService.CreateLeavePolicyAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetPolicies), new { id = created.Id }, created);
    }

    [HttpPut("policies/{id:guid}")]
    [RequirePermission("workflow.policy.manage")]
    public async Task<ActionResult<LeavePolicyItemDto>> UpdatePolicy(
        Guid id,
        [FromBody] UpdateLeavePolicyRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(await _workflowService.UpdateLeavePolicyAsync(id, request, cancellationToken));
    }

    [HttpGet("my-balances")]
    [RequirePermission("portal.leave.read")]
    public async Task<ActionResult<IReadOnlyList<MyLeaveBalanceDto>>> GetMyBalances(
        [FromQuery] int? year,
        CancellationToken cancellationToken)
    {
        return Ok(await _workflowService.GetMyLeaveBalancesAsync(year, cancellationToken));
    }

    [HttpGet("my-requests")]
    [RequirePermission("portal.leave.read")]
    public async Task<ActionResult<IReadOnlyList<ApprovalRequestDto>>> GetMyRequests(CancellationToken cancellationToken)
    {
        return Ok(await _workflowService.GetMyRequestsAsync(HrmSaas.Domain.Workflows.WorkflowRequestType.Leave, cancellationToken));
    }

    [HttpPost("requests")]
    [RequirePermission("portal.leave.read")]
    public async Task<ActionResult<ApprovalRequestDto>> SubmitLeaveRequest(
        [FromBody] SubmitLeaveRequest request,
        CancellationToken cancellationToken)
    {
        ApprovalRequestDto created = await _workflowService.SubmitLeaveRequestAsync(request, cancellationToken);
        return Accepted(created);
    }

    [HttpPost("requests/{id:guid}/cancel")]
    [RequirePermission("portal.leave.read")]
    public async Task<ActionResult<ApprovalRequestDto>> CancelLeaveRequest(
        Guid id,
        CancellationToken cancellationToken)
    {
        return Ok(await _workflowService.CancelLeaveRequestAsync(id, cancellationToken));
    }
}

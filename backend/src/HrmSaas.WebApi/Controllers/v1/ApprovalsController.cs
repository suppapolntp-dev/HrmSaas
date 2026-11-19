using HrmSaas.Application.Features.Workflows;
using HrmSaas.Infrastructure.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HrmSaas.WebApi.Controllers.v1;

[ApiController]
[Route("api/v1/approvals")]
[Authorize]
public sealed class ApprovalsController : ControllerBase
{
    private readonly LeaveAndWorkflowService _workflowService;

    public ApprovalsController(LeaveAndWorkflowService workflowService)
    {
        _workflowService = workflowService;
    }

    [HttpGet("pending")]
    [RequirePermission("workflow.approve")]
    public async Task<ActionResult<IReadOnlyList<ApprovalRequestDto>>> GetPending(CancellationToken cancellationToken)
    {
        return Ok(await _workflowService.GetPendingApprovalsAsync(cancellationToken));
    }

    [HttpGet("history")]
    [RequirePermission("workflow.approve")]
    public async Task<ActionResult<IReadOnlyList<ApprovalRequestDto>>> GetHistory(CancellationToken cancellationToken)
    {
        return Ok(await _workflowService.GetApprovalHistoryAsync(cancellationToken));
    }

    [HttpGet("routes")]
    [RequirePermission("workflow.route.manage")]
    public async Task<ActionResult<IReadOnlyList<WorkflowDefinitionDto>>> GetRoutes(CancellationToken cancellationToken)
    {
        return Ok(await _workflowService.GetWorkflowDefinitionsAsync(cancellationToken));
    }

    [HttpPost("routes")]
    [RequirePermission("workflow.route.manage")]
    public async Task<ActionResult<WorkflowDefinitionDto>> CreateRoute(
        [FromBody] CreateWorkflowDefinitionRequest request,
        CancellationToken cancellationToken)
    {
        WorkflowDefinitionDto created = await _workflowService.CreateWorkflowDefinitionAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetRoutes), new { id = created.Id }, created);
    }

    [HttpPut("routes/{id:guid}")]
    [RequirePermission("workflow.route.manage")]
    public async Task<ActionResult<WorkflowDefinitionDto>> UpdateRoute(
        Guid id,
        [FromBody] UpdateWorkflowDefinitionRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(await _workflowService.UpdateWorkflowDefinitionAsync(id, request, cancellationToken));
    }

    [HttpPost("{requestId:guid}/decide")]
    [RequirePermission("workflow.approve")]
    public async Task<ActionResult<ApprovalRequestDto>> Decide(
        Guid requestId,
        [FromBody] DecideApprovalRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(await _workflowService.DecideApprovalAsync(requestId, request, cancellationToken));
    }
}

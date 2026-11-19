using HrmSaas.Application.Features.Workflows;
using HrmSaas.Infrastructure.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HrmSaas.WebApi.Controllers.v1;

[ApiController]
[Route("api/v1/overtime")]
[Authorize]
public sealed class OvertimeController : ControllerBase
{
    private readonly LeaveAndWorkflowService _workflowService;

    public OvertimeController(LeaveAndWorkflowService workflowService)
    {
        _workflowService = workflowService;
    }

    [HttpGet("my-requests")]
    [RequirePermission("portal.overtime.read")]
    public async Task<ActionResult<IReadOnlyList<ApprovalRequestDto>>> GetMyRequests(CancellationToken cancellationToken)
    {
        return Ok(await _workflowService.GetMyRequestsAsync(HrmSaas.Domain.Workflows.WorkflowRequestType.Overtime, cancellationToken));
    }

    [HttpPost("requests")]
    [RequirePermission("portal.overtime.read")]
    public async Task<ActionResult<ApprovalRequestDto>> SubmitOvertimeRequest(
        [FromBody] SubmitOvertimeRequest request,
        CancellationToken cancellationToken)
    {
        ApprovalRequestDto created = await _workflowService.SubmitOvertimeRequestAsync(request, cancellationToken);
        return Accepted(created);
    }
}

using HrmSaas.Application.Common.Models;
using HrmSaas.Application.Features.Settings;
using HrmSaas.Infrastructure.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HrmSaas.WebApi.Controllers.v1;

[ApiController]
[Route("api/v1/audit-logs")]
[Authorize]
public sealed class AuditLogsController : ControllerBase
{
    private readonly RbacAndAuditService _service;

    public AuditLogsController(RbacAndAuditService service)
    {
        _service = service;
    }

    [HttpGet]
    [RequirePermission("settings.audit.read")]
    public async Task<ActionResult<PagedResult<AuditLogDto>>> GetAuditLogs(
        [FromQuery] AuditLogQuery query,
        CancellationToken cancellationToken)
    {
        return Ok(await _service.GetAuditLogsAsync(query, cancellationToken));
    }
}

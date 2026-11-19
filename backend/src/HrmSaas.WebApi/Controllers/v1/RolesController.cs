using HrmSaas.Application.Features.Settings;
using HrmSaas.Infrastructure.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HrmSaas.WebApi.Controllers.v1;

[ApiController]
[Route("api/v1")]
[Authorize]
public sealed class RolesController : ControllerBase
{
    private readonly RbacAndAuditService _service;

    public RolesController(RbacAndAuditService service)
    {
        _service = service;
    }

    [HttpGet("permissions")]
    [RequirePermission("settings.rbac.manage")]
    public async Task<ActionResult<IReadOnlyList<PermissionDto>>> GetPermissions(CancellationToken cancellationToken)
    {
        return Ok(await _service.GetPermissionsAsync(cancellationToken));
    }

    [HttpGet("roles")]
    [RequirePermission("settings.rbac.manage")]
    public async Task<ActionResult<IReadOnlyList<RoleDto>>> GetRoles(CancellationToken cancellationToken)
    {
        return Ok(await _service.GetRolesAsync(cancellationToken));
    }

    [HttpPost("roles")]
    [RequirePermission("settings.rbac.manage")]
    public async Task<ActionResult<RoleDto>> CreateRole(
        [FromBody] CreateRoleRequest request,
        CancellationToken cancellationToken)
    {
        RoleDto created = await _service.CreateRoleAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetRoles), new { id = created.Id }, created);
    }

    [HttpPut("roles/{id:guid}")]
    [RequirePermission("settings.rbac.manage")]
    public async Task<ActionResult<RoleDto>> UpdateRole(
        Guid id,
        [FromBody] UpdateRoleRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(await _service.UpdateRoleAsync(id, request, cancellationToken));
    }
}

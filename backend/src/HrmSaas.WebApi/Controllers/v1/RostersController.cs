using HrmSaas.Application.Features.Attendance;
using HrmSaas.Infrastructure.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HrmSaas.WebApi.Controllers.v1;

[ApiController]
[Route("api/v1/rosters")]
[Authorize]
public sealed class RostersController : ControllerBase
{
    private readonly AttendanceService _attendanceService;

    public RostersController(AttendanceService attendanceService)
    {
        _attendanceService = attendanceService;
    }

    [HttpGet("my-upcoming")]
    [RequirePermission("portal.attendance.read")]
    public async Task<ActionResult<IReadOnlyList<MyUpcomingRosterDto>>> GetMyUpcoming(
        [FromQuery] int days = 7,
        CancellationToken cancellationToken = default)
    {
        return Ok(await _attendanceService.GetMyUpcomingRostersAsync(days, cancellationToken));
    }

    [HttpGet]
    [RequirePermission("attendance.roster.manage")]
    public async Task<ActionResult<IReadOnlyList<RosterItemDto>>> GetRosters(
        [FromQuery] DateOnly startDate,
        [FromQuery] DateOnly endDate,
        [FromQuery] Guid? branchId,
        [FromQuery] Guid? departmentId,
        CancellationToken cancellationToken)
    {
        return Ok(await _attendanceService.GetRostersAsync(startDate, endDate, branchId, departmentId, cancellationToken));
    }

    [HttpPost("bulk-assign")]
    [RequirePermission("attendance.roster.manage")]
    public async Task<IActionResult> BulkAssign(
        [FromBody] BulkAssignRosterRequest request,
        CancellationToken cancellationToken)
    {
        int assignedCount = await _attendanceService.BulkAssignRostersAsync(request, cancellationToken);
        return Ok(new { assignedCount });
    }
}

using System.Net;
using HrmSaas.Application.Features.Attendance;
using HrmSaas.Infrastructure.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace HrmSaas.WebApi.Controllers.v1;

[ApiController]
[Route("api/v1/attendance")]
[Authorize]
public sealed class AttendanceController : ControllerBase
{
    private readonly AttendanceService _attendanceService;

    public AttendanceController(AttendanceService attendanceService)
    {
        _attendanceService = attendanceService;
    }

    [HttpGet("my-today")]
    [RequirePermission("portal.attendance.read")]
    public async Task<ActionResult<MyTodayAttendanceDto>> GetMyToday(CancellationToken cancellationToken)
    {
        return Ok(await _attendanceService.GetMyTodayAttendanceAsync(cancellationToken));
    }

    [HttpGet("my-history")]
    [RequirePermission("portal.attendance.read")]
    public async Task<ActionResult<IReadOnlyList<AttendanceDailySummaryDto>>> GetMyHistory(
        [FromQuery] DateOnly? startDate,
        [FromQuery] DateOnly? endDate,
        [FromQuery] string? status,
        CancellationToken cancellationToken)
    {
        return Ok(await _attendanceService.GetMyAttendanceHistoryAsync(
            startDate,
            endDate,
            status,
            cancellationToken));
    }

    [HttpPost("check-in")]
    [RequirePermission("portal.attendance.read")]
    [EnableRateLimiting("check-in")]
    public async Task<ActionResult<AttendanceDailySummaryDto>> CheckIn(
        [FromBody] CheckInOutRequest request,
        CancellationToken cancellationToken)
    {
        IPAddress clientIp = HttpContext.Connection.RemoteIpAddress ?? IPAddress.Loopback;
        return Ok(await _attendanceService.CheckInAsync(request, clientIp, cancellationToken));
    }

    [HttpPost("check-out")]
    [RequirePermission("portal.attendance.read")]
    [EnableRateLimiting("check-in")]
    public async Task<ActionResult<AttendanceDailySummaryDto>> CheckOut(
        [FromBody] CheckInOutRequest request,
        CancellationToken cancellationToken)
    {
        IPAddress clientIp = HttpContext.Connection.RemoteIpAddress ?? IPAddress.Loopback;
        return Ok(await _attendanceService.CheckOutAsync(request, clientIp, cancellationToken));
    }

    [HttpGet("daily-summary")]
    [RequirePermission("attendance.log.read")]
    public async Task<ActionResult<IReadOnlyList<AttendanceDailySummaryDto>>> GetDailySummary(
        [FromQuery] DateOnly startDate,
        [FromQuery] DateOnly endDate,
        [FromQuery] Guid? branchId,
        [FromQuery] Guid? departmentId,
        [FromQuery] string? status,
        CancellationToken cancellationToken)
    {
        return Ok(await _attendanceService.GetDailySummariesAsync(
            startDate,
            endDate,
            branchId,
            departmentId,
            status,
            cancellationToken));
    }

    [HttpPost("time-adjustments")]
    [RequirePermission("portal.attendance.read")]
    public async Task<ActionResult<TimeAdjustmentCreatedDto>> RequestTimeAdjustment(
        [FromBody] TimeAdjustmentRequest request,
        CancellationToken cancellationToken)
    {
        TimeAdjustmentCreatedDto created = await _attendanceService.RequestTimeAdjustmentAsync(request, cancellationToken);
        return Accepted(created);
    }
}

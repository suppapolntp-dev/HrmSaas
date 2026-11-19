using HrmSaas.Application.Features.Attendance;
using HrmSaas.Infrastructure.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HrmSaas.WebApi.Controllers.v1;

[ApiController]
[Route("api/v1/shifts")]
[Authorize]
public sealed class ShiftsController : ControllerBase
{
    private readonly AttendanceService _attendanceService;

    public ShiftsController(AttendanceService attendanceService)
    {
        _attendanceService = attendanceService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ShiftDefinitionDto>>> GetShifts(CancellationToken cancellationToken)
    {
        return Ok(await _attendanceService.GetShiftsAsync(cancellationToken));
    }

    [HttpPost]
    [RequirePermission("attendance.shift.manage")]
    public async Task<ActionResult<ShiftDefinitionDto>> CreateShift(
        [FromBody] CreateShiftRequest request,
        CancellationToken cancellationToken)
    {
        ShiftDefinitionDto created = await _attendanceService.CreateShiftAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetShifts), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    [RequirePermission("attendance.shift.manage")]
    public async Task<ActionResult<ShiftDefinitionDto>> UpdateShift(
        Guid id,
        [FromBody] UpdateShiftRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(await _attendanceService.UpdateShiftAsync(id, request, cancellationToken));
    }
}

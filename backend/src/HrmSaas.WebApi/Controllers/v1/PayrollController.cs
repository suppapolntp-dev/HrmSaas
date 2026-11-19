using HrmSaas.Application.Features.Payroll;
using HrmSaas.Infrastructure.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HrmSaas.WebApi.Controllers.v1;

[ApiController]
[Route("api/v1/payroll")]
[Authorize]
public sealed class PayrollController : ControllerBase
{
    private readonly PayrollService _payrollService;

    public PayrollController(PayrollService payrollService)
    {
        _payrollService = payrollService;
    }

    [HttpGet("periods")]
    [RequirePermission("payroll.period.manage")]
    public async Task<ActionResult<IReadOnlyList<PayrollPeriodDto>>> GetPeriods(CancellationToken cancellationToken)
    {
        return Ok(await _payrollService.GetPeriodsAsync(cancellationToken));
    }

    [HttpPost("periods")]
    [RequirePermission("payroll.period.manage")]
    public async Task<ActionResult<PayrollPeriodDto>> CreatePeriod(
        [FromBody] CreatePayrollPeriodRequest request,
        CancellationToken cancellationToken)
    {
        PayrollPeriodDto created = await _payrollService.CreatePeriodAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetPeriods), new { id = created.Id }, created);
    }

    [HttpPost("periods/{id:guid}/calculate")]
    [RequirePermission("payroll.calculate")]
    public async Task<ActionResult<PayrollCalculationSummaryDto>> CalculatePeriod(
        Guid id,
        CancellationToken cancellationToken)
    {
        return Ok(await _payrollService.CalculatePeriodAsync(id, cancellationToken));
    }

    [HttpPost("periods/{id:guid}/lock")]
    [RequirePermission("payroll.calculate")]
    public async Task<ActionResult<PayrollPeriodDto>> LockPeriod(
        Guid id,
        CancellationToken cancellationToken)
    {
        return Ok(await _payrollService.LockPeriodAsync(id, cancellationToken));
    }

    [HttpGet("payslips")]
    [RequirePermission("portal.payslip.read")]
    public async Task<ActionResult<IReadOnlyList<PayslipDto>>> GetPayslips(
        [FromQuery] Guid? payrollPeriodId,
        [FromQuery] bool mineOnly = false,
        CancellationToken cancellationToken = default)
    {
        bool canReadAll = !mineOnly && User.Claims.Any(
            c => c.Type == "permission" &&
                 (string.Equals(c.Value, "payroll.calculate", StringComparison.OrdinalIgnoreCase) ||
                  string.Equals(c.Value, "payroll.period.manage", StringComparison.OrdinalIgnoreCase)));

        return Ok(await _payrollService.GetPayslipsAsync(payrollPeriodId, canReadAll, cancellationToken));
    }
}

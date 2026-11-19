using HrmSaas.Application.Features.Dashboard;
using HrmSaas.Infrastructure.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HrmSaas.WebApi.Controllers.v1;

[ApiController]
[Route("api/v1/dashboard")]
[Authorize]
public sealed class DashboardController : ControllerBase
{
    private readonly DashboardService _dashboardService;

    public DashboardController(DashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("my-summary")]
    public async Task<ActionResult<MyDashboardSummaryDto>> GetMySummary(CancellationToken cancellationToken)
    {
        bool includePayslip = User.Claims.Any(c =>
            c.Type == "permission" &&
            string.Equals(c.Value, "portal.payslip.read", StringComparison.OrdinalIgnoreCase));

        return Ok(await _dashboardService.GetMySummaryAsync(includePayslip, cancellationToken));
    }

    [HttpGet("team-pulse")]
    [RequirePermission("attendance.log.read")]
    public async Task<ActionResult<TeamPulseDto>> GetTeamPulse(CancellationToken cancellationToken)
    {
        return Ok(await _dashboardService.GetTeamPulseAsync(cancellationToken));
    }

    [HttpGet("hr-overview")]
    [RequirePermission("employee.read")]
    public async Task<ActionResult<HrOverviewDto>> GetHrOverview(CancellationToken cancellationToken)
    {
        return Ok(await _dashboardService.GetHrOverviewAsync(cancellationToken));
    }

    [HttpGet("payroll-readiness")]
    public async Task<ActionResult<PayrollReadinessDto>> GetPayrollReadiness(CancellationToken cancellationToken)
    {
        bool hasPayrollAccess = User.Claims.Any(c =>
            c.Type == "permission" &&
            (string.Equals(c.Value, "payroll.period.manage", StringComparison.OrdinalIgnoreCase) ||
             string.Equals(c.Value, "payroll.calculate", StringComparison.OrdinalIgnoreCase)));

        if (!hasPayrollAccess)
        {
            return Forbid();
        }

        return Ok(await _dashboardService.GetPayrollReadinessAsync(cancellationToken));
    }
}

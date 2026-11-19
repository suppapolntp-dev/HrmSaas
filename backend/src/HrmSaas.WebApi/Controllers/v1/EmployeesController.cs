using HrmSaas.Application.Common.Models;
using HrmSaas.Application.Features.Employees;
using HrmSaas.Infrastructure.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HrmSaas.WebApi.Controllers.v1;

[ApiController]
[Route("api/v1/employees")]
[Authorize]
public sealed class EmployeesController : ControllerBase
{
    private readonly EmployeeService _employeeService;

    public EmployeesController(EmployeeService employeeService)
    {
        _employeeService = employeeService;
    }

    [HttpGet]
    [RequirePermission("employee.read")]
    public async Task<ActionResult<PagedResult<EmployeeListItemDto>>> GetEmployees(
        [FromQuery] EmployeeListQuery query,
        CancellationToken cancellationToken)
    {
        return Ok(await _employeeService.GetEmployeesAsync(query, cancellationToken));
    }

    [HttpGet("{id:guid}")]
    [RequirePermission("employee.read")]
    public async Task<ActionResult<Employee360Dto>> GetEmployeeById(
        Guid id,
        CancellationToken cancellationToken)
    {
        return Ok(await _employeeService.GetEmployeeByIdAsync(id, cancellationToken));
    }

    [HttpPost("onboarding")]
    [RequirePermission("employee.create")]
    public async Task<ActionResult<Employee360Dto>> OnboardEmployee(
        [FromBody] OnboardEmployeeRequest request,
        CancellationToken cancellationToken)
    {
        Employee360Dto created = await _employeeService.OnboardEmployeeAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetEmployeeById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    [RequirePermission("employee.update")]
    public async Task<ActionResult<Employee360Dto>> UpdateEmployee(
        Guid id,
        [FromBody] UpdateEmployeeRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(await _employeeService.UpdateEmployeeAsync(id, request, cancellationToken));
    }

    [HttpPut("{id:guid}/reporting-lines")]
    [RequirePermission("employee.update")]
    public async Task<IActionResult> UpdateReportingLines(
        Guid id,
        [FromBody] UpdateReportingLinesRequest request,
        CancellationToken cancellationToken)
    {
        await _employeeService.UpdateReportingLinesAsync(id, request, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/contracts")]
    [RequirePermission("employee.update")]
    public async Task<ActionResult<EmployeeContractDto>> CreateContract(
        Guid id,
        [FromBody] CreateContractRequest request,
        CancellationToken cancellationToken)
    {
        EmployeeContractDto created = await _employeeService.CreateContractAsync(id, request, cancellationToken);
        return CreatedAtAction(nameof(GetEmployeeById), new { id }, created);
    }
}

using HrmSaas.Application.Features.Organizations;
using HrmSaas.Infrastructure.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HrmSaas.WebApi.Controllers.v1;

[ApiController]
[Route("api/v1/organizations")]
[Authorize]
public sealed class OrganizationsController : ControllerBase
{
    private readonly OrganizationService _organizationService;

    public OrganizationsController(OrganizationService organizationService)
    {
        _organizationService = organizationService;
    }

    [HttpGet("companies")]
    public async Task<ActionResult<IReadOnlyList<CompanyDto>>> GetCompanies(CancellationToken cancellationToken)
    {
        return Ok(await _organizationService.GetCompaniesAsync(cancellationToken));
    }

    [HttpPost("companies")]
    [RequirePermission("org.branch.manage")]
    public async Task<ActionResult<CompanyDto>> CreateCompany(
        [FromBody] CreateCompanyRequest request,
        CancellationToken cancellationToken)
    {
        CompanyDto created = await _organizationService.CreateCompanyAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetCompanies), new { id = created.Id }, created);
    }

    [HttpPut("companies/{id:guid}")]
    [RequirePermission("org.branch.manage")]
    public async Task<ActionResult<CompanyDto>> UpdateCompany(
        Guid id,
        [FromBody] UpdateCompanyRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(await _organizationService.UpdateCompanyAsync(id, request, cancellationToken));
    }

    [HttpGet("branches")]
    public async Task<ActionResult<IReadOnlyList<BranchDto>>> GetBranches(CancellationToken cancellationToken)
    {
        return Ok(await _organizationService.GetBranchesAsync(cancellationToken));
    }

    [HttpPost("branches")]
    [RequirePermission("org.branch.manage")]
    public async Task<ActionResult<BranchDto>> CreateBranch(
        [FromBody] CreateBranchRequest request,
        CancellationToken cancellationToken)
    {
        BranchDto created = await _organizationService.CreateBranchAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetBranches), new { id = created.Id }, created);
    }

    [HttpPut("branches/{id:guid}")]
    [RequirePermission("org.branch.manage")]
    public async Task<ActionResult<BranchDto>> UpdateBranchGeofence(
        Guid id,
        [FromBody] UpdateBranchGeofenceRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(await _organizationService.UpdateBranchGeofenceAsync(id, request, cancellationToken));
    }

    [HttpGet("departments")]
    public async Task<ActionResult<IReadOnlyList<DepartmentDto>>> GetDepartments(CancellationToken cancellationToken)
    {
        return Ok(await _organizationService.GetDepartmentsAsync(cancellationToken));
    }

    [HttpGet("departments/tree")]
    public async Task<ActionResult<IReadOnlyList<DepartmentTreeNodeDto>>> GetDepartmentTree(CancellationToken cancellationToken)
    {
        return Ok(await _organizationService.GetDepartmentTreeAsync(cancellationToken));
    }

    [HttpPost("departments")]
    [RequirePermission("org.structure.manage")]
    public async Task<ActionResult<DepartmentDto>> CreateDepartment(
        [FromBody] CreateDepartmentRequest request,
        CancellationToken cancellationToken)
    {
        DepartmentDto created = await _organizationService.CreateDepartmentAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetDepartments), new { id = created.Id }, created);
    }

    [HttpPut("departments/{id:guid}")]
    [RequirePermission("org.structure.manage")]
    public async Task<ActionResult<DepartmentDto>> UpdateDepartment(
        Guid id,
        [FromBody] UpdateDepartmentRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(await _organizationService.UpdateDepartmentAsync(id, request, cancellationToken));
    }

    [HttpDelete("departments/{id:guid}")]
    [RequirePermission("org.structure.manage")]
    public async Task<IActionResult> DeleteDepartment(Guid id, CancellationToken cancellationToken)
    {
        await _organizationService.DeleteDepartmentAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpGet("positions")]
    public async Task<ActionResult<IReadOnlyList<PositionDto>>> GetPositions(CancellationToken cancellationToken)
    {
        return Ok(await _organizationService.GetPositionsAsync(cancellationToken));
    }

    [HttpPost("positions")]
    [RequirePermission("org.structure.manage")]
    public async Task<ActionResult<PositionDto>> CreatePosition(
        [FromBody] CreatePositionRequest request,
        CancellationToken cancellationToken)
    {
        PositionDto created = await _organizationService.CreatePositionAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetPositions), new { id = created.Id }, created);
    }

    [HttpPut("positions/{id:guid}")]
    [RequirePermission("org.structure.manage")]
    public async Task<ActionResult<PositionDto>> UpdatePosition(
        Guid id,
        [FromBody] UpdatePositionRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(await _organizationService.UpdatePositionAsync(id, request, cancellationToken));
    }

    [HttpGet("job-grades")]
    public async Task<ActionResult<IReadOnlyList<JobGradeDto>>> GetJobGrades(CancellationToken cancellationToken)
    {
        return Ok(await _organizationService.GetJobGradesAsync(cancellationToken));
    }

    [HttpPost("job-grades")]
    [RequirePermission("org.structure.manage")]
    public async Task<ActionResult<JobGradeDto>> CreateJobGrade(
        [FromBody] CreateJobGradeRequest request,
        CancellationToken cancellationToken)
    {
        JobGradeDto created = await _organizationService.CreateJobGradeAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetJobGrades), new { id = created.Id }, created);
    }

    [HttpPut("job-grades/{id:guid}")]
    [RequirePermission("org.structure.manage")]
    public async Task<ActionResult<JobGradeDto>> UpdateJobGrade(
        Guid id,
        [FromBody] UpdateJobGradeRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(await _organizationService.UpdateJobGradeAsync(id, request, cancellationToken));
    }
}

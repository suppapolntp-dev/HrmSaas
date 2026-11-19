using HrmSaas.Application.Common.Exceptions;
using HrmSaas.Application.Common.Interfaces;
using HrmSaas.Domain.Organizations;
using Microsoft.EntityFrameworkCore;

namespace HrmSaas.Application.Features.Organizations;

public sealed class OrganizationService
{
    private readonly IAppDbContext _dbContext;
    private readonly ITenantContext _tenantContext;

    public OrganizationService(IAppDbContext dbContext, ITenantContext tenantContext)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
    }

    public async Task<IReadOnlyList<CompanyDto>> GetCompaniesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Companies
            .AsNoTracking()
            .OrderBy(c => c.Code)
            .Select(c => new CompanyDto(c.Id, c.Code, c.LegalName, c.TaxId, c.SsoEmployerCode, c.BaseCurrency))
            .ToListAsync(cancellationToken);
    }

    public async Task<CompanyDto> CreateCompanyAsync(
        CreateCompanyRequest request,
        CancellationToken cancellationToken = default)
    {
        Guid tenantId = RequireTenantId();
        string normalizedCode = request.Code.Trim().ToUpperInvariant();

        bool duplicate = await _dbContext.Companies
            .AnyAsync(c => c.Code == normalizedCode, cancellationToken);

        if (duplicate)
        {
            throw new ConflictException($"Company code '{normalizedCode}' already exists.");
        }

        var company = Company.Create(
            tenantId,
            normalizedCode,
            request.LegalName,
            request.TaxId,
            request.SsoEmployerCode,
            request.BaseCurrency);

        _dbContext.Companies.Add(company);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new CompanyDto(
            company.Id,
            company.Code,
            company.LegalName,
            company.TaxId,
            company.SsoEmployerCode,
            company.BaseCurrency);
    }

    public async Task<CompanyDto> UpdateCompanyAsync(
        Guid id,
        UpdateCompanyRequest request,
        CancellationToken cancellationToken = default)
    {
        Company company = await _dbContext.Companies
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken)
            ?? throw new NotFoundException("Company not found.");

        company.UpdateDetails(request.LegalName, request.TaxId, request.SsoEmployerCode, request.BaseCurrency);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new CompanyDto(
            company.Id,
            company.Code,
            company.LegalName,
            company.TaxId,
            company.SsoEmployerCode,
            company.BaseCurrency);
    }

    public async Task<IReadOnlyList<BranchDto>> GetBranchesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Branches
            .AsNoTracking()
            .OrderBy(b => b.Code)
            .Select(b => new BranchDto(
                b.Id,
                b.CompanyId,
                b.Code,
                b.Name,
                b.Timezone,
                b.Latitude,
                b.Longitude,
                b.GeofenceRadiusMeters,
                b.AllowedIps))
            .ToListAsync(cancellationToken);
    }

    public async Task<BranchDto> CreateBranchAsync(
        CreateBranchRequest request,
        CancellationToken cancellationToken = default)
    {
        (Guid tenantId, Guid companyId) = RequireTenantAndCompany();
        string normalizedCode = request.Code.Trim().ToUpperInvariant();

        bool duplicate = await _dbContext.Branches
            .AnyAsync(b => b.Code == normalizedCode, cancellationToken);

        if (duplicate)
        {
            throw new ConflictException($"Branch code '{normalizedCode}' already exists.");
        }

        var branch = Branch.Create(
            tenantId,
            companyId,
            normalizedCode,
            request.Name,
            request.Timezone,
            request.Latitude,
            request.Longitude,
            request.GeofenceRadiusMeters,
            request.AllowedIps);

        _dbContext.Branches.Add(branch);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new BranchDto(
            branch.Id,
            branch.CompanyId,
            branch.Code,
            branch.Name,
            branch.Timezone,
            branch.Latitude,
            branch.Longitude,
            branch.GeofenceRadiusMeters,
            branch.AllowedIps);
    }

    public async Task<BranchDto> UpdateBranchGeofenceAsync(
        Guid id,
        UpdateBranchGeofenceRequest request,
        CancellationToken cancellationToken = default)
    {
        Branch branch = await _dbContext.Branches
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken)
            ?? throw new NotFoundException("Branch not found.");

        branch.UpdateGeofence(request.Latitude, request.Longitude, request.GeofenceRadiusMeters, request.AllowedIps);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new BranchDto(
            branch.Id,
            branch.CompanyId,
            branch.Code,
            branch.Name,
            branch.Timezone,
            branch.Latitude,
            branch.Longitude,
            branch.GeofenceRadiusMeters,
            branch.AllowedIps);
    }

    public async Task<IReadOnlyList<DepartmentDto>> GetDepartmentsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Departments
            .AsNoTracking()
            .OrderBy(d => d.Code)
            .Select(d => new DepartmentDto(d.Id, d.CompanyId, d.ParentDepartmentId, d.Code, d.Name, d.HeadEmployeeId))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<DepartmentTreeNodeDto>> GetDepartmentTreeAsync(CancellationToken cancellationToken = default)
    {
        List<DepartmentDto> all = await _dbContext.Departments
            .AsNoTracking()
            .OrderBy(d => d.Code)
            .Select(d => new DepartmentDto(d.Id, d.CompanyId, d.ParentDepartmentId, d.Code, d.Name, d.HeadEmployeeId))
            .ToListAsync(cancellationToken);

        var nodeById = new Dictionary<Guid, DepartmentTreeNodeDto>();
        foreach (DepartmentDto item in all)
        {
            nodeById[item.Id] = new DepartmentTreeNodeDto(item.Id, item.Code, item.Name, item.HeadEmployeeId, []);
        }

        var roots = new List<DepartmentTreeNodeDto>();
        foreach (DepartmentDto item in all)
        {
            DepartmentTreeNodeDto node = nodeById[item.Id];
            if (item.ParentDepartmentId.HasValue && nodeById.TryGetValue(item.ParentDepartmentId.Value, out DepartmentTreeNodeDto? parent))
            {
                parent.Children.Add(node);
            }
            else
            {
                roots.Add(node);
            }
        }

        return roots;
    }

    public async Task<DepartmentDto> CreateDepartmentAsync(
        CreateDepartmentRequest request,
        CancellationToken cancellationToken = default)
    {
        (Guid tenantId, Guid companyId) = RequireTenantAndCompany();
        string normalizedCode = request.Code.Trim().ToUpperInvariant();

        bool duplicate = await _dbContext.Departments
            .AnyAsync(d => d.Code == normalizedCode, cancellationToken);

        if (duplicate)
        {
            throw new ConflictException($"Department code '{normalizedCode}' already exists.");
        }

        var department = Department.Create(
            tenantId,
            companyId,
            normalizedCode,
            request.Name,
            request.ParentDepartmentId,
            request.HeadEmployeeId);

        _dbContext.Departments.Add(department);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new DepartmentDto(
            department.Id,
            department.CompanyId,
            department.ParentDepartmentId,
            department.Code,
            department.Name,
            department.HeadEmployeeId);
    }

    public async Task<DepartmentDto> UpdateDepartmentAsync(
        Guid id,
        UpdateDepartmentRequest request,
        CancellationToken cancellationToken = default)
    {
        Department department = await _dbContext.Departments
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken)
            ?? throw new NotFoundException("Department not found.");

        department.Update(request.Name, request.ParentDepartmentId, request.HeadEmployeeId);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new DepartmentDto(
            department.Id,
            department.CompanyId,
            department.ParentDepartmentId,
            department.Code,
            department.Name,
            department.HeadEmployeeId);
    }

    public async Task DeleteDepartmentAsync(Guid id, CancellationToken cancellationToken = default)
    {
        Department department = await _dbContext.Departments
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken)
            ?? throw new NotFoundException("Department not found.");

        bool hasChildren = await _dbContext.Departments
            .AnyAsync(d => d.ParentDepartmentId == id, cancellationToken);

        if (hasChildren)
        {
            throw new ConflictException("Cannot delete a department that still has child departments.");
        }

        department.SoftDelete(_tenantContext.CurrentUserId, DateTimeOffset.UtcNow);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PositionDto>> GetPositionsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Positions
            .AsNoTracking()
            .OrderBy(p => p.Code)
            .Select(p => new PositionDto(p.Id, p.CompanyId, p.Code, p.Title))
            .ToListAsync(cancellationToken);
    }

    public async Task<PositionDto> CreatePositionAsync(
        CreatePositionRequest request,
        CancellationToken cancellationToken = default)
    {
        (Guid tenantId, Guid companyId) = RequireTenantAndCompany();
        string normalizedCode = request.Code.Trim().ToUpperInvariant();

        bool duplicate = await _dbContext.Positions
            .AnyAsync(p => p.Code == normalizedCode, cancellationToken);

        if (duplicate)
        {
            throw new ConflictException($"Position code '{normalizedCode}' already exists.");
        }

        var position = Position.Create(tenantId, companyId, normalizedCode, request.Title);
        _dbContext.Positions.Add(position);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new PositionDto(position.Id, position.CompanyId, position.Code, position.Title);
    }

    public async Task<PositionDto> UpdatePositionAsync(
        Guid id,
        UpdatePositionRequest request,
        CancellationToken cancellationToken = default)
    {
        Position position = await _dbContext.Positions
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken)
            ?? throw new NotFoundException("Position not found.");

        position.Rename(request.Title);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new PositionDto(position.Id, position.CompanyId, position.Code, position.Title);
    }

    public async Task<IReadOnlyList<JobGradeDto>> GetJobGradesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.JobGrades
            .AsNoTracking()
            .OrderBy(g => g.LevelOrder)
            .Select(g => new JobGradeDto(g.Id, g.CompanyId, g.Code, g.LevelOrder, g.Name))
            .ToListAsync(cancellationToken);
    }

    public async Task<JobGradeDto> CreateJobGradeAsync(
        CreateJobGradeRequest request,
        CancellationToken cancellationToken = default)
    {
        (Guid tenantId, Guid companyId) = RequireTenantAndCompany();
        string normalizedCode = request.Code.Trim().ToUpperInvariant();

        bool duplicate = await _dbContext.JobGrades
            .AnyAsync(g => g.Code == normalizedCode, cancellationToken);

        if (duplicate)
        {
            throw new ConflictException($"Job grade code '{normalizedCode}' already exists.");
        }

        var grade = JobGrade.Create(tenantId, companyId, normalizedCode, request.LevelOrder, request.Name);
        _dbContext.JobGrades.Add(grade);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new JobGradeDto(grade.Id, grade.CompanyId, grade.Code, grade.LevelOrder, grade.Name);
    }

    public async Task<JobGradeDto> UpdateJobGradeAsync(
        Guid id,
        UpdateJobGradeRequest request,
        CancellationToken cancellationToken = default)
    {
        JobGrade grade = await _dbContext.JobGrades
            .FirstOrDefaultAsync(g => g.Id == id, cancellationToken)
            ?? throw new NotFoundException("Job grade not found.");

        grade.Update(request.LevelOrder, request.Name);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new JobGradeDto(grade.Id, grade.CompanyId, grade.Code, grade.LevelOrder, grade.Name);
    }

    private Guid RequireTenantId()
    {
        return _tenantContext.TenantId ?? throw new UnauthorizedException("Tenant context is required.");
    }

    private (Guid TenantId, Guid CompanyId) RequireTenantAndCompany()
    {
        Guid tenantId = RequireTenantId();
        Guid companyId = _tenantContext.ActiveCompanyId
            ?? throw new ForbiddenException("Active company context is required.");
        return (tenantId, companyId);
    }
}

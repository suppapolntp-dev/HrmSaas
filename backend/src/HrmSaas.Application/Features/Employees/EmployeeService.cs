using HrmSaas.Application.Common.Exceptions;
using HrmSaas.Application.Common.Interfaces;
using HrmSaas.Application.Common.Models;
using HrmSaas.Domain.Employees;
using HrmSaas.Domain.Identity;
using HrmSaas.Domain.Workflows;
using Microsoft.EntityFrameworkCore;

namespace HrmSaas.Application.Features.Employees;

public sealed class EmployeeService
{
    private readonly IAppDbContext _dbContext;
    private readonly ITenantContext _tenantContext;
    private readonly IPasswordHasher _passwordHasher;

    public EmployeeService(
        IAppDbContext dbContext,
        ITenantContext tenantContext,
        IPasswordHasher passwordHasher)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
        _passwordHasher = passwordHasher;
    }

    public async Task<PagedResult<EmployeeListItemDto>> GetEmployeesAsync(
        EmployeeListQuery query,
        CancellationToken cancellationToken = default)
    {
        int page = query.Page < 1 ? 1 : query.Page;
        int pageSize = query.PageSize is < 1 or > 100 ? 20 : query.PageSize;

        var baseQuery = _dbContext.Employees.AsNoTracking();

        if (query.BranchId.HasValue)
        {
            baseQuery = baseQuery.Where(e => e.BranchId == query.BranchId.Value);
        }

        if (query.DepartmentId.HasValue)
        {
            baseQuery = baseQuery.Where(e => e.DepartmentId == query.DepartmentId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            string normalizedStatus = query.Status.Trim().ToUpperInvariant();
            baseQuery = baseQuery.Where(e => e.EmploymentStatus == normalizedStatus);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            string term = query.Search.Trim().ToLowerInvariant();
            baseQuery = baseQuery.Where(e =>
                e.EmployeeCode.ToLower().Contains(term) ||
                e.FirstName.ToLower().Contains(term) ||
                e.LastName.ToLower().Contains(term));
        }

        int totalCount = await baseQuery.CountAsync(cancellationToken);

        IOrderedQueryable<Employee> ordered;
        string sortKey = (query.SortBy ?? "employeeCode").Trim().ToLowerInvariant();
        if (sortKey == "hiredate")
        {
            ordered = query.SortDesc
                ? baseQuery.OrderByDescending(e => e.HireDate)
                : baseQuery.OrderBy(e => e.HireDate);
        }
        else if (sortKey == "firstname")
        {
            ordered = query.SortDesc
                ? baseQuery.OrderByDescending(e => e.FirstName)
                : baseQuery.OrderBy(e => e.FirstName);
        }
        else
        {
            ordered = query.SortDesc
                ? baseQuery.OrderByDescending(e => e.EmployeeCode)
                : baseQuery.OrderBy(e => e.EmployeeCode);
        }

        List<Employee> pagedEmployees = await ordered
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        Dictionary<Guid, string> branchNames = await _dbContext.Branches
            .AsNoTracking()
            .ToDictionaryAsync(b => b.Id, b => b.Name, cancellationToken);

        Dictionary<Guid, string> deptNames = await _dbContext.Departments
            .AsNoTracking()
            .ToDictionaryAsync(d => d.Id, d => d.Name, cancellationToken);

        Dictionary<Guid, string> posTitles = await _dbContext.Positions
            .AsNoTracking()
            .ToDictionaryAsync(p => p.Id, p => p.Title, cancellationToken);

        Dictionary<Guid, string> gradeNames = await _dbContext.JobGrades
            .AsNoTracking()
            .ToDictionaryAsync(g => g.Id, g => g.Name, cancellationToken);

        var items = new List<EmployeeListItemDto>();
        foreach (Employee emp in pagedEmployees)
        {
            items.Add(new EmployeeListItemDto(
                emp.Id,
                emp.EmployeeCode,
                emp.FirstName,
                emp.LastName,
                emp.BranchId,
                branchNames.GetValueOrDefault(emp.BranchId, string.Empty),
                emp.DepartmentId,
                deptNames.GetValueOrDefault(emp.DepartmentId, string.Empty),
                emp.PositionId,
                posTitles.GetValueOrDefault(emp.PositionId, string.Empty),
                emp.JobGradeId,
                gradeNames.GetValueOrDefault(emp.JobGradeId, string.Empty),
                emp.HireDate,
                emp.EmploymentStatus));
        }

        return new PagedResult<EmployeeListItemDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<Employee360Dto> GetEmployeeByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        Employee employee = await _dbContext.Employees
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken)
            ?? throw new NotFoundException("Employee not found.");

        string branchName = await _dbContext.Branches
            .AsNoTracking()
            .Where(b => b.Id == employee.BranchId)
            .Select(b => b.Name)
            .FirstOrDefaultAsync(cancellationToken) ?? string.Empty;

        string deptName = await _dbContext.Departments
            .AsNoTracking()
            .Where(d => d.Id == employee.DepartmentId)
            .Select(d => d.Name)
            .FirstOrDefaultAsync(cancellationToken) ?? string.Empty;

        string posTitle = await _dbContext.Positions
            .AsNoTracking()
            .Where(p => p.Id == employee.PositionId)
            .Select(p => p.Title)
            .FirstOrDefaultAsync(cancellationToken) ?? string.Empty;

        string gradeName = await _dbContext.JobGrades
            .AsNoTracking()
            .Where(g => g.Id == employee.JobGradeId)
            .Select(g => g.Name)
            .FirstOrDefaultAsync(cancellationToken) ?? string.Empty;

        List<EmployeeContractDto> contracts = await _dbContext.EmploymentContracts
            .AsNoTracking()
            .Where(c => c.EmployeeId == id)
            .OrderByDescending(c => c.EffectiveFrom)
            .Select(c => new EmployeeContractDto(
                c.Id,
                c.ContractType,
                c.EffectiveFrom,
                c.EffectiveTo,
                c.BaseSalary,
                c.PaymentCycle,
                c.IsActive))
            .ToListAsync(cancellationToken);

        List<EmployeeReportingLine> rawLines = await _dbContext.EmployeeReportingLines
            .AsNoTracking()
            .Where(r => r.EmployeeId == id)
            .OrderBy(r => r.PriorityOrder)
            .ToListAsync(cancellationToken);

        List<Guid> managerIds = rawLines.Select(r => r.ManagerEmployeeId).Distinct().ToList();
        var managers = await _dbContext.Employees
            .AsNoTracking()
            .Where(e => managerIds.Contains(e.Id))
            .ToDictionaryAsync(e => e.Id, cancellationToken);

        var reportingLines = new List<EmployeeReportingLineDto>();
        foreach (EmployeeReportingLine line in rawLines)
        {
            managers.TryGetValue(line.ManagerEmployeeId, out Employee? mgr);
            string mgrName = mgr is null ? string.Empty : $"{mgr.FirstName} {mgr.LastName}".Trim();
            reportingLines.Add(new EmployeeReportingLineDto(
                line.Id,
                line.ManagerEmployeeId,
                mgr?.EmployeeCode ?? string.Empty,
                mgrName,
                line.LineType,
                line.PriorityOrder,
                line.EffectiveFrom,
                line.EffectiveTo));
        }

        int currentYear = DateTime.UtcNow.Year;
        List<EmployeeLeaveBalance> rawBalances = await _dbContext.EmployeeLeaveBalances
            .AsNoTracking()
            .Where(b => b.EmployeeId == id && b.QuotaYear == currentYear)
            .ToListAsync(cancellationToken);

        var leaveTypes = await _dbContext.LeaveTypes
            .AsNoTracking()
            .ToDictionaryAsync(lt => lt.Id, cancellationToken);

        var leaveBalances = new List<EmployeeLeaveBalanceSummaryDto>();
        foreach (EmployeeLeaveBalance bal in rawBalances)
        {
            leaveTypes.TryGetValue(bal.LeaveTypeId, out LeaveType? lt);
            leaveBalances.Add(new EmployeeLeaveBalanceSummaryDto(
                bal.LeaveTypeId,
                lt?.Code ?? string.Empty,
                lt?.Name ?? string.Empty,
                bal.QuotaYear,
                bal.CarriedOverDays,
                bal.EntitledDays,
                bal.UsedDays,
                bal.PendingDays,
                bal.RemainingDays));
        }

        return new Employee360Dto(
            employee.Id,
            employee.CompanyId,
            employee.BranchId,
            branchName,
            employee.DepartmentId,
            deptName,
            employee.PositionId,
            posTitle,
            employee.JobGradeId,
            gradeName,
            employee.UserId,
            employee.EmployeeCode,
            employee.FirstName,
            employee.LastName,
            employee.HireDate,
            employee.ProbationEndDate,
            employee.EmploymentStatus,
            contracts,
            reportingLines,
            leaveBalances);
    }

    public async Task<Employee360Dto> OnboardEmployeeAsync(
        OnboardEmployeeRequest request,
        CancellationToken cancellationToken = default)
    {
        (Guid tenantId, Guid companyId) = RequireTenantAndCompany();
        string normalizedCode = request.EmployeeCode.Trim().ToUpperInvariant();

        bool duplicateCode = await _dbContext.Employees
            .AnyAsync(e => e.EmployeeCode == normalizedCode, cancellationToken);

        if (duplicateCode)
        {
            throw new ConflictException($"Employee code '{normalizedCode}' already exists.");
        }

        Guid? userId = null;
        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            string normalizedEmail = request.Email.Trim().ToLowerInvariant();
            bool duplicateEmail = await _dbContext.Users
                .IgnoreQueryFilters()
                .AnyAsync(u => u.TenantId == tenantId && u.Email == normalizedEmail, cancellationToken);

            if (duplicateEmail)
            {
                throw new ConflictException($"User email '{normalizedEmail}' already exists.");
            }

            string rawPassword = string.IsNullOrWhiteSpace(request.InitialPassword)
                ? "Demo@123456!"
                : request.InitialPassword;

            var user = User.Create(tenantId, normalizedEmail, _passwordHasher.Hash(rawPassword));
            _dbContext.Users.Add(user);
            userId = user.Id;

            if (request.RoleId.HasValue)
            {
                _dbContext.UserCompanyRoles.Add(
                    UserCompanyRole.Create(tenantId, user.Id, companyId, request.RoleId.Value));
            }
        }

        var employee = Employee.Create(
            tenantId,
            companyId,
            request.BranchId,
            request.DepartmentId,
            request.PositionId,
            request.JobGradeId,
            userId,
            normalizedCode,
            request.FirstName,
            request.LastName,
            request.HireDate,
            request.ProbationEndDate,
            request.EmploymentStatus);

        _dbContext.Employees.Add(employee);

        var contract = EmploymentContract.Create(
            tenantId,
            companyId,
            employee.Id,
            request.ContractType,
            request.HireDate,
            null,
            request.BaseSalary,
            request.PaymentCycle,
            isActive: true);

        _dbContext.EmploymentContracts.Add(contract);

        // Initialize leave balances from company policies (first-tier 0 tenure policies)
        List<LeaveEntitlementPolicy> policies = await _dbContext.LeaveEntitlementPolicies
            .AsNoTracking()
            .Where(p => p.MinTenureMonths == 0 && (p.JobGradeId == null || p.JobGradeId == request.JobGradeId))
            .ToListAsync(cancellationToken);

        var seededLeaveTypes = new HashSet<Guid>();
        int quotaYear = request.HireDate.Year;
        foreach (LeaveEntitlementPolicy policy in policies)
        {
            if (seededLeaveTypes.Add(policy.LeaveTypeId))
            {
                _dbContext.EmployeeLeaveBalances.Add(
                    EmployeeLeaveBalance.Create(
                        tenantId,
                        companyId,
                        employee.Id,
                        policy.LeaveTypeId,
                        quotaYear,
                        carriedOverDays: 0m,
                        entitledDays: policy.AnnualQuotaDays));
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return await GetEmployeeByIdAsync(employee.Id, cancellationToken);
    }

    public async Task<Employee360Dto> UpdateEmployeeAsync(
        Guid id,
        UpdateEmployeeRequest request,
        CancellationToken cancellationToken = default)
    {
        Employee employee = await _dbContext.Employees
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken)
            ?? throw new NotFoundException("Employee not found.");

        employee.UpdateProfile(
            request.BranchId,
            request.DepartmentId,
            request.PositionId,
            request.JobGradeId,
            request.FirstName,
            request.LastName,
            request.EmploymentStatus);

        await _dbContext.SaveChangesAsync(cancellationToken);
        return await GetEmployeeByIdAsync(employee.Id, cancellationToken);
    }

    public async Task UpdateReportingLinesAsync(
        Guid employeeId,
        UpdateReportingLinesRequest request,
        CancellationToken cancellationToken = default)
    {
        (Guid tenantId, Guid companyId) = RequireTenantAndCompany();

        bool employeeExists = await _dbContext.Employees
            .AnyAsync(e => e.Id == employeeId, cancellationToken);

        if (!employeeExists)
        {
            throw new NotFoundException("Employee not found.");
        }

        List<EmployeeReportingLine> existing = await _dbContext.EmployeeReportingLines
            .Where(r => r.EmployeeId == employeeId)
            .ToListAsync(cancellationToken);

        _dbContext.EmployeeReportingLines.RemoveRange(existing);

        foreach (ReportingLineItemRequest item in request.Lines)
        {
            _dbContext.EmployeeReportingLines.Add(
                EmployeeReportingLine.Create(
                    tenantId,
                    companyId,
                    employeeId,
                    item.ManagerEmployeeId,
                    item.LineType,
                    item.PriorityOrder,
                    item.EffectiveFrom,
                    item.EffectiveTo));
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<EmployeeContractDto> CreateContractAsync(
        Guid employeeId,
        CreateContractRequest request,
        CancellationToken cancellationToken = default)
    {
        (Guid tenantId, Guid companyId) = RequireTenantAndCompany();

        bool employeeExists = await _dbContext.Employees
            .AnyAsync(e => e.Id == employeeId, cancellationToken);

        if (!employeeExists)
        {
            throw new NotFoundException("Employee not found.");
        }

        List<EmploymentContract> activeContracts = await _dbContext.EmploymentContracts
            .Where(c => c.EmployeeId == employeeId && c.IsActive)
            .ToListAsync(cancellationToken);

        DateOnly previousEnd = request.EffectiveFrom.AddDays(-1);
        foreach (EmploymentContract oldContract in activeContracts)
        {
            DateOnly endDate = previousEnd < oldContract.EffectiveFrom
                ? oldContract.EffectiveFrom
                : previousEnd;
            oldContract.Deactivate(endDate);
        }

        var contract = EmploymentContract.Create(
            tenantId,
            companyId,
            employeeId,
            request.ContractType,
            request.EffectiveFrom,
            request.EffectiveTo,
            request.BaseSalary,
            request.PaymentCycle,
            isActive: true);

        _dbContext.EmploymentContracts.Add(contract);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new EmployeeContractDto(
            contract.Id,
            contract.ContractType,
            contract.EffectiveFrom,
            contract.EffectiveTo,
            contract.BaseSalary,
            contract.PaymentCycle,
            contract.IsActive);
    }

    private (Guid TenantId, Guid CompanyId) RequireTenantAndCompany()
    {
        Guid tenantId = _tenantContext.TenantId
            ?? throw new UnauthorizedException("Tenant context is required.");
        Guid companyId = _tenantContext.ActiveCompanyId
            ?? throw new ForbiddenException("Active company context is required.");
        return (tenantId, companyId);
    }
}

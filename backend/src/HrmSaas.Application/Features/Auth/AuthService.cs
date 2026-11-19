using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using HrmSaas.Application.Common.Exceptions;
using HrmSaas.Application.Common.Interfaces;
using HrmSaas.Domain.Employees;
using HrmSaas.Domain.Identity;
using HrmSaas.Domain.Organizations;
using Microsoft.EntityFrameworkCore;

namespace HrmSaas.Application.Features.Auth;

public sealed class AuthService
{
    private readonly IAppDbContext _dbContext;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ISessionStore _sessionStore;
    private readonly ITenantContext _tenantContext;

    public AuthService(
        IAppDbContext dbContext,
        IPasswordHasher passwordHasher,
        ISessionStore sessionStore,
        ITenantContext tenantContext)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
        _sessionStore = sessionStore;
        _tenantContext = tenantContext;
    }

    public async Task<LoginOutcome> LoginAsync(
        LoginRequest request,
        IPAddress clientIp,
        string userAgent,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            throw new ValidationAppException(
                new Dictionary<string, string[]>
                {
                    ["email"] = ["Email and password are required."]
                });
        }

        string normalizedEmail = request.Email.Trim().ToLowerInvariant();

        // During login, TenantContext is not set yet, so we perform a scoped auth lookup and then bind the resolved tenant.
        User? user = await _dbContext.FindUserByEmailForLoginAsync(normalizedEmail, cancellationToken);

        if (user is null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedException("Invalid email or password.");
        }

        if (user.Status != UserStatus.Active)
        {
            throw new ForbiddenException("Your user account is not active.");
        }

        _tenantContext.SetTenant(user.TenantId, user.Id);

        List<UserCompanyRole> companyRoles = await _dbContext.UserCompanyRoles
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(x => x.TenantId == user.TenantId && x.UserId == user.Id)
            .ToListAsync(cancellationToken);

        Guid? defaultCompanyId = null;
        if (companyRoles.Count > 0)
        {
            defaultCompanyId = companyRoles[0].CompanyId;
        }

        Employee? employee = await _dbContext.Employees
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.TenantId == user.TenantId && x.UserId == user.Id,
                cancellationToken);

        Guid? defaultBranchId = employee?.BranchId;
        if (defaultBranchId is null && defaultCompanyId.HasValue)
        {
            Branch? firstBranch = await _dbContext.Branches
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(x => x.TenantId == user.TenantId && x.CompanyId == defaultCompanyId.Value)
                .OrderBy(x => x.Code)
                .FirstOrDefaultAsync(cancellationToken);

            defaultBranchId = firstBranch?.Id;
        }

        string rawToken = GenerateOpaqueToken();
        string tokenHash = ComputeSha256Hex(rawToken);

        SessionData session = await _sessionStore.CreateAsync(
            user.Id,
            user.TenantId,
            tokenHash,
            defaultCompanyId,
            defaultBranchId,
            clientIp,
            userAgent,
            cancellationToken);

        user.RecordLogin(DateTimeOffset.UtcNow);
        await _dbContext.SaveChangesAsync(cancellationToken);

        AuthMeResponse me = await BuildAuthMeAsync(
            user.Id,
            user.TenantId,
            session.ActiveCompanyId,
            session.ActiveBranchId,
            cancellationToken);

        return new LoginOutcome(rawToken, session.ExpiresAt, me);
    }

    public async Task<LoginOutcome> RegisterAsync(
        RegisterRequest request,
        IPAddress clientIp,
        string userAgent,
        CancellationToken cancellationToken = default)
    {
        var errors = new Dictionary<string, string[]>();

        string trimmedName = request.Name?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(trimmedName))
        {
            errors["name"] = ["Name is required."];
        }

        string normalizedEmail = request.Email?.Trim().ToLowerInvariant() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(normalizedEmail) || !MailAddress.TryCreate(normalizedEmail, out _))
        {
            errors["email"] = ["A valid email address is required."];
        }

        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 8)
        {
            errors["password"] = ["Password must be at least 8 characters."];
        }

        if (errors.Count > 0)
        {
            throw new ValidationAppException(errors);
        }

        User? existingUser = await _dbContext.FindUserByEmailForLoginAsync(normalizedEmail, cancellationToken);
        if (existingUser is not null)
        {
            throw new ConflictException("An account with this email already exists.");
        }

        Tenant? tenant = await _dbContext.Tenants
            .AsNoTracking()
            .Where(t => t.Status == TenantStatus.Active)
            .OrderBy(t => t.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (tenant is null)
        {
            throw new NotFoundException("No active organization tenant is available for registration.");
        }

        _tenantContext.SetTenant(tenant.Id);

        Company? company = await _dbContext.Companies
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(c => c.TenantId == tenant.Id && !c.IsDeleted)
            .OrderBy(c => c.Code)
            .FirstOrDefaultAsync(cancellationToken);

        Branch? branch = null;
        Role? userRole = null;

        if (company is not null)
        {
            branch = await _dbContext.Branches
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(b => b.TenantId == tenant.Id && b.CompanyId == company.Id && !b.IsDeleted)
                .OrderBy(b => b.Code)
                .FirstOrDefaultAsync(cancellationToken);

            userRole = await _dbContext.Roles
                .IgnoreQueryFilters()
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    r => r.TenantId == tenant.Id && r.CompanyId == company.Id && r.Code == "USER" && !r.IsDeleted,
                    cancellationToken);
        }

        string passwordHash = _passwordHasher.Hash(request.Password);
        var user = User.Create(tenant.Id, normalizedEmail, passwordHash);
        user.RecordLogin(DateTimeOffset.UtcNow);
        _dbContext.Users.Add(user);

        if (company is not null && userRole is not null)
        {
            _dbContext.UserCompanyRoles.Add(
                UserCompanyRole.Create(tenant.Id, user.Id, company.Id, userRole.Id));
        }

        if (company is not null && branch is not null)
        {
            Department? department = await _dbContext.Departments
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(d => d.TenantId == tenant.Id && d.CompanyId == company.Id && !d.IsDeleted)
                .OrderBy(d => d.Code)
                .FirstOrDefaultAsync(cancellationToken);

            Position? position = await _dbContext.Positions
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(p => p.TenantId == tenant.Id && p.CompanyId == company.Id && !p.IsDeleted)
                .OrderBy(p => p.Code)
                .FirstOrDefaultAsync(cancellationToken);

            JobGrade? jobGrade = await _dbContext.JobGrades
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(g => g.TenantId == tenant.Id && g.CompanyId == company.Id && !g.IsDeleted)
                .OrderBy(g => g.LevelOrder)
                .FirstOrDefaultAsync(cancellationToken);

            if (department is not null && position is not null && jobGrade is not null)
            {
                string[] nameParts = trimmedName.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
                string firstName = nameParts[0];
                string lastName = nameParts.Length > 1 ? nameParts[1] : "-";

                int existingCount = await _dbContext.Employees
                    .IgnoreQueryFilters()
                    .CountAsync(e => e.TenantId == tenant.Id && e.CompanyId == company.Id, cancellationToken);

                string employeeCode = $"EMP-{existingCount + 1:D4}";
                bool codeExists = await _dbContext.Employees
                    .IgnoreQueryFilters()
                    .AnyAsync(
                        e => e.TenantId == tenant.Id && e.CompanyId == company.Id && e.EmployeeCode == employeeCode,
                        cancellationToken);

                if (codeExists)
                {
                    employeeCode = $"EMP-{Guid.NewGuid().ToString("N")[..6].ToUpperInvariant()}";
                }

                DateOnly hireDate = DateOnly.FromDateTime(DateTime.UtcNow);
                var employee = Employee.Create(
                    tenant.Id,
                    company.Id,
                    branch.Id,
                    department.Id,
                    position.Id,
                    jobGrade.Id,
                    user.Id,
                    employeeCode,
                    firstName,
                    lastName,
                    hireDate,
                    hireDate.AddDays(119),
                    EmploymentStatus.Active);

                _dbContext.Employees.Add(employee);
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        _tenantContext.SetTenant(tenant.Id, user.Id);

        string rawToken = GenerateOpaqueToken();
        string tokenHash = ComputeSha256Hex(rawToken);

        SessionData session = await _sessionStore.CreateAsync(
            user.Id,
            tenant.Id,
            tokenHash,
            company?.Id,
            branch?.Id,
            clientIp,
            userAgent,
            cancellationToken);

        AuthMeResponse me = await BuildAuthMeAsync(
            user.Id,
            tenant.Id,
            session.ActiveCompanyId,
            session.ActiveBranchId,
            cancellationToken);

        return new LoginOutcome(rawToken, session.ExpiresAt, me);
    }

    public async Task LogoutAsync(string rawSessionToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(rawSessionToken))
        {
            return;
        }

        string tokenHash = ComputeSha256Hex(rawSessionToken);
        await _sessionStore.RevokeAsync(tokenHash, cancellationToken);
    }

    public async Task<AuthMeResponse> GetMeAsync(
        Guid userId,
        Guid tenantId,
        Guid? activeCompanyId,
        Guid? activeBranchId,
        CancellationToken cancellationToken = default)
    {
        return await BuildAuthMeAsync(userId, tenantId, activeCompanyId, activeBranchId, cancellationToken);
    }

    public async Task<AuthMeResponse> SwitchContextAsync(
        string rawSessionToken,
        Guid userId,
        Guid tenantId,
        SwitchContextRequest request,
        CancellationToken cancellationToken = default)
    {
        bool hasCompanyAccess = await _dbContext.UserCompanyRoles
            .IgnoreQueryFilters()
            .AsNoTracking()
            .AnyAsync(
                x => x.TenantId == tenantId && x.UserId == userId && x.CompanyId == request.CompanyId,
                cancellationToken);

        if (!hasCompanyAccess)
        {
            throw new ForbiddenException("You do not have access to the selected company.");
        }

        Guid? validatedBranchId = request.BranchId;
        if (validatedBranchId.HasValue)
        {
            bool branchExists = await _dbContext.Branches
                .IgnoreQueryFilters()
                .AsNoTracking()
                .AnyAsync(
                    x => x.TenantId == tenantId && x.CompanyId == request.CompanyId && x.Id == validatedBranchId.Value,
                    cancellationToken);

            if (!branchExists)
            {
                throw new NotFoundException("Selected branch was not found in this company.");
            }
        }
        else
        {
            Branch? fallbackBranch = await _dbContext.Branches
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(x => x.TenantId == tenantId && x.CompanyId == request.CompanyId)
                .OrderBy(x => x.Code)
                .FirstOrDefaultAsync(cancellationToken);

            validatedBranchId = fallbackBranch?.Id;
        }

        string tokenHash = ComputeSha256Hex(rawSessionToken);
        SessionData? updated = await _sessionStore.SwitchContextAsync(
            tokenHash,
            request.CompanyId,
            validatedBranchId,
            cancellationToken);

        if (updated is null)
        {
            throw new UnauthorizedException("Session is no longer valid.");
        }

        return await BuildAuthMeAsync(userId, tenantId, updated.ActiveCompanyId, updated.ActiveBranchId, cancellationToken);
    }

    public async Task<DateTimeOffset> RefreshSessionAsync(
        string rawSessionToken,
        CancellationToken cancellationToken = default)
    {
        string tokenHash = ComputeSha256Hex(rawSessionToken);
        SessionData? refreshed = await _sessionStore.RefreshAsync(tokenHash, cancellationToken);
        if (refreshed is null)
        {
            throw new UnauthorizedException("Session has expired or been revoked.");
        }

        return refreshed.ExpiresAt;
    }

    private async Task<AuthMeResponse> BuildAuthMeAsync(
        Guid userId,
        Guid tenantId,
        Guid? activeCompanyId,
        Guid? activeBranchId,
        CancellationToken cancellationToken)
    {
        User user = await _dbContext.Users
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == userId && x.TenantId == tenantId, cancellationToken)
            ?? throw new UnauthorizedException("User not found.");

        Employee? employee = await _dbContext.Employees
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.UserId == userId, cancellationToken);

        List<UserCompanyRole> userRoles = await _dbContext.UserCompanyRoles
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.UserId == userId)
            .ToListAsync(cancellationToken);

        var companyIds = new List<Guid>();
        var activeRoleIds = new List<Guid>();

        foreach (UserCompanyRole ucr in userRoles)
        {
            if (!companyIds.Contains(ucr.CompanyId))
            {
                companyIds.Add(ucr.CompanyId);
            }

            if (activeCompanyId.HasValue && ucr.CompanyId == activeCompanyId.Value)
            {
                activeRoleIds.Add(ucr.RoleId);
            }
        }

        List<string> permissionCodes = [];
        if (activeRoleIds.Count > 0)
        {
            permissionCodes = await _dbContext.RolePermissions
                .AsNoTracking()
                .Where(rp => activeRoleIds.Contains(rp.RoleId))
                .Join(
                    _dbContext.Permissions.AsNoTracking(),
                    rp => rp.PermissionId,
                    p => p.Id,
                    (rp, p) => p.Code)
                .Distinct()
                .OrderBy(code => code)
                .ToListAsync(cancellationToken);
        }

        List<Company> companies = await _dbContext.Companies
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(c => c.TenantId == tenantId && companyIds.Contains(c.Id))
            .OrderBy(c => c.Code)
            .ToListAsync(cancellationToken);

        List<Branch> branches = await _dbContext.Branches
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(b => b.TenantId == tenantId && companyIds.Contains(b.CompanyId))
            .OrderBy(b => b.Code)
            .ToListAsync(cancellationToken);

        var accessibleCompanies = new List<AccessibleCompanyDto>();
        foreach (Company company in companies)
        {
            var companyBranches = new List<AccessibleBranchDto>();
            foreach (Branch branch in branches)
            {
                if (branch.CompanyId == company.Id)
                {
                    companyBranches.Add(new AccessibleBranchDto(branch.Id, branch.Code, branch.Name));
                }
            }

            accessibleCompanies.Add(new AccessibleCompanyDto(
                company.Id,
                company.Code,
                company.LegalName,
                companyBranches));
        }

        string? fullName = employee is null ? null : $"{employee.FirstName} {employee.LastName}".Trim();

        return new AuthMeResponse(
            user.Id,
            user.TenantId,
            user.Email,
            employee?.Id,
            employee?.EmployeeCode,
            fullName,
            activeCompanyId,
            activeBranchId,
            permissionCodes,
            accessibleCompanies);
    }

    public static string ComputeSha256Hex(string rawToken)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(rawToken);
        byte[] hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static string GenerateOpaqueToken()
    {
        byte[] bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
    }
}

using HrmSaas.Application.Common.Interfaces;
using HrmSaas.Domain.Attendance;
using HrmSaas.Domain.Audit;
using HrmSaas.Domain.Common;
using HrmSaas.Domain.Employees;
using HrmSaas.Domain.Identity;
using HrmSaas.Domain.Organizations;
using HrmSaas.Domain.Payroll;
using HrmSaas.Domain.Workflows;
using Microsoft.EntityFrameworkCore;

namespace HrmSaas.Infrastructure.Persistence;

public sealed class AppDbContext : DbContext, IAppDbContext
{
    private readonly ITenantContext? _tenantContext;

    public AppDbContext(DbContextOptions<AppDbContext> options, ITenantContext? tenantContext = null)
        : base(options)
    {
        _tenantContext = tenantContext;
    }

    private Guid? CurrentTenantId => _tenantContext?.TenantId;
    private Guid? CurrentCompanyId => _tenantContext?.ActiveCompanyId;

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Position> Positions => Set<Position>();
    public DbSet<JobGrade> JobGrades => Set<JobGrade>();

    public DbSet<User> Users => Set<User>();
    public DbSet<UserSession> UserSessions => Set<UserSession>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<UserCompanyRole> UserCompanyRoles => Set<UserCompanyRole>();

    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<EmploymentContract> EmploymentContracts => Set<EmploymentContract>();
    public DbSet<EmployeeReportingLine> EmployeeReportingLines => Set<EmployeeReportingLine>();

    public DbSet<ShiftDefinition> ShiftDefinitions => Set<ShiftDefinition>();
    public DbSet<ShiftBreakRule> ShiftBreakRules => Set<ShiftBreakRule>();
    public DbSet<EmployeeShiftRoster> EmployeeShiftRosters => Set<EmployeeShiftRoster>();
    public DbSet<AttendanceLog> AttendanceLogs => Set<AttendanceLog>();
    public DbSet<AttendanceDailySummary> AttendanceDailySummaries => Set<AttendanceDailySummary>();

    public DbSet<LeaveType> LeaveTypes => Set<LeaveType>();
    public DbSet<LeaveEntitlementPolicy> LeaveEntitlementPolicies => Set<LeaveEntitlementPolicy>();
    public DbSet<EmployeeLeaveBalance> EmployeeLeaveBalances => Set<EmployeeLeaveBalance>();
    public DbSet<WorkflowDefinition> WorkflowDefinitions => Set<WorkflowDefinition>();
    public DbSet<WorkflowStep> WorkflowSteps => Set<WorkflowStep>();
    public DbSet<ApprovalRequest> ApprovalRequests => Set<ApprovalRequest>();
    public DbSet<ApprovalActionLog> ApprovalActionLogs => Set<ApprovalActionLog>();

    public DbSet<PayrollPeriod> PayrollPeriods => Set<PayrollPeriod>();
    public DbSet<SalaryComponent> SalaryComponents => Set<SalaryComponent>();
    public DbSet<TaxBracket> TaxBrackets => Set<TaxBracket>();
    public DbSet<SsoConfiguration> SsoConfigurations => Set<SsoConfiguration>();
    public DbSet<Payslip> Payslips => Set<Payslip>();
    public DbSet<PayslipItem> PayslipItems => Set<PayslipItem>();

    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    public async Task<User?> FindUserByEmailForLoginAsync(
        string normalizedEmail,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await Database.BeginTransactionAsync(cancellationToken);
        await Database.ExecuteSqlInterpolatedAsync(
            $"SELECT set_config('app.auth_lookup_email', {normalizedEmail}, true);",
            cancellationToken);

        User? user = await Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Email == normalizedEmail, cancellationToken);

        await transaction.CommitAsync(cancellationToken);
        return user;
    }

    public async Task SetTenantRlsSessionAsync(CancellationToken cancellationToken = default)
    {
        if (!CurrentTenantId.HasValue)
        {
            return;
        }

        string tenantIdText = CurrentTenantId.Value.ToString();
        await Database.ExecuteSqlInterpolatedAsync(
            $"SELECT set_config('app.current_tenant_id', {tenantIdText}, true);",
            cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        ApplyGlobalQueryFilters(modelBuilder);
    }

    private void ApplyGlobalQueryFilters(ModelBuilder modelBuilder)
    {
        // Tenant + SoftDelete entities (no CompanyId)
        ApplyTenantSoftDeleteFilter<Company>(modelBuilder);
        ApplyTenantSoftDeleteFilter<User>(modelBuilder);
        ApplyTenantSoftDeleteFilter<Role>(modelBuilder);

        // Tenant-only entities (immutable / append-only / child configuration)
        ApplyTenantOnlyFilter<UserSession>(modelBuilder);
        ApplyTenantOnlyFilter<ShiftBreakRule>(modelBuilder);
        ApplyTenantOnlyFilter<AttendanceLog>(modelBuilder);
        ApplyTenantOnlyFilter<WorkflowStep>(modelBuilder);
        ApplyTenantOnlyFilter<ApprovalActionLog>(modelBuilder);
        ApplyTenantOnlyFilter<PayslipItem>(modelBuilder);
        ApplyTenantOnlyFilter<AuditLog>(modelBuilder);

        // Tenant + Company + SoftDelete entities
        ApplyTenantCompanySoftDeleteFilter<Branch>(modelBuilder);
        ApplyTenantCompanySoftDeleteFilter<Department>(modelBuilder);
        ApplyTenantCompanySoftDeleteFilter<Position>(modelBuilder);
        ApplyTenantCompanySoftDeleteFilter<JobGrade>(modelBuilder);
        ApplyTenantCompanySoftDeleteFilter<UserCompanyRole>(modelBuilder);
        ApplyTenantCompanySoftDeleteFilter<Employee>(modelBuilder);
        ApplyTenantCompanySoftDeleteFilter<EmploymentContract>(modelBuilder);
        ApplyTenantCompanySoftDeleteFilter<EmployeeReportingLine>(modelBuilder);
        ApplyTenantCompanySoftDeleteFilter<ShiftDefinition>(modelBuilder);
        ApplyTenantCompanySoftDeleteFilter<EmployeeShiftRoster>(modelBuilder);
        ApplyTenantCompanySoftDeleteFilter<AttendanceDailySummary>(modelBuilder);
        ApplyTenantCompanySoftDeleteFilter<LeaveType>(modelBuilder);
        ApplyTenantCompanySoftDeleteFilter<LeaveEntitlementPolicy>(modelBuilder);
        ApplyTenantCompanySoftDeleteFilter<EmployeeLeaveBalance>(modelBuilder);
        ApplyTenantCompanySoftDeleteFilter<WorkflowDefinition>(modelBuilder);
        ApplyTenantCompanySoftDeleteFilter<ApprovalRequest>(modelBuilder);
        ApplyTenantCompanySoftDeleteFilter<PayrollPeriod>(modelBuilder);
        ApplyTenantCompanySoftDeleteFilter<SalaryComponent>(modelBuilder);
        ApplyTenantCompanySoftDeleteFilter<Payslip>(modelBuilder);
    }

    private void ApplyTenantOnlyFilter<TEntity>(ModelBuilder modelBuilder)
        where TEntity : class, ITenantScoped
    {
        modelBuilder.Entity<TEntity>().HasQueryFilter(e =>
            CurrentTenantId.HasValue &&
            e.TenantId == CurrentTenantId.Value);
    }

    private void ApplyTenantSoftDeleteFilter<TEntity>(ModelBuilder modelBuilder)
        where TEntity : class, ITenantScoped, ISoftDeletable
    {
        modelBuilder.Entity<TEntity>().HasQueryFilter(e =>
            !e.IsDeleted &&
            CurrentTenantId.HasValue &&
            e.TenantId == CurrentTenantId.Value);
    }

    private void ApplyTenantCompanySoftDeleteFilter<TEntity>(ModelBuilder modelBuilder)
        where TEntity : class, ITenantScoped, ICompanyScoped, ISoftDeletable
    {
        modelBuilder.Entity<TEntity>().HasQueryFilter(e =>
            !e.IsDeleted &&
            CurrentTenantId.HasValue &&
            e.TenantId == CurrentTenantId.Value &&
            (!CurrentCompanyId.HasValue || e.CompanyId == CurrentCompanyId.Value));
    }
}

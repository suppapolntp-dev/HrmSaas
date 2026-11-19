using HrmSaas.Domain.Attendance;
using HrmSaas.Domain.Audit;
using HrmSaas.Domain.Employees;
using HrmSaas.Domain.Identity;
using HrmSaas.Domain.Organizations;
using HrmSaas.Domain.Payroll;
using HrmSaas.Domain.Workflows;
using Microsoft.EntityFrameworkCore;

namespace HrmSaas.Application.Common.Interfaces;

public interface IAppDbContext
{
    DbSet<Tenant> Tenants { get; }
    DbSet<Company> Companies { get; }
    DbSet<Branch> Branches { get; }
    DbSet<Department> Departments { get; }
    DbSet<Position> Positions { get; }
    DbSet<JobGrade> JobGrades { get; }

    DbSet<User> Users { get; }
    DbSet<UserSession> UserSessions { get; }
    DbSet<Role> Roles { get; }
    DbSet<Permission> Permissions { get; }
    DbSet<RolePermission> RolePermissions { get; }
    DbSet<UserCompanyRole> UserCompanyRoles { get; }

    DbSet<Employee> Employees { get; }
    DbSet<EmploymentContract> EmploymentContracts { get; }
    DbSet<EmployeeReportingLine> EmployeeReportingLines { get; }

    DbSet<ShiftDefinition> ShiftDefinitions { get; }
    DbSet<ShiftBreakRule> ShiftBreakRules { get; }
    DbSet<EmployeeShiftRoster> EmployeeShiftRosters { get; }
    DbSet<AttendanceLog> AttendanceLogs { get; }
    DbSet<AttendanceDailySummary> AttendanceDailySummaries { get; }

    DbSet<LeaveType> LeaveTypes { get; }
    DbSet<LeaveEntitlementPolicy> LeaveEntitlementPolicies { get; }
    DbSet<EmployeeLeaveBalance> EmployeeLeaveBalances { get; }
    DbSet<WorkflowDefinition> WorkflowDefinitions { get; }
    DbSet<WorkflowStep> WorkflowSteps { get; }
    DbSet<ApprovalRequest> ApprovalRequests { get; }
    DbSet<ApprovalActionLog> ApprovalActionLogs { get; }

    DbSet<PayrollPeriod> PayrollPeriods { get; }
    DbSet<SalaryComponent> SalaryComponents { get; }
    DbSet<TaxBracket> TaxBrackets { get; }
    DbSet<SsoConfiguration> SsoConfigurations { get; }
    DbSet<Payslip> Payslips { get; }
    DbSet<PayslipItem> PayslipItems { get; }

    DbSet<AuditLog> AuditLogs { get; }

    Task<User?> FindUserByEmailForLoginAsync(string normalizedEmail, CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

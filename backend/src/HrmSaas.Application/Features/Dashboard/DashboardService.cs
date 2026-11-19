using HrmSaas.Application.Common.Interfaces;
using HrmSaas.Domain.Attendance;
using HrmSaas.Domain.Employees;
using HrmSaas.Domain.Organizations;
using HrmSaas.Domain.Payroll;
using HrmSaas.Domain.Workflows;
using Microsoft.EntityFrameworkCore;

namespace HrmSaas.Application.Features.Dashboard;

public sealed class DashboardService
{
    private readonly IAppDbContext _dbContext;
    private readonly ITenantContext _tenantContext;

    public DashboardService(IAppDbContext dbContext, ITenantContext tenantContext)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
    }

    public async Task<MyDashboardSummaryDto> GetMySummaryAsync(
        bool includePayslip,
        CancellationToken cancellationToken = default)
    {
        Guid? userId = _tenantContext.CurrentUserId;
        if (!userId.HasValue)
        {
            return new MyDashboardSummaryDto(null, [], null);
        }

        Employee? employee = await _dbContext.Employees
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.UserId == userId.Value, cancellationToken);

        if (employee is null)
        {
            return new MyDashboardSummaryDto(null, [], null);
        }

        DateOnly today = await ResolveBranchTodayAsync(employee.BranchId, cancellationToken);
        DateOnly lookbackStart = today.AddDays(-7);

        DateOnly? recentMissingPunchDate = await _dbContext.AttendanceDailySummaries
            .AsNoTracking()
            .Where(s =>
                s.EmployeeId == employee.Id &&
                s.WorkDate >= lookbackStart &&
                s.WorkDate < today &&
                s.AttendanceStatus == AttendanceStatus.MissingPunch)
            .OrderByDescending(s => s.WorkDate)
            .Select(s => (DateOnly?)s.WorkDate)
            .FirstOrDefaultAsync(cancellationToken);

        List<MyDashboardRequestItemDto> recentRequests = await _dbContext.ApprovalRequests
            .AsNoTracking()
            .Where(r => r.RequesterEmployeeId == employee.Id)
            .OrderByDescending(r => r.SubmittedAt)
            .Take(5)
            .Select(r => new MyDashboardRequestItemDto(
                r.Id,
                r.RequestType,
                r.CurrentStepOrder,
                r.Status,
                r.PayloadSummary,
                r.SubmittedAt))
            .ToListAsync(cancellationToken);

        LatestPayslipSnapshotDto? latestPayslip = null;
        if (includePayslip)
        {
            latestPayslip = await (
                from p in _dbContext.Payslips.AsNoTracking()
                join period in _dbContext.PayrollPeriods.AsNoTracking() on p.PayrollPeriodId equals period.Id
                where p.EmployeeId == employee.Id
                orderby period.PeriodEnd descending
                select new LatestPayslipSnapshotDto(
                    p.Id,
                    period.Id,
                    period.PeriodCode,
                    period.PayDate,
                    p.GrossIncome,
                    p.TotalDeductions,
                    p.NetPay,
                    p.IsLocked))
                .FirstOrDefaultAsync(cancellationToken);
        }

        return new MyDashboardSummaryDto(
            recentMissingPunchDate,
            recentRequests,
            latestPayslip);
    }

    public async Task<TeamPulseDto> GetTeamPulseAsync(CancellationToken cancellationToken = default)
    {
        Guid? activeBranchId = _tenantContext.ActiveBranchId;
        DateOnly today = await ResolveBranchTodayAsync(activeBranchId, cancellationToken);

        var todayQuery =
            from s in _dbContext.AttendanceDailySummaries.AsNoTracking()
            join e in _dbContext.Employees.AsNoTracking() on s.EmployeeId equals e.Id
            join d in _dbContext.Departments.AsNoTracking() on e.DepartmentId equals d.Id
            where s.WorkDate == today
            select new
            {
                s.EmployeeId,
                e.BranchId,
                e.EmployeeCode,
                e.FirstName,
                e.LastName,
                DepartmentName = d.Name,
                s.AttendanceStatus,
                s.LateMinutes,
                s.ActualInUtc
            };

        if (activeBranchId.HasValue)
        {
            todayQuery = todayQuery.Where(x => x.BranchId == activeBranchId.Value);
        }

        var todayRows = await todayQuery.ToListAsync(cancellationToken);

        int presentCount = 0;
        int lateCount = 0;
        int missingPunchCount = 0;
        int absentCount = 0;
        int onLeaveCount = 0;

        var exceptions = new List<TeamExceptionItemDto>();
        var whoIsOut = new List<WhoIsOutItemDto>();

        foreach (var row in todayRows)
        {
            string fullName = $"{row.FirstName} {row.LastName}".Trim();

            if (row.AttendanceStatus == AttendanceStatus.Present)
            {
                presentCount++;
            }
            else if (row.AttendanceStatus == AttendanceStatus.Late)
            {
                lateCount++;
                if (exceptions.Count < 8)
                {
                    exceptions.Add(new TeamExceptionItemDto(
                        row.EmployeeId,
                        row.EmployeeCode,
                        fullName,
                        row.AttendanceStatus,
                        row.LateMinutes,
                        row.ActualInUtc));
                }
            }
            else if (row.AttendanceStatus == AttendanceStatus.MissingPunch)
            {
                missingPunchCount++;
                if (exceptions.Count < 8)
                {
                    exceptions.Add(new TeamExceptionItemDto(
                        row.EmployeeId,
                        row.EmployeeCode,
                        fullName,
                        row.AttendanceStatus,
                        row.LateMinutes,
                        row.ActualInUtc));
                }
            }
            else if (row.AttendanceStatus == AttendanceStatus.Absent)
            {
                absentCount++;
                if (exceptions.Count < 8)
                {
                    exceptions.Add(new TeamExceptionItemDto(
                        row.EmployeeId,
                        row.EmployeeCode,
                        fullName,
                        row.AttendanceStatus,
                        row.LateMinutes,
                        row.ActualInUtc));
                }
            }
            else if (row.AttendanceStatus == AttendanceStatus.OnLeave)
            {
                onLeaveCount++;
                if (whoIsOut.Count < 8)
                {
                    whoIsOut.Add(new WhoIsOutItemDto(
                        row.EmployeeId,
                        row.EmployeeCode,
                        fullName,
                        row.DepartmentName));
                }
            }
        }

        IQueryable<EmployeeShiftRoster> rosterQuery = _dbContext.EmployeeShiftRosters
            .AsNoTracking()
            .Where(r => r.WorkDate == today && r.DayType == RosterDayType.Workday);

        if (activeBranchId.HasValue)
        {
            rosterQuery =
                from r in rosterQuery
                join e in _dbContext.Employees.AsNoTracking() on r.EmployeeId equals e.Id
                where e.BranchId == activeBranchId.Value
                select r;
        }

        int totalScheduled = await rosterQuery.CountAsync(cancellationToken);
        if (totalScheduled == 0 && todayRows.Count > 0)
        {
            totalScheduled = todayRows.Count;
        }

        return new TeamPulseDto(
            today,
            totalScheduled,
            presentCount,
            lateCount,
            missingPunchCount,
            absentCount,
            onLeaveCount,
            exceptions,
            whoIsOut);
    }

    public async Task<HrOverviewDto> GetHrOverviewAsync(CancellationToken cancellationToken = default)
    {
        DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(7));
        DateOnly monthStart = new(today.Year, today.Month, 1);
        DateOnly alertHorizon = today.AddDays(30);

        var employees = await (
            from e in _dbContext.Employees.AsNoTracking()
            join d in _dbContext.Departments.AsNoTracking() on e.DepartmentId equals d.Id
            where e.EmploymentStatus == EmploymentStatus.Active || e.EmploymentStatus == EmploymentStatus.Probation
            select new
            {
                e.Id,
                e.EmployeeCode,
                e.FirstName,
                e.LastName,
                e.DepartmentId,
                DepartmentName = d.Name,
                e.HireDate,
                e.ProbationEndDate,
                e.EmploymentStatus
            })
            .ToListAsync(cancellationToken);

        int activeCount = 0;
        int probationCount = 0;
        int newHiresThisMonthCount = 0;

        var deptBuckets = new Dictionary<Guid, (string Name, int Active, int Probation)>();
        var alerts = new List<LifecycleAlertDto>();

        foreach (var emp in employees)
        {
            bool isProbation = emp.EmploymentStatus == EmploymentStatus.Probation;
            if (isProbation)
            {
                probationCount++;
            }
            else
            {
                activeCount++;
            }

            if (emp.HireDate >= monthStart && emp.HireDate <= today)
            {
                newHiresThisMonthCount++;
            }

            if (!deptBuckets.TryGetValue(emp.DepartmentId, out var bucket))
            {
                bucket = (emp.DepartmentName, 0, 0);
            }

            deptBuckets[emp.DepartmentId] = isProbation
                ? (bucket.Name, bucket.Active, bucket.Probation + 1)
                : (bucket.Name, bucket.Active + 1, bucket.Probation);

            if (isProbation && emp.ProbationEndDate.HasValue && emp.ProbationEndDate.Value <= alertHorizon)
            {
                int daysRemaining = emp.ProbationEndDate.Value.DayNumber - today.DayNumber;
                alerts.Add(new LifecycleAlertDto(
                    emp.Id,
                    emp.EmployeeCode,
                    $"{emp.FirstName} {emp.LastName}".Trim(),
                    "PROBATION_END",
                    emp.ProbationEndDate.Value,
                    daysRemaining));
            }
        }

        var expiringContracts = await (
            from c in _dbContext.EmploymentContracts.AsNoTracking()
            join e in _dbContext.Employees.AsNoTracking() on c.EmployeeId equals e.Id
            where c.IsActive &&
                  c.EffectiveTo.HasValue &&
                  c.EffectiveTo.Value <= alertHorizon &&
                  (e.EmploymentStatus == EmploymentStatus.Active || e.EmploymentStatus == EmploymentStatus.Probation)
            select new
            {
                e.Id,
                e.EmployeeCode,
                e.FirstName,
                e.LastName,
                DueDate = c.EffectiveTo!.Value
            })
            .ToListAsync(cancellationToken);

        foreach (var contract in expiringContracts)
        {
            int daysRemaining = contract.DueDate.DayNumber - today.DayNumber;
            alerts.Add(new LifecycleAlertDto(
                contract.Id,
                contract.EmployeeCode,
                $"{contract.FirstName} {contract.LastName}".Trim(),
                "CONTRACT_EXPIRY",
                contract.DueDate,
                daysRemaining));
        }

        List<DepartmentHeadcountDto> departmentBreakdown = deptBuckets
            .Select(kvp => new DepartmentHeadcountDto(
                kvp.Key,
                kvp.Value.Name,
                kvp.Value.Active,
                kvp.Value.Probation))
            .OrderByDescending(d => d.ActiveCount + d.ProbationCount)
            .Take(6)
            .ToList();

        List<LifecycleAlertDto> sortedAlerts = alerts
            .OrderBy(a => a.DaysRemaining)
            .Take(8)
            .ToList();

        return new HrOverviewDto(
            activeCount,
            probationCount,
            newHiresThisMonthCount,
            departmentBreakdown,
            sortedAlerts);
    }

    public async Task<PayrollReadinessDto> GetPayrollReadinessAsync(CancellationToken cancellationToken = default)
    {
        PayrollPeriod? latestPeriod = await _dbContext.PayrollPeriods
            .AsNoTracking()
            .OrderByDescending(p => p.PeriodEnd)
            .FirstOrDefaultAsync(cancellationToken);

        int pendingApprovalsCount = await _dbContext.ApprovalRequests
            .AsNoTracking()
            .CountAsync(r => r.Status == ApprovalStatus.Pending, cancellationToken);

        if (latestPeriod is null)
        {
            return new PayrollReadinessDto(
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                pendingApprovalsCount,
                0,
                null,
                null);
        }

        DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(7));
        int daysUntilCutoff = latestPeriod.AttendanceCutoffDate.DayNumber - today.DayNumber;

        int missingPunchCount = await _dbContext.AttendanceDailySummaries
            .AsNoTracking()
            .CountAsync(s =>
                s.WorkDate >= latestPeriod.PeriodStart &&
                s.WorkDate <= latestPeriod.AttendanceCutoffDate &&
                s.AttendanceStatus == AttendanceStatus.MissingPunch,
                cancellationToken);

        var totals = await _dbContext.Payslips
            .AsNoTracking()
            .Where(p => p.PayrollPeriodId == latestPeriod.Id)
            .GroupBy(p => p.PayrollPeriodId)
            .Select(g => new
            {
                TotalGross = g.Sum(x => x.GrossIncome),
                TotalNet = g.Sum(x => x.NetPay)
            })
            .FirstOrDefaultAsync(cancellationToken);

        return new PayrollReadinessDto(
            latestPeriod.Id,
            latestPeriod.PeriodCode,
            latestPeriod.PeriodStart,
            latestPeriod.PeriodEnd,
            latestPeriod.AttendanceCutoffDate,
            latestPeriod.PayDate,
            latestPeriod.Status,
            daysUntilCutoff,
            pendingApprovalsCount,
            missingPunchCount,
            totals?.TotalGross,
            totals?.TotalNet);
    }

    private async Task<DateOnly> ResolveBranchTodayAsync(Guid? branchId, CancellationToken cancellationToken)
    {
        string timeZoneId = "Asia/Bangkok";
        if (branchId.HasValue)
        {
            string? branchTz = await _dbContext.Branches
                .AsNoTracking()
                .Where(b => b.Id == branchId.Value)
                .Select(b => b.Timezone)
                .FirstOrDefaultAsync(cancellationToken);

            if (!string.IsNullOrWhiteSpace(branchTz))
            {
                timeZoneId = branchTz;
            }
        }

        try
        {
            TimeZoneInfo tz = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            DateTime localNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);
            return DateOnly.FromDateTime(localNow);
        }
        catch (TimeZoneNotFoundException)
        {
            return DateOnly.FromDateTime(DateTime.UtcNow.AddHours(7));
        }
    }
}

using System.Net;
using System.Text.Json;
using HrmSaas.Application.Common.Exceptions;
using HrmSaas.Application.Common.Interfaces;
using HrmSaas.Domain.Attendance;
using HrmSaas.Domain.Employees;
using HrmSaas.Domain.Organizations;
using HrmSaas.Domain.Workflows;
using Microsoft.EntityFrameworkCore;

namespace HrmSaas.Application.Features.Attendance;

public sealed class AttendanceService
{
    private readonly IAppDbContext _dbContext;
    private readonly ITenantContext _tenantContext;

    public AttendanceService(IAppDbContext dbContext, ITenantContext tenantContext)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
    }

    public async Task<IReadOnlyList<ShiftDefinitionDto>> GetShiftsAsync(CancellationToken cancellationToken = default)
    {
        List<ShiftDefinition> shifts = await _dbContext.ShiftDefinitions
            .AsNoTracking()
            .OrderBy(s => s.Code)
            .ToListAsync(cancellationToken);

        List<Guid> shiftIds = shifts.Select(s => s.Id).ToList();
        List<ShiftBreakRule> breakRules = await _dbContext.ShiftBreakRules
            .AsNoTracking()
            .Where(b => shiftIds.Contains(b.ShiftDefinitionId))
            .OrderBy(b => b.BreakStart)
            .ToListAsync(cancellationToken);

        var result = new List<ShiftDefinitionDto>();
        foreach (ShiftDefinition shift in shifts)
        {
            var rules = new List<ShiftBreakRuleDto>();
            foreach (ShiftBreakRule br in breakRules)
            {
                if (br.ShiftDefinitionId == shift.Id)
                {
                    rules.Add(new ShiftBreakRuleDto(br.Id, br.BreakStart, br.BreakEnd, br.DurationMinutes, br.IsPaid));
                }
            }

            result.Add(new ShiftDefinitionDto(
                shift.Id,
                shift.Code,
                shift.Name,
                shift.ShiftType,
                shift.StartTime,
                shift.EndTime,
                shift.IsOvernight,
                shift.GracePeriodMinutes,
                shift.LateAfterMinutes,
                shift.StandardWorkMinutes,
                rules));
        }

        return result;
    }

    public async Task<ShiftDefinitionDto> CreateShiftAsync(
        CreateShiftRequest request,
        CancellationToken cancellationToken = default)
    {
        (Guid tenantId, Guid companyId) = RequireTenantAndCompany();
        string normalizedCode = request.Code.Trim().ToUpperInvariant();

        bool duplicate = await _dbContext.ShiftDefinitions
            .AnyAsync(s => s.Code == normalizedCode, cancellationToken);

        if (duplicate)
        {
            throw new ConflictException($"Shift code '{normalizedCode}' already exists.");
        }

        var shift = ShiftDefinition.Create(
            tenantId,
            companyId,
            normalizedCode,
            request.Name,
            request.ShiftType,
            request.StartTime,
            request.EndTime,
            request.IsOvernight,
            request.GracePeriodMinutes,
            request.LateAfterMinutes,
            request.StandardWorkMinutes);

        _dbContext.ShiftDefinitions.Add(shift);

        var ruleDtos = new List<ShiftBreakRuleDto>();
        foreach (ShiftBreakRuleInput input in request.BreakRules)
        {
            var rule = ShiftBreakRule.Create(
                tenantId,
                shift.Id,
                input.BreakStart,
                input.BreakEnd,
                input.DurationMinutes,
                input.IsPaid);
            _dbContext.ShiftBreakRules.Add(rule);
            ruleDtos.Add(new ShiftBreakRuleDto(rule.Id, rule.BreakStart, rule.BreakEnd, rule.DurationMinutes, rule.IsPaid));
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new ShiftDefinitionDto(
            shift.Id,
            shift.Code,
            shift.Name,
            shift.ShiftType,
            shift.StartTime,
            shift.EndTime,
            shift.IsOvernight,
            shift.GracePeriodMinutes,
            shift.LateAfterMinutes,
            shift.StandardWorkMinutes,
            ruleDtos);
    }

    public async Task<ShiftDefinitionDto> UpdateShiftAsync(
        Guid id,
        UpdateShiftRequest request,
        CancellationToken cancellationToken = default)
    {
        Guid tenantId = _tenantContext.TenantId
            ?? throw new UnauthorizedException("Tenant context is required.");

        ShiftDefinition shift = await _dbContext.ShiftDefinitions
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken)
            ?? throw new NotFoundException("Shift definition not found.");

        shift.Update(
            request.Name,
            request.ShiftType,
            request.StartTime,
            request.EndTime,
            request.IsOvernight,
            request.GracePeriodMinutes,
            request.LateAfterMinutes,
            request.StandardWorkMinutes);

        List<ShiftBreakRule> existingRules = await _dbContext.ShiftBreakRules
            .Where(b => b.ShiftDefinitionId == shift.Id)
            .ToListAsync(cancellationToken);

        _dbContext.ShiftBreakRules.RemoveRange(existingRules);

        var ruleDtos = new List<ShiftBreakRuleDto>();
        foreach (ShiftBreakRuleInput input in request.BreakRules)
        {
            var rule = ShiftBreakRule.Create(
                tenantId,
                shift.Id,
                input.BreakStart,
                input.BreakEnd,
                input.DurationMinutes,
                input.IsPaid);
            _dbContext.ShiftBreakRules.Add(rule);
            ruleDtos.Add(new ShiftBreakRuleDto(rule.Id, rule.BreakStart, rule.BreakEnd, rule.DurationMinutes, rule.IsPaid));
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new ShiftDefinitionDto(
            shift.Id,
            shift.Code,
            shift.Name,
            shift.ShiftType,
            shift.StartTime,
            shift.EndTime,
            shift.IsOvernight,
            shift.GracePeriodMinutes,
            shift.LateAfterMinutes,
            shift.StandardWorkMinutes,
            ruleDtos);
    }

    public async Task<IReadOnlyList<RosterItemDto>> GetRostersAsync(
        DateOnly startDate,
        DateOnly endDate,
        Guid? branchId,
        Guid? departmentId,
        CancellationToken cancellationToken = default)
    {
        var employeeQuery = _dbContext.Employees.AsNoTracking();
        if (branchId.HasValue)
        {
            employeeQuery = employeeQuery.Where(e => e.BranchId == branchId.Value);
        }

        if (departmentId.HasValue)
        {
            employeeQuery = employeeQuery.Where(e => e.DepartmentId == departmentId.Value);
        }

        var employees = await employeeQuery
            .ToDictionaryAsync(e => e.Id, cancellationToken);

        List<Guid> employeeIds = employees.Keys.ToList();
        List<EmployeeShiftRoster> rosters = await _dbContext.EmployeeShiftRosters
            .AsNoTracking()
            .Where(r => employeeIds.Contains(r.EmployeeId) && r.WorkDate >= startDate && r.WorkDate <= endDate)
            .OrderBy(r => r.WorkDate)
            .ToListAsync(cancellationToken);

        var shifts = await _dbContext.ShiftDefinitions
            .AsNoTracking()
            .ToDictionaryAsync(s => s.Id, cancellationToken);

        var result = new List<RosterItemDto>();
        foreach (EmployeeShiftRoster roster in rosters)
        {
            employees.TryGetValue(roster.EmployeeId, out Employee? emp);
            ShiftDefinition? shift = null;
            if (roster.ShiftDefinitionId.HasValue)
            {
                shifts.TryGetValue(roster.ShiftDefinitionId.Value, out shift);
            }

            string empName = emp is null ? string.Empty : $"{emp.FirstName} {emp.LastName}".Trim();
            result.Add(new RosterItemDto(
                roster.Id,
                roster.EmployeeId,
                emp?.EmployeeCode ?? string.Empty,
                empName,
                roster.WorkDate,
                roster.ShiftDefinitionId,
                shift?.Code,
                shift?.Name,
                roster.DayType));
        }

        return result;
    }

    public async Task<MyTodayAttendanceDto> GetMyTodayAttendanceAsync(
        CancellationToken cancellationToken = default)
    {
        RequireTenantAndCompany();
        Employee employee = await RequireCurrentEmployeeAsync(cancellationToken);

        Branch branch = await _dbContext.Branches
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == employee.BranchId, cancellationToken)
            ?? throw new NotFoundException("Employee branch not found.");

        DateOnly workDate = ToBranchDate(DateTimeOffset.UtcNow, branch.Timezone);

        EmployeeShiftRoster? roster = await _dbContext.EmployeeShiftRosters
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.EmployeeId == employee.Id && r.WorkDate == workDate, cancellationToken);

        AttendanceDailySummary? summary = await _dbContext.AttendanceDailySummaries
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.EmployeeId == employee.Id && s.WorkDate == workDate, cancellationToken);

        Guid? shiftId = summary?.ShiftDefinitionId ?? roster?.ShiftDefinitionId;
        ShiftDefinition? shift = null;
        if (shiftId.HasValue)
        {
            shift = await _dbContext.ShiftDefinitions
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == shiftId.Value, cancellationToken);
        }

        string dayType = roster?.DayType ?? "UNSCHEDULED";
        string status = summary?.AttendanceStatus ?? "NOT_CHECKED_IN";

        return new MyTodayAttendanceDto(
            workDate,
            branch.Name,
            branch.Timezone,
            dayType,
            shift?.Id,
            shift?.Code,
            shift?.Name,
            shift?.StartTime,
            shift?.EndTime,
            shift?.IsOvernight ?? false,
            summary?.ActualInUtc,
            summary?.ActualOutUtc,
            summary?.WorkedMinutes ?? 0,
            summary?.LateMinutes ?? 0,
            summary?.EarlyOutMinutes ?? 0,
            summary?.OtMinutes ?? 0,
            status,
            summary?.IsLockedByPayroll ?? false);
    }

    public async Task<IReadOnlyList<MyUpcomingRosterDto>> GetMyUpcomingRostersAsync(
        int days = 7,
        CancellationToken cancellationToken = default)
    {
        RequireTenantAndCompany();
        Employee employee = await RequireCurrentEmployeeAsync(cancellationToken);

        int normalizedDays = days;
        if (normalizedDays < 1)
        {
            normalizedDays = 7;
        }
        else if (normalizedDays > 31)
        {
            normalizedDays = 31;
        }

        Branch branch = await _dbContext.Branches
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == employee.BranchId, cancellationToken)
            ?? throw new NotFoundException("Employee branch not found.");

        DateOnly startDate = ToBranchDate(DateTimeOffset.UtcNow, branch.Timezone);
        DateOnly endDate = startDate.AddDays(normalizedDays - 1);

        List<EmployeeShiftRoster> rosters = await _dbContext.EmployeeShiftRosters
            .AsNoTracking()
            .Where(r => r.EmployeeId == employee.Id && r.WorkDate >= startDate && r.WorkDate <= endDate)
            .OrderBy(r => r.WorkDate)
            .ToListAsync(cancellationToken);

        List<Guid> shiftIds = new List<Guid>();
        foreach (EmployeeShiftRoster roster in rosters)
        {
            if (roster.ShiftDefinitionId.HasValue && !shiftIds.Contains(roster.ShiftDefinitionId.Value))
            {
                shiftIds.Add(roster.ShiftDefinitionId.Value);
            }
        }

        Dictionary<Guid, ShiftDefinition> shifts = await _dbContext.ShiftDefinitions
            .AsNoTracking()
            .Where(s => shiftIds.Contains(s.Id))
            .ToDictionaryAsync(s => s.Id, cancellationToken);

        var result = new List<MyUpcomingRosterDto>();
        foreach (EmployeeShiftRoster roster in rosters)
        {
            ShiftDefinition? shift = null;
            if (roster.ShiftDefinitionId.HasValue)
            {
                shifts.TryGetValue(roster.ShiftDefinitionId.Value, out shift);
            }

            result.Add(new MyUpcomingRosterDto(
                roster.Id,
                roster.WorkDate,
                roster.DayType,
                roster.ShiftDefinitionId,
                shift?.Code,
                shift?.Name,
                shift?.StartTime,
                shift?.EndTime,
                shift?.IsOvernight ?? false));
        }

        return result;
    }

    public async Task<int> BulkAssignRostersAsync(
        BulkAssignRosterRequest request,
        CancellationToken cancellationToken = default)
    {
        (Guid tenantId, Guid companyId) = RequireTenantAndCompany();

        if (request.EndDate < request.StartDate)
        {
            throw new ValidationAppException(new Dictionary<string, string[]>
            {
                ["endDate"] = ["End date cannot be earlier than start date."]
            });
        }

        List<EmployeeShiftRoster> existing = await _dbContext.EmployeeShiftRosters
            .Where(r =>
                request.EmployeeIds.Contains(r.EmployeeId) &&
                r.WorkDate >= request.StartDate &&
                r.WorkDate <= request.EndDate)
            .ToListAsync(cancellationToken);

        _dbContext.EmployeeShiftRosters.RemoveRange(existing);

        int count = 0;
        for (DateOnly date = request.StartDate; date <= request.EndDate; date = date.AddDays(1))
        {
            foreach (Guid empId in request.EmployeeIds)
            {
                _dbContext.EmployeeShiftRosters.Add(
                    EmployeeShiftRoster.Create(
                        tenantId,
                        companyId,
                        empId,
                        request.ShiftDefinitionId,
                        date,
                        request.DayType));
                count++;
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return count;
    }

    public async Task<AttendanceDailySummaryDto> CheckInAsync(
        CheckInOutRequest request,
        IPAddress clientIp,
        CancellationToken cancellationToken = default)
    {
        (Guid tenantId, Guid companyId) = RequireTenantAndCompany();
        Employee employee = await RequireCurrentEmployeeAsync(cancellationToken);
        Branch branch = await _dbContext.Branches
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == employee.BranchId, cancellationToken)
            ?? throw new NotFoundException("Employee branch not found.");

        DateTimeOffset utcNow = DateTimeOffset.UtcNow;
        DateOnly workDate = ToBranchDate(utcNow, branch.Timezone);
        bool isWithinGeofence = CheckGeofenceOrIp(branch, request.Latitude, request.Longitude, clientIp);

        _dbContext.AttendanceLogs.Add(
            AttendanceLog.Create(
                tenantId,
                companyId,
                employee.Id,
                branch.Id,
                utcNow,
                PunchType.CheckIn,
                request.Source,
                request.Latitude,
                request.Longitude,
                clientIp,
                isWithinGeofence));

        EmployeeShiftRoster? roster = await _dbContext.EmployeeShiftRosters
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.EmployeeId == employee.Id && r.WorkDate == workDate, cancellationToken);

        ShiftDefinition? shift = null;
        if (roster?.ShiftDefinitionId is not null)
        {
            shift = await _dbContext.ShiftDefinitions
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == roster.ShiftDefinitionId.Value, cancellationToken);
        }
        else
        {
            shift = await _dbContext.ShiftDefinitions
                .AsNoTracking()
                .OrderBy(s => s.Code)
                .FirstOrDefaultAsync(cancellationToken);
        }

        int lateMinutes = CalculateLateMinutes(utcNow, workDate, branch.Timezone, shift);
        string status = lateMinutes > 0 ? AttendanceStatus.Late : AttendanceStatus.Present;

        AttendanceDailySummary? summary = await _dbContext.AttendanceDailySummaries
            .FirstOrDefaultAsync(s => s.EmployeeId == employee.Id && s.WorkDate == workDate, cancellationToken);

        if (summary is null)
        {
            summary = AttendanceDailySummary.Create(
                tenantId,
                companyId,
                employee.Id,
                workDate,
                shift?.Id,
                utcNow,
                null,
                workedMinutes: 0,
                lateMinutes: lateMinutes,
                earlyOutMinutes: 0,
                otMinutes: 0,
                attendanceStatus: status);
            _dbContext.AttendanceDailySummaries.Add(summary);
        }
        else if (!summary.ActualInUtc.HasValue)
        {
            _dbContext.AttendanceDailySummaries.Remove(summary);
            summary = AttendanceDailySummary.Create(
                tenantId,
                companyId,
                employee.Id,
                workDate,
                shift?.Id ?? summary.ShiftDefinitionId,
                utcNow,
                summary.ActualOutUtc,
                summary.WorkedMinutes,
                lateMinutes,
                summary.EarlyOutMinutes,
                summary.OtMinutes,
                status);
            _dbContext.AttendanceDailySummaries.Add(summary);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return BuildSummaryDto(summary, employee, shift);
    }

    public async Task<AttendanceDailySummaryDto> CheckOutAsync(
        CheckInOutRequest request,
        IPAddress clientIp,
        CancellationToken cancellationToken = default)
    {
        (Guid tenantId, Guid companyId) = RequireTenantAndCompany();
        Employee employee = await RequireCurrentEmployeeAsync(cancellationToken);
        Branch branch = await _dbContext.Branches
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == employee.BranchId, cancellationToken)
            ?? throw new NotFoundException("Employee branch not found.");

        DateTimeOffset utcNow = DateTimeOffset.UtcNow;
        DateOnly workDate = ToBranchDate(utcNow, branch.Timezone);
        bool isWithinGeofence = CheckGeofenceOrIp(branch, request.Latitude, request.Longitude, clientIp);

        _dbContext.AttendanceLogs.Add(
            AttendanceLog.Create(
                tenantId,
                companyId,
                employee.Id,
                branch.Id,
                utcNow,
                PunchType.CheckOut,
                request.Source,
                request.Latitude,
                request.Longitude,
                clientIp,
                isWithinGeofence));

        AttendanceDailySummary? existing = await _dbContext.AttendanceDailySummaries
            .FirstOrDefaultAsync(s => s.EmployeeId == employee.Id && s.WorkDate == workDate, cancellationToken);

        ShiftDefinition? shift = null;
        if (existing?.ShiftDefinitionId is not null)
        {
            shift = await _dbContext.ShiftDefinitions
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == existing.ShiftDefinitionId.Value, cancellationToken);
        }

        int workedMinutes = 0;
        if (existing?.ActualInUtc is not null)
        {
            workedMinutes = Math.Max(0, (int)(utcNow - existing.ActualInUtc.Value).TotalMinutes);
        }

        int earlyOutMinutes = CalculateEarlyOutMinutes(utcNow, workDate, branch.Timezone, shift);
        string status = earlyOutMinutes > 0
            ? AttendanceStatus.EarlyOut
            : existing?.AttendanceStatus ?? AttendanceStatus.Present;

        if (existing is not null)
        {
            _dbContext.AttendanceDailySummaries.Remove(existing);
        }

        var updated = AttendanceDailySummary.Create(
            tenantId,
            companyId,
            employee.Id,
            workDate,
            shift?.Id ?? existing?.ShiftDefinitionId,
            existing?.ActualInUtc,
            utcNow,
            workedMinutes,
            existing?.LateMinutes ?? 0,
            earlyOutMinutes,
            existing?.OtMinutes ?? 0,
            status,
            existing?.IsLockedByPayroll ?? false);

        _dbContext.AttendanceDailySummaries.Add(updated);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return BuildSummaryDto(updated, employee, shift);
    }

    public async Task<IReadOnlyList<AttendanceDailySummaryDto>> GetDailySummariesAsync(
        DateOnly startDate,
        DateOnly endDate,
        Guid? branchId,
        Guid? departmentId,
        string? status,
        CancellationToken cancellationToken = default)
    {
        var empQuery = _dbContext.Employees.AsNoTracking();
        if (branchId.HasValue)
        {
            empQuery = empQuery.Where(e => e.BranchId == branchId.Value);
        }

        if (departmentId.HasValue)
        {
            empQuery = empQuery.Where(e => e.DepartmentId == departmentId.Value);
        }

        var employees = await empQuery.ToDictionaryAsync(e => e.Id, cancellationToken);
        List<Guid> employeeIds = employees.Keys.ToList();

        var summaryQuery = _dbContext.AttendanceDailySummaries
            .AsNoTracking()
            .Where(s => employeeIds.Contains(s.EmployeeId) && s.WorkDate >= startDate && s.WorkDate <= endDate);

        if (!string.IsNullOrWhiteSpace(status))
        {
            string normalizedStatus = status.Trim().ToUpperInvariant();
            summaryQuery = summaryQuery.Where(s => s.AttendanceStatus == normalizedStatus);
        }

        List<AttendanceDailySummary> summaries = await summaryQuery
            .OrderByDescending(s => s.WorkDate)
            .ToListAsync(cancellationToken);

        var shifts = await _dbContext.ShiftDefinitions
            .AsNoTracking()
            .ToDictionaryAsync(s => s.Id, cancellationToken);

        var result = new List<AttendanceDailySummaryDto>();
        foreach (AttendanceDailySummary summary in summaries)
        {
            if (!employees.TryGetValue(summary.EmployeeId, out Employee? emp))
            {
                continue;
            }

            ShiftDefinition? shift = null;
            if (summary.ShiftDefinitionId.HasValue)
            {
                shifts.TryGetValue(summary.ShiftDefinitionId.Value, out shift);
            }

            result.Add(BuildSummaryDto(summary, emp, shift));
        }

        return result;
    }

    public async Task<IReadOnlyList<AttendanceDailySummaryDto>> GetMyAttendanceHistoryAsync(
        DateOnly? startDate,
        DateOnly? endDate,
        string? status,
        CancellationToken cancellationToken = default)
    {
        Employee employee = await RequireCurrentEmployeeAsync(cancellationToken);

        Branch? branch = await _dbContext.Branches
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == employee.BranchId, cancellationToken);
        string branchTimezone = branch?.Timezone ?? "Asia/Bangkok";

        DateOnly today = ToBranchDate(DateTimeOffset.UtcNow, branchTimezone);
        DateOnly effectiveEnd = endDate ?? today;
        DateOnly effectiveStart = startDate ?? effectiveEnd.AddDays(-29);

        if (effectiveStart > effectiveEnd)
        {
            throw new ValidationAppException(
                new Dictionary<string, string[]>
                {
                    ["startDate"] = ["startDate must be on or before endDate."],
                },
                "Invalid date range.");
        }

        IQueryable<AttendanceDailySummary> summaryQuery = _dbContext.AttendanceDailySummaries
            .AsNoTracking()
            .Where(s =>
                s.EmployeeId == employee.Id &&
                s.WorkDate >= effectiveStart &&
                s.WorkDate <= effectiveEnd);

        if (!string.IsNullOrWhiteSpace(status))
        {
            string normalizedStatus = status.Trim().ToUpperInvariant();
            summaryQuery = summaryQuery.Where(s => s.AttendanceStatus == normalizedStatus);
        }

        List<AttendanceDailySummary> summaries = await summaryQuery
            .OrderByDescending(s => s.WorkDate)
            .ToListAsync(cancellationToken);

        Dictionary<Guid, ShiftDefinition> shifts = await _dbContext.ShiftDefinitions
            .AsNoTracking()
            .ToDictionaryAsync(s => s.Id, cancellationToken);

        var result = new List<AttendanceDailySummaryDto>();
        foreach (AttendanceDailySummary summary in summaries)
        {
            ShiftDefinition? shift = null;
            if (summary.ShiftDefinitionId.HasValue)
            {
                shifts.TryGetValue(summary.ShiftDefinitionId.Value, out shift);
            }

            result.Add(BuildSummaryDto(summary, employee, shift));
        }

        return result;
    }

    public async Task<TimeAdjustmentCreatedDto> RequestTimeAdjustmentAsync(
        TimeAdjustmentRequest request,
        CancellationToken cancellationToken = default)
    {
        (Guid tenantId, Guid companyId) = RequireTenantAndCompany();
        Employee employee = await RequireCurrentEmployeeAsync(cancellationToken);

        WorkflowDefinition? wf = await _dbContext.WorkflowDefinitions
            .AsNoTracking()
            .Where(w =>
                w.IsActive &&
                w.RequestType == WorkflowRequestType.TimeAdjustment &&
                (w.DepartmentId == employee.DepartmentId || w.DepartmentId == null))
            .OrderByDescending(w => w.DepartmentId.HasValue)
            .FirstOrDefaultAsync(cancellationToken);

        if (wf is null)
        {
            throw new NotFoundException("No active workflow definition found for time adjustment.");
        }

        string payloadJson = JsonSerializer.Serialize(new
        {
            workDate = request.WorkDate,
            requestedInUtc = request.RequestedInUtc,
            requestedOutUtc = request.RequestedOutUtc,
            reason = request.Reason
        });

        DateTimeOffset utcNow = DateTimeOffset.UtcNow;
        var approval = ApprovalRequest.Create(
            tenantId,
            companyId,
            employee.Id,
            WorkflowRequestType.TimeAdjustment,
            Guid.CreateVersion7(),
            wf.Id,
            payloadJson,
            utcNow);

        _dbContext.ApprovalRequests.Add(approval);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new TimeAdjustmentCreatedDto(approval.Id, approval.Status, approval.SubmittedAt);
    }

    private async Task<Employee> RequireCurrentEmployeeAsync(CancellationToken cancellationToken)
    {
        Guid userId = _tenantContext.CurrentUserId
            ?? throw new UnauthorizedException("Authenticated user context is required.");

        return await _dbContext.Employees
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.UserId == userId, cancellationToken)
            ?? throw new NotFoundException("Employee profile not found for the current user.");
    }

    private (Guid TenantId, Guid CompanyId) RequireTenantAndCompany()
    {
        Guid tenantId = _tenantContext.TenantId
            ?? throw new UnauthorizedException("Tenant context is required.");
        Guid companyId = _tenantContext.ActiveCompanyId
            ?? throw new ForbiddenException("Active company context is required.");
        return (tenantId, companyId);
    }

    private static DateOnly ToBranchDate(DateTimeOffset utcNow, string timezoneId)
    {
        try
        {
            TimeZoneInfo tz = TimeZoneInfo.FindSystemTimeZoneById(timezoneId);
            DateTimeOffset localTime = TimeZoneInfo.ConvertTime(utcNow, tz);
            return DateOnly.FromDateTime(localTime.DateTime);
        }
        catch (TimeZoneNotFoundException)
        {
            return DateOnly.FromDateTime(utcNow.UtcDateTime);
        }
    }

    private static int CalculateLateMinutes(
        DateTimeOffset checkInUtc,
        DateOnly workDate,
        string timezoneId,
        ShiftDefinition? shift)
    {
        if (shift is null)
        {
            return 0;
        }

        try
        {
            TimeZoneInfo tz = TimeZoneInfo.FindSystemTimeZoneById(timezoneId);
            DateTime localShiftStart = workDate.ToDateTime(shift.StartTime);
            DateTimeOffset shiftStartUtc = new DateTimeOffset(localShiftStart, tz.GetUtcOffset(localShiftStart)).ToUniversalTime();
            int diffMinutes = (int)(checkInUtc - shiftStartUtc).TotalMinutes;
            return diffMinutes > shift.GracePeriodMinutes ? diffMinutes : 0;
        }
        catch (TimeZoneNotFoundException)
        {
            return 0;
        }
    }

    private static int CalculateEarlyOutMinutes(
        DateTimeOffset checkOutUtc,
        DateOnly workDate,
        string timezoneId,
        ShiftDefinition? shift)
    {
        if (shift is null)
        {
            return 0;
        }

        try
        {
            TimeZoneInfo tz = TimeZoneInfo.FindSystemTimeZoneById(timezoneId);
            DateOnly endDate = shift.IsOvernight ? workDate.AddDays(1) : workDate;
            DateTime localShiftEnd = endDate.ToDateTime(shift.EndTime);
            DateTimeOffset shiftEndUtc = new DateTimeOffset(localShiftEnd, tz.GetUtcOffset(localShiftEnd)).ToUniversalTime();
            int diffMinutes = (int)(shiftEndUtc - checkOutUtc).TotalMinutes;
            return diffMinutes > 0 ? diffMinutes : 0;
        }
        catch (TimeZoneNotFoundException)
        {
            return 0;
        }
    }

    private static bool CheckGeofenceOrIp(Branch branch, decimal? lat, decimal? lng, IPAddress clientIp)
    {
        string ipText = clientIp.ToString();
        if (branch.AllowedIps.Contains(ipText, StringComparer.OrdinalIgnoreCase))
        {
            return true;
        }

        if (!lat.HasValue || !lng.HasValue || branch.GeofenceRadiusMeters <= 0)
        {
            return false;
        }

        double distanceMeters = CalculateHaversineMeters(
            (double)branch.Latitude,
            (double)branch.Longitude,
            (double)lat.Value,
            (double)lng.Value);

        return distanceMeters <= branch.GeofenceRadiusMeters;
    }

    private static double CalculateHaversineMeters(double lat1, double lon1, double lat2, double lon2)
    {
        const double EarthRadiusMeters = 6_371_000;
        double dLat = (lat2 - lat1) * Math.PI / 180.0;
        double dLon = (lon2 - lon1) * Math.PI / 180.0;
        double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                   Math.Cos(lat1 * Math.PI / 180.0) * Math.Cos(lat2 * Math.PI / 180.0) *
                   Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return EarthRadiusMeters * c;
    }

    private static AttendanceDailySummaryDto BuildSummaryDto(
        AttendanceDailySummary summary,
        Employee emp,
        ShiftDefinition? shift)
    {
        return new AttendanceDailySummaryDto(
            summary.Id,
            summary.EmployeeId,
            emp.EmployeeCode,
            $"{emp.FirstName} {emp.LastName}".Trim(),
            emp.BranchId,
            emp.DepartmentId,
            summary.WorkDate,
            summary.ShiftDefinitionId,
            shift?.Name,
            summary.ActualInUtc,
            summary.ActualOutUtc,
            summary.WorkedMinutes,
            summary.LateMinutes,
            summary.EarlyOutMinutes,
            summary.OtMinutes,
            summary.AttendanceStatus,
            summary.IsLockedByPayroll);
    }
}

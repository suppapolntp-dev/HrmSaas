using HrmSaas.Application.Common.Exceptions;
using HrmSaas.Application.Common.Interfaces;
using HrmSaas.Domain.Attendance;
using HrmSaas.Domain.Employees;
using HrmSaas.Domain.Payroll;
using Microsoft.EntityFrameworkCore;

namespace HrmSaas.Application.Features.Payroll;

public sealed class PayrollService
{
    private readonly IAppDbContext _dbContext;
    private readonly ITenantContext _tenantContext;

    public PayrollService(IAppDbContext dbContext, ITenantContext tenantContext)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
    }

    public async Task<IReadOnlyList<PayrollPeriodDto>> GetPeriodsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.PayrollPeriods
            .AsNoTracking()
            .OrderByDescending(p => p.PeriodStart)
            .Select(p => new PayrollPeriodDto(
                p.Id,
                p.PeriodCode,
                p.PeriodStart,
                p.PeriodEnd,
                p.AttendanceCutoffDate,
                p.PayDate,
                p.Status))
            .ToListAsync(cancellationToken);
    }

    public async Task<PayrollPeriodDto> CreatePeriodAsync(
        CreatePayrollPeriodRequest request,
        CancellationToken cancellationToken = default)
    {
        (Guid tenantId, Guid companyId) = RequireTenantAndCompany();
        string normalizedCode = request.PeriodCode.Trim().ToUpperInvariant();

        bool duplicate = await _dbContext.PayrollPeriods
            .AnyAsync(p => p.PeriodCode == normalizedCode, cancellationToken);

        if (duplicate)
        {
            throw new ConflictException($"Payroll period '{normalizedCode}' already exists.");
        }

        var period = PayrollPeriod.Create(
            tenantId,
            companyId,
            normalizedCode,
            request.PeriodStart,
            request.PeriodEnd,
            request.AttendanceCutoffDate,
            request.PayDate);

        _dbContext.PayrollPeriods.Add(period);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new PayrollPeriodDto(
            period.Id,
            period.PeriodCode,
            period.PeriodStart,
            period.PeriodEnd,
            period.AttendanceCutoffDate,
            period.PayDate,
            period.Status);
    }

    public async Task<PayrollCalculationSummaryDto> CalculatePeriodAsync(
        Guid periodId,
        CancellationToken cancellationToken = default)
    {
        (Guid tenantId, Guid companyId) = RequireTenantAndCompany();

        PayrollPeriod period = await _dbContext.PayrollPeriods
            .FirstOrDefaultAsync(p => p.Id == periodId, cancellationToken)
            ?? throw new NotFoundException("Payroll period not found.");

        if (period.Status == PayrollPeriodStatus.Locked)
        {
            throw new ConflictException("Cannot recalculate a locked payroll period.");
        }

        // Remove previous unlocked payslips for this period before recalculating
        List<Payslip> existingPayslips = await _dbContext.Payslips
            .Where(ps => ps.PayrollPeriodId == period.Id)
            .ToListAsync(cancellationToken);

        if (existingPayslips.Count > 0)
        {
            List<Guid> oldPayslipIds = existingPayslips.Select(p => p.Id).ToList();
            List<PayslipItem> oldItems = await _dbContext.PayslipItems
                .Where(pi => oldPayslipIds.Contains(pi.PayslipId))
                .ToListAsync(cancellationToken);

            _dbContext.PayslipItems.RemoveRange(oldItems);
            _dbContext.Payslips.RemoveRange(existingPayslips);
        }

        List<Employee> activeEmployees = await _dbContext.Employees
            .AsNoTracking()
            .Where(e => e.EmploymentStatus == EmploymentStatus.Active || e.EmploymentStatus == EmploymentStatus.Probation)
            .ToListAsync(cancellationToken);

        List<EmploymentContract> activeContracts = await _dbContext.EmploymentContracts
            .AsNoTracking()
            .Where(c => c.IsActive)
            .ToListAsync(cancellationToken);

        var contractByEmployeeId = new Dictionary<Guid, EmploymentContract>();
        foreach (EmploymentContract contract in activeContracts)
        {
            contractByEmployeeId[contract.EmployeeId] = contract;
        }

        List<AttendanceDailySummary> attendanceList = await _dbContext.AttendanceDailySummaries
            .AsNoTracking()
            .Where(a => a.WorkDate >= period.PeriodStart && a.WorkDate <= period.AttendanceCutoffDate)
            .ToListAsync(cancellationToken);

        List<SalaryComponent> components = await _dbContext.SalaryComponents
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        SalaryComponent? baseSalaryComp = components.FirstOrDefault(c => c.Code == "BASE_SALARY");
        SalaryComponent? otComp = components.FirstOrDefault(c => c.Code == "OT_1_5X");
        SalaryComponent? lateComp = components.FirstOrDefault(c => c.Code == "LATE_DEDUCTION");

        SsoConfiguration? ssoConfig = await _dbContext.SsoConfigurations
            .AsNoTracking()
            .Where(s =>
                s.CountryCode == "TH" &&
                s.EffectiveFrom <= period.PayDate &&
                (s.EffectiveTo == null || s.EffectiveTo >= period.PayDate))
            .OrderByDescending(s => s.EffectiveFrom)
            .FirstOrDefaultAsync(cancellationToken);

        List<TaxBracket> taxBrackets = await _dbContext.TaxBrackets
            .AsNoTracking()
            .Where(t => t.CountryCode == "TH" && t.TaxYear == period.PayDate.Year)
            .OrderBy(t => t.MinNetIncome)
            .ToListAsync(cancellationToken);

        decimal totalGrossAll = 0m;
        decimal totalNetAll = 0m;
        int processedCount = 0;

        foreach (Employee emp in activeEmployees)
        {
            if (!contractByEmployeeId.TryGetValue(emp.Id, out EmploymentContract? contract))
            {
                continue;
            }

            decimal baseSalary = contract.BaseSalary;
            int totalOtMinutes = 0;
            int totalLateMinutes = 0;

            foreach (AttendanceDailySummary att in attendanceList)
            {
                if (att.EmployeeId == emp.Id)
                {
                    totalOtMinutes += att.OtMinutes;
                    totalLateMinutes += att.LateMinutes;
                }
            }

            decimal hourlyRate = baseSalary / 240m;
            decimal otPay = Math.Round((totalOtMinutes / 60m) * hourlyRate * 1.5m, 2);
            decimal lateDeduction = Math.Round((totalLateMinutes / 60m) * hourlyRate, 2);

            decimal grossIncome = baseSalary + otPay;
            decimal ssoEmployee = CalculateSsoEmployee(baseSalary, ssoConfig);
            decimal monthlyTax = CalculateMonthlyWithholdingTax(grossIncome - ssoEmployee, taxBrackets);
            decimal totalDeductions = ssoEmployee + monthlyTax + lateDeduction;
            decimal netPay = Math.Max(0m, grossIncome - totalDeductions);

            var payslip = Payslip.Create(
                tenantId,
                companyId,
                period.Id,
                emp.Id,
                grossIncome,
                totalDeductions,
                ssoEmployee,
                monthlyTax,
                netPay,
                isLocked: false);

            _dbContext.Payslips.Add(payslip);

            if (baseSalaryComp is not null)
            {
                _dbContext.PayslipItems.Add(
                    PayslipItem.Create(tenantId, payslip.Id, baseSalaryComp.Id, baseSalaryComp.Name, baseSalary, 1m));
            }

            if (otComp is not null && otPay > 0m)
            {
                decimal otHours = Math.Round(totalOtMinutes / 60m, 2);
                _dbContext.PayslipItems.Add(
                    PayslipItem.Create(tenantId, payslip.Id, otComp.Id, otComp.Name, otPay, otHours));
            }

            if (lateComp is not null && lateDeduction > 0m)
            {
                decimal lateHours = Math.Round(totalLateMinutes / 60m, 2);
                _dbContext.PayslipItems.Add(
                    PayslipItem.Create(tenantId, payslip.Id, lateComp.Id, lateComp.Name, -lateDeduction, lateHours));
            }

            totalGrossAll += grossIncome;
            totalNetAll += netPay;
            processedCount++;
        }

        var markResult = period.MarkCalculated(DateTimeOffset.UtcNow);
        if (markResult.IsFailure)
        {
            throw new ConflictException(markResult.Error.Message);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new PayrollCalculationSummaryDto(
            period.Id,
            period.PeriodCode,
            period.Status,
            processedCount,
            totalGrossAll,
            totalNetAll);
    }

    public async Task<PayrollPeriodDto> LockPeriodAsync(Guid periodId, CancellationToken cancellationToken = default)
    {
        PayrollPeriod period = await _dbContext.PayrollPeriods
            .FirstOrDefaultAsync(p => p.Id == periodId, cancellationToken)
            ?? throw new NotFoundException("Payroll period not found.");

        var lockResult = period.Lock(DateTimeOffset.UtcNow);
        if (lockResult.IsFailure)
        {
            throw new ConflictException(lockResult.Error.Message);
        }

        List<Payslip> payslips = await _dbContext.Payslips
            .Where(ps => ps.PayrollPeriodId == period.Id)
            .ToListAsync(cancellationToken);

        foreach (Payslip ps in payslips)
        {
            ps.Lock();
        }

        List<AttendanceDailySummary> summaries = await _dbContext.AttendanceDailySummaries
            .Where(a => a.WorkDate >= period.PeriodStart && a.WorkDate <= period.AttendanceCutoffDate)
            .ToListAsync(cancellationToken);

        foreach (AttendanceDailySummary summary in summaries)
        {
            summary.LockForPayroll();
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new PayrollPeriodDto(
            period.Id,
            period.PeriodCode,
            period.PeriodStart,
            period.PeriodEnd,
            period.AttendanceCutoffDate,
            period.PayDate,
            period.Status);
    }

    public async Task<IReadOnlyList<PayslipDto>> GetPayslipsAsync(
        Guid? payrollPeriodId,
        bool canReadAllEmployees,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Payslips.AsNoTracking();
        if (payrollPeriodId.HasValue)
        {
            query = query.Where(p => p.PayrollPeriodId == payrollPeriodId.Value);
        }

        if (!canReadAllEmployees)
        {
            Guid userId = _tenantContext.CurrentUserId
                ?? throw new UnauthorizedException("User context is required.");
            Employee? currentEmp = await _dbContext.Employees
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.UserId == userId, cancellationToken);

            if (currentEmp is null)
            {
                return [];
            }

            query = query.Where(p => p.EmployeeId == currentEmp.Id);
        }

        List<Payslip> payslips = await query
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);

        List<Guid> payslipIds = payslips.Select(p => p.Id).ToList();
        List<PayslipItem> items = await _dbContext.PayslipItems
            .AsNoTracking()
            .Where(i => payslipIds.Contains(i.PayslipId))
            .ToListAsync(cancellationToken);

        var periods = await _dbContext.PayrollPeriods
            .AsNoTracking()
            .ToDictionaryAsync(p => p.Id, p => p.PeriodCode, cancellationToken);

        var employees = await _dbContext.Employees
            .AsNoTracking()
            .ToDictionaryAsync(e => e.Id, cancellationToken);

        var result = new List<PayslipDto>();
        foreach (Payslip ps in payslips)
        {
            employees.TryGetValue(ps.EmployeeId, out Employee? emp);
            string empName = emp is null ? string.Empty : $"{emp.FirstName} {emp.LastName}".Trim();

            var itemDtos = new List<PayslipItemDto>();
            foreach (PayslipItem item in items)
            {
                if (item.PayslipId == ps.Id)
                {
                    itemDtos.Add(new PayslipItemDto(
                        item.Id,
                        item.SalaryComponentId,
                        item.ComponentNameSnapshot,
                        item.Amount,
                        item.Units));
                }
            }

            result.Add(new PayslipDto(
                ps.Id,
                ps.PayrollPeriodId,
                periods.GetValueOrDefault(ps.PayrollPeriodId, string.Empty),
                ps.EmployeeId,
                emp?.EmployeeCode ?? string.Empty,
                empName,
                ps.GrossIncome,
                ps.TotalDeductions,
                ps.SsoEmployeeAmount,
                ps.WithholdingTaxAmount,
                ps.NetPay,
                ps.IsLocked,
                itemDtos));
        }

        return result;
    }

    private static decimal CalculateSsoEmployee(decimal baseSalary, SsoConfiguration? config)
    {
        if (config is null || baseSalary <= 0m)
        {
            return 0m;
        }

        decimal wageBase = Math.Min(Math.Max(baseSalary, config.MinWageBase), config.MaxWageBase);
        return Math.Round(wageBase * (config.EmployeeRatePercent / 100m), 2);
    }

    private static decimal CalculateMonthlyWithholdingTax(decimal monthlyTaxableIncome, List<TaxBracket> brackets)
    {
        if (brackets.Count == 0 || monthlyTaxableIncome <= 0m)
        {
            return 0m;
        }

        decimal annualTaxable = Math.Max(0m, (monthlyTaxableIncome * 12m) - 160_000m);
        decimal annualTax = 0m;

        foreach (TaxBracket bracket in brackets)
        {
            if (annualTaxable <= bracket.MinNetIncome)
            {
                break;
            }

            decimal upper = bracket.MaxNetIncome ?? annualTaxable;
            decimal taxableInTier = Math.Min(annualTaxable, upper) - bracket.MinNetIncome;
            if (taxableInTier > 0m)
            {
                annualTax += taxableInTier * (bracket.TaxRatePercent / 100m);
            }
        }

        return Math.Round(annualTax / 12m, 2);
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

using HrmSaas.Domain.Common;

namespace HrmSaas.Domain.Payroll;

public static class PayrollPeriodStatus
{
    public const string Draft = "DRAFT";
    public const string Calculated = "CALCULATED";
    public const string Approved = "APPROVED";
    public const string Locked = "LOCKED";
}

public sealed class PayrollPeriod : TenantCompanyEntity
{
    private PayrollPeriod()
    {
    }

    public string PeriodCode { get; private set; } = string.Empty;
    public DateOnly PeriodStart { get; private set; }
    public DateOnly PeriodEnd { get; private set; }
    public DateOnly AttendanceCutoffDate { get; private set; }
    public DateOnly PayDate { get; private set; }
    public string Status { get; private set; } = PayrollPeriodStatus.Draft;
    public uint RowVersion { get; private set; }

    public static PayrollPeriod Create(
        Guid tenantId,
        Guid companyId,
        string periodCode,
        DateOnly periodStart,
        DateOnly periodEnd,
        DateOnly attendanceCutoffDate,
        DateOnly payDate,
        Guid? id = null)
    {
        if (string.IsNullOrWhiteSpace(periodCode))
        {
            throw new ArgumentException("Period code is required.", nameof(periodCode));
        }

        if (periodEnd < periodStart)
        {
            throw new ArgumentException("PeriodEnd cannot be earlier than PeriodStart.");
        }

        var period = new PayrollPeriod
        {
            TenantId = tenantId,
            CompanyId = companyId,
            PeriodCode = periodCode.Trim().ToUpperInvariant(),
            PeriodStart = periodStart,
            PeriodEnd = periodEnd,
            AttendanceCutoffDate = attendanceCutoffDate,
            PayDate = payDate,
            Status = PayrollPeriodStatus.Draft
        };

        if (id.HasValue)
        {
            period.Id = id.Value;
        }

        return period;
    }

    public Result MarkCalculated(DateTimeOffset utcNow)
    {
        if (Status == PayrollPeriodStatus.Locked)
        {
            return Result.Failure(DomainErrors.Payroll.PeriodAlreadyLocked);
        }

        Status = PayrollPeriodStatus.Calculated;
        UpdatedAt = utcNow;
        return Result.Success();
    }

    public Result Approve(DateTimeOffset utcNow)
    {
        if (Status == PayrollPeriodStatus.Locked)
        {
            return Result.Failure(DomainErrors.Payroll.PeriodAlreadyLocked);
        }

        if (Status != PayrollPeriodStatus.Calculated)
        {
            return Result.Failure(DomainErrors.Payroll.PeriodNotCalculated);
        }

        Status = PayrollPeriodStatus.Approved;
        UpdatedAt = utcNow;
        return Result.Success();
    }

    public Result Lock(DateTimeOffset utcNow)
    {
        if (Status == PayrollPeriodStatus.Locked)
        {
            return Result.Failure(DomainErrors.Payroll.PeriodAlreadyLocked);
        }

        if (Status != PayrollPeriodStatus.Calculated && Status != PayrollPeriodStatus.Approved)
        {
            return Result.Failure(DomainErrors.Payroll.PeriodNotCalculated);
        }

        Status = PayrollPeriodStatus.Locked;
        UpdatedAt = utcNow;
        return Result.Success();
    }
}

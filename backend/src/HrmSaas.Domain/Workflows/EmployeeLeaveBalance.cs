using HrmSaas.Domain.Common;

namespace HrmSaas.Domain.Workflows;

public sealed class EmployeeLeaveBalance : TenantCompanyEntity
{
    private EmployeeLeaveBalance()
    {
    }

    public Guid EmployeeId { get; private set; }
    public Guid LeaveTypeId { get; private set; }
    public int QuotaYear { get; private set; }
    public decimal CarriedOverDays { get; private set; }
    public decimal EntitledDays { get; private set; }
    public decimal UsedDays { get; private set; }
    public decimal PendingDays { get; private set; }
    public uint RowVersion { get; private set; }

    public decimal RemainingDays => CarriedOverDays + EntitledDays - UsedDays - PendingDays;

    public static EmployeeLeaveBalance Create(
        Guid tenantId,
        Guid companyId,
        Guid employeeId,
        Guid leaveTypeId,
        int quotaYear,
        decimal carriedOverDays,
        decimal entitledDays,
        decimal usedDays = 0m,
        decimal pendingDays = 0m,
        Guid? id = null)
    {
        if (carriedOverDays < 0 || entitledDays < 0 || usedDays < 0 || pendingDays < 0)
        {
            throw new ArgumentException("Leave balance days cannot be negative.");
        }

        var balance = new EmployeeLeaveBalance
        {
            TenantId = tenantId,
            CompanyId = companyId,
            EmployeeId = employeeId,
            LeaveTypeId = leaveTypeId,
            QuotaYear = quotaYear,
            CarriedOverDays = carriedOverDays,
            EntitledDays = entitledDays,
            UsedDays = usedDays,
            PendingDays = pendingDays
        };

        if (id.HasValue)
        {
            balance.Id = id.Value;
        }

        return balance;
    }

    public Result ReservePending(decimal days)
    {
        if (days <= 0)
        {
            return Result.Failure(DomainErrors.Leave.InvalidQuotaAmount);
        }

        if (RemainingDays < days)
        {
            return Result.Failure(DomainErrors.Leave.InsufficientBalance);
        }

        PendingDays += days;
        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }

    public Result ApprovePending(decimal days)
    {
        if (days <= 0 || PendingDays < days)
        {
            return Result.Failure(DomainErrors.Leave.InvalidQuotaAmount);
        }

        PendingDays -= days;
        UsedDays += days;
        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }

    public Result ReleasePending(decimal days)
    {
        if (days <= 0 || PendingDays < days)
        {
            return Result.Failure(DomainErrors.Leave.InvalidQuotaAmount);
        }

        PendingDays -= days;
        UpdatedAt = DateTimeOffset.UtcNow;
        return Result.Success();
    }
}

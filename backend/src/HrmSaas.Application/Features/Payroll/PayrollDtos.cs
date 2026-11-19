namespace HrmSaas.Application.Features.Payroll;

public sealed record PayrollPeriodDto(
    Guid Id,
    string PeriodCode,
    DateOnly PeriodStart,
    DateOnly PeriodEnd,
    DateOnly AttendanceCutoffDate,
    DateOnly PayDate,
    string Status);

public sealed record CreatePayrollPeriodRequest(
    string PeriodCode,
    DateOnly PeriodStart,
    DateOnly PeriodEnd,
    DateOnly AttendanceCutoffDate,
    DateOnly PayDate);

public sealed record PayslipItemDto(
    Guid Id,
    Guid SalaryComponentId,
    string ComponentName,
    decimal Amount,
    decimal? Units);

public sealed record PayslipDto(
    Guid Id,
    Guid PayrollPeriodId,
    string PeriodCode,
    Guid EmployeeId,
    string EmployeeCode,
    string EmployeeName,
    decimal GrossIncome,
    decimal TotalDeductions,
    decimal SsoEmployeeAmount,
    decimal WithholdingTaxAmount,
    decimal NetPay,
    bool IsLocked,
    IReadOnlyList<PayslipItemDto> Items);

public sealed record PayrollCalculationSummaryDto(
    Guid PayrollPeriodId,
    string PeriodCode,
    string Status,
    int ProcessedEmployees,
    decimal TotalGrossIncome,
    decimal TotalNetPay);

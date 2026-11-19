namespace HrmSaas.Domain.Common;

public sealed record Error(string Code, string Message)
{
    public static readonly Error None = new(string.Empty, string.Empty);
}

public class Result
{
    protected Result(bool isSuccess, Error error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error Error { get; }

    public static Result Success() => new(true, Error.None);
    public static Result Failure(Error error) => new(false, error);
    public static Result<TValue> Success<TValue>(TValue value) => new(value, true, Error.None);
    public static Result<TValue> Failure<TValue>(Error error) => new(default, false, error);
}

public sealed class Result<TValue> : Result
{
    private readonly TValue? _value;

    internal Result(TValue? value, bool isSuccess, Error error)
        : base(isSuccess, error)
    {
        _value = value;
    }

    public TValue Value
    {
        get
        {
            if (IsFailure || _value is null)
            {
                throw new InvalidOperationException("Cannot access Value on a failed Result.");
            }

            return _value;
        }
    }
}

public static class DomainErrors
{
    public static class Workflow
    {
        public static readonly Error InvalidTransition = new(
            "WORKFLOW_INVALID_TRANSITION",
            "Cannot transition approval request from its current status.");

        public static readonly Error InvalidStepOrder = new(
            "WORKFLOW_INVALID_STEP_ORDER",
            "Step order must be greater than zero.");
    }

    public static class Leave
    {
        public static readonly Error InsufficientBalance = new(
            "LEAVE_INSUFFICIENT_BALANCE",
            "Remaining leave days are insufficient for this request.");

        public static readonly Error InvalidQuotaAmount = new(
            "LEAVE_INVALID_QUOTA",
            "Leave quota amount cannot be negative.");
    }

    public static class Payroll
    {
        public static readonly Error PeriodAlreadyLocked = new(
            "PAYROLL_PERIOD_LOCKED",
            "Payroll period is already locked and cannot be modified.");

        public static readonly Error PeriodNotCalculated = new(
            "PAYROLL_PERIOD_NOT_CALCULATED",
            "Payroll period must be calculated or approved before locking.");
    }
}

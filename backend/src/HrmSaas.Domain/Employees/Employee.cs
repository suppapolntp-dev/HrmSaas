using HrmSaas.Domain.Common;

namespace HrmSaas.Domain.Employees;

public static class EmploymentStatus
{
    public const string Probation = "PROBATION";
    public const string Active = "ACTIVE";
    public const string Resigned = "RESIGNED";
    public const string Terminated = "TERMINATED";
}

public sealed class Employee : TenantCompanyEntity
{
    private Employee()
    {
    }

    public Guid BranchId { get; private set; }
    public Guid DepartmentId { get; private set; }
    public Guid PositionId { get; private set; }
    public Guid JobGradeId { get; private set; }
    public Guid? UserId { get; private set; }
    public string EmployeeCode { get; private set; } = string.Empty;
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public DateOnly HireDate { get; private set; }
    public DateOnly? ProbationEndDate { get; private set; }
    public string EmploymentStatus { get; private set; } = Employees.EmploymentStatus.Probation;

    public static Employee Create(
        Guid tenantId,
        Guid companyId,
        Guid branchId,
        Guid departmentId,
        Guid positionId,
        Guid jobGradeId,
        Guid? userId,
        string employeeCode,
        string firstName,
        string lastName,
        DateOnly hireDate,
        DateOnly? probationEndDate,
        string employmentStatus = Employees.EmploymentStatus.Probation,
        Guid? id = null)
    {
        if (tenantId == Guid.Empty || companyId == Guid.Empty)
        {
            throw new ArgumentException("TenantId and CompanyId are required.");
        }

        if (string.IsNullOrWhiteSpace(employeeCode) || string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
        {
            throw new ArgumentException("EmployeeCode, FirstName, and LastName are required.");
        }

        var employee = new Employee
        {
            TenantId = tenantId,
            CompanyId = companyId,
            BranchId = branchId,
            DepartmentId = departmentId,
            PositionId = positionId,
            JobGradeId = jobGradeId,
            UserId = userId,
            EmployeeCode = employeeCode.Trim().ToUpperInvariant(),
            FirstName = firstName.Trim(),
            LastName = lastName.Trim(),
            HireDate = hireDate,
            ProbationEndDate = probationEndDate,
            EmploymentStatus = employmentStatus.Trim().ToUpperInvariant()
        };

        if (id.HasValue)
        {
            employee.Id = id.Value;
        }

        return employee;
    }

    public void UpdateProfile(
        Guid branchId,
        Guid departmentId,
        Guid positionId,
        Guid jobGradeId,
        string firstName,
        string lastName,
        string employmentStatus)
    {
        BranchId = branchId;
        DepartmentId = departmentId;
        PositionId = positionId;
        JobGradeId = jobGradeId;
        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        EmploymentStatus = employmentStatus.Trim().ToUpperInvariant();
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}

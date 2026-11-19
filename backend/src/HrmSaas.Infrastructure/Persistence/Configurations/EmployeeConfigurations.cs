using HrmSaas.Domain.Employees;
using HrmSaas.Domain.Identity;
using HrmSaas.Domain.Organizations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HrmSaas.Infrastructure.Persistence.Configurations;

internal sealed class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.ToTable("employees", table =>
        {
            table.HasCheckConstraint(
                "ck_employees_status",
                "employment_status IN ('PROBATION', 'ACTIVE', 'RESIGNED', 'TERMINATED')");
        });

        builder.HasKey(x => x.Id).HasName("pk_employees");
        builder.Property(x => x.Id).HasColumnName("id").HasColumnType("uuid").ValueGeneratedNever();
        builder.ConfigureTenantCompanyColumns();

        builder.Property(x => x.BranchId).HasColumnName("branch_id").HasColumnType("uuid").IsRequired();
        builder.Property(x => x.DepartmentId).HasColumnName("department_id").HasColumnType("uuid").IsRequired();
        builder.Property(x => x.PositionId).HasColumnName("position_id").HasColumnType("uuid").IsRequired();
        builder.Property(x => x.JobGradeId).HasColumnName("job_grade_id").HasColumnType("uuid").IsRequired();
        builder.Property(x => x.UserId).HasColumnName("user_id").HasColumnType("uuid");
        builder.Property(x => x.EmployeeCode).HasColumnName("employee_code").HasMaxLength(50).IsRequired();
        builder.Property(x => x.FirstName).HasColumnName("first_name").HasMaxLength(100).IsRequired();
        builder.Property(x => x.LastName).HasColumnName("last_name").HasMaxLength(100).IsRequired();
        builder.Property(x => x.HireDate).HasColumnName("hire_date").HasColumnType("date").IsRequired();
        builder.Property(x => x.ProbationEndDate).HasColumnName("probation_end_date").HasColumnType("date");
        builder.Property(x => x.EmploymentStatus).HasColumnName("employment_status").HasMaxLength(30).IsRequired();

        builder.HasOne<Company>()
            .WithMany()
            .HasForeignKey(x => x.CompanyId)
            .HasConstraintName("fk_employees_companies")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Branch>()
            .WithMany()
            .HasForeignKey(x => x.BranchId)
            .HasConstraintName("fk_employees_branches")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Department>()
            .WithMany()
            .HasForeignKey(x => x.DepartmentId)
            .HasConstraintName("fk_employees_departments")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Position>()
            .WithMany()
            .HasForeignKey(x => x.PositionId)
            .HasConstraintName("fk_employees_positions")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<JobGrade>()
            .WithMany()
            .HasForeignKey(x => x.JobGradeId)
            .HasConstraintName("fk_employees_job_grades")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .HasConstraintName("fk_employees_users")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.CompanyId, x.EmployeeCode })
            .IsUnique()
            .HasDatabaseName("uq_employees_company_code");

        builder.HasIndex(x => new { x.TenantId, x.CompanyId, x.DepartmentId })
            .HasDatabaseName("ix_employees_tenant_company_dept");
    }
}

internal sealed class EmploymentContractConfiguration : IEntityTypeConfiguration<EmploymentContract>
{
    public void Configure(EntityTypeBuilder<EmploymentContract> builder)
    {
        builder.ToTable("employment_contracts", table =>
        {
            table.HasCheckConstraint(
                "ck_employment_contracts_type",
                "contract_type IN ('FULL_TIME', 'PART_TIME', 'CONTRACT', 'INTERN')");
            table.HasCheckConstraint(
                "ck_employment_contracts_cycle",
                "payment_cycle IN ('MONTHLY', 'BI_WEEKLY')");
        });

        builder.HasKey(x => x.Id).HasName("pk_employment_contracts");
        builder.Property(x => x.Id).HasColumnName("id").HasColumnType("uuid").ValueGeneratedNever();
        builder.ConfigureTenantCompanyColumns();

        builder.Property(x => x.EmployeeId).HasColumnName("employee_id").HasColumnType("uuid").IsRequired();
        builder.Property(x => x.ContractType).HasColumnName("contract_type").HasMaxLength(40).IsRequired();
        builder.Property(x => x.EffectiveFrom).HasColumnName("effective_from").HasColumnType("date").IsRequired();
        builder.Property(x => x.EffectiveTo).HasColumnName("effective_to").HasColumnType("date");
        builder.Property(x => x.BaseSalary).HasColumnName("base_salary").HasColumnType("numeric(18,4)").IsRequired();
        builder.Property(x => x.PaymentCycle).HasColumnName("payment_cycle").HasMaxLength(20).IsRequired();
        builder.Property(x => x.IsActive).HasColumnName("is_active").IsRequired();

        builder.HasOne<Employee>()
            .WithMany()
            .HasForeignKey(x => x.EmployeeId)
            .HasConstraintName("fk_employment_contracts_employees")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.TenantId, x.CompanyId, x.EmployeeId })
            .HasDatabaseName("ix_employment_contracts_tenant_company_emp");
    }
}

internal sealed class EmployeeReportingLineConfiguration : IEntityTypeConfiguration<EmployeeReportingLine>
{
    public void Configure(EntityTypeBuilder<EmployeeReportingLine> builder)
    {
        builder.ToTable("employee_reporting_lines", table =>
        {
            table.HasCheckConstraint(
                "ck_employee_reporting_lines_type",
                "line_type IN ('DIRECT', 'SECONDARY')");
        });

        builder.HasKey(x => x.Id).HasName("pk_employee_reporting_lines");
        builder.Property(x => x.Id).HasColumnName("id").HasColumnType("uuid").ValueGeneratedNever();
        builder.ConfigureTenantCompanyColumns();

        builder.Property(x => x.EmployeeId).HasColumnName("employee_id").HasColumnType("uuid").IsRequired();
        builder.Property(x => x.ManagerEmployeeId).HasColumnName("manager_employee_id").HasColumnType("uuid").IsRequired();
        builder.Property(x => x.LineType).HasColumnName("line_type").HasMaxLength(30).IsRequired();
        builder.Property(x => x.PriorityOrder).HasColumnName("priority_order").IsRequired();
        builder.Property(x => x.EffectiveFrom).HasColumnName("effective_from").HasColumnType("date").IsRequired();
        builder.Property(x => x.EffectiveTo).HasColumnName("effective_to").HasColumnType("date");

        builder.HasOne<Employee>()
            .WithMany()
            .HasForeignKey(x => x.EmployeeId)
            .HasConstraintName("fk_reporting_lines_employee")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Employee>()
            .WithMany()
            .HasForeignKey(x => x.ManagerEmployeeId)
            .HasConstraintName("fk_reporting_lines_manager")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.TenantId, x.CompanyId, x.EmployeeId })
            .HasDatabaseName("ix_employee_reporting_lines_tenant_company_emp");
    }
}

using HrmSaas.Domain.Employees;
using HrmSaas.Domain.Identity;
using HrmSaas.Domain.Organizations;
using HrmSaas.Domain.Workflows;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HrmSaas.Infrastructure.Persistence.Configurations;

internal sealed class LeaveTypeConfiguration : IEntityTypeConfiguration<LeaveType>
{
    public void Configure(EntityTypeBuilder<LeaveType> builder)
    {
        builder.ToTable("leave_types");
        builder.HasKey(x => x.Id).HasName("pk_leave_types");
        builder.Property(x => x.Id).HasColumnName("id").HasColumnType("uuid").ValueGeneratedNever();
        builder.ConfigureTenantCompanyColumns();

        builder.Property(x => x.Code).HasColumnName("code").HasMaxLength(50).IsRequired();
        builder.Property(x => x.Name).HasColumnName("name").HasMaxLength(150).IsRequired();
        builder.Property(x => x.IsPaid).HasColumnName("is_paid").IsRequired();
        builder.Property(x => x.AllowHalfDay).HasColumnName("allow_half_day").IsRequired();
        builder.Property(x => x.AllowHourly).HasColumnName("allow_hourly").IsRequired();
        builder.Property(x => x.RequiresAttachment).HasColumnName("requires_attachment").IsRequired();

        builder.HasOne<Company>()
            .WithMany()
            .HasForeignKey(x => x.CompanyId)
            .HasConstraintName("fk_leave_types_companies")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.CompanyId, x.Code })
            .IsUnique()
            .HasDatabaseName("uq_leave_types_company_code");
    }
}

internal sealed class LeaveEntitlementPolicyConfiguration : IEntityTypeConfiguration<LeaveEntitlementPolicy>
{
    public void Configure(EntityTypeBuilder<LeaveEntitlementPolicy> builder)
    {
        builder.ToTable("leave_entitlement_policies", table =>
        {
            table.HasCheckConstraint(
                "ck_leave_policies_prorate",
                "prorate_rule IN ('NONE', 'MONTHLY_PRORATE', 'DAILY_PRORATE')");
        });

        builder.HasKey(x => x.Id).HasName("pk_leave_entitlement_policies");
        builder.Property(x => x.Id).HasColumnName("id").HasColumnType("uuid").ValueGeneratedNever();
        builder.ConfigureTenantCompanyColumns();

        builder.Property(x => x.LeaveTypeId).HasColumnName("leave_type_id").HasColumnType("uuid").IsRequired();
        builder.Property(x => x.JobGradeId).HasColumnName("job_grade_id").HasColumnType("uuid");
        builder.Property(x => x.MinTenureMonths).HasColumnName("min_tenure_months").IsRequired();
        builder.Property(x => x.MaxTenureMonths).HasColumnName("max_tenure_months");
        builder.Property(x => x.AnnualQuotaDays).HasColumnName("annual_quota_days").HasColumnType("numeric(10,2)").IsRequired();
        builder.Property(x => x.ProrateRule).HasColumnName("prorate_rule").HasMaxLength(30).IsRequired();
        builder.Property(x => x.MaxCarryOverDays).HasColumnName("max_carry_over_days").HasColumnType("numeric(10,2)").IsRequired();

        builder.HasOne<LeaveType>()
            .WithMany()
            .HasForeignKey(x => x.LeaveTypeId)
            .HasConstraintName("fk_leave_policies_leave_types")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<JobGrade>()
            .WithMany()
            .HasForeignKey(x => x.JobGradeId)
            .HasConstraintName("fk_leave_policies_job_grades")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.TenantId, x.CompanyId, x.LeaveTypeId })
            .HasDatabaseName("ix_leave_policies_tenant_company_type");
    }
}

internal sealed class EmployeeLeaveBalanceConfiguration : IEntityTypeConfiguration<EmployeeLeaveBalance>
{
    public void Configure(EntityTypeBuilder<EmployeeLeaveBalance> builder)
    {
        builder.ToTable("employee_leave_balances");
        builder.HasKey(x => x.Id).HasName("pk_employee_leave_balances");
        builder.Property(x => x.Id).HasColumnName("id").HasColumnType("uuid").ValueGeneratedNever();
        builder.ConfigureTenantCompanyColumns();

        builder.Property(x => x.EmployeeId).HasColumnName("employee_id").HasColumnType("uuid").IsRequired();
        builder.Property(x => x.LeaveTypeId).HasColumnName("leave_type_id").HasColumnType("uuid").IsRequired();
        builder.Property(x => x.QuotaYear).HasColumnName("quota_year").IsRequired();
        builder.Property(x => x.CarriedOverDays).HasColumnName("carried_over_days").HasColumnType("numeric(10,2)").IsRequired();
        builder.Property(x => x.EntitledDays).HasColumnName("entitled_days").HasColumnType("numeric(10,2)").IsRequired();
        builder.Property(x => x.UsedDays).HasColumnName("used_days").HasColumnType("numeric(10,2)").IsRequired();
        builder.Property(x => x.PendingDays).HasColumnName("pending_days").HasColumnType("numeric(10,2)").IsRequired();
        builder.Property(x => x.RowVersion).IsRowVersion();

        builder.Ignore(x => x.RemainingDays);

        builder.HasOne<Employee>()
            .WithMany()
            .HasForeignKey(x => x.EmployeeId)
            .HasConstraintName("fk_leave_balances_employees")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<LeaveType>()
            .WithMany()
            .HasForeignKey(x => x.LeaveTypeId)
            .HasConstraintName("fk_leave_balances_leave_types")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.EmployeeId, x.LeaveTypeId, x.QuotaYear })
            .IsUnique()
            .HasDatabaseName("uq_employee_leave_balances_emp_type_year");
    }
}

internal sealed class WorkflowDefinitionConfiguration : IEntityTypeConfiguration<WorkflowDefinition>
{
    public void Configure(EntityTypeBuilder<WorkflowDefinition> builder)
    {
        builder.ToTable("workflow_definitions", table =>
        {
            table.HasCheckConstraint(
                "ck_workflow_definitions_request_type",
                "request_type IN ('LEAVE', 'OVERTIME', 'TIME_ADJUSTMENT')");
        });

        builder.HasKey(x => x.Id).HasName("pk_workflow_definitions");
        builder.Property(x => x.Id).HasColumnName("id").HasColumnType("uuid").ValueGeneratedNever();
        builder.ConfigureTenantCompanyColumns();

        builder.Property(x => x.RequestType).HasColumnName("request_type").HasMaxLength(40).IsRequired();
        builder.Property(x => x.DepartmentId).HasColumnName("department_id").HasColumnType("uuid");
        builder.Property(x => x.Name).HasColumnName("name").HasMaxLength(150).IsRequired();
        builder.Property(x => x.IsActive).HasColumnName("is_active").IsRequired();

        builder.HasOne<Company>()
            .WithMany()
            .HasForeignKey(x => x.CompanyId)
            .HasConstraintName("fk_workflow_defs_companies")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Department>()
            .WithMany()
            .HasForeignKey(x => x.DepartmentId)
            .HasConstraintName("fk_workflow_defs_departments")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.TenantId, x.CompanyId, x.RequestType })
            .HasDatabaseName("ix_workflow_definitions_tenant_company_type");
    }
}

internal sealed class WorkflowStepConfiguration : IEntityTypeConfiguration<WorkflowStep>
{
    public void Configure(EntityTypeBuilder<WorkflowStep> builder)
    {
        builder.ToTable("workflow_steps", table =>
        {
            table.HasCheckConstraint(
                "ck_workflow_steps_strategy",
                "approver_strategy IN ('DIRECT_MANAGER', 'SECONDARY_MANAGER', 'HR_ROLE', 'SPECIFIC_ROLE', 'SPECIFIC_EMPLOYEE')");
        });

        builder.HasKey(x => x.Id).HasName("pk_workflow_steps");
        builder.Property(x => x.Id).HasColumnName("id").HasColumnType("uuid").ValueGeneratedNever();

        builder.Property(x => x.TenantId).HasColumnName("tenant_id").HasColumnType("uuid").IsRequired();
        builder.Property(x => x.WorkflowDefinitionId).HasColumnName("workflow_definition_id").HasColumnType("uuid").IsRequired();
        builder.Property(x => x.StepOrder).HasColumnName("step_order").IsRequired();
        builder.Property(x => x.ApproverStrategy).HasColumnName("approver_strategy").HasMaxLength(40).IsRequired();
        builder.Property(x => x.SpecificRoleId).HasColumnName("specific_role_id").HasColumnType("uuid");
        builder.Property(x => x.SpecificEmployeeId).HasColumnName("specific_employee_id").HasColumnType("uuid");

        builder.HasOne<WorkflowDefinition>()
            .WithMany()
            .HasForeignKey(x => x.WorkflowDefinitionId)
            .HasConstraintName("fk_workflow_steps_definitions")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Role>()
            .WithMany()
            .HasForeignKey(x => x.SpecificRoleId)
            .HasConstraintName("fk_workflow_steps_roles")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Employee>()
            .WithMany()
            .HasForeignKey(x => x.SpecificEmployeeId)
            .HasConstraintName("fk_workflow_steps_employees")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.WorkflowDefinitionId, x.StepOrder })
            .IsUnique()
            .HasDatabaseName("uq_workflow_steps_def_order");
    }
}

internal sealed class ApprovalRequestConfiguration : IEntityTypeConfiguration<ApprovalRequest>
{
    public void Configure(EntityTypeBuilder<ApprovalRequest> builder)
    {
        builder.ToTable("approval_requests", table =>
        {
            table.HasCheckConstraint(
                "ck_approval_requests_status",
                "status IN ('PENDING', 'APPROVED', 'REJECTED', 'CANCELLED')");
        });

        builder.HasKey(x => x.Id).HasName("pk_approval_requests");
        builder.Property(x => x.Id).HasColumnName("id").HasColumnType("uuid").ValueGeneratedNever();
        builder.ConfigureTenantCompanyColumns();

        builder.Property(x => x.RequesterEmployeeId).HasColumnName("requester_employee_id").HasColumnType("uuid").IsRequired();
        builder.Property(x => x.RequestType).HasColumnName("request_type").HasMaxLength(40).IsRequired();
        builder.Property(x => x.ReferenceId).HasColumnName("reference_id").HasColumnType("uuid").IsRequired();
        builder.Property(x => x.WorkflowDefinitionId).HasColumnName("workflow_definition_id").HasColumnType("uuid").IsRequired();
        builder.Property(x => x.CurrentStepOrder).HasColumnName("current_step_order").IsRequired();
        builder.Property(x => x.Status).HasColumnName("status").HasMaxLength(30).IsRequired();
        builder.Property(x => x.PayloadSummary).HasColumnName("payload_summary").HasColumnType("jsonb").IsRequired();
        builder.Property(x => x.SubmittedAt).HasColumnName("submitted_at").HasColumnType("timestamptz").IsRequired();
        builder.Property(x => x.CompletedAt).HasColumnName("completed_at").HasColumnType("timestamptz");
        builder.Property(x => x.RowVersion).IsRowVersion();

        builder.HasOne<Employee>()
            .WithMany()
            .HasForeignKey(x => x.RequesterEmployeeId)
            .HasConstraintName("fk_approval_requests_requester")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<WorkflowDefinition>()
            .WithMany()
            .HasForeignKey(x => x.WorkflowDefinitionId)
            .HasConstraintName("fk_approval_requests_workflow")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.TenantId, x.CompanyId, x.Status, x.SubmittedAt })
            .HasDatabaseName("ix_approval_requests_tenant_company_status");
    }
}

internal sealed class ApprovalActionLogConfiguration : IEntityTypeConfiguration<ApprovalActionLog>
{
    public void Configure(EntityTypeBuilder<ApprovalActionLog> builder)
    {
        builder.ToTable("approval_action_logs", table =>
        {
            table.HasCheckConstraint(
                "ck_approval_action_logs_action",
                "action IN ('APPROVE', 'REJECT', 'CANCEL')");
        });

        builder.HasKey(x => x.Id).HasName("pk_approval_action_logs");
        builder.Property(x => x.Id).HasColumnName("id").HasColumnType("uuid").ValueGeneratedNever();

        builder.Property(x => x.TenantId).HasColumnName("tenant_id").HasColumnType("uuid").IsRequired();
        builder.Property(x => x.ApprovalRequestId).HasColumnName("approval_request_id").HasColumnType("uuid").IsRequired();
        builder.Property(x => x.StepOrder).HasColumnName("step_order").IsRequired();
        builder.Property(x => x.ActorEmployeeId).HasColumnName("actor_employee_id").HasColumnType("uuid").IsRequired();
        builder.Property(x => x.Action).HasColumnName("action").HasMaxLength(30).IsRequired();
        builder.Property(x => x.Comment).HasColumnName("comment").HasColumnType("text");
        builder.Property(x => x.ActedAt).HasColumnName("acted_at").HasColumnType("timestamptz").IsRequired();

        builder.HasOne<ApprovalRequest>()
            .WithMany()
            .HasForeignKey(x => x.ApprovalRequestId)
            .HasConstraintName("fk_approval_action_logs_request")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Employee>()
            .WithMany()
            .HasForeignKey(x => x.ActorEmployeeId)
            .HasConstraintName("fk_approval_action_logs_actor")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.TenantId, x.ApprovalRequestId })
            .HasDatabaseName("ix_approval_action_logs_tenant_request");
    }
}

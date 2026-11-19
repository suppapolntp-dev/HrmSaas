using HrmSaas.Domain.Attendance;
using HrmSaas.Domain.Employees;
using HrmSaas.Domain.Organizations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HrmSaas.Infrastructure.Persistence.Configurations;

internal sealed class ShiftDefinitionConfiguration : IEntityTypeConfiguration<ShiftDefinition>
{
    public void Configure(EntityTypeBuilder<ShiftDefinition> builder)
    {
        builder.ToTable("shift_definitions", table =>
        {
            table.HasCheckConstraint(
                "ck_shift_definitions_type",
                "shift_type IN ('NORMAL', 'OVERNIGHT', 'FLEXIBLE')");
        });

        builder.HasKey(x => x.Id).HasName("pk_shift_definitions");
        builder.Property(x => x.Id).HasColumnName("id").HasColumnType("uuid").ValueGeneratedNever();
        builder.ConfigureTenantCompanyColumns();

        builder.Property(x => x.Code).HasColumnName("code").HasMaxLength(50).IsRequired();
        builder.Property(x => x.Name).HasColumnName("name").HasMaxLength(150).IsRequired();
        builder.Property(x => x.ShiftType).HasColumnName("shift_type").HasMaxLength(30).IsRequired();
        builder.Property(x => x.StartTime).HasColumnName("start_time").HasColumnType("time").IsRequired();
        builder.Property(x => x.EndTime).HasColumnName("end_time").HasColumnType("time").IsRequired();
        builder.Property(x => x.IsOvernight).HasColumnName("is_overnight").IsRequired();
        builder.Property(x => x.GracePeriodMinutes).HasColumnName("grace_period_minutes").IsRequired();
        builder.Property(x => x.LateAfterMinutes).HasColumnName("late_after_minutes").IsRequired();
        builder.Property(x => x.StandardWorkMinutes).HasColumnName("standard_work_minutes").IsRequired();

        builder.HasOne<Company>()
            .WithMany()
            .HasForeignKey(x => x.CompanyId)
            .HasConstraintName("fk_shift_definitions_companies")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.CompanyId, x.Code })
            .IsUnique()
            .HasDatabaseName("uq_shift_definitions_company_code");
    }
}

internal sealed class ShiftBreakRuleConfiguration : IEntityTypeConfiguration<ShiftBreakRule>
{
    public void Configure(EntityTypeBuilder<ShiftBreakRule> builder)
    {
        builder.ToTable("shift_break_rules");
        builder.HasKey(x => x.Id).HasName("pk_shift_break_rules");
        builder.Property(x => x.Id).HasColumnName("id").HasColumnType("uuid").ValueGeneratedNever();

        builder.Property(x => x.TenantId).HasColumnName("tenant_id").HasColumnType("uuid").IsRequired();
        builder.Property(x => x.ShiftDefinitionId).HasColumnName("shift_definition_id").HasColumnType("uuid").IsRequired();
        builder.Property(x => x.BreakStart).HasColumnName("break_start").HasColumnType("time").IsRequired();
        builder.Property(x => x.BreakEnd).HasColumnName("break_end").HasColumnType("time").IsRequired();
        builder.Property(x => x.DurationMinutes).HasColumnName("duration_minutes").IsRequired();
        builder.Property(x => x.IsPaid).HasColumnName("is_paid").IsRequired();

        builder.HasOne<ShiftDefinition>()
            .WithMany()
            .HasForeignKey(x => x.ShiftDefinitionId)
            .HasConstraintName("fk_shift_break_rules_shifts")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.TenantId, x.ShiftDefinitionId })
            .HasDatabaseName("ix_shift_break_rules_tenant_shift");
    }
}

internal sealed class EmployeeShiftRosterConfiguration : IEntityTypeConfiguration<EmployeeShiftRoster>
{
    public void Configure(EntityTypeBuilder<EmployeeShiftRoster> builder)
    {
        builder.ToTable("employee_shift_rosters", table =>
        {
            table.HasCheckConstraint(
                "ck_employee_shift_rosters_day_type",
                "day_type IN ('WORKDAY', 'DAY_OFF', 'PUBLIC_HOLIDAY')");
        });

        builder.HasKey(x => x.Id).HasName("pk_employee_shift_rosters");
        builder.Property(x => x.Id).HasColumnName("id").HasColumnType("uuid").ValueGeneratedNever();
        builder.ConfigureTenantCompanyColumns();

        builder.Property(x => x.EmployeeId).HasColumnName("employee_id").HasColumnType("uuid").IsRequired();
        builder.Property(x => x.ShiftDefinitionId).HasColumnName("shift_definition_id").HasColumnType("uuid");
        builder.Property(x => x.WorkDate).HasColumnName("work_date").HasColumnType("date").IsRequired();
        builder.Property(x => x.DayType).HasColumnName("day_type").HasMaxLength(30).IsRequired();

        builder.HasOne<Employee>()
            .WithMany()
            .HasForeignKey(x => x.EmployeeId)
            .HasConstraintName("fk_shift_rosters_employees")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ShiftDefinition>()
            .WithMany()
            .HasForeignKey(x => x.ShiftDefinitionId)
            .HasConstraintName("fk_shift_rosters_shifts")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.EmployeeId, x.WorkDate })
            .IsUnique()
            .HasDatabaseName("uq_employee_shift_rosters_emp_date");

        builder.HasIndex(x => new { x.TenantId, x.CompanyId, x.WorkDate })
            .HasDatabaseName("ix_employee_shift_rosters_tenant_company_date");
    }
}

internal sealed class AttendanceLogConfiguration : IEntityTypeConfiguration<AttendanceLog>
{
    public void Configure(EntityTypeBuilder<AttendanceLog> builder)
    {
        builder.ToTable("attendance_logs", table =>
        {
            table.HasCheckConstraint(
                "ck_attendance_logs_punch_type",
                "punch_type IN ('CHECK_IN', 'CHECK_OUT')");
            table.HasCheckConstraint(
                "ck_attendance_logs_source",
                "source IN ('WEB', 'MOBILE', 'BIOMETRIC', 'ADJUSTMENT')");
        });

        builder.HasKey(x => x.Id).HasName("pk_attendance_logs");
        builder.Property(x => x.Id).HasColumnName("id").HasColumnType("uuid").ValueGeneratedNever();

        builder.Property(x => x.TenantId).HasColumnName("tenant_id").HasColumnType("uuid").IsRequired();
        builder.Property(x => x.CompanyId).HasColumnName("company_id").HasColumnType("uuid").IsRequired();
        builder.Property(x => x.EmployeeId).HasColumnName("employee_id").HasColumnType("uuid").IsRequired();
        builder.Property(x => x.BranchId).HasColumnName("branch_id").HasColumnType("uuid").IsRequired();
        builder.Property(x => x.PunchTimeUtc).HasColumnName("punch_time_utc").HasColumnType("timestamptz").IsRequired();
        builder.Property(x => x.PunchType).HasColumnName("punch_type").HasMaxLength(20).IsRequired();
        builder.Property(x => x.Source).HasColumnName("source").HasMaxLength(30).IsRequired();
        builder.Property(x => x.Latitude).HasColumnName("latitude").HasColumnType("numeric(10,7)");
        builder.Property(x => x.Longitude).HasColumnName("longitude").HasColumnType("numeric(10,7)");
        builder.Property(x => x.IpAddress).HasColumnName("ip_address").HasColumnType("inet").IsRequired();
        builder.Property(x => x.IsWithinGeofence).HasColumnName("is_within_geofence").IsRequired();

        builder.HasOne<Employee>()
            .WithMany()
            .HasForeignKey(x => x.EmployeeId)
            .HasConstraintName("fk_attendance_logs_employees")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Branch>()
            .WithMany()
            .HasForeignKey(x => x.BranchId)
            .HasConstraintName("fk_attendance_logs_branches")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.TenantId, x.CompanyId, x.EmployeeId, x.PunchTimeUtc })
            .HasDatabaseName("ix_attendance_logs_tenant_company_emp_time");
    }
}

internal sealed class AttendanceDailySummaryConfiguration : IEntityTypeConfiguration<AttendanceDailySummary>
{
    public void Configure(EntityTypeBuilder<AttendanceDailySummary> builder)
    {
        builder.ToTable("attendance_daily_summaries", table =>
        {
            table.HasCheckConstraint(
                "ck_attendance_daily_status",
                "attendance_status IN ('PRESENT', 'LATE', 'EARLY_OUT', 'MISSING_PUNCH', 'ABSENT', 'ON_LEAVE', 'DAY_OFF')");
        });

        builder.HasKey(x => x.Id).HasName("pk_attendance_daily_summaries");
        builder.Property(x => x.Id).HasColumnName("id").HasColumnType("uuid").ValueGeneratedNever();
        builder.ConfigureTenantCompanyColumns();

        builder.Property(x => x.EmployeeId).HasColumnName("employee_id").HasColumnType("uuid").IsRequired();
        builder.Property(x => x.WorkDate).HasColumnName("work_date").HasColumnType("date").IsRequired();
        builder.Property(x => x.ShiftDefinitionId).HasColumnName("shift_definition_id").HasColumnType("uuid");
        builder.Property(x => x.ActualInUtc).HasColumnName("actual_in_utc").HasColumnType("timestamptz");
        builder.Property(x => x.ActualOutUtc).HasColumnName("actual_out_utc").HasColumnType("timestamptz");
        builder.Property(x => x.WorkedMinutes).HasColumnName("worked_minutes").IsRequired();
        builder.Property(x => x.LateMinutes).HasColumnName("late_minutes").IsRequired();
        builder.Property(x => x.EarlyOutMinutes).HasColumnName("early_out_minutes").IsRequired();
        builder.Property(x => x.OtMinutes).HasColumnName("ot_minutes").IsRequired();
        builder.Property(x => x.AttendanceStatus).HasColumnName("attendance_status").HasMaxLength(30).IsRequired();
        builder.Property(x => x.IsLockedByPayroll).HasColumnName("is_locked_by_payroll").IsRequired();

        builder.HasOne<Employee>()
            .WithMany()
            .HasForeignKey(x => x.EmployeeId)
            .HasConstraintName("fk_attendance_daily_employees")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ShiftDefinition>()
            .WithMany()
            .HasForeignKey(x => x.ShiftDefinitionId)
            .HasConstraintName("fk_attendance_daily_shifts")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.EmployeeId, x.WorkDate })
            .IsUnique()
            .HasDatabaseName("uq_attendance_daily_summaries_emp_date");

        builder.HasIndex(x => new { x.TenantId, x.CompanyId, x.WorkDate })
            .HasDatabaseName("ix_attendance_daily_tenant_company_date");
    }
}

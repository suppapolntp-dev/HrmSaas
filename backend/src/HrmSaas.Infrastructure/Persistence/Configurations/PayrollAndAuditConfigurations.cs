using HrmSaas.Domain.Audit;
using HrmSaas.Domain.Employees;
using HrmSaas.Domain.Organizations;
using HrmSaas.Domain.Payroll;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HrmSaas.Infrastructure.Persistence.Configurations;

internal sealed class PayrollPeriodConfiguration : IEntityTypeConfiguration<PayrollPeriod>
{
    public void Configure(EntityTypeBuilder<PayrollPeriod> builder)
    {
        builder.ToTable("payroll_periods", table =>
        {
            table.HasCheckConstraint(
                "ck_payroll_periods_status",
                "status IN ('DRAFT', 'CALCULATED', 'APPROVED', 'LOCKED')");
        });

        builder.HasKey(x => x.Id).HasName("pk_payroll_periods");
        builder.Property(x => x.Id).HasColumnName("id").HasColumnType("uuid").ValueGeneratedNever();
        builder.ConfigureTenantCompanyColumns();

        builder.Property(x => x.PeriodCode).HasColumnName("period_code").HasMaxLength(30).IsRequired();
        builder.Property(x => x.PeriodStart).HasColumnName("period_start").HasColumnType("date").IsRequired();
        builder.Property(x => x.PeriodEnd).HasColumnName("period_end").HasColumnType("date").IsRequired();
        builder.Property(x => x.AttendanceCutoffDate).HasColumnName("attendance_cutoff_date").HasColumnType("date").IsRequired();
        builder.Property(x => x.PayDate).HasColumnName("pay_date").HasColumnType("date").IsRequired();
        builder.Property(x => x.Status).HasColumnName("status").HasMaxLength(30).IsRequired();
        builder.Property(x => x.RowVersion).IsRowVersion();

        builder.HasOne<Company>()
            .WithMany()
            .HasForeignKey(x => x.CompanyId)
            .HasConstraintName("fk_payroll_periods_companies")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.CompanyId, x.PeriodCode })
            .IsUnique()
            .HasDatabaseName("uq_payroll_periods_company_code");

        builder.HasIndex(x => new { x.TenantId, x.CompanyId, x.PeriodStart })
            .HasDatabaseName("ix_payroll_periods_tenant_company_start");
    }
}

internal sealed class SalaryComponentConfiguration : IEntityTypeConfiguration<SalaryComponent>
{
    public void Configure(EntityTypeBuilder<SalaryComponent> builder)
    {
        builder.ToTable("salary_components", table =>
        {
            table.HasCheckConstraint(
                "ck_salary_components_type",
                "component_type IN ('EARNING', 'DEDUCTION')");
        });

        builder.HasKey(x => x.Id).HasName("pk_salary_components");
        builder.Property(x => x.Id).HasColumnName("id").HasColumnType("uuid").ValueGeneratedNever();
        builder.ConfigureTenantCompanyColumns();

        builder.Property(x => x.Code).HasColumnName("code").HasMaxLength(50).IsRequired();
        builder.Property(x => x.Name).HasColumnName("name").HasMaxLength(150).IsRequired();
        builder.Property(x => x.ComponentType).HasColumnName("component_type").HasMaxLength(20).IsRequired();
        builder.Property(x => x.IsTaxable).HasColumnName("is_taxable").IsRequired();
        builder.Property(x => x.IsSsoBase).HasColumnName("is_sso_base").IsRequired();
        builder.Property(x => x.CalculationFormulaType).HasColumnName("calculation_formula_type").HasMaxLength(40).IsRequired();

        builder.HasOne<Company>()
            .WithMany()
            .HasForeignKey(x => x.CompanyId)
            .HasConstraintName("fk_salary_components_companies")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.CompanyId, x.Code })
            .IsUnique()
            .HasDatabaseName("uq_salary_components_company_code");
    }
}

internal sealed class TaxBracketConfiguration : IEntityTypeConfiguration<TaxBracket>
{
    public void Configure(EntityTypeBuilder<TaxBracket> builder)
    {
        builder.ToTable("tax_brackets");
        builder.HasKey(x => x.Id).HasName("pk_tax_brackets");
        builder.Property(x => x.Id).HasColumnName("id").HasColumnType("uuid").ValueGeneratedNever();

        builder.Property(x => x.TaxYear).HasColumnName("tax_year").IsRequired();
        builder.Property(x => x.CountryCode).HasColumnName("country_code").HasMaxLength(5).IsRequired();
        builder.Property(x => x.MinNetIncome).HasColumnName("min_net_income").HasColumnType("numeric(18,4)").IsRequired();
        builder.Property(x => x.MaxNetIncome).HasColumnName("max_net_income").HasColumnType("numeric(18,4)");
        builder.Property(x => x.TaxRatePercent).HasColumnName("tax_rate_percent").HasColumnType("numeric(7,4)").IsRequired();

        builder.HasIndex(x => new { x.CountryCode, x.TaxYear, x.MinNetIncome })
            .HasDatabaseName("ix_tax_brackets_country_year");
    }
}

internal sealed class SsoConfigurationEntityConfiguration : IEntityTypeConfiguration<SsoConfiguration>
{
    public void Configure(EntityTypeBuilder<SsoConfiguration> builder)
    {
        builder.ToTable("sso_configurations");
        builder.HasKey(x => x.Id).HasName("pk_sso_configurations");
        builder.Property(x => x.Id).HasColumnName("id").HasColumnType("uuid").ValueGeneratedNever();

        builder.Property(x => x.CountryCode).HasColumnName("country_code").HasMaxLength(5).IsRequired();
        builder.Property(x => x.EffectiveFrom).HasColumnName("effective_from").HasColumnType("date").IsRequired();
        builder.Property(x => x.EffectiveTo).HasColumnName("effective_to").HasColumnType("date");
        builder.Property(x => x.EmployeeRatePercent).HasColumnName("employee_rate_percent").HasColumnType("numeric(7,4)").IsRequired();
        builder.Property(x => x.EmployerRatePercent).HasColumnName("employer_rate_percent").HasColumnType("numeric(7,4)").IsRequired();
        builder.Property(x => x.MinWageBase).HasColumnName("min_wage_base").HasColumnType("numeric(18,4)").IsRequired();
        builder.Property(x => x.MaxWageBase).HasColumnName("max_wage_base").HasColumnType("numeric(18,4)").IsRequired();

        builder.HasIndex(x => new { x.CountryCode, x.EffectiveFrom })
            .HasDatabaseName("ix_sso_configurations_country_effective");
    }
}

internal sealed class PayslipConfiguration : IEntityTypeConfiguration<Payslip>
{
    public void Configure(EntityTypeBuilder<Payslip> builder)
    {
        builder.ToTable("payslips");
        builder.HasKey(x => x.Id).HasName("pk_payslips");
        builder.Property(x => x.Id).HasColumnName("id").HasColumnType("uuid").ValueGeneratedNever();
        builder.ConfigureTenantCompanyColumns();

        builder.Property(x => x.PayrollPeriodId).HasColumnName("payroll_period_id").HasColumnType("uuid").IsRequired();
        builder.Property(x => x.EmployeeId).HasColumnName("employee_id").HasColumnType("uuid").IsRequired();
        builder.Property(x => x.GrossIncome).HasColumnName("gross_income").HasColumnType("numeric(18,4)").IsRequired();
        builder.Property(x => x.TotalDeductions).HasColumnName("total_deductions").HasColumnType("numeric(18,4)").IsRequired();
        builder.Property(x => x.SsoEmployeeAmount).HasColumnName("sso_employee_amount").HasColumnType("numeric(18,4)").IsRequired();
        builder.Property(x => x.WithholdingTaxAmount).HasColumnName("withholding_tax_amount").HasColumnType("numeric(18,4)").IsRequired();
        builder.Property(x => x.NetPay).HasColumnName("net_pay").HasColumnType("numeric(18,4)").IsRequired();
        builder.Property(x => x.IsLocked).HasColumnName("is_locked").IsRequired();

        builder.HasOne<PayrollPeriod>()
            .WithMany()
            .HasForeignKey(x => x.PayrollPeriodId)
            .HasConstraintName("fk_payslips_payroll_periods")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Employee>()
            .WithMany()
            .HasForeignKey(x => x.EmployeeId)
            .HasConstraintName("fk_payslips_employees")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.PayrollPeriodId, x.EmployeeId })
            .IsUnique()
            .HasDatabaseName("uq_payslips_period_emp");

        builder.HasIndex(x => new { x.TenantId, x.CompanyId, x.PayrollPeriodId })
            .HasDatabaseName("ix_payslips_tenant_company_period");
    }
}

internal sealed class PayslipItemConfiguration : IEntityTypeConfiguration<PayslipItem>
{
    public void Configure(EntityTypeBuilder<PayslipItem> builder)
    {
        builder.ToTable("payslip_items");
        builder.HasKey(x => x.Id).HasName("pk_payslip_items");
        builder.Property(x => x.Id).HasColumnName("id").HasColumnType("uuid").ValueGeneratedNever();

        builder.Property(x => x.TenantId).HasColumnName("tenant_id").HasColumnType("uuid").IsRequired();
        builder.Property(x => x.PayslipId).HasColumnName("payslip_id").HasColumnType("uuid").IsRequired();
        builder.Property(x => x.SalaryComponentId).HasColumnName("salary_component_id").HasColumnType("uuid").IsRequired();
        builder.Property(x => x.ComponentNameSnapshot).HasColumnName("component_name_snapshot").HasMaxLength(150).IsRequired();
        builder.Property(x => x.Amount).HasColumnName("amount").HasColumnType("numeric(18,4)").IsRequired();
        builder.Property(x => x.Units).HasColumnName("units").HasColumnType("numeric(10,2)");

        builder.HasOne<Payslip>()
            .WithMany()
            .HasForeignKey(x => x.PayslipId)
            .HasConstraintName("fk_payslip_items_payslips")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<SalaryComponent>()
            .WithMany()
            .HasForeignKey(x => x.SalaryComponentId)
            .HasConstraintName("fk_payslip_items_components")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.TenantId, x.PayslipId })
            .HasDatabaseName("ix_payslip_items_tenant_payslip");
    }
}

internal sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("audit_logs");
        builder.HasKey(x => x.Id).HasName("pk_audit_logs");
        builder.Property(x => x.Id).HasColumnName("id").HasColumnType("uuid").ValueGeneratedNever();

        builder.Property(x => x.TenantId).HasColumnName("tenant_id").HasColumnType("uuid").IsRequired();
        builder.Property(x => x.CompanyId).HasColumnName("company_id").HasColumnType("uuid");
        builder.Property(x => x.ActorUserId).HasColumnName("actor_user_id").HasColumnType("uuid");
        builder.Property(x => x.EntityName).HasColumnName("entity_name").HasMaxLength(100).IsRequired();
        builder.Property(x => x.EntityId).HasColumnName("entity_id").HasColumnType("uuid").IsRequired();
        builder.Property(x => x.Action).HasColumnName("action").HasMaxLength(30).IsRequired();
        builder.Property(x => x.OldValues).HasColumnName("old_values").HasColumnType("jsonb");
        builder.Property(x => x.NewValues).HasColumnName("new_values").HasColumnType("jsonb");
        builder.Property(x => x.IpAddress).HasColumnName("ip_address").HasColumnType("inet");
        builder.Property(x => x.CorrelationId).HasColumnName("correlation_id").HasMaxLength(64).IsRequired();
        builder.Property(x => x.OccurredAt).HasColumnName("occurred_at").HasColumnType("timestamptz").IsRequired();

        builder.HasIndex(x => new { x.TenantId, x.EntityName, x.EntityId, x.OccurredAt })
            .HasDatabaseName("ix_audit_logs_tenant_entity_occurred");
    }
}

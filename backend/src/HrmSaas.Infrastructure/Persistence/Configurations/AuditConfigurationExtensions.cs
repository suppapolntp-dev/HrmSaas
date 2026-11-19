using HrmSaas.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HrmSaas.Infrastructure.Persistence.Configurations;

internal static class AuditConfigurationExtensions
{
    public static void ConfigureAuditableColumns<TEntity>(this EntityTypeBuilder<TEntity> builder)
        where TEntity : AuditableEntity
    {
        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .IsRequired();

        builder.Property(x => x.CreatedBy)
            .HasColumnName("created_by")
            .HasColumnType("uuid");

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamptz");

        builder.Property(x => x.UpdatedBy)
            .HasColumnName("updated_by")
            .HasColumnType("uuid");

        builder.Property(x => x.IsDeleted)
            .HasColumnName("is_deleted")
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(x => x.DeletedAt)
            .HasColumnName("deleted_at")
            .HasColumnType("timestamptz");
    }

    public static void ConfigureTenantColumn<TEntity>(this EntityTypeBuilder<TEntity> builder)
        where TEntity : TenantEntity
    {
        builder.ConfigureAuditableColumns();

        builder.Property(x => x.TenantId)
            .HasColumnName("tenant_id")
            .HasColumnType("uuid")
            .IsRequired();
    }

    public static void ConfigureTenantCompanyColumns<TEntity>(this EntityTypeBuilder<TEntity> builder)
        where TEntity : TenantCompanyEntity
    {
        builder.ConfigureTenantColumn();

        builder.Property(x => x.CompanyId)
            .HasColumnName("company_id")
            .HasColumnType("uuid")
            .IsRequired();
    }
}

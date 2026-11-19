using HrmSaas.Domain.Identity;
using HrmSaas.Domain.Organizations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HrmSaas.Infrastructure.Persistence.Configurations;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users", table =>
        {
            table.HasCheckConstraint(
                "ck_users_status",
                "status IN ('ACTIVE', 'LOCKED', 'INACTIVE')");
        });

        builder.HasKey(x => x.Id).HasName("pk_users");
        builder.Property(x => x.Id).HasColumnName("id").HasColumnType("uuid").ValueGeneratedNever();
        builder.ConfigureTenantColumn();

        builder.Property(x => x.Email).HasColumnName("email").HasMaxLength(255).IsRequired();
        builder.Property(x => x.PasswordHash).HasColumnName("password_hash").HasColumnType("text").IsRequired();
        builder.Property(x => x.MfaEnabled).HasColumnName("mfa_enabled").IsRequired();
        builder.Property(x => x.Status).HasColumnName("status").HasMaxLength(30).IsRequired();
        builder.Property(x => x.LastLoginAt).HasColumnName("last_login_at").HasColumnType("timestamptz");

        builder.HasOne<Tenant>()
            .WithMany()
            .HasForeignKey(x => x.TenantId)
            .HasConstraintName("fk_users_tenants")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.TenantId, x.Email })
            .IsUnique()
            .HasDatabaseName("uq_users_tenant_email");
    }
}

internal sealed class UserSessionConfiguration : IEntityTypeConfiguration<UserSession>
{
    public void Configure(EntityTypeBuilder<UserSession> builder)
    {
        builder.ToTable("user_sessions");
        builder.HasKey(x => x.Id).HasName("pk_user_sessions");
        builder.Property(x => x.Id).HasColumnName("id").HasColumnType("uuid").ValueGeneratedNever();

        builder.Property(x => x.UserId).HasColumnName("user_id").HasColumnType("uuid").IsRequired();
        builder.Property(x => x.TenantId).HasColumnName("tenant_id").HasColumnType("uuid").IsRequired();
        builder.Property(x => x.SessionTokenHash).HasColumnName("session_token_hash").HasMaxLength(128).IsRequired();
        builder.Property(x => x.ActiveCompanyId).HasColumnName("active_company_id").HasColumnType("uuid");
        builder.Property(x => x.ActiveBranchId).HasColumnName("active_branch_id").HasColumnType("uuid");
        builder.Property(x => x.IpAddress).HasColumnName("ip_address").HasColumnType("inet").IsRequired();
        builder.Property(x => x.UserAgent).HasColumnName("user_agent").HasColumnType("text").IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").HasColumnType("timestamptz").IsRequired();
        builder.Property(x => x.LastAccessedAt).HasColumnName("last_accessed_at").HasColumnType("timestamptz").IsRequired();
        builder.Property(x => x.ExpiresAt).HasColumnName("expires_at").HasColumnType("timestamptz").IsRequired();
        builder.Property(x => x.IsRevoked).HasColumnName("is_revoked").IsRequired();

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .HasConstraintName("fk_user_sessions_users")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Tenant>()
            .WithMany()
            .HasForeignKey(x => x.TenantId)
            .HasConstraintName("fk_user_sessions_tenants")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.SessionTokenHash)
            .IsUnique()
            .HasDatabaseName("uq_user_sessions_token_hash");

        builder.HasIndex(x => new { x.UserId, x.ExpiresAt })
            .HasFilter("is_revoked = false")
            .HasDatabaseName("ix_user_sessions_active");
    }
}

internal sealed class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("roles");
        builder.HasKey(x => x.Id).HasName("pk_roles");
        builder.Property(x => x.Id).HasColumnName("id").HasColumnType("uuid").ValueGeneratedNever();
        builder.ConfigureTenantColumn();

        builder.Property(x => x.CompanyId).HasColumnName("company_id").HasColumnType("uuid");
        builder.Property(x => x.Code).HasColumnName("code").HasMaxLength(50).IsRequired();
        builder.Property(x => x.Name).HasColumnName("name").HasMaxLength(120).IsRequired();
        builder.Property(x => x.IsSystemRole).HasColumnName("is_system_role").IsRequired();

        builder.HasOne<Tenant>()
            .WithMany()
            .HasForeignKey(x => x.TenantId)
            .HasConstraintName("fk_roles_tenants")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Company>()
            .WithMany()
            .HasForeignKey(x => x.CompanyId)
            .HasConstraintName("fk_roles_companies")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.TenantId, x.CompanyId, x.Code })
            .IsUnique()
            .HasDatabaseName("uq_roles_tenant_company_code");
    }
}

internal sealed class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("permissions");
        builder.HasKey(x => x.Id).HasName("pk_permissions");
        builder.Property(x => x.Id).HasColumnName("id").HasColumnType("uuid").ValueGeneratedNever();

        builder.Property(x => x.Code).HasColumnName("code").HasMaxLength(100).IsRequired();
        builder.Property(x => x.Module).HasColumnName("module").HasMaxLength(50).IsRequired();
        builder.Property(x => x.Description).HasColumnName("description").HasMaxLength(250).IsRequired();

        builder.HasIndex(x => x.Code)
            .IsUnique()
            .HasDatabaseName("uq_permissions_code");
    }
}

internal sealed class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        builder.ToTable("role_permissions");
        builder.HasKey(x => new { x.RoleId, x.PermissionId }).HasName("pk_role_permissions");

        builder.Property(x => x.RoleId).HasColumnName("role_id").HasColumnType("uuid").IsRequired();
        builder.Property(x => x.PermissionId).HasColumnName("permission_id").HasColumnType("uuid").IsRequired();

        builder.HasOne<Role>()
            .WithMany()
            .HasForeignKey(x => x.RoleId)
            .HasConstraintName("fk_role_permissions_roles")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Permission>()
            .WithMany()
            .HasForeignKey(x => x.PermissionId)
            .HasConstraintName("fk_role_permissions_permissions")
            .OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class UserCompanyRoleConfiguration : IEntityTypeConfiguration<UserCompanyRole>
{
    public void Configure(EntityTypeBuilder<UserCompanyRole> builder)
    {
        builder.ToTable("user_company_roles");
        builder.HasKey(x => x.Id).HasName("pk_user_company_roles");
        builder.Property(x => x.Id).HasColumnName("id").HasColumnType("uuid").ValueGeneratedNever();
        builder.ConfigureTenantCompanyColumns();

        builder.Property(x => x.UserId).HasColumnName("user_id").HasColumnType("uuid").IsRequired();
        builder.Property(x => x.RoleId).HasColumnName("role_id").HasColumnType("uuid").IsRequired();
        builder.Property(x => x.BranchScopeIds).HasColumnName("branch_scope_ids").HasColumnType("uuid[]");

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .HasConstraintName("fk_user_company_roles_users")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Company>()
            .WithMany()
            .HasForeignKey(x => x.CompanyId)
            .HasConstraintName("fk_user_company_roles_companies")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Role>()
            .WithMany()
            .HasForeignKey(x => x.RoleId)
            .HasConstraintName("fk_user_company_roles_roles")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.UserId, x.CompanyId, x.RoleId })
            .IsUnique()
            .HasDatabaseName("uq_user_company_roles_user_comp_role");

        builder.HasIndex(x => new { x.TenantId, x.CompanyId, x.UserId })
            .HasDatabaseName("ix_user_company_roles_tenant_company_user");
    }
}

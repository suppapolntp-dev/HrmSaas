using HrmSaas.Application.Common.Interfaces;
using HrmSaas.Application.Features.Attendance;
using HrmSaas.Application.Features.Auth;
using HrmSaas.Application.Features.Dashboard;
using HrmSaas.Application.Features.Employees;
using HrmSaas.Application.Features.Organizations;
using HrmSaas.Application.Features.Payroll;
using HrmSaas.Application.Features.Settings;
using HrmSaas.Application.Features.Workflows;
using HrmSaas.Infrastructure.Options;
using HrmSaas.Infrastructure.Persistence;
using HrmSaas.Infrastructure.Persistence.Seed;
using HrmSaas.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HrmSaas.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<SessionOptions>()
            .BindConfiguration(SessionOptions.SectionName)
            .ValidateOnStart();

        services.AddOptions<ProxyOptions>()
            .BindConfiguration(ProxyOptions.SectionName)
            .ValidateOnStart();

        services.AddScoped<TenantContext>();
        services.AddScoped<ITenantContext>(sp => sp.GetRequiredService<TenantContext>());
        services.AddScoped<TenantRlsConnectionInterceptor>();

        string connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

        services.AddDbContext<AppDbContext>((serviceProvider, options) =>
        {
            options.UseNpgsql(connectionString, npgsql =>
            {
                npgsql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
            });
            options.AddInterceptors(serviceProvider.GetRequiredService<TenantRlsConnectionInterceptor>());
        });

        services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddScoped<DatabaseSeeder>();

        services.AddDistributedMemoryCache();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<ISessionStore, DistributedSessionStore>();
        services.AddScoped<AuthService>();
        services.AddScoped<OrganizationService>();
        services.AddScoped<EmployeeService>();
        services.AddScoped<AttendanceService>();
        services.AddScoped<LeaveAndWorkflowService>();
        services.AddScoped<PayrollService>();
        services.AddScoped<RbacAndAuditService>();
        services.AddScoped<DashboardService>();

        services.AddAuthentication(SessionAuthDefaults.AuthenticationScheme)
            .AddScheme<AuthenticationSchemeOptions, CookieSessionAuthHandler>(
                SessionAuthDefaults.AuthenticationScheme,
                _ => { });

        services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
        services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();

        return services;
    }
}

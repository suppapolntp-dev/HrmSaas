using System.Net;
using System.Threading.RateLimiting;
using HrmSaas.Infrastructure;
using HrmSaas.Infrastructure.Options;
using HrmSaas.Infrastructure.Persistence;
using HrmSaas.Infrastructure.Persistence.Seed;
using HrmSaas.WebApi.Middleware;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    ProxyOptions? proxyOptions = builder.Configuration
        .GetSection(ProxyOptions.SectionName)
        .Get<ProxyOptions>();

    if (proxyOptions is not null)
    {
        foreach (string proxyIp in proxyOptions.KnownProxies)
        {
            if (IPAddress.TryParse(proxyIp, out IPAddress? parsed))
            {
                options.KnownProxies.Add(parsed);
            }
        }
    }
});

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("login", httpContext =>
        RateLimitPartition.GetSlidingWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new SlidingWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                SegmentsPerWindow = 4,
                QueueLimit = 0
            }));
    options.AddPolicy("check-in", httpContext =>
        RateLimitPartition.GetSlidingWindowLimiter(
            partitionKey: httpContext.User.Identity?.Name ?? httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new SlidingWindowRateLimiterOptions
            {
                PermitLimit = 20,
                Window = TimeSpan.FromMinutes(1),
                SegmentsPerWindow = 4,
                QueueLimit = 0
            }));
});

var app = builder.Build();

if (args.Contains("--seed", StringComparer.OrdinalIgnoreCase))
{
    using IServiceScope scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
    await seeder.SeedAsync(dbContext);
    Console.WriteLine("Database seeding completed successfully.");
    return;
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Mandatory Middleware Execution Order (plan/02-backend-dotnet.md Section 4)
app.UseForwardedHeaders();
app.UseMiddleware<GlobalExceptionHandlingMiddleware>();
app.UseMiddleware<RequestCorrelationAndLoggingMiddleware>();
app.UseRateLimiter();
app.UseAuthentication();
app.UseMiddleware<TenantAndCompanyContextMiddleware>();
app.UseMiddleware<XsrfValidationMiddleware>();
app.UseAuthorization();

app.MapControllers();

app.Run();

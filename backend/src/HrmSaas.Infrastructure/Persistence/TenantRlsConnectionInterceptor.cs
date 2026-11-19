using System.Data.Common;
using HrmSaas.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace HrmSaas.Infrastructure.Persistence;

public sealed class TenantRlsConnectionInterceptor : DbConnectionInterceptor
{
    private readonly ITenantContext _tenantContext;

    public TenantRlsConnectionInterceptor(ITenantContext tenantContext)
    {
        _tenantContext = tenantContext;
    }

    public override void ConnectionOpened(DbConnection connection, ConnectionEndEventData eventData)
    {
        using DbCommand command = CreateSetRlsCommand(connection);
        command.ExecuteNonQuery();
    }

    public override async Task ConnectionOpenedAsync(
        DbConnection connection,
        ConnectionEndEventData eventData,
        CancellationToken cancellationToken = default)
    {
        await using DbCommand command = CreateSetRlsCommand(connection);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private DbCommand CreateSetRlsCommand(DbConnection connection)
    {
        DbCommand command = connection.CreateCommand();
        command.CommandText =
            "SELECT set_config('app.current_tenant_id', @tenant_id, false), " +
            "set_config('app.auth_lookup_email', '', false), " +
            "set_config('app.auth_lookup_token_hash', '', false);";

        DbParameter tenantParam = command.CreateParameter();
        tenantParam.ParameterName = "tenant_id";
        tenantParam.Value = _tenantContext.TenantId?.ToString() ?? string.Empty;
        command.Parameters.Add(tenantParam);

        return command;
    }
}

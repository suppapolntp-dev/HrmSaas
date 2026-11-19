using System.Net;
using HrmSaas.Application.Common.Interfaces;

namespace HrmSaas.Infrastructure.Security;

public sealed class TenantContext : ITenantContext
{
    public Guid? TenantId { get; private set; }
    public Guid? ActiveCompanyId { get; private set; }
    public Guid? ActiveBranchId { get; private set; }
    public Guid? CurrentUserId { get; private set; }
    public IPAddress? ClientIpAddress { get; private set; }
    public string CorrelationId { get; private set; } = string.Empty;

    public void SetContext(
        Guid tenantId,
        Guid? activeCompanyId,
        Guid? activeBranchId,
        Guid? currentUserId,
        IPAddress? clientIpAddress,
        string correlationId)
    {
        TenantId = tenantId;
        ActiveCompanyId = activeCompanyId;
        ActiveBranchId = activeBranchId;
        CurrentUserId = currentUserId;
        ClientIpAddress = clientIpAddress;
        CorrelationId = correlationId;
    }

    public void SetTenant(Guid tenantId, Guid? currentUserId = null)
    {
        TenantId = tenantId;
        if (currentUserId.HasValue)
        {
            CurrentUserId = currentUserId;
        }
    }

    public void SetCorrelation(IPAddress? clientIpAddress, string correlationId)
    {
        ClientIpAddress = clientIpAddress;
        CorrelationId = correlationId;
    }
}

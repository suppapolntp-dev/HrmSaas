using System.Net;

namespace HrmSaas.Application.Common.Interfaces;

public interface ITenantContext
{
    Guid? TenantId { get; }
    Guid? ActiveCompanyId { get; }
    Guid? ActiveBranchId { get; }
    Guid? CurrentUserId { get; }
    IPAddress? ClientIpAddress { get; }
    string CorrelationId { get; }

    void SetTenant(Guid tenantId, Guid? currentUserId = null);
}

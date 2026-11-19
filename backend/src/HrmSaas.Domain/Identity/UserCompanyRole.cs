using HrmSaas.Domain.Common;

namespace HrmSaas.Domain.Identity;

public sealed class UserCompanyRole : TenantCompanyEntity
{
    private UserCompanyRole()
    {
    }

    public Guid UserId { get; private set; }
    public Guid RoleId { get; private set; }
    public Guid[]? BranchScopeIds { get; private set; }

    public static UserCompanyRole Create(
        Guid tenantId,
        Guid userId,
        Guid companyId,
        Guid roleId,
        Guid[]? branchScopeIds = null,
        Guid? id = null)
    {
        if (tenantId == Guid.Empty || userId == Guid.Empty || companyId == Guid.Empty || roleId == Guid.Empty)
        {
            throw new ArgumentException("TenantId, UserId, CompanyId, and RoleId are required.");
        }

        var assignment = new UserCompanyRole
        {
            TenantId = tenantId,
            UserId = userId,
            CompanyId = companyId,
            RoleId = roleId,
            BranchScopeIds = branchScopeIds
        };

        if (id.HasValue)
        {
            assignment.Id = id.Value;
        }

        return assignment;
    }
}

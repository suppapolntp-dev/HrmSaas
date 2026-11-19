using HrmSaas.Domain.Common;

namespace HrmSaas.Domain.Identity;

public sealed class Permission : BaseEntity
{
    private Permission()
    {
    }

    public string Code { get; private set; } = string.Empty;
    public string Module { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;

    public static Permission Create(
        string code,
        string module,
        string description,
        Guid? id = null)
    {
        if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(module))
        {
            throw new ArgumentException("Permission code and module are required.");
        }

        var permission = new Permission
        {
            Code = code.Trim().ToLowerInvariant(),
            Module = module.Trim().ToUpperInvariant(),
            Description = description.Trim()
        };

        if (id.HasValue)
        {
            permission.Id = id.Value;
        }

        return permission;
    }
}

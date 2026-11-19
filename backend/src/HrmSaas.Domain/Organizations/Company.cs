using HrmSaas.Domain.Common;

namespace HrmSaas.Domain.Organizations;

public sealed class Company : TenantEntity
{
    private Company()
    {
    }

    public string Code { get; private set; } = string.Empty;
    public string LegalName { get; private set; } = string.Empty;
    public string TaxId { get; private set; } = string.Empty;
    public string SsoEmployerCode { get; private set; } = string.Empty;
    public string BaseCurrency { get; private set; } = "THB";

    public static Company Create(
        Guid tenantId,
        string code,
        string legalName,
        string taxId,
        string ssoEmployerCode,
        string baseCurrency = "THB",
        Guid? id = null)
    {
        if (tenantId == Guid.Empty)
        {
            throw new ArgumentException("TenantId is required.", nameof(tenantId));
        }

        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("Company code is required.", nameof(code));
        }

        if (string.IsNullOrWhiteSpace(legalName))
        {
            throw new ArgumentException("Legal name is required.", nameof(legalName));
        }

        var company = new Company
        {
            TenantId = tenantId,
            Code = code.Trim().ToUpperInvariant(),
            LegalName = legalName.Trim(),
            TaxId = taxId.Trim(),
            SsoEmployerCode = ssoEmployerCode.Trim(),
            BaseCurrency = baseCurrency.Trim().ToUpperInvariant()
        };

        if (id.HasValue)
        {
            company.Id = id.Value;
        }

        return company;
    }

    public void UpdateDetails(string legalName, string taxId, string ssoEmployerCode, string baseCurrency)
    {
        if (string.IsNullOrWhiteSpace(legalName))
        {
            throw new ArgumentException("Legal name is required.", nameof(legalName));
        }

        LegalName = legalName.Trim();
        TaxId = taxId.Trim();
        SsoEmployerCode = ssoEmployerCode.Trim();
        BaseCurrency = baseCurrency.Trim().ToUpperInvariant();
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}

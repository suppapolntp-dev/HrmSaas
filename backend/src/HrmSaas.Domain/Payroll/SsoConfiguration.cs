using HrmSaas.Domain.Common;

namespace HrmSaas.Domain.Payroll;

public sealed class SsoConfiguration : BaseEntity
{
    private SsoConfiguration()
    {
    }

    public string CountryCode { get; private set; } = "TH";
    public DateOnly EffectiveFrom { get; private set; }
    public DateOnly? EffectiveTo { get; private set; }
    public decimal EmployeeRatePercent { get; private set; }
    public decimal EmployerRatePercent { get; private set; }
    public decimal MinWageBase { get; private set; }
    public decimal MaxWageBase { get; private set; }

    public static SsoConfiguration Create(
        string countryCode,
        DateOnly effectiveFrom,
        DateOnly? effectiveTo,
        decimal employeeRatePercent,
        decimal employerRatePercent,
        decimal minWageBase,
        decimal maxWageBase,
        Guid? id = null)
    {
        if (employeeRatePercent < 0 || employerRatePercent < 0 || minWageBase < 0 || maxWageBase < minWageBase)
        {
            throw new ArgumentException("SSO configuration rates and wage bounds are invalid.");
        }

        var config = new SsoConfiguration
        {
            CountryCode = countryCode.Trim().ToUpperInvariant(),
            EffectiveFrom = effectiveFrom,
            EffectiveTo = effectiveTo,
            EmployeeRatePercent = employeeRatePercent,
            EmployerRatePercent = employerRatePercent,
            MinWageBase = minWageBase,
            MaxWageBase = maxWageBase
        };

        if (id.HasValue)
        {
            config.Id = id.Value;
        }

        return config;
    }
}

using HrmSaas.Domain.Common;

namespace HrmSaas.Domain.Payroll;

public sealed class TaxBracket : BaseEntity
{
    private TaxBracket()
    {
    }

    public int TaxYear { get; private set; }
    public string CountryCode { get; private set; } = "TH";
    public decimal MinNetIncome { get; private set; }
    public decimal? MaxNetIncome { get; private set; }
    public decimal TaxRatePercent { get; private set; }

    public static TaxBracket Create(
        int taxYear,
        string countryCode,
        decimal minNetIncome,
        decimal? maxNetIncome,
        decimal taxRatePercent,
        Guid? id = null)
    {
        if (minNetIncome < 0 || taxRatePercent < 0)
        {
            throw new ArgumentException("Tax income threshold and rate cannot be negative.");
        }

        if (maxNetIncome.HasValue && maxNetIncome.Value < minNetIncome)
        {
            throw new ArgumentException("MaxNetIncome cannot be less than MinNetIncome.");
        }

        var bracket = new TaxBracket
        {
            TaxYear = taxYear,
            CountryCode = countryCode.Trim().ToUpperInvariant(),
            MinNetIncome = minNetIncome,
            MaxNetIncome = maxNetIncome,
            TaxRatePercent = taxRatePercent
        };

        if (id.HasValue)
        {
            bracket.Id = id.Value;
        }

        return bracket;
    }
}

using PM.Domain.Entities;
using PM.Domain.Enums;
using PM.Domain.Values;
using PM.DTO;

namespace PM.Domain.Mappers;

/// <summary>
/// Provides mapping between <see cref="ValuationSnapshot"/> domain entities and <see cref="ValuationSnapshotDTO"/> objects.
/// </summary>
public static class ValuationSnapshotMapper
{
    /// <summary>
    /// Converts a domain <see cref="ValuationSnapshot"/> to a <see cref="ValuationSnapshotDTO"/>.
    /// </summary>
    public static ValuationSnapshotDTO ToDTO(this ValuationSnapshot entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));

        return new ValuationSnapshotDTO
        {
            Date = entity.Date,
            Period = entity.Period.ToString(),
            ReportingCurrency = entity.ReportingCurrency.Code,
            Value = entity.Value.Amount,
            ValueCurrency = entity.Value.Currency.Code,
            AccountId = entity.AccountId,
            PortfolioId = entity.PortfolioId,
            AssetClass = entity.AssetClass?.ToString(),
            Percentage = entity.Percentage,
            SecuritiesValue = entity.SecuritiesValue?.Amount,
            SecuritiesValueCurrency = entity.SecuritiesValue?.Currency.Code,
            CashValue = entity.CashValue?.Amount,
            CashValueCurrency = entity.CashValue?.Currency.Code,
            IncomeForDay = entity.IncomeForDay?.Amount,
            IncomeForDayCurrency = entity.IncomeForDay?.Currency.Code
        };
    }

    /// <summary>
    /// Converts a <see cref="ValuationSnapshotDTO"/> to a domain <see cref="ValuationSnapshot"/>.
    /// </summary>
    public static ValuationSnapshot ToEntity(this ValuationSnapshotDTO dto)
    {
        if (dto == null) throw new ArgumentNullException(nameof(dto));

        var reportingCurrency = new Currency(dto.ReportingCurrency);

        return new ValuationSnapshot
        {
            Date = dto.Date,
            Period = Enum.TryParse<ValuationPeriod>(dto.Period, true, out var period)
                ? period
                : throw new ArgumentException($"Invalid valuation period: {dto.Period}"),
            ReportingCurrency = reportingCurrency,
            Value = new Money(dto.Value, new Currency(dto.ValueCurrency)),
            AccountId = dto.AccountId,
            PortfolioId = dto.PortfolioId,
            AssetClass = dto.AssetClass != null
                ? Enum.TryParse<AssetClass>(dto.AssetClass, true, out var ac)
                    ? ac
                    : throw new ArgumentException($"Invalid asset class: {dto.AssetClass}")
                : null,
            Percentage = dto.Percentage,
            SecuritiesValue = dto.SecuritiesValue.HasValue
                ? new Money(dto.SecuritiesValue.Value, new Currency(dto.SecuritiesValueCurrency ?? dto.ReportingCurrency))
                : null,
            CashValue = dto.CashValue.HasValue
                ? new Money(dto.CashValue.Value, new Currency(dto.CashValueCurrency ?? dto.ReportingCurrency))
                : null,
            IncomeForDay = dto.IncomeForDay.HasValue
                ? new Money(dto.IncomeForDay.Value, new Currency(dto.IncomeForDayCurrency ?? dto.ReportingCurrency))
                : null
        };
    }
}

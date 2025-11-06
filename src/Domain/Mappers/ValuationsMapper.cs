using PM.Domain.Entities;
using PM.Domain.Enums;
using PM.Domain.Values;
using PM.DTO;

namespace PM.Domain.Mappers;

/// <summary>
/// Provides mapping between <see cref="ValuationRecord"/> domain entities and <see cref="ValuationRecordDTO"/> objects.
/// </summary>
public static class ValuationRecordMapper
{
    /// <summary>
    /// Converts a domain <see cref="ValuationRecord"/> to a <see cref="ValuationRecordDTO"/>.
    /// </summary>
    public static ValuationRecordDTO ToDTO(this ValuationRecord entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));

        return new ValuationRecordDTO
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
    /// Converts a <see cref="ValuationRecordDTO"/> to a domain <see cref="ValuationRecord"/>.
    /// </summary>
    public static ValuationRecord ToEntity(this ValuationRecordDTO dto)
    {
        if (dto == null) throw new ArgumentNullException(nameof(dto));

        var reportingCurrency = new Currency(dto.ReportingCurrency);

        return new ValuationRecord
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

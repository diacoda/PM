using PM.Domain.Enums;
namespace PM.Domain.Values;

public record PeriodPerformance(
    DateTime Start,
    DateTime End,
    Currency ReportingCurrency,
    ReturnMethod Method,
    decimal Return,          // 0.0123 => 1.23%
    Money BeginningValue,
    Money EndingValue,
    Money NetFlows
);
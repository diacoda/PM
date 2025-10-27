using PM.Domain.Enums;
using PM.Domain.Values;

namespace PM.Domain.Entities;

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
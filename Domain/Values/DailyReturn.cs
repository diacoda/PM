using PM.Domain.Values;

namespace PM.Domain.Values;

public record DailyReturn(
    DateTime Date,
    EntityKind EntityType,    // enum now
    int EntityId,
    Currency ReportingCurrency,
    decimal Return            // 0.0042 => 0.42%
);

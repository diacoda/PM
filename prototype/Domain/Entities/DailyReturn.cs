using model.Domain.Values;

namespace model.Domain.Entities;

public record DailyReturn(
    DateTime Date,
    EntityKind EntityType,    // enum now
    int EntityId,
    Currency ReportingCurrency,
    decimal Return            // 0.0042 => 0.42%
);

using model.Domain.Values;

namespace model.Domain.Entities;

public record ContributionRecord(
    DateTime Start,
    DateTime End,
    Currency ReportingCurrency,
    ContributionLevel Level, // enum now
    string Key,              // e.g., "VFV.TO" or "Equity"
    decimal StartWeight,     // fraction of parent at start
    decimal Return,          // holding-level return over [Start,End]
    decimal Contribution     // StartWeight * Return
);

using PM.Domain.Enums;
using PM.Domain.Values;

namespace PM.Domain.Values;

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

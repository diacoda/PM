// Events/DailyPricesFetchedEvent.cs
using System;
using System.Collections.Generic;
using PM.Domain.Enums;
using PM.Domain.Values;
namespace PM.Domain.Events;

public class DailyPricesFetchedEvent
{
    public DateOnly EffectiveDate { get; }
    public DateTime RunTimestamp { get; }
    public bool AllSucceeded { get; }
    public int FetchedCount { get; }
    public int SkippedCount { get; }
    public int ErrorCount { get; }
    public IEnumerable<SymbolFetchDetail> Details { get; }
    public string Notes { get; }

    public DailyPricesFetchedEvent(
        DateOnly effectiveDate,
        DateTime runTimestamp,
        bool allSucceeded,
        int fetchedCount,
        int skippedCount,
        int errorCount,
        IEnumerable<SymbolFetchDetail> details,
        string? notes = null)
    {
        EffectiveDate = effectiveDate;
        RunTimestamp = runTimestamp;
        AllSucceeded = allSucceeded;
        FetchedCount = fetchedCount;
        SkippedCount = skippedCount;
        ErrorCount = errorCount;
        Details = details ?? Array.Empty<SymbolFetchDetail>();
        Notes = notes ?? string.Empty;
    }
}

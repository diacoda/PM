namespace PM.Domain.Values;

/// <summary>
/// Represents a single component of a benchmark index, including the underlying symbol and its weight in the index.
/// </summary>
/// <param name="Symbol">The financial symbol included in the benchmark.</param>
/// <param name="Weight">The relative weight of the symbol within the benchmark (typically between 0 and 1).</param>
public record BenchmarkComponent(Symbol Symbol, decimal Weight);
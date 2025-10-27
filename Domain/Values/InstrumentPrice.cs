namespace PM.Domain.Values;

public record InstrumentPrice(Symbol Symbol, DateTime Date, Money Price);
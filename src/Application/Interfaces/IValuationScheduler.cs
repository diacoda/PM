using PM.Domain.Enums;

namespace PM.Application.Interfaces;

public interface IValuationScheduler
{
    IEnumerable<ValuationPeriod> GetValuationsForToday(DateTime date);
}

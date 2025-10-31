using PM.Domain.Entities;
namespace PM.Application.Interfaces;

public interface ITransactionWorkflowService
{
    Task<Transaction> ProcessTransactionAsync(Transaction tx, CancellationToken ct = default);
}

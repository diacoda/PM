using PM.Domain.Entities;
using PM.DTO;
namespace PM.Application.Interfaces;

public interface ITransactionWorkflowService
{
    Task<Transaction> ProcessTransactionAsync(Transaction tx, CancellationToken ct = default);
}

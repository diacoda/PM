using PM.Domain.Entities;
using PM.DTO;
namespace PM.Application.Interfaces;

public interface ITransactionWorkflowService
{
    Task<TransactionDTO> ProcessTransactionAsync(int portfolioId, Transaction tx, CancellationToken ct = default);
}

using PM.Application.Interfaces;
using PM.Domain.Entities;
using PM.Domain.Enums;
using PM.Domain.Events;
using PM.Domain.Mappers;
using PM.Domain.Values;
using PM.DTO;
using PM.InMemoryEventBus;
using PM.SharedKernel.Events;

namespace PM.Application.Services;

public class TransactionWorkflowService : ITransactionWorkflowService
{
    private readonly ITransactionService _transactionService;
    private readonly ICashFlowService _cashFlowService;
    private readonly IHoldingService _holdingService;
    private readonly PM.SharedKernel.Events.IDomainEventDispatcher _dispatcher;
    private readonly PM.InMemoryEventBus.IDomainEventDispatcher _eventDispatcher;

    public TransactionWorkflowService(
        ITransactionService transactionService,
        ICashFlowService cashFlowService,
        IHoldingService holdingService,
        PM.SharedKernel.Events.IDomainEventDispatcher dispatcher,
        PM.InMemoryEventBus.IDomainEventDispatcher eventDispatcher)
    {
        _transactionService = transactionService;
        _cashFlowService = cashFlowService;
        _holdingService = holdingService;
        _dispatcher = dispatcher;
        _eventDispatcher = eventDispatcher;
    }

    public async Task<TransactionDTO> ProcessTransactionAsync(int portfolioId, Transaction tx, CancellationToken ct = default)
    {
        var savedTx = await _transactionService.CreateAsync(tx, ct);
        savedTx.Raise(new TransactionAddedEvent(portfolioId, savedTx.AccountId, savedTx.Id, savedTx.Date));

        var txDto = TransactionMapper.ToDTO(savedTx);

        if (tx.Type is TransactionType.Deposit or TransactionType.Withdrawal or TransactionType.Buy or TransactionType.Sell or TransactionType.Dividend)
        {
            var flowType = tx.Type switch
            {
                TransactionType.Deposit => CashFlowType.Deposit,
                TransactionType.Withdrawal => CashFlowType.Withdrawal,
                TransactionType.Buy => CashFlowType.Buy,
                TransactionType.Sell => CashFlowType.Sell,
                TransactionType.Dividend => CashFlowType.Dividend,
                TransactionType.Interest => CashFlowType.Interest,
                _ => CashFlowType.Other
            };

            CashFlow cashFlow = await _cashFlowService.RecordCashFlowAsync(
                tx.AccountId,
                tx.Date,
                tx.Amount,
                flowType,
                "tx.Note",
                ct);
            txDto.CashFlowId = cashFlow.Id;
        }

        IReadOnlyList<Holding?> holdings = await ApplyToHoldingsAsync(tx, ct);
        txDto.HoldingIds = holdings.Select(h => h!.Id).ToArray();

        await _dispatcher.DispatchEntityEventsAsync(savedTx);

        savedTx.Raise(new TransactionAddedEvent(portfolioId, savedTx.AccountId, savedTx.Id, savedTx.Date));
        await _eventDispatcher.DispatchEntityEventsAsync(savedTx, ct);

        return txDto;
    }

    private async Task<IReadOnlyList<Holding>> ApplyToHoldingsAsync(Transaction tx, CancellationToken ct)
    {
        var symbol = tx.Symbol;
        var currency = tx.Amount.Currency;
        var cashSymbol = new Symbol(currency.Code);
        var cost = tx.Costs?.Amount ?? 0m;

        var updatedHoldings = new List<Holding>();

        switch (tx.Type)
        {
            case TransactionType.Deposit:
                {
                    var cashHolding = await _holdingService.UpsertHoldingAsync(
                        tx.AccountId,
                        cashSymbol.ToAsset(),
                        tx.Amount.Amount,
                        ct);

                    updatedHoldings.Add(cashHolding);
                    break;
                }

            case TransactionType.Withdrawal:
                {
                    var cashHolding = await _holdingService.UpsertHoldingAsync(
                        tx.AccountId,
                        cashSymbol.ToAsset(),
                        -tx.Amount.Amount,
                        ct);

                    updatedHoldings.Add(cashHolding);
                    break;
                }

            case TransactionType.Buy:
                {
                    var securityHolding = await _holdingService.UpsertHoldingAsync(
                        tx.AccountId,
                        symbol.ToAsset(),
                        tx.Quantity,
                        ct);

                    var cashHolding = await _holdingService.UpsertHoldingAsync(
                        tx.AccountId,
                        cashSymbol.ToAsset(),
                        -(tx.Amount.Amount + cost),
                        ct);

                    updatedHoldings.AddRange(new[] { securityHolding, cashHolding });
                    break;
                }

            case TransactionType.Sell:
                {
                    var securityHolding = await _holdingService.UpsertHoldingAsync(
                        tx.AccountId,
                        symbol.ToAsset(),
                        -tx.Quantity,
                        ct);

                    var cashHolding = await _holdingService.UpsertHoldingAsync(
                        tx.AccountId,
                        cashSymbol.ToAsset(),
                        tx.Amount.Amount - cost,
                        ct);

                    updatedHoldings.AddRange(new[] { securityHolding, cashHolding });
                    break;
                }

            case TransactionType.Dividend:
            case TransactionType.Interest:
                {
                    var cashHolding = await _holdingService.UpsertHoldingAsync(
                        tx.AccountId,
                        cashSymbol.ToAsset(),
                        tx.Amount.Amount - cost,
                        ct);

                    updatedHoldings.Add(cashHolding);
                    break;
                }

            default:
                // Optionally handle other transaction types (Interest, Split, Fee, etc.)
                break;
        }

        return updatedHoldings;
    }

}
using PM.Application.Interfaces;
using PM.Domain.Entities;
using PM.Domain.Enums;
using PM.Domain.Mappers;
using PM.Domain.Values;
using PM.DTO;

namespace PM.Application.Services;

public class TransactionWorkflowService : ITransactionWorkflowService
{
    private readonly ITransactionService _transactionService;
    private readonly ICashFlowService _cashFlowService;
    private readonly IHoldingService _holdingService;

    public TransactionWorkflowService(
        ITransactionService transactionService,
        ICashFlowService cashFlowService,
        IHoldingService holdingService)
    {
        _transactionService = transactionService;
        _cashFlowService = cashFlowService;
        _holdingService = holdingService;
    }

    public async Task<Transaction> ProcessTransactionAsync(Transaction tx, CancellationToken ct = default)
    {
        var savedTx = await _transactionService.CreateAsync(tx, ct);

        if (tx.Type is TransactionType.Deposit or TransactionType.Withdrawal or TransactionType.Buy or TransactionType.Sell or TransactionType.Dividend)
        {
            var flowType = tx.Type switch
            {
                TransactionType.Deposit => CashFlowType.Deposit,
                TransactionType.Withdrawal => CashFlowType.Withdrawal,
                TransactionType.Buy => CashFlowType.Buy,
                TransactionType.Sell => CashFlowType.Sell,
                TransactionType.Dividend => CashFlowType.Dividend,
                _ => CashFlowType.Other
            };

            await _cashFlowService.RecordCashFlowAsync(
                tx.AccountId,
                tx.Date,
                tx.Amount,
                flowType,
                "tx.Note",
                ct);
        }

        Holding? holding = await ApplyToHoldingsAsync(tx, ct);

        return savedTx;
    }

    private async Task<Holding?> ApplyToHoldingsAsync(Transaction tx, CancellationToken ct)
    {
        var symbol = tx.Symbol;
        var currency = tx.Amount.Currency;
        var cashSymbol = new Symbol(currency.Code);
        var cost = tx.Costs?.Amount ?? 0m;

        Holding holding = null;
        switch (tx.Type)
        {
            case TransactionType.Deposit:
                holding = await _holdingService.UpsertHoldingAsync(tx.AccountId, cashSymbol, tx.Amount.Amount, ct);
                break;

            case TransactionType.Withdrawal:
                holding = await _holdingService.UpsertHoldingAsync(tx.AccountId, cashSymbol, -tx.Amount.Amount, ct);
                break;

            case TransactionType.Buy:
                holding = await _holdingService.UpsertHoldingAsync(tx.AccountId, symbol, tx.Quantity, ct);
                await _holdingService.UpsertHoldingAsync(tx.AccountId, cashSymbol, -(tx.Amount.Amount + cost), ct);
                break;

            case TransactionType.Sell:
                holding = await _holdingService.UpsertHoldingAsync(tx.AccountId, symbol, -tx.Quantity, ct);
                await _holdingService.UpsertHoldingAsync(tx.AccountId, cashSymbol, tx.Amount.Amount - cost, ct);
                break;

            case TransactionType.Dividend:
                holding = await _holdingService.UpsertHoldingAsync(tx.AccountId, cashSymbol, tx.Amount.Amount - cost, ct);
                break;

            default:
                // Other types can be handled here if needed
                break;
        }
        return holding;
    }
}
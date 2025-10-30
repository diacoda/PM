using PM.Application.Interfaces;
using PM.Domain.Entities;
using PM.Domain.Values;
using PM.SharedKernel;

namespace PM.Application.Services
{
    public class HoldingService : IHoldingService
    {
        private readonly IAccountRepository _accountRepo;
        private readonly IHoldingRepository _holdingRepo;

        public HoldingService(IAccountRepository accountRepo, IHoldingRepository holdingRepo)
        {
            _accountRepo = accountRepo;
            _holdingRepo = holdingRepo;
        }

        public async Task UpsertHoldingAsync(int accountId, Symbol symbol, decimal quantity, CancellationToken ct = default)
        {
            var account = await _accountRepo.GetByIdWithIncludesAsync(accountId, includes: new IncludeOption[] { IncludeOption.Holdings }, ct);
            if (account == null)
                throw new InvalidOperationException($"Account {accountId} not found");

            account.UpsertHolding(new Holding(symbol, quantity));
            await _accountRepo.UpdateAsync(account, ct);
            await _accountRepo.SaveChangesAsync(ct);
        }

        public async Task UpdateHoldingQuantityAsync(int accountId, Symbol symbol, decimal newQty, CancellationToken ct = default)
        {
            if (newQty < 0) throw new InvalidOperationException($"Quantity {newQty} must be positive");

            var account = await _accountRepo.GetByIdAsync(accountId, ct);
            if (account == null)
                throw new InvalidOperationException($"Account {accountId} not found");

            account.UpdateHoldingQuantity(symbol, newQty);
            await _accountRepo.UpdateAsync(account, ct);
            await _accountRepo.SaveChangesAsync(ct);
        }

        public async Task RemoveHoldingAsync(int accountId, Symbol symbol, CancellationToken ct = default)
        {
            var account = await _accountRepo.GetByIdAsync(accountId, ct);
            if (account == null)
                throw new InvalidOperationException($"Account {accountId} not found");

            var holding = account.Holdings.FirstOrDefault(h => h.Symbol.Equals(symbol));
            if (holding == null)
                return;

            account.RemoveHolding(holding);
            await _accountRepo.UpdateAsync(account, ct);
            await _accountRepo.SaveChangesAsync(ct);
        }

        public async Task<Holding?> GetHoldingAsync(int accountId, Symbol symbol, CancellationToken ct = default)
        {
            var holdings = await _holdingRepo.ListByAccountAsync(accountId, ct);
            return holdings.FirstOrDefault(h => h.Symbol.Equals(symbol));
        }

        public async Task<decimal> GetCashBalanceAsync(int accountId, Currency currency, CancellationToken ct = default)
        {
            var symbol = new Symbol(currency.Code);
            var account = await _accountRepo.GetByIdAsync(accountId, ct);
            if (account == null)
                return 0.0m;

            var holding = account.Holdings.FirstOrDefault(h => h.Symbol == symbol);
            return holding?.Quantity ?? 0;
        }

        public async Task<IEnumerable<Holding>> ListHoldingsAsync(int accountId, CancellationToken ct = default)
        {
            return await _holdingRepo.ListByAccountAsync(accountId, ct);
        }

        public async Task AddTagAsync(int accountId, Symbol symbol, Tag tag, CancellationToken ct = default)
        {
            var account = await _accountRepo.GetByIdAsync(accountId, ct);
            if (account == null)
                throw new InvalidOperationException($"Account {accountId} not found");

            var holding = account.Holdings.FirstOrDefault(h => h.Symbol.Equals(symbol));
            if (holding == null)
                throw new InvalidOperationException($"Holding not found for {symbol}");

            holding.AddTag(tag);
            await _accountRepo.UpdateAsync(account, ct);
            await _accountRepo.SaveChangesAsync(ct);
        }

        public async Task RemoveTagAsync(int accountId, Symbol symbol, Tag tag, CancellationToken ct = default)
        {
            var account = await _accountRepo.GetByIdAsync(accountId, ct);
            if (account == null)
                throw new InvalidOperationException($"Account {accountId} not found");

            var holding = account.Holdings.FirstOrDefault(h => h.Symbol.Equals(symbol));
            if (holding == null)
                return;

            holding.RemoveTag(tag);
            await _accountRepo.UpdateAsync(account, ct);
            await _accountRepo.SaveChangesAsync(ct);
        }
    }
}

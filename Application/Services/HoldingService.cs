using PM.Application.Interfaces;
using PM.Domain.Entities;
using PM.Domain.Values;

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

        public async Task AddHoldingAsync(int accountId, Symbol instrument, decimal quantity, CancellationToken ct = default)
        {
            var account = await _accountRepo.GetByIdAsync(accountId, ct);
            if (account == null) return;

            var holding = new Holding(instrument, quantity);
            account.AddHolding(holding);

            await _accountRepo.UpdateAsync(account, ct);
            await _accountRepo.SaveChangesAsync(ct);
        }

        public async Task RemoveHoldingAsync(int accountId, Symbol symbol, CancellationToken ct = default)
        {
            var account = await _accountRepo.GetByIdAsync(accountId, ct);
            if (account == null) return;

            var holding = account.Holdings.FirstOrDefault(h => h.Symbol.Equals(symbol));
            if (holding == null) return;

            account.RemoveHolding(holding);
            await _accountRepo.UpdateAsync(account, ct);
            await _accountRepo.SaveChangesAsync(ct);
        }

        public async Task<Holding?> GetHoldingAsync(int accountId, Symbol symbol, CancellationToken ct = default)
        {
            var holdings = await _holdingRepo.ListByAccountAsync(accountId, ct);
            return holdings.FirstOrDefault(h => h.Symbol.Equals(symbol));
        }

        public async Task UpdateHoldingQuantityAsync(Holding holding, decimal newQty, CancellationToken ct = default)
        {
            holding.Quantity = newQty;
            await _holdingRepo.UpdateAsync(holding, ct);
            await _holdingRepo.SaveChangesAsync(ct);
        }

        public async Task<decimal> GetCashBalanceAsync(int accountId, Currency currency, CancellationToken ct = default)
        {
            var symbol = new Symbol(currency.Code);
            var account = await _accountRepo.GetByIdAsync(accountId, ct);
            if (account == null) return 0.0m;
            var holding = account.Holdings.FirstOrDefault(h => h.Symbol == symbol);
            return holding?.Quantity ?? 0;
        }

        public async Task<IEnumerable<Holding>> ListHoldingsAsync(int accountId, CancellationToken ct = default)
        {
            return await _holdingRepo.ListByAccountAsync(accountId, ct);
        }

        public async Task AddTagAsync(int holdingId, Tag tag, CancellationToken ct = default)
        {
            var holding = await _holdingRepo.GetByIdAsync(holdingId, ct);
            if (holding == null) return;

            holding.AddTag(tag);
            await _holdingRepo.UpdateAsync(holding, ct);
            await _holdingRepo.SaveChangesAsync(ct);
        }

        public async Task RemoveTagAsync(int holdingId, Tag tag, CancellationToken ct = default)
        {
            var holding = await _holdingRepo.GetByIdAsync(holdingId, ct);
            if (holding == null) return;

            holding.RemoveTag(tag);
            await _holdingRepo.UpdateAsync(holding, ct);
            await _holdingRepo.SaveChangesAsync(ct);
        }
    }
}

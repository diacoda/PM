using PM.Application.Interfaces;
using PM.Domain.Entities;
using PM.Domain.Interfaces;
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

        public async Task<Holding> UpsertHoldingAsync(int accountId, IAsset asset, decimal quantity, CancellationToken ct = default)
        {
            var account = await _accountRepo.GetByIdWithIncludesAsync(accountId, includes: new IncludeOption[] { IncludeOption.Holdings }, ct);
            if (account == null)
                throw new InvalidOperationException($"Account {accountId} not found");

            Holding holding = account.UpsertHolding(new Holding(asset, quantity));
            await _accountRepo.UpdateAsync(account, ct);
            await _accountRepo.SaveChangesAsync(ct);
            return holding;
        }

        public async Task<Holding> UpdateHoldingQuantityAsync(int accountId, IAsset asset, decimal newQty, CancellationToken ct = default)
        {
            if (newQty < 0) throw new InvalidOperationException($"Quantity {newQty} must be positive");

            var account = await _accountRepo.GetByIdAsync(accountId, ct);
            if (account == null)
                throw new InvalidOperationException($"Account {accountId} not found");

            Holding holding = account.UpdateHoldingQuantity(asset, newQty);
            await _accountRepo.UpdateAsync(account, ct);
            await _accountRepo.SaveChangesAsync(ct);
            return holding;
        }

        public async Task RemoveHoldingAsync(int accountId, IAsset asset, CancellationToken ct = default)
        {
            var account = await _accountRepo.GetByIdAsync(accountId, ct);
            if (account == null)
                throw new InvalidOperationException($"Account {accountId} not found");

            var holding = account.Holdings.FirstOrDefault(h => h.Asset.Equals(asset));
            if (holding == null)
                return;

            account.RemoveHolding(holding);
            await _accountRepo.UpdateAsync(account, ct);
            await _accountRepo.SaveChangesAsync(ct);
        }

        public async Task<Holding?> GetHoldingAsync(int accountId, IAsset asset, CancellationToken ct = default)
        {
            var holdings = await _holdingRepo.ListByAccountAsync(accountId, ct);
            return holdings.FirstOrDefault(h => h.Asset.Equals(asset));
        }

        public async Task<decimal> GetCashBalanceAsync(int accountId, Currency currency, CancellationToken ct = default)
        {
            // TODO: we could use currency to fx on it but not used right now
            var asset = new Asset() { Code = currency.Code, Currency = currency, AssetClass = AssetClass.Cash };
            var account = await _accountRepo.GetByIdAsync(accountId, ct);
            if (account == null)
                return 0.0m;

            var holding = account.Holdings.FirstOrDefault(h => h.Asset.Equals(asset));
            return holding?.Quantity ?? 0;
        }

        public async Task<IEnumerable<Holding>> ListHoldingsAsync(int accountId, CancellationToken ct = default)
        {
            return await _holdingRepo.ListByAccountAsync(accountId, ct);
        }

        public async Task AddTagAsync(int accountId, IAsset asset, Tag tag, CancellationToken ct = default)
        {
            var account = await _accountRepo.GetByIdAsync(accountId, ct);
            if (account == null)
                throw new InvalidOperationException($"Account {accountId} not found");

            var holding = account.Holdings.FirstOrDefault(h => h.Asset.Equals(asset));
            if (holding == null)
                throw new InvalidOperationException($"Holding not found for {asset.Code}");

            holding.AddTag(tag);
            await _accountRepo.UpdateAsync(account, ct);
            await _accountRepo.SaveChangesAsync(ct);
        }

        public async Task RemoveTagAsync(int accountId, IAsset asset, Tag tag, CancellationToken ct = default)
        {
            var account = await _accountRepo.GetByIdAsync(accountId, ct);
            if (account == null)
                throw new InvalidOperationException($"Account {accountId} not found");

            var holding = account.Holdings.FirstOrDefault(h => h.Asset.Equals(asset));
            if (holding == null)
                return;

            holding.RemoveTag(tag);
            await _accountRepo.UpdateAsync(account, ct);
            await _accountRepo.SaveChangesAsync(ct);
        }
    }
}

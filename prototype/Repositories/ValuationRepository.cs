using model.Domain.Entities;
using model.Domain.Values;

namespace model.Repositories
{
    /// <summary>
    /// In-memory store for valuation snapshots.
    /// Supports totals and asset-class slices (records with AssetClass set).
    /// </summary>
    public class ValuationRepository
    {
        private readonly List<ValuationRecord> _records = new();

        // Basic CRUD-ish
        public void Save(ValuationRecord record) => _records.Add(record);

        public IEnumerable<ValuationRecord> GetAll() => _records;

        // Totals by entity
        public IEnumerable<ValuationRecord> GetByPortfolio(int portfolioId, ValuationPeriod period)
            => _records.Where(r => r.PortfolioId == portfolioId && r.Period == period);

        public IEnumerable<ValuationRecord> GetByAccount(int accountId, ValuationPeriod period)
            => _records.Where(r => r.AccountId == accountId && r.Period == period);

        // ---------------------------------------------------------------------
        // Asset-class slice getters (Portfolio)
        // ---------------------------------------------------------------------

        /// <summary>
        /// All asset-class snapshots for a portfolio in the given period and optional date range.
        /// Only returns records where AssetClass.HasValue == true.
        /// </summary>
        public IEnumerable<ValuationRecord> GetPortfolioAssetClassSnapshots(
            int portfolioId,
            ValuationPeriod period,
            DateTime? from = null,
            DateTime? to = null)
        {
            var q = _records.Where(r =>
                r.PortfolioId == portfolioId &&
                r.Period == period &&
                r.AssetClass.HasValue);

            if (from.HasValue) q = q.Where(r => r.Date.Date >= from.Value.Date);
            if (to.HasValue) q = q.Where(r => r.Date.Date <= to.Value.Date);

            return q.OrderBy(r => r.Date).ThenBy(r => r.AssetClass);
        }

        /// <summary>
        /// Asset-class snapshot set for a portfolio on a specific date (one record per asset class present that day).
        /// </summary>
        public IEnumerable<ValuationRecord> GetPortfolioAssetClassOnDate(
            int portfolioId,
            DateTime date,
            ValuationPeriod period)
        {
            return _records.Where(r =>
                    r.PortfolioId == portfolioId &&
                    r.Period == period &&
                    r.AssetClass.HasValue &&
                    r.Date.Date == date.Date)
                .OrderBy(r => r.AssetClass);
        }

        // ---------------------------------------------------------------------
        // Asset-class slice getters (Account)
        // ---------------------------------------------------------------------

        /// <summary>
        /// All asset-class snapshots for an account in the given period and optional date range.
        /// Only returns records where AssetClass.HasValue == true.
        /// </summary>
        public IEnumerable<ValuationRecord> GetAccountAssetClassSnapshots(
            int accountId,
            ValuationPeriod period,
            DateTime? from = null,
            DateTime? to = null)
        {
            var q = _records.Where(r =>
                r.AccountId == accountId &&
                r.Period == period &&
                r.AssetClass.HasValue);

            if (from.HasValue) q = q.Where(r => r.Date.Date >= from.Value.Date);
            if (to.HasValue) q = q.Where(r => r.Date.Date <= to.Value.Date);

            return q.OrderBy(r => r.Date).ThenBy(r => r.AssetClass);
        }

        /// <summary>
        /// Asset-class snapshot set for an account on a specific date (one record per asset class present that day).
        /// </summary>
        public IEnumerable<ValuationRecord> GetAccountAssetClassOnDate(
            int accountId,
            DateTime date,
            ValuationPeriod period)
        {
            return _records.Where(r =>
                    r.AccountId == accountId &&
                    r.Period == period &&
                    r.AssetClass.HasValue &&
                    r.Date.Date == date.Date)
                .OrderBy(r => r.AssetClass);
        }
    }
}
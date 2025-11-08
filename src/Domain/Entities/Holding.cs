using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PM.Domain.Interfaces;
using PM.Domain.Values;
using PM.SharedKernel;

namespace PM.Domain.Entities
{
    /// <summary>
    /// Represents a holding within an account, including the quantity and associated symbol.
    /// </summary>
    public class Holding : Entity
    {
        /// <summary>
        /// Parameterless constructor for EF Core.
        /// </summary>
        private Holding() { }

        /// <summary>
        /// Creates a new holding with a specified symbol and quantity.
        /// </summary>
        /// <param name="symbol">The symbol of the asset being held.</param>
        /// <param name="quantity">The quantity of the asset.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="symbol"/> is null.</exception>
        public Holding(IAsset asset, decimal quantity)
        {
            _asset = asset as Asset ?? new Asset
            {
                Code = asset.Code,
                Currency = asset.Currency,
                AssetClass = asset.AssetClass
            };
            Quantity = quantity;
        }
        private Asset _asset = default!;
        /// <summary>
        /// Exposes the asset as IAsset for domain use.
        /// </summary>
        [NotMapped]
        public IAsset Asset
        {
            get => _asset;
            set => _asset = value as Asset ?? new Asset
            {
                Code = value.Code,
                Currency = value.Currency,
                AssetClass = value.AssetClass
            };
        }

        /// <summary>
        /// Gets or sets the quantity of the asset held.
        /// </summary>
        public decimal Quantity { get; set; }

        /// <summary>
        /// Gets or sets the foreign key of the account that owns this holding.
        /// </summary>
        public int AccountId { get; set; }

        /// <summary>
        /// Navigation property to the owning account.
        /// </summary>
        public Account? Account { get; private set; }

        /// <summary>
        /// Gets or sets the collection of tags associated with this holding.
        /// </summary>
        public List<Tag> Tags { get; set; } = new();

        /// <summary>
        /// Adds a quantity to the current holding.
        /// </summary>
        /// <param name="qty">The quantity to add.</param>
        public void AddQuantity(decimal qty) => Quantity += qty;

        /// <summary>
        /// Updates the quantity of the holding to a new value.
        /// </summary>
        /// <param name="newQuantity">The new quantity to set.</param>
        public void UpdateQuantity(decimal newQuantity) => Quantity = newQuantity;

        /// <summary>
        /// Adds a tag to the holding if it does not already exist.
        /// </summary>
        /// <param name="tag">The tag to add.</param>
        public void AddTag(Tag tag)
        {
            if (!Tags.Contains(tag))
                Tags.Add(tag);
        }

        /// <summary>
        /// Removes a tag from the holding.
        /// </summary>
        /// <param name="tag">The tag to remove.</param>
        public void RemoveTag(Tag tag) => Tags.Remove(tag);


        public override bool Equals(object? obj)
        {
            if (obj is Holding other)
            {
                return Asset.Equals(other.Asset);
            }
            return false;
        }

        public override int GetHashCode() => Asset.GetHashCode();

    }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PM.SharedKernel.Events;

namespace PM.SharedKernel
{
    /// <summary>
    /// Represents the base class for all domain entities.
    /// </summary>
    /// <remarks>
    /// This class provides domain event support for derived entities. 
    /// Entities can raise domain events during state changes, which can later be 
    /// dispatched by an <c>IDomainEventDispatcher</c> or similar mechanism.
    /// </remarks>
    public abstract class Entity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; protected set; }

        /// <summary>
        /// Assigns a deterministic ID for testing. Internal to preserve DDD boundaries.
        /// </summary>
        protected internal void SetIdForTest(int id)
        {
            Id = id;
        }
    }
}

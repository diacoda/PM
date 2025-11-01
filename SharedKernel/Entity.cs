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
        private readonly List<IDomainEvent> _domainEvents = [];

        /// <summary>
        /// Gets a read-only copy of the domain events that have been raised by this entity.
        /// </summary>
        /// <remarks>
        /// Domain events represent significant business occurrences that other parts 
        /// of the system may react to asynchronously.
        /// </remarks>
        public List<IDomainEvent> DomainEvents => [.. _domainEvents];

        /// <summary>
        /// Clears all domain events that have been recorded for this entity.
        /// </summary>
        /// <remarks>
        /// This method is typically called after domain events have been dispatched.
        /// </remarks>
        public void ClearDomainEvents()
        {
            _domainEvents.Clear();
        }

        /// <summary>
        /// Raises a new domain event for this entity.
        /// </summary>
        /// <param name="domainEvent">The domain event to raise.</param>
        /// <remarks>
        /// The event is added to the internal collection of domain events. 
        /// It is not immediately published; a separate dispatcher is responsible 
        /// for propagating these events after the entity has been persisted.
        /// </remarks>
        public void Raise(IDomainEvent domainEvent)
        {
            _domainEvents.Add(domainEvent);
        }
    }
}

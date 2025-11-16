namespace PM.InMemoryEventBus;

public record Event<T>(T Data, EventMetadata? Metadata = null);
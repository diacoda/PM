namespace PM.SharedKernel.Events;

public record Event<T>(T? Data, EventMetadata? Metadata = default);
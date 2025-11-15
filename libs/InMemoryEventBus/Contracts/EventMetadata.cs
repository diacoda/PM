// from https://github.com/maranmaran/InMemChannelEventBus
// explained at: https://medium.com/@sociable_flamingo_goose_694/lightweight-net-channel-pub-sub-implementation-aed696337cc9
namespace PM.InMemoryEventBus.Contracts;

public record EventMetadata(string CorrelationId);
using FluentAssertions;
using Moq;
using Xunit;
namespace PM.SharedKernel.Events.Tests;

public class DomainEventDispatcherTests
{
    [Fact]
    public async Task DispatchEntityEventsAsync_Dispatches_All_Events()
    {
        // Arrange
        var publisher = new Mock<IDomainEventPublisher>();
        var dispatcher = new DomainEventDispatcher(publisher.Object);
        var entity = new TestEntity();

        var evt1 = new TestEvent("hello");
        var evt2 = new TestEvent("world");

        entity.AddEvent(evt1);
        entity.AddEvent(evt2);

        // Act
        await dispatcher.DispatchEntityEventsAsync(entity);

        // Assert
        publisher.Verify(p => p.PublishAsync(It.Is<IDomainEvent>(e => (TestEvent)e == evt1), It.IsAny<CancellationToken>()), Times.Once);
        publisher.Verify(p => p.PublishAsync(It.Is<IDomainEvent>(e => (TestEvent)e == evt2), It.IsAny<CancellationToken>()), Times.Once);

        Assert.Empty(entity.DomainEvents);
    }
}

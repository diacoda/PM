
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
namespace PM.SharedKernel.Events.Tests;

public class InMemoryDomainEventPublisherTests
{
    private InMemoryDomainEventPublisher CreatePublisher(out ServiceProvider provider)
    {
        var services = new ServiceCollection();
        // Register the handler as Singleton for the test so the same instance is used by publisher scopes
        services.AddSingleton<TestEventHandler>();
        services.AddSingleton<SecondTestEventHandler>();
        provider = services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateScopes = true
        });
        return new InMemoryDomainEventPublisher(provider.GetRequiredService<IServiceScopeFactory>());
    }


    [Fact]
    public async Task PublishAsync_Invokes_Handler()
    {
        // Arrange
        var publisher = CreatePublisher(out var provider);
        publisher.Register<TestEvent, TestEventHandler>();

        var evt = new TestEvent("hello");

        // Act
        await publisher.PublishAsync(evt);

        // Assert
        var handler = provider.GetRequiredService<TestEventHandler>();
        Assert.True(handler.WasCalled);
        Assert.Equal(evt, handler.ReceivedEvent);
    }

    [Fact]
    public async Task PublishAsync_NoHandlers_DoesNotThrow()
    {
        var publisher = CreatePublisher(out _);

        var evt = new TestEvent("unused");

        var exception = await Record.ExceptionAsync(() => publisher.PublishAsync(evt));

        Assert.Null(exception);
    }

    [Fact]
    public async Task PublishAsync_Invokes_All_Handlers()
    {
        // Arrange DI
        var services = new ServiceCollection();
        services.AddSingleton<TestEventHandler>();
        services.AddSingleton<SecondTestEventHandler>();

        var provider = services.BuildServiceProvider();
        var publisher = new InMemoryDomainEventPublisher(
            provider.GetRequiredService<IServiceScopeFactory>());

        publisher.Register<TestEvent, TestEventHandler>();
        publisher.Register<TestEvent, SecondTestEventHandler>();

        // 🔥 Capture handler instances BEFORE publishing
        var h1 = provider.GetRequiredService<TestEventHandler>();
        var h2 = provider.GetRequiredService<SecondTestEventHandler>();

        // Act
        await publisher.PublishAsync(new TestEvent("multi"));

        // Assert
        Assert.True(h1.WasCalled);
        Assert.True(h2.WasCalled);
    }


    public class ThrowingHandler : IDomainEventHandler<TestEvent>
    {
        public Task Handle(TestEvent domainEvent, CancellationToken ct)
            => throw new InvalidOperationException("fail");
    }

    [Fact]
    public async Task PublishAsync_HandlerThrows_OtherHandlersStillRun()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ThrowingHandler>();
        services.AddSingleton<TestEventHandler>();

        var provider = services.BuildServiceProvider();
        var publisher = new InMemoryDomainEventPublisher(
            provider.GetRequiredService<IServiceScopeFactory>());

        publisher.Register<TestEvent, ThrowingHandler>();
        publisher.Register<TestEvent, TestEventHandler>();

        var evt = new TestEvent("test");

        // Capture instance BEFORE publishing
        var goodHandler = provider.GetRequiredService<TestEventHandler>();

        // Act
        var ex = await Record.ExceptionAsync(() => publisher.PublishAsync(evt));

        // Assert
        Assert.Null(ex); // publisher must swallow handler errors
        Assert.True(goodHandler.WasCalled);
    }
}

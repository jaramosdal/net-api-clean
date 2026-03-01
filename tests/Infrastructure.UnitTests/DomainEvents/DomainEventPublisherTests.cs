using Application.Abstractions.Events;
using Domain;
using Infrastructure.Events;
using Infrastructure.UnitTests.Fakes;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace Infrastructure.UnitTests.DomainEvents;

public class DomainEventPublisherTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IDomainEventHandler<FakeDomainEvent> _handler;
    private readonly DomainEventPublisher _publisher;

    public DomainEventPublisherTests()
    {
        _handler = Substitute.For<IDomainEventHandler<FakeDomainEvent>>();
        _handler
            .Handle(Arg.Any<FakeDomainEvent>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton(_handler);

        _serviceProvider = serviceCollection.BuildServiceProvider();
        _publisher = new DomainEventPublisher(_serviceProvider);
    }

    [Fact]
    public async Task PublishAsync_ShouldInvokeRegisteredHandler()
    {
        // Arrange
        var domainEvent = new FakeDomainEvent();

        // Act
        await _publisher.PublishAsync(domainEvent);

        // Assert
        await _handler.Received(1).Handle(domainEvent, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task PublishAsync_ShouldPassCancellationTokenToHandler()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        var domainEvent = new FakeDomainEvent();

        // Act
        await _publisher.PublishAsync(domainEvent, cts.Token);

        // Assert
        await _handler.Received(1).Handle(domainEvent, cts.Token);
    }

    [Fact]
    public async Task PublishAsync_WhenHandlerThrows_ShouldPropagateException()
    {
        // Arrange
        var domainEvent = new FakeDomainEvent();
        _handler
            .Handle(Arg.Any<FakeDomainEvent>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new InvalidOperationException("Handler failed")));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _publisher.PublishAsync(domainEvent));
    }

    [Fact]
    public async Task PublishAsync_ShouldInvokeAllRegisteredHandlers()
    {
        // Arrange
        var secondHandler = Substitute.For<IDomainEventHandler<FakeDomainEvent>>();
        secondHandler
            .Handle(Arg.Any<FakeDomainEvent>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton(_handler);
        serviceCollection.AddSingleton(secondHandler);

        var provider = serviceCollection.BuildServiceProvider();
        var publisher = new DomainEventPublisher(provider);

        var domainEvent = new FakeDomainEvent();

        // Act
        await publisher.PublishAsync(domainEvent);

        // Assert
        await _handler.Received(1).Handle(domainEvent, Arg.Any<CancellationToken>());
        await secondHandler.Received(1).Handle(domainEvent, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task PublishAsync_WhenNoHandlersRegistered_ShouldNotThrow()
    {
        // Arrange
        var emptyProvider = new ServiceCollection().BuildServiceProvider();
        var publisher = new DomainEventPublisher(emptyProvider);
        var domainEvent = new FakeDomainEvent();

        // Act & Assert — should complete without exception
        await publisher.PublishAsync(domainEvent);
    }
}

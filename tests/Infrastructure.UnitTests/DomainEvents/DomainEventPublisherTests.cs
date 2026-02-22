using Application.Abstractions.Events;
using Domain.DomainEvents;
using Infrastructure.Services;
using Infrastructure.UnitTests.Fakes;
using NSubstitute;
using Soliss.NuGetRepo.Mediator;

namespace Infrastructure.UnitTests.DomainEvents;

public class DomainEventPublisherTests
{
    private readonly IMediator _mediator;
    private readonly DomainEventPublisher _publisher;

    public DomainEventPublisherTests()
    {
        _mediator = Substitute.For<IMediator>();
        _mediator
            .Publish(Arg.Any<INotification>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        _publisher = new DomainEventPublisher(_mediator);
    }

    [Fact]
    public async Task PublishAsync_ShouldWrapEventInDomainEventNotification()
    {
        // Arrange
        var domainEvent = new FakeDomainEvent();

        // Act
        await _publisher.PublishAsync(domainEvent);

        // Assert
        await _mediator.Received(1).Publish(
            Arg.Is<DomainEventNotification<FakeDomainEvent>>(n => n.Event == domainEvent),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task PublishAsync_ShouldPassCancellationTokenToMediator()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        var domainEvent = new FakeDomainEvent();

        // Act
        await _publisher.PublishAsync(domainEvent, cts.Token);

        // Assert
        await _mediator.Received(1).Publish(
            Arg.Any<DomainEventNotification<FakeDomainEvent>>(),
            cts.Token);
    }

    [Fact]
    public async Task PublishAsync_WhenMediatorThrows_ShouldPropagateException()
    {
        // Arrange
        var domainEvent = new FakeDomainEvent();
        _mediator
            .Publish(Arg.Any<INotification>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new InvalidOperationException("Mediator failed")));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _publisher.PublishAsync(domainEvent));
    }

    [Fact]
    public async Task PublishAsync_ShouldPublishNotificationOfCorrectGenericType()
    {
        // Arrange
        var domainEvent = new FakeDomainEvent();

        // Act
        await _publisher.PublishAsync(domainEvent);

        // Assert — Verify the notification is specifically DomainEventNotification<FakeDomainEvent>
        await _mediator.Received(1).Publish(
            Arg.Is<INotification>(n => n is DomainEventNotification<FakeDomainEvent>),
            Arg.Any<CancellationToken>());
    }
}

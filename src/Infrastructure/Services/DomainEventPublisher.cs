using Application.Abstractions.Events;
using Domain.DomainEvents;
using Soliss.NuGetRepo.Mediator;

namespace Infrastructure.Services;

public sealed class DomainEventPublisher(IMediator mediator) : IDomainEventPublisher
{
    public Task PublishAsync<TDomainEvent>(TDomainEvent domainEvent, CancellationToken cancellationToken = default) 
        where TDomainEvent : IDomainEvent
    {
        var notification = new DomainEventNotification<TDomainEvent>(domainEvent);
        return mediator.Publish(notification, cancellationToken);
    }
}

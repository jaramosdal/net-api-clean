using Domain.DomainEvents;
using Soliss.NuGetRepo.Mediator;

namespace Application.Abstractions.Events;

/// <summary>
/// Handler para eventos de dominio. Implementa INotificationHandler trabajando con el wrapper.
/// </summary>
public interface IDomainEventHandler<TDomainEvent> : INotificationHandler<DomainEventNotification<TDomainEvent>>
    where TDomainEvent : IDomainEvent
{
    // Método conveniente para trabajar con el evento directamente
    Task Handle(TDomainEvent domainEvent, CancellationToken cancellationToken);
    
    // Implementación del INotificationHandler que desempaqueta el wrapper
    async Task INotificationHandler<DomainEventNotification<TDomainEvent>>.Handle(
        DomainEventNotification<TDomainEvent> notification, 
        CancellationToken cancellationToken)
    {
        await Handle(notification.Event, cancellationToken);
    }
}

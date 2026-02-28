namespace Application.Abstractions.Events;

/// <summary>
/// Handler para eventos de dominio.
/// </summary>
public interface IDomainEventHandler<TDomainEvent> 
    where TDomainEvent : IDomainEvent
{
    Task Handle(TDomainEvent domainEvent, CancellationToken cancellationToken);
}

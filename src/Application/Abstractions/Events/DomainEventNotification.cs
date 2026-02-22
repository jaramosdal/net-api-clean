using Domain.DomainEvents;
using Soliss.NuGetRepo.Mediator;

namespace Application.Abstractions.Events;

/// <summary>
/// Wrapper que convierte un IDomainEvent en INotification para el Mediator.
/// </summary>
public sealed record DomainEventNotification<TDomainEvent>(TDomainEvent Event) : INotification
    where TDomainEvent : IDomainEvent;

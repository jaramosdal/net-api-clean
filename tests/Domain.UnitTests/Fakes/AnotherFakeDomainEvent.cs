using Domain.DomainEvents;

namespace Domain.UnitTests.Fakes;

internal sealed class AnotherFakeDomainEvent(string description) : IDomainEvent
{
    public string Description { get; } = description;
}

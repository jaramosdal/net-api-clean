using Domain.DomainEvents;

namespace Domain.UnitTests.Fakes;

/// <summary>
/// Entidad concreta de prueba que expone <see cref="Entity{TId}.RaiseDomainEvent"/>
/// para poder invocarlo desde los tests.
/// </summary>
internal sealed class FakeEntity : Entity<int>
{
    public void SimulateRaiseDomainEvent(IDomainEvent domainEvent) =>
        RaiseDomainEvent(domainEvent);
}

using Domain;

namespace Infrastructure.UnitTests.Fakes;

internal sealed class FakeEntity : Entity<int>
{
    public string Name { get; set; } = string.Empty;

    public void SimulateRaiseDomainEvent(IDomainEvent domainEvent) =>
        RaiseDomainEvent(domainEvent);
}

using Domain.UnitTests.Fakes;

namespace Domain.UnitTests.DomainEvents;

public class HasDomainEventsTests
{
    [Fact]
    public void Entity_ShouldImplementIHasDomainEvents()
    {
        // Arrange & Act
        var entity = new FakeEntity();

        // Assert
        Assert.IsAssignableFrom<IHasDomainEvents>(entity);
    }

    [Fact]
    public void IHasDomainEvents_DomainEvents_ShouldExposeRaisedEvents()
    {
        // Arrange
        var entity = new FakeEntity();
        var domainEvent = new FakeDomainEvent();
        entity.SimulateRaiseDomainEvent(domainEvent);

        // Act
        IHasDomainEvents hasEvents = entity;

        // Assert
        Assert.Single(hasEvents.DomainEvents);
        Assert.Contains(domainEvent, hasEvents.DomainEvents);
    }

    [Fact]
    public void IHasDomainEvents_ClearDomainEvents_ShouldRemoveAllEvents()
    {
        // Arrange
        var entity = new FakeEntity();
        entity.SimulateRaiseDomainEvent(new FakeDomainEvent());
        entity.SimulateRaiseDomainEvent(new AnotherFakeDomainEvent("test"));
        IHasDomainEvents hasEvents = entity;

        // Act
        hasEvents.ClearDomainEvents();

        // Assert
        Assert.Empty(entity.DomainEvents);
    }
}

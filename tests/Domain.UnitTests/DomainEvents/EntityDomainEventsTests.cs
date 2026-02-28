using Domain.UnitTests.Fakes;

namespace Domain.UnitTests.DomainEvents;

public class EntityDomainEventsTests
{
    [Fact]
    public void DomainEvents_WhenNewEntity_ShouldBeEmpty()
    {
        // Arrange & Act
        var entity = new FakeEntity();

        // Assert
        Assert.Empty(entity.DomainEvents);
    }

    [Fact]
    public void RaiseDomainEvent_WhenSingleEvent_ShouldContainOneEvent()
    {
        // Arrange
        var entity = new FakeEntity();
        var domainEvent = new FakeDomainEvent();

        // Act
        entity.SimulateRaiseDomainEvent(domainEvent);

        // Assert
        Assert.Single(entity.DomainEvents);
        Assert.Contains(domainEvent, entity.DomainEvents);
    }

    [Fact]
    public void RaiseDomainEvent_WhenMultipleEvents_ShouldContainAllEventsInOrder()
    {
        // Arrange
        var entity = new FakeEntity();
        var firstEvent = new FakeDomainEvent();
        var secondEvent = new AnotherFakeDomainEvent("test");
        var thirdEvent = new FakeDomainEvent();

        // Act
        entity.SimulateRaiseDomainEvent(firstEvent);
        entity.SimulateRaiseDomainEvent(secondEvent);
        entity.SimulateRaiseDomainEvent(thirdEvent);

        // Assert
        Assert.Equal(3, entity.DomainEvents.Count);
        Assert.Equal(firstEvent, entity.DomainEvents.ElementAt(0));
        Assert.Equal(secondEvent, entity.DomainEvents.ElementAt(1));
        Assert.Equal(thirdEvent, entity.DomainEvents.ElementAt(2));
    }

    [Fact]
    public void RaiseDomainEvent_WhenMultipleEventsOfDifferentTypes_ShouldContainMixedTypes()
    {
        // Arrange
        var entity = new FakeEntity();
        var fakeEvent = new FakeDomainEvent();
        var anotherEvent = new AnotherFakeDomainEvent("description");

        // Act
        entity.SimulateRaiseDomainEvent(fakeEvent);
        entity.SimulateRaiseDomainEvent(anotherEvent);

        // Assert
        Assert.Equal(2, entity.DomainEvents.Count);
        Assert.Contains(entity.DomainEvents, e => e is FakeDomainEvent);
        Assert.Contains(entity.DomainEvents, e => e is AnotherFakeDomainEvent);
    }

    [Fact]
    public void ClearDomainEvents_WhenEventsExist_ShouldRemoveAllEvents()
    {
        // Arrange
        var entity = new FakeEntity();
        entity.SimulateRaiseDomainEvent(new FakeDomainEvent());
        entity.SimulateRaiseDomainEvent(new AnotherFakeDomainEvent("test"));

        // Act
        entity.ClearDomainEvents();

        // Assert
        Assert.Empty(entity.DomainEvents);
    }

    [Fact]
    public void ClearDomainEvents_WhenNoEvents_ShouldRemainEmpty()
    {
        // Arrange
        var entity = new FakeEntity();

        // Act
        entity.ClearDomainEvents();

        // Assert
        Assert.Empty(entity.DomainEvents);
    }

    [Fact]
    public void RaiseDomainEvent_AfterClear_ShouldOnlyContainNewEvents()
    {
        // Arrange
        var entity = new FakeEntity();
        entity.SimulateRaiseDomainEvent(new FakeDomainEvent());
        entity.ClearDomainEvents();
        var newEvent = new AnotherFakeDomainEvent("after-clear");

        // Act
        entity.SimulateRaiseDomainEvent(newEvent);

        // Assert
        Assert.Single(entity.DomainEvents);
        Assert.Same(newEvent, entity.DomainEvents.First());
    }

    [Fact]
    public void DomainEvents_ShouldReturnReadOnlyCollection()
    {
        // Arrange
        var entity = new FakeEntity();
        entity.SimulateRaiseDomainEvent(new FakeDomainEvent());

        // Act
        var events = entity.DomainEvents;

        // Assert
        Assert.IsAssignableFrom<IReadOnlyCollection<Domain.IDomainEvent>>(events);
    }

    [Fact]
    public void DomainEvents_WhenModifiedExternally_ShouldNotAffectInternalState()
    {
        // Arrange
        var entity = new FakeEntity();
        entity.SimulateRaiseDomainEvent(new FakeDomainEvent());
        var eventsList = entity.DomainEvents.ToList();

        // Act
        eventsList.Clear();

        // Assert
        Assert.Single(entity.DomainEvents);
    }
}

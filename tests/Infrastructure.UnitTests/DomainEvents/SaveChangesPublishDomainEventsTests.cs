using Application.Abstractions.Events;
using Domain;
using Infrastructure.Database;
using Infrastructure.UnitTests.Fakes;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace Infrastructure.UnitTests.DomainEvents;

public class SaveChangesPublishDomainEventsTests : IDisposable
{
    private readonly IDomainEventPublisher _publisher;
    private readonly TestDbContext _dbContext;

    public SaveChangesPublishDomainEventsTests()
    {
        _publisher = Substitute.For<IDomainEventPublisher>();

        // Configurar el mock para que retorne Task.CompletedTask por defecto
        _publisher
            .PublishAsync(Arg.Any<IDomainEvent>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _dbContext = new TestDbContext(options, _publisher);
    }

    [Fact]
    public async Task SaveChangesAsync_WhenEntityHasEvents_ShouldPublishAllEvents()
    {
        // Arrange
        var entity = new FakeEntity { Id = 1, Name = "Test" };
        var firstEvent = new FakeDomainEvent();
        var secondEvent = new FakeDomainEvent();
        entity.SimulateRaiseDomainEvent(firstEvent);
        entity.SimulateRaiseDomainEvent(secondEvent);

        _dbContext.FakeEntities.Add(entity);

        // Act
        await _dbContext.SaveChangesAsync();

        // Assert
        await _publisher.Received(2).PublishAsync(
            Arg.Any<IDomainEvent>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SaveChangesAsync_WhenEntityHasEvents_ShouldPublishEachEventInOrder()
    {
        // Arrange
        var entity = new FakeEntity { Id = 1, Name = "Test" };
        var firstEvent = new FakeDomainEvent();
        var secondEvent = new FakeDomainEvent();
        entity.SimulateRaiseDomainEvent(firstEvent);
        entity.SimulateRaiseDomainEvent(secondEvent);

        _dbContext.FakeEntities.Add(entity);

        var publishedEvents = new List<IDomainEvent>();
        _publisher
            .PublishAsync(Arg.Any<IDomainEvent>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask)
            .AndDoes(call => publishedEvents.Add(call.Arg<IDomainEvent>()));

        // Act
        await _dbContext.SaveChangesAsync();

        // Assert
        Assert.Equal(2, publishedEvents.Count);
        Assert.Same(firstEvent, publishedEvents[0]);
        Assert.Same(secondEvent, publishedEvents[1]);
    }

    [Fact]
    public async Task SaveChangesAsync_WhenEntityHasEvents_ShouldClearEventsAfterPublishing()
    {
        // Arrange
        var entity = new FakeEntity { Id = 1, Name = "Test" };
        entity.SimulateRaiseDomainEvent(new FakeDomainEvent());

        _dbContext.FakeEntities.Add(entity);

        // Act
        await _dbContext.SaveChangesAsync();

        // Assert
        Assert.Empty(entity.DomainEvents);
    }

    [Fact]
    public async Task SaveChangesAsync_WhenNoEvents_ShouldNotPublish()
    {
        // Arrange
        var entity = new FakeEntity { Id = 1, Name = "Test" };
        _dbContext.FakeEntities.Add(entity);

        // Act
        await _dbContext.SaveChangesAsync();

        // Assert
        await _publisher.DidNotReceive().PublishAsync(
            Arg.Any<IDomainEvent>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SaveChangesAsync_WhenMultipleEntitiesWithEvents_ShouldPublishAllEvents()
    {
        // Arrange
        var entity1 = new FakeEntity { Id = 1, Name = "First" };
        var entity2 = new FakeEntity { Id = 2, Name = "Second" };
        entity1.SimulateRaiseDomainEvent(new FakeDomainEvent());
        entity2.SimulateRaiseDomainEvent(new FakeDomainEvent());
        entity2.SimulateRaiseDomainEvent(new FakeDomainEvent());

        _dbContext.FakeEntities.AddRange(entity1, entity2);

        // Act
        await _dbContext.SaveChangesAsync();

        // Assert
        await _publisher.Received(3).PublishAsync(
            Arg.Any<IDomainEvent>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SaveChangesAsync_WhenMultipleEntities_ShouldClearEventsFromAll()
    {
        // Arrange
        var entity1 = new FakeEntity { Id = 1, Name = "First" };
        var entity2 = new FakeEntity { Id = 2, Name = "Second" };
        entity1.SimulateRaiseDomainEvent(new FakeDomainEvent());
        entity2.SimulateRaiseDomainEvent(new FakeDomainEvent());

        _dbContext.FakeEntities.AddRange(entity1, entity2);

        // Act
        await _dbContext.SaveChangesAsync();

        // Assert
        Assert.Empty(entity1.DomainEvents);
        Assert.Empty(entity2.DomainEvents);
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldPersistChangesBeforePublishing()
    {
        // Arrange
        var entity = new FakeEntity { Id = 1, Name = "Test" };
        entity.SimulateRaiseDomainEvent(new FakeDomainEvent());

        var savedBeforePublish = false;
        _publisher
            .PublishAsync(Arg.Any<IDomainEvent>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                // Al momento de publicar, la entidad ya debería estar persistida
                savedBeforePublish = _dbContext.FakeEntities.Any(e => e.Id == 1);
                return Task.CompletedTask;
            });

        _dbContext.FakeEntities.Add(entity);

        // Act
        await _dbContext.SaveChangesAsync();

        // Assert
        Assert.True(savedBeforePublish);
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldReturnNumberOfStateEntries()
    {
        // Arrange
        var entity = new FakeEntity { Id = 1, Name = "Test" };
        entity.SimulateRaiseDomainEvent(new FakeDomainEvent());

        _dbContext.FakeEntities.Add(entity);

        // Act
        var result = await _dbContext.SaveChangesAsync();

        // Assert
        Assert.Equal(1, result);
    }

    [Fact]
    public async Task SaveChangesAsync_WhenCalledTwice_ShouldNotRepublishClearedEvents()
    {
        // Arrange
        var entity = new FakeEntity { Id = 1, Name = "Test" };
        entity.SimulateRaiseDomainEvent(new FakeDomainEvent());
        _dbContext.FakeEntities.Add(entity);

        // Act
        await _dbContext.SaveChangesAsync();

        // Modificar entidad para generar un nuevo cambio sin nuevos eventos
        entity.Name = "Updated";
        await _dbContext.SaveChangesAsync();

        // Assert — solo 1 publicación, la segunda llamada no re-publica
        await _publisher.Received(1).PublishAsync(
            Arg.Any<IDomainEvent>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldPassCancellationTokenToPublisher()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        var entity = new FakeEntity { Id = 1, Name = "Test" };
        entity.SimulateRaiseDomainEvent(new FakeDomainEvent());
        _dbContext.FakeEntities.Add(entity);

        // Act
        await _dbContext.SaveChangesAsync(cts.Token);

        // Assert
        await _publisher.Received(1).PublishAsync(
            Arg.Any<IDomainEvent>(), cts.Token);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}

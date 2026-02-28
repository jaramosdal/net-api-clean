using Application.Abstractions.Data;
using Application.Abstractions.Events;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Database;

public class ApplicationDbContext(
    DbContextOptions<ApplicationDbContext> options,
    IDomainEventPublisher domainEventPublisher) : DbContext(options), IApplicationDbContext
{
    private readonly IDomainEventPublisher _domainEventPublisher = domainEventPublisher;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        modelBuilder.HasDefaultSchema(Schemas.Default);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        List<IHasDomainEvents> entitiesWithEvents = ChangeTracker
            .Entries<IHasDomainEvents>()
            .Where(e => e.Entity.DomainEvents.Count != 0)
            .Select(e => e.Entity)
            .ToList();

        // Extraer y limpiar eventos ANTES de publicar (evita reprocesamiento)
        List<IDomainEvent> allEvents = entitiesWithEvents
            .SelectMany(e => e.DomainEvents)
            .ToList();

        foreach (IHasDomainEvents entity in entitiesWithEvents)
        {
            entity.ClearDomainEvents();
        }

        int result = await base.SaveChangesAsync(cancellationToken);

        // Publicar eventos solo tras persistencia exitosa
        foreach (IDomainEvent domainEvent in allEvents)
        {
            await _domainEventPublisher.PublishAsync(domainEvent, cancellationToken);
        }

        return result;
    }
}

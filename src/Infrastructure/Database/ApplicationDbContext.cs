using Application.Abstractions.Data;
using Application.Abstractions.Events;
using Domain;
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
        // Recolectar todas las entidades con eventos de dominio antes de guardar
        var entitiesWithEvents = ChangeTracker
            .Entries<IHasDomainEvents>()
            .Where(e => e.Entity.DomainEvents.Count != 0)
            .Select(e => e.Entity)
            .ToList();

        // Guardar los cambios en la base de datos primero
        var result = await base.SaveChangesAsync(cancellationToken);

        // Publicar eventos solo después de que la transacción haya tenido éxito
        foreach (var entity in entitiesWithEvents)
        {
            var events = entity.DomainEvents.ToList();
            
            foreach (var domainEvent in events)
            {
                await _domainEventPublisher.PublishAsync(domainEvent, cancellationToken);
            }

            // Limpiar los eventos después de publicarlos
            entity.ClearDomainEvents();
        }

        return result;
    }
}

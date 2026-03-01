using Infrastructure.Database;
using Infrastructure.Events;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.UnitTests.Fakes;

/// <summary>
/// DbContext de test que hereda de <see cref="ApplicationDbContext"/> para reutilizar
/// la lógica de publicación de eventos de dominio en <c>SaveChangesAsync</c>,
/// y registra <see cref="FakeEntity"/> como entidad trackeada.
/// </summary>
internal sealed class TestDbContext : ApplicationDbContext
{
    public DbSet<FakeEntity> FakeEntities => Set<FakeEntity>();

    public TestDbContext(DbContextOptions<ApplicationDbContext> options, IDomainEventPublisher domainEventPublisher)
        : base(options, domainEventPublisher)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<FakeEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name);
        });
    }
}

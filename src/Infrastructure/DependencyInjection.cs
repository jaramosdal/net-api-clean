using Application.Abstractions.Data;
using Application.Abstractions.Events;
using Infrastructure.Database;
using Infrastructure.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        string connectionString = configuration.GetConnectionString("Database") ?? throw new InvalidOperationException("La cadena de conexión 'Database' no está configurada.");

        // Registrar el publisher de eventos de dominio
        services.AddScoped<IDomainEventPublisher, DomainEventPublisher>();

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseSqlServer(connectionString, sqlOptions => 
                sqlOptions.EnableRetryOnFailure(maxRetryCount: 3));
        });
        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());
        return services;
    }
}

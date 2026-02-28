using Application.Behaviors;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Soliss.NuGetRepo.Mediator;

namespace Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediator(typeof(DependencyInjection).Assembly);
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly, includeInternalTypes: true);

        // Registrar behavior de validación para comandos y queries (open-generic)
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        return services;
    }
}

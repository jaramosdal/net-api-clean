using Microsoft.Extensions.DependencyInjection;
using Soliss.NuGetRepo.Mediator;

namespace Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediator(typeof(DependencyInjection).Assembly);

        return services;
    }
}

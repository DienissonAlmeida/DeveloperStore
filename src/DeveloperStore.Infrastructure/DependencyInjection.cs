using DeveloperStore.Application.Common.Interfaces;
using DeveloperStore.Domain.Interfaces;
using DeveloperStore.Infrastructure.Identity;
using DeveloperStore.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace DeveloperStore.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUserService>();
        services.AddSingleton<ISaleRepository, SaleRepository>();

        return services;
    }
}

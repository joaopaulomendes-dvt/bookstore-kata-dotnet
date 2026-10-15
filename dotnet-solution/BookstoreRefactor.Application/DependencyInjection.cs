using Microsoft.Extensions.DependencyInjection;

namespace BookstoreRefactor.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddScoped<BookApplicationService>();
        services.AddScoped<CategoryApplicationService>();

        return services;
    }
}

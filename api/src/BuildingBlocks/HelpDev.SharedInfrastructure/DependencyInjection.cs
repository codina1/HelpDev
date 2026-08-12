using HelpDev.SharedApplication.Abstractions.Events;
using HelpDev.SharedInfrastructure.Events;
using HelpDev.SharedInfrastructure.Outbox;
using HelpDev.SharedInfrastructure.Time;
using HelpDev.SharedKernel.Time;
using Microsoft.Extensions.DependencyInjection;

namespace HelpDev.SharedInfrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddSharedInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
        return services;
    }
}

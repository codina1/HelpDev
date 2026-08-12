using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace HelpDev.SharedContracts.Modules;

/// <summary>
/// Public registration contract for a modular monolith module.
/// Runtime discovery is intentionally not implemented yet.
/// </summary>
public interface IModule
{
    string Name { get; }

    IServiceCollection RegisterServices(IServiceCollection services);

    IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder endpoints);
}

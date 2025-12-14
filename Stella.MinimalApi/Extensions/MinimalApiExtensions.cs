using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Stella.MinimalApi.Extensions;

/// <summary>
/// Runtime (reflection-based) endpoint registration and mapping helpers.
///
/// These extensions provide a convenient fallback for discovering and mapping
/// <see cref="IEndpoint"/> implementations using reflection. They are suitable
/// for development, prototyping, and non-AOT builds.
///
/// IMPORTANT:
/// - This implementation relies on runtime type scanning and reflection.
/// - It should NOT be used in NativeAOT or aggressively trimmed environments.
/// - For AOT-safe builds, prefer the source-generator-based extensions which
///   perform the same work at compile time without reflection.
/// </summary>
public static class MinimalApiExtensions
{
    /// <summary>
    /// Registers all <see cref="IEndpoint"/> implementations found in the given assembly.
    /// </summary>
    /// <remarks>
    /// This method scans the provided <paramref name="assembly"/> at runtime to locate
    /// concrete types implementing <see cref="IEndpoint"/> and registers them as
    /// transient services.
    ///
    /// Because this method uses reflection and runtime type discovery, it is not
    /// compatible with NativeAOT or trimming scenarios. It is intended as a
    /// reflection-based fallback when source generators are not used.
    /// </remarks>
    public static IServiceCollection AddEndpoints(
        this IServiceCollection services,
        IEndpointDiscovery discovery)
    {
        discovery.Register(services);
        return services;
    }

    /// <summary>
    /// Maps all registered <see cref="IEndpoint"/> instances to the application.
    /// </summary>
    /// <remarks>
    /// This method resolves all registered <see cref="IEndpoint"/> services and
    /// maps them to the application using runtime reflection to determine their
    /// route groups via <see cref="EndpointGroupAttribute"/>.
    ///
    /// Route groups are created lazily and cached so that each distinct route
    /// prefix is mapped only once.
    ///
    /// This implementation is reflection-based and should not be used for
    /// NativeAOT targets. Source-generator-based mapping should be preferred
    /// for production and AOT scenarios.
    /// </remarks>
    public static IApplicationBuilder MapEndpoints(this WebApplication app)
    {
        var endpoints = app.Services.GetRequiredService<IEnumerable<IEndpoint>>();

        // Cache groups so we only create each RouteGroupBuilder once.
        // This avoids duplicate group creation and ensures shared conventions
        // (authorization, filters, metadata) are applied consistently.
        var groups = new Dictionary<string, RouteGroupBuilder>(
            StringComparer.OrdinalIgnoreCase);

        foreach (var endpoint in endpoints)
        {
            var endpointType = endpoint.GetType();
            var endpointGroups = endpointType.GetEndpointGroups();

            // No group attribute -> map directly to the application root.
            if (endpointGroups.Count == 0)
            {
                endpoint.MapEndpoint(app);
                continue;
            }

            // Map the endpoint into each declared route group.
            foreach (var route in endpointGroups)
            {
                if (!groups.TryGetValue(route, out var group))
                {
                    group = app.MapGroup(route);
                    groups[route] = group;
                }

                endpoint.MapEndpoint(group);
            }
        }

        return app;
    }
}

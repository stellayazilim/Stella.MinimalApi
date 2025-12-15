
# Inline Usage (Single-File Integration)

This document describes the **inline / single-file usage** of **Stella.MinimalApi**.


```cs
// Stella.MinimalApi.Inline.cs
//
// Inline / single-file implementation of Stella.MinimalApi.
//
// This version provides the full *runtime* discovery and mapping model,
// including endpoint grouping and hybrid discovery strategies.
// It intentionally omits source generators and analyzers.
//
// Not suitable for NativeAOT. Use the NuGet packages for compile-time discovery.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Stella.MinimalApi;

/// <summary>
/// Marks an endpoint as belonging to one or more route groups.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class EndpointGroupAttribute(string route) : Attribute
{
    public string Route { get; } = route;
}

/// <summary>
/// Defines a discovery strategy for registering endpoints.
/// </summary>
public interface IEndpointDiscovery
{
    void Register(IServiceCollection services);
}

/// <summary>
/// Reflection-based endpoint discovery using assembly scanning.
/// </summary>
public sealed class AssemblyEndpointDiscovery : IEndpointDiscovery
{
    private readonly Assembly _assembly;

    public AssemblyEndpointDiscovery(Assembly assembly)
    {
        _assembly = assembly;
    }

    public void Register(IServiceCollection services)
    {
        var endpoints =
            _assembly
                .GetTypes()
                .Where(t =>
                    !t.IsAbstract &&
                    typeof(IEndpoint).IsAssignableFrom(t));

        foreach (var endpointType in endpoints)
        {
            services.AddSingleton(typeof(IEndpoint), endpointType);
        }
    }
}

/// <summary>
/// Represents a self-contained Minimal API endpoint.
/// </summary>
public interface IEndpoint
{
    void MapEndpoint(IEndpointRouteBuilder app);
}

public static class MinimalApiExtensions
{
    /// <summary>
    /// Registers endpoints using the provided discovery strategy.
    /// </summary>
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
    public static WebApplication MapEndpoints(this WebApplication app)
    {
        var endpoints = app.Services.GetRequiredService<IEnumerable<IEndpoint>>();

        // Cache RouteGroupBuilder instances so each group is created once.
        var groups = new Dictionary<string, RouteGroupBuilder>(
            StringComparer.OrdinalIgnoreCase);

        foreach (var endpoint in endpoints)
        {
            var endpointType = endpoint.GetType();
            var endpointGroups = endpointType.GetEndpointGroups();

            // No groups -> map directly to root.
            if (endpointGroups.Count == 0)
            {
                endpoint.MapEndpoint(app);
                continue;
            }

            // Map endpoint into each declared group.
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

    /// <summary>
    /// Gets all route groups declared on an endpoint type.
    /// </summary>
    /// <remarks>
    /// This represents the reference runtime behavior that
    /// source generators should replicate at compile time.
    /// </remarks>
    private static IReadOnlyList<string> GetEndpointGroups(this Type endpointType)
    {
        return endpointType
            .GetCustomAttributes<EndpointGroupAttribute>(inherit: false)
            .Select(a => a.Route)
            .Where(route => !string.IsNullOrWhiteSpace(route))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }
}

```

After this point, usage and behavior are identical to the main framework.
You can continue with the examples and usage patterns described in README.md,
simply skipping the source-generator–specific sections.


## TL;DR

Inline usage allows you to copy a minimal implementation directly into your
project without referencing any NuGet packages.
This approach is intentionally **simple**, **transparent**, and **dependency-free**.

It is best suited for:

* Small services
* Internal tools
* Prototypes and experiments
* Environments where external dependencies are undesirable

---

## What this mode provides

The inline version includes the **full runtime discovery model**:

* The `IEndpoint` contract
* Reflection-based endpoint discovery
* Multi-assembly scanning (you control which assemblies are scanned)
* Hybrid discovery (manual + reflection)
* Runtime endpoint mapping
* Optional route grouping via `RouteGroupBuilder`

From a **runtime behavior perspective**, the inline version is fully capable.

---

## What this mode does *not* provide

The inline version intentionally omits **compile-time tooling**:

* ❌ Source generator support
* ❌ Compile-time endpoint discovery
* ❌ Native AOT guarantees
* ❌ Analyzer & CodeFix support
  *(Analyzers are only meaningful when a source generator is present)*

All endpoint discovery and validation happens **at runtime**.

---

## Design intent

Inline usage exists to support a **copy-paste-friendly** workflow and to make
the core ideas of Stella.MinimalApi easy to understand.

It is not a restricted subset of the framework, but a **different delivery
strategy**:

* Same mental model
* Same endpoint definitions
* Same discovery concepts
* Runtime instead of compile time

---

## When to migrate

If your project requires:

* Zero-reflection startup
* Native AOT compilation
* Compile-time safety guarantees
* IDE diagnostics and auto-fixes

you should migrate to the **NuGet-based setup with the source generator**.

Migration is straightforward, as endpoint definitions remain unchanged.


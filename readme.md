
# Stella.MinimalApi
A lightweight, modular **endpoint discovery framework** for ASP.NET Core Minimal APIs.
> **Note**  
> This project is inspired by architectural ideas and educational content shared by
> Milan Jovanović on ASP.NET Core Minimal APIs.



`Stella.MinimalApi` provides **source-generator–based tooling** for discovering and wiring `IEndpoint` implementations at compile time, aiming for **Native AOT–friendly** applications with **zero runtime reflection**.

If you prefer not to use code generators, Stella.MinimalApi also includes a **reflection-based discovery** mechanism via the built-in `AssemblyEndpointDiscovery` class.

Designed for **modular monoliths**, **vertical slice architectures**, and **clean, test-first Minimal API designs**.

## 🚀 Installation

Stella.MinimalApi is distributed as **three composable packages**.
You can use only the parts you need.

### Core package (required)

```bash
dotnet add package Stella.MinimalApi
```

Provides the **base abstractions and runtime infrastructure**, including:

* `IEndpoint` contract
* Endpoint grouping attributes
* Reflection-based discovery (`AssemblyEndpointDiscovery`)
* Runtime endpoint mapping

This package works **standalone** and does **not** require code generation.

### Source Generator (recommended)

```bash
dotnet add package Stella.MinimalApi.SourceGenerator
```

Adds **source-generator–based endpoint discovery**.

* Compile-time scanning of `IEndpoint` implementations
* Zero runtime reflection
* Designed for **Native AOT–friendly** applications

When this package is referenced, endpoint discovery can be performed entirely at
compile time.

---

### Analyzer & CodeFix (optional)

```bash
dotnet add package Stella.MinimalApi.SourceGenerator.Analyzer
```

Provides **IDE-time diagnostics and CodeFixes** when using the source generator.

* Validates endpoint contracts at compile time
* Surfaces errors as IDE diagnostics (red squiggles)
* Offers automatic fixes for common mistakes

This package is **fully optional** and only useful when the source generator is enabled.


### Choosing the right setup

| Scenario                  | Recommended packages                    |
| ------------------------- | --------------------------------------- |
| Small service / prototype | `Stella.MinimalApi`                     |
| Modular monolith          | `Stella.MinimalApi`                     |
| Native AOT                | `Stella.MinimalApi` + `SourceGenerator` |
| Compile-time guarantees   | All three packages                      |

---
### Inline / Single-file usage

An **inline single-file** version is also available [here](docs/inline-usage.md) for users who prefer
copy-paste integration or zero dependencies.
---


## 📝 Quick Example

A minimal example demonstrating the complete Stella.MinimalApi workflow:
endpoint definition, discovery, and routing.

### 🧩 Defining Endpoints

Endpoints in **Stella.MinimalApi** are defined by implementing the `IEndpoint`
interface.
The endpoint implementation itself is **always the same**, regardless of how
it is grouped or exposed.

Grouping is **optional** and applied declaratively via attributes.

---

#### Basic endpoint (no grouping)

This is the simplest form of an endpoint definition.

```csharp
public class UserGetEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/users/{id:int}", (int id) => $"User {id}");
    }
}
```

**Resulting route:**

```
GET /users/{id:int}
```

---

#### Endpoint with a single group

The endpoint code remains unchanged.
Only an optional `[EndpointGroup]` attribute is added.

```csharp
[EndpointGroup("/api")]
public class UserGetEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/users/{id:int}", (int id) => $"User {id}");
    }
}
```

**Resulting route:**

```
GET /api/users/{id:int}
```

---

#### Endpoint with multiple groups

An endpoint can be decorated with **multiple group attributes**.

Each group produces a separate route mapping, without duplicating logic.

```csharp
[EndpointGroup("/api")]
[EndpointGroup("/internal")]
public class UserGetEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/users/{id:int}", (int id) => $"User {id}");
    }
}
```

**Resulting routes:**

```
GET /api/users/{id:int}
GET /internal/users/{id:int}
```


#### Design guarantees

* Endpoint definitions are **self-contained**
* Grouping is **additive and non-invasive**
* No route overriding occurs
* The same endpoint can be exposed under multiple prefixes
* Compatible with both **source generator** and **reflection-based** discovery


### 🔍 Registering Endpoints (Discovery Strategies)

Stella.MinimalApi supports **multiple endpoint discovery strategies**.
You are free to choose the one that best fits your application — or combine them.

All strategies are based on the same core abstraction:

```csharp
public interface IEndpointDiscovery
{
    void Register(IServiceCollection services);
}
```


#### Manual (Hard-coded) discovery

Endpoints are registered explicitly by implementing `IEndpointDiscovery`.

This approach offers **full control** and **zero reflection**, but requires
manual maintenance.

```csharp
public sealed class ManualEndpointDiscovery : IEndpointDiscovery
{
    public void Register(IServiceCollection services)
    {
        services.AddSingleton<IEndpoint, UserGetEndpoint>();
        services.AddSingleton<IEndpoint, UserCreateEndpoint>();
    }
}
```

```csharp
builder.Services.AddEndpoints(new ManualEndpointDiscovery());
```
**When to use:**

* Very small services
* Highly controlled environments
* Explicit wiring preferred over automation

---

#### Reflection-based assembly discovery

Endpoints are discovered by scanning an assembly at runtime.

```csharp
builder.Services.AddEndpoints(
    new AssemblyEndpointDiscovery(typeof(SomeEndpoint).Assembly)
);
```

Internally, this scans the given assembly for all `IEndpoint` implementations
and registers them.

**Characteristics:**

* Simple and flexible
* Runtime reflection
* No code generation required

**When to use:**

* Rapid prototyping
* Non-AOT applications
* Dynamic module loading

---

#### Source-generator-based discovery (recommended)

Endpoints are discovered at **compile time** using a source generator.

You declare a discovery class, decorate it with `[EndpointDiscovery]`,
and let the generator produce the registration code.

```csharp
[EndpointDiscovery]
public partial class ApiEndpointDiscovery : IEndpointDiscovery
{
    public void Register(IServiceCollection services)
    {
        RegisterGenerated(services);
    }

    private partial void RegisterGenerated(IServiceCollection services);
}
```

```csharp
builder.Services.AddEndpoints( new ApiEndpointDiscovery() );
```

The generator emits the implementation of `RegisterGenerated`.

**Characteristics:**

* Zero runtime reflection
* Native AOT friendly
* Compile-time validation
* Deterministic behavior

**When to use:**

* Production systems
* Native AOT
* Large modular applications

---

#### Hybrid: generator + manual endpoints

You can combine **generated discovery** with **explicit registrations**.

```csharp
[EndpointDiscovery]
public partial class ApiEndpointDiscovery : IEndpointDiscovery
{
    public void Register(IServiceCollection services)
    {
        services.AddSingleton<IEndpoint, HealthCheckEndpoint>();
        RegisterGenerated(services);
    }

    private partial void RegisterGenerated(IServiceCollection services);
}
```
```csharp
builder.Services.AddEndpoints( new ApiEndpointDiscovery() );
```
**Use case:**

* Generated endpoints for most modules
* Hand-crafted or conditional endpoints alongside them

---

#### Hybrid: reflection + manual endpoints

Reflection-based discovery can also be combined with explicit registrations.

```csharp
public sealed class ApiEndpointDiscovery : IEndpointDiscovery
{
    public void Register(IServiceCollection services)
    {
        services.AddSingleton<IEndpoint, HealthCheckEndpoint>();

        new AssemblyEndpointDiscovery(
            typeof(SomeEndpoint).Assembly
        ).Register(services);
    }
}
```

```csharp
builder.Services.AddEndpoints( new ApiEndpointDiscovery() );
```
**Use case:**

* Gradual migration
* Mixed legacy and new modules
* Partial automation


### 🧠 Choosing the right strategy

| Strategy            | Reflection | Generator | AOT-friendly | Control |
| ------------------- | ---------- | --------- | ------------ | ------- |
| Manual              | ❌          | ❌         | ✅            | ⭐⭐⭐⭐    |
| Assembly scan       | ✅          | ❌         | ❌            | ⭐⭐      |
| Source generator    | ❌          | ✅         | ✅            | ⭐⭐⭐     |
| Generator + manual  | ❌          | ✅         | ✅            | ⭐⭐⭐⭐    |
| Reflection + manual | ✅          | ❌         | ❌            | ⭐⭐⭐     |

---

#### ✨ Design philosophy

Discovery strategies are **composable**, not mutually exclusive.

This allows you to:

* start simple
* evolve gradually
* avoid lock-in
* keep endpoint definitions unchanged

Discovery is treated as **infrastructure**, not business logic.


### Map endpoints at startup

After the application is built, all registered endpoints must be mapped
into the ASP.NET Core routing pipeline.

```csharp
var app = builder.Build();

app.MapEndpoints();

app.Run();
```

Calling `MapEndpoints()`:

* Resolves all registered `IEndpoint` implementations
* Applies endpoint grouping (`[EndpointGroup]`)
* Maps each endpoint into the routing table
* Guarantees that endpoints do **not override each other**

This call is typically performed **once**, during application startup,
and works the same regardless of how endpoints were discovered
(manual, reflection-based, or source-generated).



##  License

This project is released into the **public domain** via the [Unlicense](./UNLICENSE).
You may freely copy, reuse, modify, inline, or distribute this code — including
using it without attribution or embedding it directly inside your project.


# Stella.MinimalApi

A lightweight, modular **endpoint discovery library** for ASP.NET Core Minimal APIs.

`Stella.MinimalApi` scans an assembly for all `IEndpoint` implementations and automatically maps them at startup — allowing clean, modular, feature-based API architectures.

Perfect for **modular monoliths**, **vertical slice architectures**, and **clean Minimal API designs**.

---

## 🚀 Installation

### Option 1 — Install via NuGet (recommended)

```bash
dotnet add package Stella.MinimalApi
````

### Option 2 — Inline Single-File Version (zero dependencies)

If you prefer not to install a package, you can simply copy this **single file**
directly into your project:

<details>
<summary><strong>Click to expand: MinimalApiExtensions.cs</strong></summary>

```csharp
// MinimalApiExtensions.cs (inline version)

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Reflection;

namespace Stella.MinimalApi;

public interface IEndpoint
{
    void MapEndpoint(IEndpointRouteBuilder app);
}

public static class MinimalApiExtensions
{
    public static IServiceCollection AddEndpoints(this IServiceCollection services, Assembly assembly)
    {
        var descriptors = assembly
            .DefinedTypes
            .Where(t => !t.IsAbstract && !t.IsInterface && typeof(IEndpoint).IsAssignableFrom(t))
            .Select(t => ServiceDescriptor.Transient(typeof(IEndpoint), t))
            .ToArray();

        services.TryAddEnumerable(descriptors);
        return services;
    }

    public static WebApplication MapEndpoints(
        this WebApplication app,
        RouteGroupBuilder? routeGroupBuilder = null)
    {
        var endpoints = app.Services.GetRequiredService<IEnumerable<IEndpoint>>();
        IEndpointRouteBuilder builder = routeGroupBuilder ?? app;

        foreach (var ep in endpoints)
            ep.MapEndpoint(builder);

        return app;
    }
}
```

</details>

---

## 📝 Quick Example

### 1. Define an endpoint

```csharp
public class UserGetEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/users/{id:int}", (int id) => $"User {id}");
    }
}
```

---

### 2. Register endpoints from an assembly

```csharp
builder.Services.AddEndpoints(typeof(UserGetEndpoint).Assembly);
```

---

### 3. Map endpoints at startup

```csharp
var app = builder.Build();

app.MapEndpoints();      // auto-maps all IEndpoint implementations

app.Run();
```

(Optional) group endpoints under a common prefix:

```csharp
app.MapEndpoints(app.MapGroup("/api"));
```

---

## 📁 Example Project Structure

```
src/
  MyApi/
    Program.cs
    Modules/
      Users/
        UserGetEndpoint.cs
        UserCreateEndpoint.cs
      Billing/
        BillingGetEndpoint.cs
  Stella.MinimalApi/
    IEndpoint.cs
    MinimalApiExtensions.cs
```

This works extremely well in **feature-based** or **DDD-inspired** designs.

---

## 🧠 How It Works

* Discovers all types implementing `IEndpoint` in the provided assembly
* Registers each endpoint as a transient service
* Resolves and maps them during app startup
* Enables clean modularity without manual wiring

---

## 🧪 Testing Example

```csharp
[Fact]
public void AddEndpoints_ShouldRegisterAllImplementations()
{
    var services = new ServiceCollection();
    var assembly = typeof(TestEndpoint).Assembly;

    services.AddEndpoints(assembly);
    var provider = services.BuildServiceProvider();

    provider.GetServices<IEndpoint>()
            .Should().ContainSingle()
            .Which.Should().BeOfType<TestEndpoint>();
}
```

---

## 📜 License

MIT License © Stella Yazılım
Open to contributions and improvements. Enjoy!

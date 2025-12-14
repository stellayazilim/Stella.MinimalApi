namespace Stella.MinimalApi;

using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

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

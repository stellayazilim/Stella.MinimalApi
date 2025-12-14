
using Microsoft.Extensions.DependencyInjection;

namespace Stella.MinimalApi.Tests;


[EndpointDiscovery]
public partial class TestEndpointDiscoverer: IEndpointDiscovery
{
    public void Register(IServiceCollection services)
    {
        RegisterGenerated(services);
    }

    private partial void RegisterGenerated(IServiceCollection services);
}
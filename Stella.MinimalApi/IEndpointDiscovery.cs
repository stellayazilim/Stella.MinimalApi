using Microsoft.Extensions.DependencyInjection;

namespace Stella.MinimalApi;


public interface IEndpointDiscovery
{
    void Register(IServiceCollection services);
}

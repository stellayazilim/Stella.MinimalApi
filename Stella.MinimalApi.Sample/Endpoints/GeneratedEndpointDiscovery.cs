namespace Stella.MinimalApi.Sample.Endpoints;


[EndpointDiscovery]
// ReSharper disable once UnusedType.Global
public partial class GeneratedEndpointDiscovery: IEndpointDiscovery
{
   
    
    public void Register(IServiceCollection services)
    {
        RegisterGenerated(services);
    }
    
    partial void RegisterGenerated(IServiceCollection services);
}
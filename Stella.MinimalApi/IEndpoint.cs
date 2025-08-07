using Microsoft.AspNetCore.Routing;

namespace Stella.MinimalApi;

public interface IEndpoint
{
    void MapEndpoint(IEndpointRouteBuilder app);
}
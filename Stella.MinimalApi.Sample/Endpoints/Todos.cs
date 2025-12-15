using Stella.MinimalApi.Sample.Data;

namespace Stella.MinimalApi.Sample.Endpoints;

[EndpointGroup("/todos")]
// ReSharper disable once UnusedType.Global
public class Todos: IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/", () => SampleTodos.GetTodos);
    }
}
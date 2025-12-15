using Stella.MinimalApi.Sample.Data;

namespace Stella.MinimalApi.Sample.Endpoints;

[EndpointGroup("/todos")]
// ReSharper disable once UnusedType.Global
public class TodoById: IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/{id}", (int id) => SampleTodos.GetTodos.FirstOrDefault(
            a => a.Id == id) is {} todo ? 
                Results.Ok(todo):
                Results.NotFound());
    }
}
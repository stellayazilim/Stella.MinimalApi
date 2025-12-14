using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Stella.MinimalApi.Tests;


[EndpointGroup("/test")]
public class TestEndpoint: IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/",  () => "hello world");
    }
}


[EndpointGroup("/test")]
public class TestEndpoint2: IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/{id:int}",  ( [FromRoute] int id ) => $"hello world from {id}");
    }
} 
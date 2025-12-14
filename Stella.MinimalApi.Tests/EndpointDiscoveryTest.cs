using System.Net;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Stella.MinimalApi.Extensions;

namespace Stella.MinimalApi.Tests;

public class EndpointDiscoveryTest
{


    [Fact]
    public async Task EndpointDiscovery_Registers_And_Maps_Endpoints()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();

        builder.Services.AddEndpoints(new TestEndpointDiscoverer());

        var app = builder.Build();

        // Act
        app.MapEndpoints();
        
        await app.StartAsync();

        // Assert
        var endpoints = app.Services.GetServices<IEndpoint>().ToList();

        Assert.Equal(2, endpoints.Count);
        Assert.IsType<TestEndpoint>(endpoints[0]);

        var dataSource = app.Services.GetRequiredService<EndpointDataSource>();

        var routeEndpoints = dataSource.Endpoints
            .OfType<RouteEndpoint>()
            .ToList();

        Assert.NotEmpty(routeEndpoints);

        Assert.Contains(routeEndpoints, e =>
            e.RoutePattern.RawText == "/test/");
        
        var testEndpoint = routeEndpoints.Single(e =>
            e.RoutePattern.RawText == "/test/");

        Assert.NotNull(testEndpoint.RequestDelegate);

        //  Cleanup
        await app.StopAsync();
    }
    
    
    [Theory]
    [InlineData("/test",   "hello world")]
    [InlineData("/test/1", "hello world from 1")]
    [InlineData("/test/2", "hello world from 2")]
    public async Task EndpointDiscovery_Should_Handle_Multiple_Routes(
        string path,
        string expectedBody)
    {
        // Arrange 
        var builder = WebApplication.CreateBuilder();

        builder.WebHost.UseTestServer();
        builder.Services.AddEndpoints(new TestEndpointDiscoverer());

        var app = builder.Build();
        app.MapEndpoints();

        await app.StartAsync();

        var client = app.GetTestClient();

        // Act
        var response = await client.GetAsync(path);

        // Assert 
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        Assert.Equal(expectedBody, body);

        // Cleanup
        await app.StopAsync();
    }

    
    
    
    [Theory]
    [InlineData("/test",   "hello world")]
    [InlineData("/test/1", "hello world from 1")]
    [InlineData("/test/2", "hello world from 2")]
    public async Task AssemblyEndpointDiscovery_Should_Handle_Http_Request(     
        string path,
        string expectedBody)
    {
        // Arrange 
        var builder = WebApplication.CreateBuilder();

        builder.WebHost.UseTestServer();
        builder.Services.AddEndpoints(new AssemblyEndpointDiscovery(typeof(TestEndpoint).Assembly));

        var app = builder.Build();
        app.MapEndpoints();

        await app.StartAsync();

        var client = app.GetTestClient();

        // Act
        var response = await client.GetAsync(path);

        // Assert 
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        Assert.Equal(expectedBody, body);

        // Cleanup
        await app.StopAsync();
    }

}
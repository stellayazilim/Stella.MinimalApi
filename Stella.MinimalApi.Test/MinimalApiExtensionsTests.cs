using System.Reflection;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Stella.MinimalApi.Extensions;

namespace Stella.MinimalApi.Test
{
    public class MinimalApiExtensionsTests
    {
        [Fact]
        public void AddEndpoints_ShouldRegisterAllImplementations()
        {
            // Arrange
            var services = new ServiceCollection();
            var assembly = typeof(TestEndpoint).Assembly;

            // Act
            services.AddEndpoints(assembly);
            var provider = services.BuildServiceProvider();

            // Assert
            var endpoint = provider.GetService<IEndpoint>();
            endpoint.Should().NotBeNull();
            endpoint.Should().BeOfType<TestEndpoint>();
        }
    }

    public class TestEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("/", () => "Hello World!");
        }
    }
}
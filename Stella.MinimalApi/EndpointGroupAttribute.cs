namespace Stella.MinimalApi;



/// <summary>
/// Specifies the route group an endpoint should be mapped to.
/// </summary>
/// <remarks>
/// This attribute is used to associate an <see cref="IEndpoint"/> implementation
/// with one or more route groups (for example: "/users", "/orders").
///
/// In the current implementation, this attribute is evaluated at runtime
/// during endpoint mapping to determine which <see cref="RouteGroupBuilder"/>
/// instances the endpoint should be registered with.
///
/// In future versions, tooling such as source generators may use this
/// attribute to perform the same grouping logic at compile time, eliminating
/// the need for reflection or assembly scanning.
///
/// The <paramref name="route"/> value must be a valid, constant route prefix.
/// </remarks>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class EndpointGroupAttribute(string route) : Attribute
{
    public string Route { get; } = route;
}
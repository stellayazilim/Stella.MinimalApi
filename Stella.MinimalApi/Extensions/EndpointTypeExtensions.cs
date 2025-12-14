using System.Reflection;

namespace Stella.MinimalApi.Extensions;

/// <summary>
/// Reflection-based helpers for working with endpoint metadata.
/// </summary>
public static class EndpointTypeExtensions
{
    /// <summary>
    /// Gets the route groups defined for the given endpoint type.
    /// </summary>
    /// <remarks>
    /// This method reads <see cref="EndpointGroupAttribute"/> instances
    /// applied to the endpoint type and returns their declared routes.
    ///
    /// If no attributes are present, an empty sequence is returned,
    /// indicating that the endpoint should be mapped to the root.
    ///
    /// This logic represents the reference behavior that source
    /// generators should replicate at compile time.
    /// </remarks>
    public static IReadOnlyList<string> GetEndpointGroups(this Type endpointType)
    {
        return endpointType
            .GetCustomAttributes<EndpointGroupAttribute>(inherit: false)
            .Select(a => a.Route)
            .Where(route => !string.IsNullOrWhiteSpace(route))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }
}
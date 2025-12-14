using Microsoft.CodeAnalysis;

namespace Stella.MinimalApi.SourceGenerator.Analyzer;


public static class Diagnostics
{
    private const string Category = "Stella.MinimalApi";

    public static readonly DiagnosticDescriptor EndpointDiscoveryMissingRegisterGenerated =
        new(
            id: "GED001",
            title: "Missing generated Register method",
            messageFormat:
            "Class '{0}' is marked with [EndpointDiscovery] but does not declare " +
            "'private partial void RegisterGenerated(IServiceCollection services);'",
            category: Category,
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description:
            "EndpointDiscovery classes must declare a private partial RegisterGenerated method " +
            "so the source generator can emit endpoint registrations."
        );

    public static readonly DiagnosticDescriptor EndpointDiscoveryMustBePartial =
        new(
            id: "GED002",
            title: "EndpointDiscovery classes must be partial",
            messageFormat:
            "Class '{0}' is marked with [EndpointDiscovery] but is not declared partial",
            category: Category,
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description:
            "EndpointDiscovery classes must be partial so the source generator can extend them."
        );
}

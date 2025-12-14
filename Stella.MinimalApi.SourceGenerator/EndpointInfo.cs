using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Stella.MinimalApi.SourceGenerator;

internal sealed record EndpointInfo(
    INamedTypeSymbol Symbol,
    ImmutableArray<string> Groups)
{
    public string FullyQualifiedName =>
        Symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
}

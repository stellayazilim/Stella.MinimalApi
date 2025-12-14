using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Stella.MinimalApi.SourceGenerator;


internal static class SymbolExtensions
{
    public static IEnumerable<INamedTypeSymbol> GetAllTypes(
        this INamespaceSymbol @namespace)
    {
        foreach (var member in @namespace.GetMembers())
        {
            if (member is INamespaceSymbol ns)
            {
                foreach (var type in ns.GetAllTypes())
                    yield return type;
            }
            else if (member is INamedTypeSymbol type)
            {
                yield return type;
            }
        }
    }

    public static bool IsPartial(this INamedTypeSymbol symbol)
        => symbol.DeclaringSyntaxReferences
            .Select(r => r.GetSyntax())
            .OfType<ClassDeclarationSyntax>()
            .Any(c => c.Modifiers.Any(SyntaxKind.PartialKeyword));
}

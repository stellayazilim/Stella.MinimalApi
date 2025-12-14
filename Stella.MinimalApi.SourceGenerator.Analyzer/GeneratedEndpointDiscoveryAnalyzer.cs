using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Stella.MinimalApi.SourceGenerator.Analyzer;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class EndpointDiscoveryAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(
            Diagnostics.EndpointDiscoveryMissingRegisterGenerated, // GED001
            Diagnostics.EndpointDiscoveryMustBePartial              // GED002
        );

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        // ⚠️ Editor pipeline için şart
        context.RegisterSyntaxNodeAction(
            AnalyzeClass,
            SyntaxKind.ClassDeclaration);
    }

    private static void AnalyzeClass(SyntaxNodeAnalysisContext context)
    {
        var classDecl = (ClassDeclarationSyntax)context.Node;

        // örnek: [EndpointDiscovery] kontrolü
        if (!HasEndpointDiscoveryAttribute(classDecl, context.SemanticModel))
            return;

        // GED002 – partial değilse
        if (!classDecl.Modifiers.Any(SyntaxKind.PartialKeyword))
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    Diagnostics.EndpointDiscoveryMustBePartial,
                    classDecl.Identifier.GetLocation()));
        }

        // GED001 – RegisterGenerated yoksa
        if (!HasRegisterGeneratedMethod(classDecl))
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    Diagnostics.EndpointDiscoveryMissingRegisterGenerated,
                    classDecl.Identifier.GetLocation()));
        }
    }

    private static bool HasEndpointDiscoveryAttribute(
        ClassDeclarationSyntax classDecl,
        SemanticModel semanticModel)
    {
        foreach (var attrList in classDecl.AttributeLists)
        foreach (var attr in attrList.Attributes)
        {
            var symbol = semanticModel.GetSymbolInfo(attr).Symbol;
            if (symbol?.ContainingType.Name == "EndpointDiscoveryAttribute")
                return true;
        }

        return false;
    }

    private static bool HasRegisterGeneratedMethod(ClassDeclarationSyntax classDecl)
    {
        return classDecl.Members
            .OfType<MethodDeclarationSyntax>()
            .Any(m =>
                m.Identifier.Text == "RegisterGenerated" &&
                m.Modifiers.Any(SyntaxKind.PartialKeyword));
    }
}

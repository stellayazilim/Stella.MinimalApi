using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Document = Microsoft.CodeAnalysis.Document;

namespace Stella.MinimalApi.SourceGenerator.Analyzer;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(EndpointDiscoveryCodeFixProvider))]
[Shared]
public sealed class EndpointDiscoveryCodeFixProvider : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ["GED001", "GED002"];

    public override FixAllProvider GetFixAllProvider()
        => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document
            .GetSyntaxRootAsync(context.CancellationToken)
            .ConfigureAwait(false);

        if (root is null)
            return;

        foreach (var diagnostic in context.Diagnostics)
        {
            var classDecl = root
                .FindNode(diagnostic.Location.SourceSpan)
                .FirstAncestorOrSelf<ClassDeclarationSyntax>();

            if (classDecl is null)
                continue;

            if (diagnostic.Id == "GED002")
            {
                context.RegisterCodeFix(
                    CodeAction.Create(
                        "Make class partial",
                        ct => AddPartialAsync(context.Document, classDecl, ct),
                        "GED002"),
                    diagnostic);
            }

            if (diagnostic.Id == "GED001")
            {
                context.RegisterCodeFix(
                    CodeAction.Create(
                        "Generate RegisterGenerated method",
                        ct => AddRegisterGeneratedAsync(context.Document, classDecl, ct),
                        "GED001"),
                    diagnostic);
            }
        }
    }

    private static async Task<Document> AddPartialAsync(
        Document document,
        ClassDeclarationSyntax classDecl,
        CancellationToken ct)
    {
        if (classDecl.Modifiers.Any(SyntaxKind.PartialKeyword))
            return document;

        var newClass = classDecl.WithModifiers(
            classDecl.Modifiers.Add(
                SyntaxFactory.Token(SyntaxKind.PartialKeyword)));

        var root = await document.GetSyntaxRootAsync(ct);
        return document.WithSyntaxRoot(root!.ReplaceNode(classDecl, newClass));
    }

    private static async Task<Document> AddRegisterGeneratedAsync(
        Document document,
        ClassDeclarationSyntax classDecl,
        CancellationToken ct)
    {
        var method =
            SyntaxFactory.MethodDeclaration(
                    SyntaxFactory.PredefinedType(
                        SyntaxFactory.Token(SyntaxKind.VoidKeyword)),
                    "RegisterGenerated")
                .WithModifiers(
                    SyntaxFactory.TokenList(
                        SyntaxFactory.Token(SyntaxKind.PrivateKeyword),
                        SyntaxFactory.Token(SyntaxKind.PartialKeyword)))
                .WithParameterList(
                    SyntaxFactory.ParameterList(
                        SyntaxFactory.SingletonSeparatedList(
                            SyntaxFactory.Parameter(
                                    SyntaxFactory.Identifier("services"))
                                .WithType(
                                    SyntaxFactory.IdentifierName("IServiceCollection")))))
                .WithSemicolonToken(
                    SyntaxFactory.Token(SyntaxKind.SemicolonToken));

        var newClass = classDecl.AddMembers(method);

        var root = await document.GetSyntaxRootAsync(ct);
        return document.WithSyntaxRoot(root!.ReplaceNode(classDecl, newClass));
    }
}

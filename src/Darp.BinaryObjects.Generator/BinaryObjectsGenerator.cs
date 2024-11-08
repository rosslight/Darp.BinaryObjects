﻿namespace Darp.BinaryObjects.Generator;

using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

[Generator]
public class BinaryObjectsGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValuesProvider<TargetTypeInfo> attributes = context
            .SyntaxProvider.ForAttributeWithMetadataName(
                "Darp.BinaryObjects.BinaryObjectAttribute",
                Predicate,
                GetTypeInfo
            )
            .Collect()
            .SelectMany((x, _) => x.Distinct());

        context.RegisterSourceOutput(attributes, Execute);
    }

    private static bool Predicate(SyntaxNode syntaxNode, CancellationToken cancellationToken)
    {
        return syntaxNode is TypeDeclarationSyntax;
    }

    private static TargetTypeInfo GetTypeInfo(
        GeneratorAttributeSyntaxContext context,
        CancellationToken cancellationToken
    )
    {
        var type = (INamedTypeSymbol)context.TargetSymbol;
        var node = (TypeDeclarationSyntax)context.TargetNode;
        return new TargetTypeInfo(type, node);
    }

    private static void Execute(SourceProductionContext spc, TargetTypeInfo info)
    {
        var isCodeGenerated = TryGenerateSourceCode(info, out BinaryObjectBuilder builder);
        foreach (Diagnostic diagnostic in builder.Diagnostics)
            spc.ReportDiagnostic(diagnostic);
        if (!isCodeGenerated)
            return;
        spc.AddSource(
            $"{info.Symbol.Name}.g.cs",
            SourceText.From(builder.ToString(), Encoding.UTF8, SourceHashAlgorithm.Sha256)
        );
    }

    private static bool TryGenerateSourceCode(TargetTypeInfo info, out BinaryObjectBuilder builder)
    {
        builder = new BinaryObjectBuilder(info.Symbol, info.Syntax);
        builder.AddFileHeader();
        builder.StringBuilder.AppendLine();
        if (!builder.TryAddTypeDeclaration())
            return false;
        builder.StringBuilder.AppendLine("{");
        builder.StringBuilder.AppendLine();
        builder.StringBuilder.AppendLine("}");
        return true;
    }
}

internal sealed record TargetTypeInfo(INamedTypeSymbol Symbol, TypeDeclarationSyntax Syntax);

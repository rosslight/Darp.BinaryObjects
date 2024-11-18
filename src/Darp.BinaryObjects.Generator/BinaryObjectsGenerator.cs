﻿namespace Darp.BinaryObjects.Generator;

using System.CodeDom.Compiler;
using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

internal readonly record struct BinaryObjectStruct(
    ImmutableEquatableArray<DiagnosticData> Diagnostics,
    string? Code,
    ImmutableEquatableArray<UtilityData> RequiredUtilities
);

/// <summary>Stores the data necessary to create a Diagnostic.</summary>
/// <remarks>
/// Diagnostics do not have value equality semantics.  Storing them in an object model
/// used in the pipeline can result in unnecessary recompilation.
/// </remarks>
/// <seealso href="https://github.com/dotnet/runtime/blob/main/src/libraries/System.Text.RegularExpressions/gen/RegexGenerator.cs#L355"/>
internal readonly record struct DiagnosticData(
    DiagnosticDescriptor Descriptor,
    Location Location,
    object?[]? MessageArgs
)
{
    /// <summary>Create a <see cref="Diagnostic"/> from the data.</summary>
    public Diagnostic ToDiagnostic() => Diagnostic.Create(Descriptor, Location, MessageArgs);

    public static DiagnosticData Create(
        DiagnosticDescriptor descriptor,
        Location location,
        object?[]? messageArgs = null
    )
    {
        return new DiagnosticData(descriptor, location, messageArgs);
    }
}

internal readonly record struct UtilityData(WellKnownCollectionKind CollectionKind, WellKnownTypeKind TypeKind);

[Generator(LanguageNames.CSharp)]
public partial class BinaryObjectsGenerator : IIncrementalGenerator
{
    private const string BinaryObjectAttributeName = "Darp.BinaryObjects.BinaryObjectAttribute";
    private const string GeneratedFileName = "BinaryObjectsGenerator.g.cs";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValueProvider<ImmutableArray<BinaryObjectStruct>> attributes = context
            // Only target specific attributes
            .SyntaxProvider.ForAttributeWithMetadataName(
                BinaryObjectAttributeName,
                static (node, _) => node is TypeDeclarationSyntax,
                GetTypeInfo
            )
            .Select(
                static (info, _) =>
                {
                    if (!TryParseType(info.Symbol, out Aaa a))
                    {
                        return new BinaryObjectStruct(
                            a.Diagnostics.ToImmutableEquatableArray(),
                            null,
                            ImmutableEquatableArray<UtilityData>.Empty
                        );
                    }
                    if (!TryEmit(info, a.MemberGroups, a.MembersInitializedByConstructor, out Aaa2 aaa))
                    {
                        return new BinaryObjectStruct(
                            aaa.Diagnostics.Concat(a.Diagnostics).ToImmutableEquatableArray(),
                            null,
                            ImmutableEquatableArray<UtilityData>.Empty
                        );
                    }

                    var utilities = a
                        .MemberGroups.SelectMembers()
                        .Select(x => new UtilityData(x.CollectionKind, x.TypeKind))
                        .Distinct()
                        .ToImmutableEquatableArray();
                    return new BinaryObjectStruct(
                        aaa.Diagnostics.Concat(a.Diagnostics).ToImmutableEquatableArray(),
                        aaa.Code,
                        utilities
                    );
                }
            )
            .Collect();

        context.RegisterSourceOutput(attributes, Execute);
    }

    private static void Execute(SourceProductionContext spc, ImmutableArray<BinaryObjectStruct> infos)
    {
        var shouldGenerateCode = false;
        foreach (BinaryObjectStruct info in infos)
        {
            foreach (DiagnosticData diagnosticData in info.Diagnostics)
            {
                spc.ReportDiagnostic(diagnosticData.ToDiagnostic());
            }

            shouldGenerateCode = shouldGenerateCode || info.Code is not null;
        }

        if (!shouldGenerateCode)
        {
            return;
        }

        var sw = new StringWriter();
        using var writer = new IndentedTextWriter(sw);

        EmitFileHeader(writer);

        foreach (BinaryObjectStruct info in infos)
        {
            writer.Write(info.Code);
        }

        IEnumerable<UtilityData> requestedUtilities = infos.SelectMany(x => x.RequiredUtilities).Distinct();
        EmitUtilityClass(writer, requestedUtilities);

        spc.AddSource(GeneratedFileName, SourceText.From(sw.ToString(), Encoding.UTF8, SourceHashAlgorithm.Sha256));
    }

    private static TargetTypeInfo GetTypeInfo(
        GeneratorAttributeSyntaxContext context,
        CancellationToken cancellationToken
    )
    {
        var compilation = context.SemanticModel.Compilation as CSharpCompilation;
        LanguageVersion languageVersion = compilation?.LanguageVersion ?? LanguageVersion.CSharp1;

        var type = (INamedTypeSymbol)context.TargetSymbol;
        var node = (TypeDeclarationSyntax)context.TargetNode;
        return new TargetTypeInfo(type, node, languageVersion);
    }
}

internal readonly record struct TargetTypeInfo(
    INamedTypeSymbol Symbol,
    TypeDeclarationSyntax Syntax,
    LanguageVersion LanguageVersion
);

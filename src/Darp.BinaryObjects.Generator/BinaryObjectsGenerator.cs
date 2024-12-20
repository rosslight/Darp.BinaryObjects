namespace Darp.BinaryObjects.Generator;

using System.CodeDom.Compiler;
using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

#pragma warning disable CA1031 // Do not catch general exception types - allow for source generator to avoid have it crashing on unexpected behavior

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

internal readonly record struct UtilityData(
    bool IsReadUtility,
    WellKnownCollectionKind CollectionKind,
    WellKnownTypeKind TypeKind,
    int? ByteLength,
    bool EmitLittleAndBigEndian
)
{
    public static int? UnknownLength { get; } = null!;
}

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
                    try
                    {
                        if (!TryParseType(info.Symbol, out ParsedObjectInfo parsedObject))
                        {
                            return new BinaryObjectStruct(
                                parsedObject.Diagnostics.ToImmutableEquatableArray(),
                                null,
                                ImmutableEquatableArray<UtilityData>.Empty
                            );
                        }

                        if (
                            !TryEmit(
                                info,
                                parsedObject.MemberGroups,
                                parsedObject.MembersInitializedByConstructor,
                                out Aaa2 aaa
                            )
                        )
                        {
                            return new BinaryObjectStruct(
                                aaa.Diagnostics.Concat(parsedObject.Diagnostics).ToImmutableEquatableArray(),
                                null,
                                ImmutableEquatableArray<UtilityData>.Empty
                            );
                        }

                        var utilities = parsedObject
                            .MemberGroups.SelectMembers()
                            .SelectMany(x =>
                                GetWriteUtilities(x.CollectionKind, x.TypeKind, x.TypeSymbol)
                                    .Concat(GetReadUtilities(x.CollectionKind, x.TypeKind, x.TypeSymbol))
                            )
                            .Distinct()
                            .ToImmutableEquatableArray();
                        return new BinaryObjectStruct(
                            aaa.Diagnostics.Concat(parsedObject.Diagnostics).ToImmutableEquatableArray(),
                            aaa.Code,
                            utilities
                        );
                    }
                    catch (Exception e)
                    {
                        var diagnostic = DiagnosticData.Create(
                            DiagnosticDescriptors.GeneralError,
                            info.Symbol.GetSourceLocation(),
                            [e.Message]
                        );
                        return new BinaryObjectStruct([diagnostic], null, ImmutableEquatableArray<UtilityData>.Empty);
                    }
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

        try
        {
            var sw = new StringWriter();
            using var writer = new IndentedTextWriter(sw);

            EmitFileHeader(writer);

            foreach (BinaryObjectStruct info in infos)
            {
                if (info.Code is null)
                    continue;
                writer.Write(info.Code);
                writer.WriteEmptyLine();
            }

            var requestedUtilities = infos.SelectMany(x => x.RequiredUtilities).Distinct().ToImmutableArray();
            EmitUtilityClass(writer, requestedUtilities);

            spc.AddSource(GeneratedFileName, SourceText.From(sw.ToString(), Encoding.UTF8, SourceHashAlgorithm.Sha256));
        }
        catch (Exception e)
        {
            var diagnostic = Diagnostic.Create(DiagnosticDescriptors.GeneralError, null, e.Message);
            spc.ReportDiagnostic(diagnostic);
        }
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

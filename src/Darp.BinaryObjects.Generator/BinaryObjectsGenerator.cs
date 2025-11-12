namespace Darp.BinaryObjects.Generator;

using System.CodeDom.Compiler;
using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

/*
- Get parsing information for each BinaryObject in the project
- Collect all parsing information, create a lookup dictionary for each length
- Combine both previous and generate code / errors for each binary object
- Combine generated code and utility classes to generated code
 */


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
public readonly record struct DiagnosticData(
    DiagnosticDescriptor Descriptor,
    Location Location,
    object? Arg0,
    object? Arg1,
    object? Arg2,
    object? Arg3
)
{
    public DiagnosticSeverity DefaultSeverity => Descriptor.DefaultSeverity;

    /// <summary>Create a <see cref="Diagnostic"/> from the data.</summary>
    public Diagnostic ToDiagnostic() => Diagnostic.Create(Descriptor, Location, Arg0, Arg1, Arg2, Arg3);

    public static DiagnosticData Create(
        DiagnosticDescriptor descriptor,
        Location? location,
        object? arg0 = null,
        object? arg1 = null,
        object? arg2 = null,
        object? arg3 = null
    )
    {
        return new DiagnosticData(descriptor, location ?? Location.None, arg0, arg1, arg2, arg3);
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
        IncrementalValueProvider<AnalyzerConfigOptionsProvider> configProvider = context.AnalyzerConfigOptionsProvider;
        // Only target specific attributes
        IncrementalValuesProvider<TargetTypeInfo> binaryObjectProvider =
            context.SyntaxProvider.ForAttributeWithMetadataName(
                BinaryObjectAttributeName,
                static (node, _) => node is TypeDeclarationSyntax,
                GetTypeInfo
            );
        IncrementalValuesProvider<PreParsedObjectInfo> preparsedBinaryObjectProvider = binaryObjectProvider.Select(
            static (x, _) => ParseType(x.Symbol)
        );
        IncrementalValuesProvider<ParsedObjectInfo> binaryObjectLengthProvider = preparsedBinaryObjectProvider
            .Collect()
            .SelectMany(
                static (infos, _) =>
                {
                    Dictionary<ISymbol, ParsedObjectInfo> dictionary = new(SymbolEqualityComparer.Default);
                    HashSet<ISymbol> workingSet = [];
                    foreach (PreParsedObjectInfo preParsedObjectInfo in infos)
                    {
                        ISymbol symbol = preParsedObjectInfo.Symbol;
                        workingSet.Add(symbol);
                        ParsedObjectInfo parsedInfo = GetConstantLength(preParsedObjectInfo, workingSet);
                        dictionary[symbol] = parsedInfo;
                        workingSet.Remove(symbol);
                    }
                    return dictionary.Values.ToImmutableArray();
                }
            );
        IncrementalValueProvider<ImmutableArray<BinaryObjectStruct>> attributes = binaryObjectLengthProvider
            .Select(
                static (parsedObject, _) =>
                {
                    try
                    {
                        if (
                            !TryEmit(
                                default!,
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
                            {
                                var typeByteLength = x switch
                                {
                                    IConstantMember c => c.TypeByteLength,
                                    IVariableMemberGroup v => v.TypeByteLength,
                                    _ => UtilityData.UnknownLength,
                                };
                                return GetWriteUtilities(x.CollectionKind, x.TypeKind, x.TypeSymbol, typeByteLength)
                                    .Concat(
                                        GetReadUtilities(x.CollectionKind, x.TypeKind, x.TypeSymbol, typeByteLength)
                                    );
                            })
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
                            default!, //Symbol.GetSourceLocation(),
                            e.Message
                        );
                        return new BinaryObjectStruct([diagnostic], null, ImmutableEquatableArray<UtilityData>.Empty);
                    }
                }
            )
            .Collect();

        context.RegisterSourceOutput(attributes.Combine(configProvider), Execute);
    }

    private static void Execute(
        SourceProductionContext spc,
        (ImmutableArray<BinaryObjectStruct> StructInfos, AnalyzerConfigOptionsProvider Options) provided
    )
    {
        ImmutableArray<BinaryObjectStruct> infos = provided.StructInfos;
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

            var isInternal =
                provided.Options.GlobalOptions.TryGetValue(
                    "build_property.BinaryObjectsGenerator_IsUtilityClassInternal",
                    out var isInternalString
                )
                && bool.TryParse(isInternalString, out var isInternalParsed)
                && isInternalParsed;
            var requestedUtilities = infos.SelectMany(x => x.RequiredUtilities).Distinct().ToImmutableArray();
            EmitUtilityClass(writer, requestedUtilities, isInternal);

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

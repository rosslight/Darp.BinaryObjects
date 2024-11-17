namespace Darp.BinaryObjects.Generator;

using System.CodeDom.Compiler;
using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
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

internal readonly record struct UtilityData(string Name);

[Generator]
public class BinaryObjectsGenerator : IIncrementalGenerator
{
    public const string GeneratedFileName = "BinaryObjectsGenerator.g.cs";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValueProvider<ImmutableArray<BinaryObjectStruct>> attributes = context
            .SyntaxProvider.ForAttributeWithMetadataName(
                "Darp.BinaryObjects.BinaryObjectAttribute",
                static (node, _) => node is TypeDeclarationSyntax,
                GetTypeInfo
            )
            .Select(
                static (info, _) =>
                {
                    var wasCodeGenerated = TryGenerateSourceCode(info, out BinaryObjectBuilder builder);
                    return new BinaryObjectStruct(
                        builder.Diagnostics.ToImmutableEquatableArray(),
                        wasCodeGenerated ? builder.StringBuilder.ToString() : null,
                        ImmutableEquatableArray<UtilityData>.Empty
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

        BinaryObjectBuilder.AddFileHeader(writer);

        foreach (BinaryObjectStruct info in infos)
        {
            writer.Write(info.Code);
        }

        IEnumerable<UtilityData> requestedUtilities = infos.SelectMany(x => x.RequiredUtilities).Distinct();
        WriteUtilityClass(writer, requestedUtilities);

        spc.AddSource(GeneratedFileName, SourceText.From(sw.ToString(), Encoding.UTF8, SourceHashAlgorithm.Sha256));
    }

    private static TargetTypeInfo GetTypeInfo(
        GeneratorAttributeSyntaxContext context,
        CancellationToken cancellationToken
    )
    {
        // if (
        //     context.SemanticModel.Compilation
        //     is not CSharpCompilation { LanguageVersion: var version, Options: { } options }
        // )
        // {
        //     throw;
        // }

        var type = (INamedTypeSymbol)context.TargetSymbol;
        var node = (TypeDeclarationSyntax)context.TargetNode;
        return new TargetTypeInfo(type, node);
    }

    private static void WriteUtilityClass(IndentedTextWriter writer, IEnumerable<UtilityData> requestedUtilities) { }

    private static bool TryGenerateSourceCode(TargetTypeInfo info, out BinaryObjectBuilder builder)
    {
        builder = BinaryObjectBuilder.Create(info.Symbol, info.Syntax);
        builder.StringBuilder.AppendLine();
        builder.AddOptionalNamespaceStart();
        if (!builder.TryAddTypeDeclaration())
            return false;
        builder.StringBuilder.AppendLine("{");
        builder.AddGetByteCountMethod();
        builder.StringBuilder.AppendLine();
        builder.AddWriteImplementationMethod(true);
        builder.AddWriteImplementationMethod(false);
        builder.StringBuilder.AppendLine();
        builder.AddReadImplementationMethod(true);
        builder.AddReadImplementationMethod(false);
        builder.StringBuilder.AppendLine("}");
        builder.AddOptionalNamespaceEnd();
        return true;
    }
}

internal sealed record TargetTypeInfo(INamedTypeSymbol Symbol, TypeDeclarationSyntax Syntax);

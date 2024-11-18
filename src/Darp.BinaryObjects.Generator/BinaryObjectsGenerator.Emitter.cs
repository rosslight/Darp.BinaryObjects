namespace Darp.BinaryObjects.Generator;

using System.CodeDom.Compiler;
using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

internal readonly record struct Aaa2(
    ImmutableArray<DiagnosticData> Diagnostics,
    string? Code,
    ImmutableArray<UtilityData> Utilities
)
{
    public static Aaa2 Fail(IEnumerable<DiagnosticData> diagnostics)
    {
        return new Aaa2(diagnostics.ToImmutableArray(), null, ImmutableArray<UtilityData>.Empty);
    }
}

partial class BinaryObjectsGenerator
{
    public const string Prefix = "___";

    private static bool TryEmit(
        TargetTypeInfo info,
        ImmutableArray<IGroup> memberGroups,
        ImmutableHashSet<IMember> constructorParameters,
        out Aaa2 aaa2
    )
    {
        var sw = new StringWriter();
        using var writer = new IndentedTextWriter(sw);
        var diagnostics = new List<DiagnosticData>();
        var utilities = new List<UtilityData>();

        EmitOptionalNamespaceStart(writer, info.Symbol);
        EmitAddTypeDeclaration(writer, info.Syntax, memberGroups);
        writer.WriteLine("{");
        writer.Indent++;
        EmitGetByteCountMethod(writer, memberGroups);
        writer.WriteEmptyLine();
        EmitWriteImplementationMethod(writer, memberGroups, true, utilities);
        EmitWriteImplementationMethod(writer, memberGroups, false, utilities);
        writer.WriteEmptyLine();
        EmitReadImplementationMethod(writer, info.Symbol, memberGroups, constructorParameters, true, utilities);
        EmitReadImplementationMethod(writer, info.Symbol, memberGroups, constructorParameters, false, utilities);
        writer.Indent--;
        writer.WriteLine("}");
        EmitOptionalNamespaceEnd(writer, info.Symbol);

        aaa2 = new Aaa2(diagnostics.ToImmutableArray(), sw.ToString(), ImmutableArray<UtilityData>.Empty);
        return true;
    }

    private static void EmitFileHeader(IndentedTextWriter writer)
    {
        writer.WriteMultiLine(
            """
// <auto-generated/>
#nullable enable

"""
        );
    }

    private static void EmitOptionalNamespaceStart(IndentedTextWriter writer, INamedTypeSymbol symbol)
    {
        var typeNamespace = symbol.GetNamespace();
        if (string.IsNullOrWhiteSpace(typeNamespace))
            return;
        writer.WriteLine($"namespace {typeNamespace}");
        writer.WriteLine("{");
        writer.Indent++;
    }

    private static void EmitAddTypeDeclaration(
        IndentedTextWriter writer,
        TypeDeclarationSyntax syntax,
        ImmutableArray<IGroup> memberGroups
    )
    {
        // Add xml docs
        IEnumerable<string> memberDocs = memberGroups
            .SelectMembers()
            .Select(memberInfo =>
                $"""/// <item> <term><see cref="{memberInfo.MemberSymbol.Name}"/></term> <description>{memberInfo.GetDocCommentLength()}</description> </item>"""
            );
        var summedConstantLength = memberGroups.Sum(x => x.ConstantByteLength);
        var variableLength = string.Join(
            " + ",
            memberGroups.OfType<IVariableMemberGroup>().Select(x => x.GetVariableDocCommentLength())
        );
        var summedLength = string.IsNullOrEmpty(variableLength)
            ? $"{summedConstantLength}"
            : string.Join(" + ", summedConstantLength, variableLength);
        writer.WriteMultiLine(
            $"""
/// <remarks> <list type="table">
/// <item> <term><b>Field</b></term> <description><b>Byte Length</b></description> </item>
{string.Join("\n", memberDocs)}
/// <item> <term> --- </term> <description>{summedLength}</description> </item>
/// </list> </remarks>
"""
        );
        // Add actual type. declaration
        var recordClassOrStruct =
            syntax is RecordDeclarationSyntax recordSyntax
            && !string.IsNullOrWhiteSpace(recordSyntax.ClassOrStructKeyword.Text)
                ? $" {recordSyntax.ClassOrStructKeyword}"
                : "";
        writer.WriteLine(
            $"{syntax.Modifiers} {syntax.Keyword}{recordClassOrStruct} {syntax.Identifier} : global::Darp.BinaryObjects.IWritable, global::Darp.BinaryObjects.ISpanReadable<{syntax.Identifier}>"
        );
    }

    private static void EmitGetByteCountMethod(IndentedTextWriter writer, ImmutableArray<IGroup> memberGroups)
    {
        var constantLength = memberGroups.Sum(x => x.ConstantByteLength);
        var variableLength = string.Join(
            "+",
            memberGroups.OfType<IVariableMemberGroup>().Select(x => x.GetVariableByteLength())
        );
        var summedLength = string.IsNullOrEmpty(variableLength)
            ? $"{constantLength}"
            : string.Join("+", constantLength, variableLength);
        writer.WriteMultiLine(
            $"""
/// <inheritdoc />
[global::System.Diagnostics.Contracts.Pure]
[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
{RoslynHelper.GetGeneratedVersionAttribute(fullNamespace: true)}
public int GetByteCount() => {summedLength};
"""
        );
    }

    private static void EmitWriteImplementationMethod(
        IndentedTextWriter writer,
        ImmutableArray<IGroup> memberGroups,
        bool littleEndian,
        List<UtilityData> utilities
    )
    {
        // Method start
        var methodNameEndianness = littleEndian ? "LittleEndian" : "BigEndian";
        writer.WriteMultiLine(
            $$"""
/// <inheritdoc />
{{RoslynHelper.GetGeneratedVersionAttribute(fullNamespace: true)}}
public bool TryWrite{{methodNameEndianness}}(global::System.Span<byte> destination) => TryWrite{{methodNameEndianness}}(destination, out _);
/// <inheritdoc />
{{RoslynHelper.GetGeneratedVersionAttribute(fullNamespace: true)}}
public bool TryWrite{{methodNameEndianness}}(global::System.Span<byte> destination, out int bytesWritten)
{
"""
        );
        writer.Indent++;
        writer.WriteLine("bytesWritten = 0;");
        writer.WriteEmptyLine();
        // All the members in the group
        var currentByteIndex = 0;
        foreach (IGroup memberInfoGroup in memberGroups)
        {
            // Ensure length of destination
            var offsetString = currentByteIndex > 0 ? "- bytesWritten " : "";
            var summedLength = memberInfoGroup.GetLengthCodeString();
            writer.WriteLine($"if (destination.Length {offsetString}< {summedLength})");
            writer.Indent++;
            writer.WriteLine("return false;");
            writer.Indent--;
            if (memberInfoGroup is IVariableMemberGroup variableGroup)
            {
                writer.WriteLine("");
                //currentByteIndex += variableGroup.ComputeLength();
            }
            else if (memberInfoGroup is ConstantBinaryMemberGroup constantGroup)
            {
                foreach (IConstantMember memberInfo in constantGroup.Members)
                {
                    if (
                        !memberInfo.TryGetWriteString(
                            littleEndian,
                            currentByteIndex,
                            out var writeString,
                            out var bytesWritten
                        )
                    )
                    {
                        continue;
                    }
                    writer.WriteMultiLine(writeString);
                    currentByteIndex += bytesWritten;
                }
            }
        }
        writer.WriteLine($"bytesWritten += {currentByteIndex};");
        writer.WriteEmptyLine();

        // The end of the method
        writer.WriteLine("return true;");
        writer.Indent--;
        writer.WriteLine("}");
    }

    private static void EmitReadImplementationMethod(
        IndentedTextWriter writer,
        INamedTypeSymbol symbol,
        ImmutableArray<IGroup> memberGroups,
        ImmutableHashSet<IMember> constructorParameters,
        bool littleEndian,
        List<UtilityData> utilities
    )
    {
        // Method start
        var methodNameEndianness = littleEndian ? "LittleEndian" : "BigEndian";

        var valueParameter = symbol.IsValueType
            ? $"out {symbol.Name}"
            : $"[global::System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out {symbol.Name}?";
        writer.WriteMultiLine(
            $$"""
/// <inheritdoc />
{{RoslynHelper.GetGeneratedVersionAttribute(fullNamespace: true)}}
public static bool TryRead{{methodNameEndianness}}(global::System.ReadOnlySpan<byte> source, {{valueParameter}} value) => TryRead{{methodNameEndianness}}(source, out value, out _);
/// <inheritdoc />
{{RoslynHelper.GetGeneratedVersionAttribute(fullNamespace: true)}}
public static bool TryRead{{methodNameEndianness}}(global::System.ReadOnlySpan<byte> source, {{valueParameter}} value, out int bytesRead)
{
"""
        );
        writer.Indent++;
        writer.WriteMultiLine(
            """
bytesRead = 0;
value = default;
"""
        );
        writer.WriteEmptyLine();
        var currentByteIndex = 0;
        foreach (IGroup memberInfoGroup in memberGroups)
        {
            // Ensure length of source
            var summedLength = memberInfoGroup.GetLengthCodeString();
            writer.WriteLine($"if (source.Length < {summedLength})");
            writer.Indent++;
            writer.WriteLine("return false;");
            writer.Indent--;

            if (memberInfoGroup is IVariableMemberGroup arrayMemberInfo) { }
            else if (memberInfoGroup is ConstantBinaryMemberGroup constantGroup)
            {
                foreach (IConstantMember memberInfo in constantGroup.Members)
                {
                    if (
                        !memberInfo.TryGetReadString(
                            methodNameEndianness,
                            currentByteIndex,
                            out var writeString,
                            out var bytesRead
                        )
                    )
                    {
                        continue;
                    }
                    writer.WriteMultiLine(writeString);
                    currentByteIndex += bytesRead;
                }
            }
        }
        writer.WriteLine($"bytesRead += {currentByteIndex};");
        writer.WriteEmptyLine();

        // Method end
        IEnumerable<string> constructorNames = constructorParameters.Select(x => $"{Prefix}read{x.MemberSymbol.Name}");
        writer.WriteMultiLine(
            $"""
value = new {symbol.Name}({string.Join(", ", constructorNames)});
return true;
"""
        );
        writer.Indent--;
        writer.WriteLine("}");
    }

    private static void EmitOptionalNamespaceEnd(IndentedTextWriter writer, INamedTypeSymbol symbol)
    {
        var typeNamespace = symbol.GetNamespace();
        if (string.IsNullOrWhiteSpace(typeNamespace))
            return;
        writer.WriteLine("}");
        writer.Indent--;
    }

    private static void EmitUtilityClass(IndentedTextWriter writer, IEnumerable<UtilityData> requestedUtilities)
    {
        writer.WriteEmptyLine();
        writer.WriteLine(
            $$"""
namespace Darp.BinaryObjects.Generated
{
    using System;
    using System.CodeDom.Compiler;
    using System.Runtime.CompilerServices;

    /// <summary>Helper methods used by generated BinaryObjects.</summary>
    {{RoslynHelper.GetGeneratedVersionAttribute(false)}}
    file static class Utilities
    {
"""
        );
        writer.Indent++;
        writer.Indent++;
        foreach ((WellKnownCollectionKind collectionKind, WellKnownTypeKind typeKind) in requestedUtilities)
        {
            EmitWriteUtility(writer, collectionKind, typeKind, true);
            EmitWriteUtility(writer, collectionKind, typeKind, false);
        }
        writer.Indent--;
        writer.WriteLine("}");
        writer.Indent--;
        writer.WriteLine("}");
    }
}

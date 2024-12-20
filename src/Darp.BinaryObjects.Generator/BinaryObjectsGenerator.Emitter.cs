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
        ImmutableArray<IMember> constructorParameters,
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
        EmitGetByteCountMethod(writer, info.Symbol, memberGroups);
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
        var memberDocs = memberGroups
            .SelectMembers()
            .Select(memberInfo =>
                $"""/// <item> <term><see cref="{memberInfo.MemberSymbol.Name}"/></term> <description>{memberInfo.GetDocCommentLength()}</description> </item>"""
            )
            .ToArray();
        //var summedConstantLength = memberGroups.Sum(x => x.ConstantByteLength);
        var summedLength = memberGroups.Length switch
        {
            > 0 => string.Join(
                " + ",
                memberGroups.Select(x => x.GetVariableDocCommentLength() ?? $"{x.ConstantByteLength}")
            ),
            _ => "0",
        };
        writer.WriteMultiLine(
            """
/// <remarks> <list type="table">
/// <item> <term><b>Field</b></term> <description><b>Byte Length</b></description> </item>
"""
        );
        if (memberDocs.Length > 0)
            writer.WriteMultiLine(string.Join("\n", memberDocs));
        writer.WriteMultiLine(
            $"""
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

        var isPure = memberGroups.SelectMembers().All(x => x is IConstantMember);
        if (isPure)
        {
            var constantLength = memberGroups.Sum(x => x.ConstantByteLength);
            writer.WriteLine($"[global::Darp.BinaryObjects.BinaryConstant({constantLength})]");
        }
        writer.WriteLine(
            $"{syntax.Modifiers} {syntax.Keyword}{recordClassOrStruct} {syntax.Identifier} : global::Darp.BinaryObjects.IBinaryObject<{syntax.Identifier}>"
        );
    }

    private static void EmitGetByteCountMethod(
        IndentedTextWriter writer,
        INamedTypeSymbol infoSymbol,
        ImmutableArray<IGroup> memberGroups
    )
    {
        var summedLength = memberGroups.Length switch
        {
            > 0 => string.Join(" + ", memberGroups.Select(x => x.GetVariableByteLength() ?? $"{x.ConstantByteLength}")),
            _ => "0",
        };
        writer.WriteLine("/// <inheritdoc />");
        var isPure = memberGroups.SelectMembers().All(x => x is IConstantMember);
        if (isPure)
            writer.WriteLine("[global::System.Diagnostics.Contracts.Pure]");
        writer.WriteMultiLine(
            $"""
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
        foreach ((IGroup? memberInfoGroup, var index) in memberGroups.Select((group, i) => (group, i)))
        {
            var currentByteIndex = 0;
            if (memberInfoGroup is BinaryObjectMemberGroup binaryObjectsGroup)
            {
                var endianness = littleEndian ? "LittleEndian" : "BigEndian";
                var bytesWrittenVariable = $"{Prefix}bytesWritten{binaryObjectsGroup.MemberSymbol.Name}";
                writer.WriteLine(
                    $"if (!this.{binaryObjectsGroup.MemberSymbol.Name}.TryWrite{endianness}(destination[{currentByteIndex}..], out var {bytesWrittenVariable}))"
                );
                writer.Indent++;
                writer.WriteLine("return false;");
                writer.Indent--;
                if (index != memberGroups.Length - 1)
                {
                    writer.WriteLine($"destination = destination[{bytesWrittenVariable}..];");
                }
                if (index != memberGroups.Length - 1)
                {
                    writer.WriteLine($"destination = destination[{currentByteIndex}..];");
                }
                writer.WriteLine($"bytesWritten += {bytesWrittenVariable};");
                writer.WriteEmptyLine();
            }
            else if (memberInfoGroup is IVariableMemberGroup variableGroup)
            {
                if (
                    !variableGroup.TryGetWriteString(
                        littleEndian,
                        currentByteIndex,
                        out var writeString,
                        out var bytesWrittenString
                    )
                )
                {
                    continue;
                }

                writer.WriteMultiLine(writeString);
                if (index != memberGroups.Length - 1)
                {
                    writer.WriteLine($"destination = destination[{bytesWrittenString}..];");
                }
                if (bytesWrittenString is not null)
                {
                    writer.WriteLine($"bytesWritten += {bytesWrittenString};");
                }
                writer.WriteEmptyLine();
            }
            else if (memberInfoGroup is ConstantBinaryMemberGroup constantGroup)
            {
                // Ensure length of destination
                var summedLength = memberInfoGroup.GetLengthCodeString();
                writer.WriteLine($"if (destination.Length < {summedLength})");
                writer.Indent++;
                writer.WriteLine("return false;");
                writer.Indent--;
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
                if (index != memberGroups.Length - 1)
                {
                    writer.WriteLine($"destination = destination[{currentByteIndex}..];");
                }
                writer.WriteLine($"bytesWritten += {currentByteIndex};");
                writer.WriteEmptyLine();
            }
        }

        // The end of the method
        writer.WriteLine("return true;");
        writer.Indent--;
        writer.WriteLine("}");
    }

    private static void EmitReadImplementationMethod(
        IndentedTextWriter writer,
        INamedTypeSymbol symbol,
        ImmutableArray<IGroup> memberGroups,
        ImmutableArray<IMember> constructorParameters,
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
        writer.WriteLine("bytesRead = 0;");
        if (memberGroups.Length > 0)
        {
            writer.WriteLine("value = default;");
        }
        writer.WriteEmptyLine();
        foreach ((IGroup? memberInfoGroup, var index) in memberGroups.Select((group, i) => (group, i)))
        {
            var currentByteIndex = 0;
            if (memberInfoGroup is BinaryObjectMemberGroup binaryObjectsGroup)
            {
                var endianness = littleEndian ? "LittleEndian" : "BigEndian";
                var variableName = $"{Prefix}read{binaryObjectsGroup.MemberSymbol.Name}";
                var bytesReadVariable = $"{Prefix}bytesRead{binaryObjectsGroup.MemberSymbol.Name}";
                writer.WriteLine(
                    $"if (!{binaryObjectsGroup.TypeSymbol.ToDisplayString()}.TryRead{endianness}(source[{currentByteIndex}..], out var {variableName}, out var {bytesReadVariable}))"
                );
                writer.Indent++;
                writer.WriteLine("return false;");
                writer.Indent--;
                if (index != memberGroups.Length - 1)
                {
                    writer.WriteLine($"source = source[{bytesReadVariable}..];");
                }
                writer.WriteLine($"bytesRead += {bytesReadVariable};");
                writer.WriteEmptyLine();
            }
            else if (memberInfoGroup is IVariableMemberGroup arrayMemberInfo)
            {
                if (
                    !arrayMemberInfo.TryGetReadString(
                        littleEndian,
                        currentByteIndex,
                        out var readString,
                        out var bytesReadString
                    )
                )
                {
                    continue;
                }
                writer.WriteMultiLine(readString);
                if (index != memberGroups.Length - 1)
                {
                    writer.WriteLine($"source = source[{bytesReadString}..];");
                }
                writer.WriteLine($"bytesRead += {bytesReadString};");
                writer.WriteEmptyLine();
            }
            else if (memberInfoGroup is ConstantBinaryMemberGroup constantGroup)
            {
                // Ensure length of source
                var summedLength = memberInfoGroup.GetLengthCodeString();
                writer.WriteLine($"if (source.Length < {summedLength})");
                writer.Indent++;
                writer.WriteLine("return false;");
                writer.Indent--;
                foreach (IConstantMember memberInfo in constantGroup.Members)
                {
                    if (
                        !memberInfo.TryGetReadString(
                            littleEndian,
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
                if (index != memberGroups.Length - 1)
                {
                    writer.WriteLine($"source = source[{currentByteIndex}..];");
                }
                if (constantGroup.Members.Count > 0)
                {
                    writer.WriteLine($"bytesRead += {currentByteIndex};");
                    writer.WriteEmptyLine();
                }
            }
        }

        // Method end
        IEnumerable<string> constructorNames = constructorParameters.Select(x => $"{Prefix}read{x.MemberSymbol.Name}");
        writer.Write($"value = new {symbol.Name}({string.Join(", ", constructorNames)})");
        IMember[] objectInitializerMembers = memberGroups.SelectMembers().Except(constructorParameters).ToArray();
        if (objectInitializerMembers.Length > 0)
        {
            writer.WriteLine();
            writer.WriteLine("{");
            writer.Indent++;
            foreach (IMember x in objectInitializerMembers)
            {
                writer.WriteLine($"{x.MemberSymbol.Name} = {Prefix}read{x.MemberSymbol.Name},");
            }
            writer.Indent--;
            writer.Write("}");
        }
        writer.WriteLine(";");
        writer.WriteLine("return true;");
        writer.Indent--;
        writer.WriteLine("}");
    }

    private static void EmitOptionalNamespaceEnd(IndentedTextWriter writer, INamedTypeSymbol symbol)
    {
        var typeNamespace = symbol.GetNamespace();
        if (string.IsNullOrWhiteSpace(typeNamespace))
            return;
        writer.Indent--;
        writer.WriteLine("}");
    }

    private static void EmitUtilityClass(IndentedTextWriter writer, ImmutableArray<UtilityData> requestedUtilities)
    {
        if (requestedUtilities.Length == 0)
        {
            return;
        }
        writer.WriteLine(
            $$"""
namespace Darp.BinaryObjects.Generated
{
    using Darp.BinaryObjects;
    using System;
    using System.Buffers.Binary;
    using System.CodeDom.Compiler;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    /// <summary>Helper methods used by generated BinaryObjects.</summary>
    {{RoslynHelper.GetGeneratedVersionAttribute(false)}}
    file static class Utilities
    {
"""
        );
        writer.Indent++;
        writer.Indent++;
        foreach (var utilityData in requestedUtilities.OrderBy(x => x.TypeKind).ThenBy(x => x.CollectionKind))
        {
            (
                var isReadUtility,
                WellKnownCollectionKind collectionKind,
                WellKnownTypeKind typeKind,
                var constLength,
                var emitLittleAndBigEndian
            ) = utilityData;
            if (isReadUtility)
            {
                EmitReadUtility(writer, collectionKind, typeKind, constLength, emitLittleAndBigEndian);
            }
            else
            {
                EmitWriteUtility(writer, collectionKind, typeKind, emitLittleAndBigEndian);
            }
        }
        writer.Indent--;
        writer.WriteLine("}");
        writer.Indent--;
        writer.WriteLine("}");
    }
}

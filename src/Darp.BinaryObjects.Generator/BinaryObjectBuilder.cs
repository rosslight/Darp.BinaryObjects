namespace Darp.BinaryObjects.Generator;

using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

internal sealed class BinaryObjectBuilder(
    INamedTypeSymbol symbol,
    TypeDeclarationSyntax syntax,
    IReadOnlyList<IGroup> members,
    List<DiagnosticData> diagnostics
)
{
    public const string Prefix = "___";

    public StringBuilder StringBuilder { get; } = new();
    private readonly INamedTypeSymbol _symbol = symbol;
    private readonly TypeDeclarationSyntax _syntax = syntax;
    private readonly IReadOnlyList<IGroup> _members = members;

    public List<DiagnosticData> Diagnostics { get; } = diagnostics;

    public static BinaryObjectBuilder Create(INamedTypeSymbol symbol, TypeDeclarationSyntax syntax)
    {
        List<DiagnosticData> diagnostics = [];
        List<IMember> members = [];
        foreach (
            ISymbol memberSymbol in symbol
                .GetMembers()
                .Where(x => x.Kind is SymbolKind.Field or SymbolKind.Property)
                .Where(x => !x.IsImplicitlyDeclared)
        // .Where(x => x is not IPropertySymbol { IsReadOnly: true } && x is not IFieldSymbol { IsReadOnly: true })
        )
        {
            if (!TryGet(diagnostics, members, memberSymbol, out IMember? info))
                continue;
            members.Add(info);
        }

        IGroup[] groupedMembers = members.GroupInfos().ToArray();
        return new BinaryObjectBuilder(symbol, syntax, groupedMembers, diagnostics);
    }

    public static bool TryGet(
        List<DiagnosticData> diagnostics,
        IReadOnlyList<IMember> previousMembers,
        ISymbol symbol,
        [NotNullWhen(true)] out IMember? info
    )
    {
        info = null;
        ITypeSymbol? typeSymbol = symbol switch
        {
            IPropertySymbol s => s.Type,
            IFieldSymbol s => s.Type,
            _ => default,
        };
        if (typeSymbol is null)
            return false;
        ITypeSymbol arrayTypeSymbol = typeSymbol;
        if (typeSymbol.TryGetArrayType(out ArrayKind arrayKind, out ITypeSymbol? underlyingTypeSymbol))
            typeSymbol = underlyingTypeSymbol;
        if (!typeSymbol.TryGetLength(out var length))
        {
            var diagnostic = DiagnosticData.Create(
                descriptor: DiagnosticDescriptors.MemberTypeNotSupported,
                location: symbol.GetSourceLocation()
            );
            diagnostics.Add(diagnostic);
            return false;
        }

        int? arrayMinLength = null;
        IMember? arrayLengthMember = null;
        int? arrayLength = null;
        ImmutableArray<AttributeData> attributes = symbol.GetAttributes();
        foreach (AttributeData attributeData in attributes)
        {
            if (attributeData.AttributeClass is null)
                continue;
            switch (attributeData.AttributeClass.ToDisplayString())
            {
                case "Darp.BinaryObjects.BinaryIgnoreAttribute":
                    return false;
                case "Darp.BinaryObjects.BinaryElementCountAttribute":
                    foreach (KeyValuePair<string, TypedConstant> pair in attributeData.GetArguments())
                    {
                        if (pair is { Key: "memberWithLength", Value.Value: string memberName })
                        {
                            IMember? previousMember = previousMembers.FirstOrDefault(x =>
                                x.TypeSymbol.Name.Equals(memberName, StringComparison.Ordinal)
                            );
                            if (previousMember is null)
                            {
                                var diagnostic = DiagnosticData.Create(
                                    descriptor: DiagnosticDescriptors.MemberDefiningLengthNotFound,
                                    location: attributeData.GetLocationOfConstructorArgument(0)
                                        ?? symbol.GetSourceLocation(),
                                    messageArgs: [memberName, symbol.Name]
                                );
                                diagnostics.Add(diagnostic);
                                return false;
                            }
                            if (!previousMember.TypeSymbol.IsValidLengthInteger())
                            {
                                var diagnostic = DiagnosticData.Create(
                                    descriptor: DiagnosticDescriptors.MemberDefiningLengthDataInvalidType,
                                    location: previousMember.TypeSymbol.GetSourceLocation(),
                                    messageArgs: [memberName, symbol.Name]
                                );
                                diagnostics.Add(diagnostic);
                                return false;
                            }
                            arrayLengthMember = previousMember;
                        }
                        else if (pair is { Key: "length", Value.Value: int lengthValue })
                        {
                            arrayLength = lengthValue;
                        }
                    }
                    continue;
                case "Darp.BinaryObjects.BinaryReadRemainingAttribute":
                    continue;
                case "Darp.BinaryObjects.BinaryByteLengthAttribute":
                    foreach (KeyValuePair<string, TypedConstant> pair in attributeData.GetArguments())
                    {
                        if (pair is { Key: "byteLength", Value.Value: int value })
                        {
                            length = value;
                        }
                    }
                    continue;
                default:
                    continue;
            }
        }
        info = (arrayKind, arrayLength, arrayMinLength, arrayLengthMember) switch
        {
            (not ArrayKind.None, _, not null, not null) => new VariableArrayMemberGroup
            {
                MemberSymbol = symbol,
                TypeSymbol = typeSymbol,
                TypeByteLength = length,
                ArrayTypeSymbol = arrayTypeSymbol,
                ArrayKind = arrayKind,
                ArrayMinLength = arrayMinLength.Value,
                ArrayLengthMemberName = arrayLengthMember.TypeSymbol.Name,
            },
            (not ArrayKind.None, not null, _, _) => new ConstantArrayMember
            {
                MemberSymbol = symbol,
                TypeSymbol = typeSymbol,
                TypeByteLength = length,
                ArrayTypeSymbol = arrayTypeSymbol,
                ArrayKind = arrayKind,
                ArrayLength = arrayLength.Value,
            },
            (ArrayKind.None, _, _, _) => new ConstantPrimitiveMember
            {
                MemberSymbol = symbol,
                TypeByteLength = length,
                TypeSymbol = typeSymbol,
            },
            _ => throw new ArgumentOutOfRangeException(nameof(symbol)),
        };
        return true;
    }

    public void AddFileHeader()
    {
        StringBuilder.AppendLine(
            """
// <auto-generated/>
#nullable enable

using BinaryHelpers = global::Darp.BinaryObjects.BinaryHelpers;
using NotNullWhenAttribute = global::System.Diagnostics.CodeAnalysis.NotNullWhenAttribute;
"""
        );
    }

    public void TryAddNamespace()
    {
        var typeNamespace = _symbol.GetNamespace();
        if (string.IsNullOrWhiteSpace(typeNamespace))
            return;
        StringBuilder.AppendLine($"namespace {typeNamespace};");
        StringBuilder.AppendLine();
    }

    public bool TryAddTypeDeclaration()
    {
        // Add xml docs
        IEnumerable<string> memberDocs = _members
            .SelectMembers()
            .Select(memberInfo =>
                $"""/// <item> <term><see cref="{memberInfo.MemberSymbol.Name}"/></term> <description>{memberInfo.GetDocCommentLength()}</description> </item>"""
            );
        var summedConstantLength = _members.Sum(x => x.ConstantByteLength);
        var variableLength = string.Join(
            " + ",
            _members.OfType<IVariableMemberGroup>().Select(x => x.GetVariableDocCommentLength())
        );
        var summedLength = string.IsNullOrEmpty(variableLength)
            ? $"{summedConstantLength}"
            : string.Join(" + ", summedConstantLength, variableLength);
        StringBuilder.AppendLine(
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
            _syntax is RecordDeclarationSyntax recordSyntax
            && !string.IsNullOrWhiteSpace(recordSyntax.ClassOrStructKeyword.Text)
                ? $" {recordSyntax.ClassOrStructKeyword}"
                : "";
        StringBuilder.AppendLine(
            $"{_syntax.Modifiers} {_syntax.Keyword}{recordClassOrStruct} {_syntax.Identifier} : global::Darp.BinaryObjects.IWritable, global::Darp.BinaryObjects.ISpanReadable<{_syntax.Identifier}>"
        );
        return true;
    }

    public override string ToString() => StringBuilder.ToString().Replace("\r", "");

    public void AddGetByteCountMethod()
    {
        var constantLength = _members.Sum(x => x.ConstantByteLength);
        var variableLength = string.Join(
            "+",
            _members.OfType<IVariableMemberGroup>().Select(x => x.GetVariableByteLength())
        );
        var summedLength = string.IsNullOrEmpty(variableLength)
            ? $"{constantLength}"
            : string.Join("+", constantLength, variableLength);
        StringBuilder.AppendLine(
            $"""
    /// <inheritdoc />
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    {RoslynHelper.GetGeneratedVersionAttribute(fullNamespace: true)}
    public int GetByteCount() => {summedLength};
"""
        );
    }

    public void AddWriteImplementationMethod(bool littleEndian)
    {
        // Method start
        var methodNameEndianness = littleEndian ? "LittleEndian" : "BigEndian";
        StringBuilder.AppendLine(
            $$"""
    /// <inheritdoc />
    {{RoslynHelper.GetGeneratedVersionAttribute(fullNamespace: true)}}
    public bool TryWrite{{methodNameEndianness}}(global::System.Span<byte> destination) => TryWrite{{methodNameEndianness}}(destination, out _);
    /// <inheritdoc />
    {{RoslynHelper.GetGeneratedVersionAttribute(fullNamespace: true)}}
    public bool TryWrite{{methodNameEndianness}}(global::System.Span<byte> destination, out int bytesWritten)
    {
        bytesWritten = 0;

"""
        );
        // All the members in the group
        var currentByteIndex = 0;
        foreach (IGroup memberInfoGroup in _members)
        {
            // Ensure length of destination
            var offsetString = currentByteIndex > 0 ? "- bytesWritten " : "";
            var summedLength = memberInfoGroup.GetLengthCodeString();
            StringBuilder.AppendLine(
                $"""
                        if (destination.Length {offsetString}< {summedLength})
                            return false;
                """
            );
            if (memberInfoGroup is IVariableMemberGroup variableGroup)
            {
                StringBuilder.AppendLine("");
                //currentByteIndex += variableGroup.ComputeLength();
            }
            else if (memberInfoGroup is ConstantBinaryMemberGroup constantGroup)
            {
                foreach (IConstantMember memberInfo in constantGroup.Members)
                {
                    if (
                        !memberInfo.TryGetWriteString(
                            methodNameEndianness,
                            currentByteIndex,
                            out var writeString,
                            out var bytesWritten
                        )
                    )
                    {
                        continue;
                    }
                    StringBuilder.AppendLine(writeString);
                    currentByteIndex += bytesWritten;
                }
            }
        }
        StringBuilder.AppendLine($"        bytesWritten += {currentByteIndex};");

        // The end of the method
        StringBuilder.AppendLine(
            """

        return true;
    }
"""
        );
    }

    public void AddReadImplementationMethod(bool littleEndian)
    {
        // Method start
        var methodNameEndianness = littleEndian ? "LittleEndian" : "BigEndian";

        var valueParameter = _symbol.IsValueType ? $"out {_symbol.Name}" : $"[NotNullWhen(true)] out {_symbol.Name}?";
        StringBuilder.AppendLine(
            $$"""
    /// <inheritdoc />
    {{RoslynHelper.GetGeneratedVersionAttribute(fullNamespace: true)}}
    public static bool TryRead{{methodNameEndianness}}(global::System.ReadOnlySpan<byte> source, {{valueParameter}} value) => TryRead{{methodNameEndianness}}(source, out value, out _);
    /// <inheritdoc />
    {{RoslynHelper.GetGeneratedVersionAttribute(fullNamespace: true)}}
    public static bool TryRead{{methodNameEndianness}}(global::System.ReadOnlySpan<byte> source, {{valueParameter}} value, out int bytesRead)
    {
        bytesRead = 0;
        value = default;

"""
        );
        List<string> constructorParameters = [];
        var currentByteIndex = 0;
        foreach (IGroup memberInfoGroup in _members)
        {
            // Ensure length of source
            var summedLength = memberInfoGroup.GetLengthCodeString();
            StringBuilder.AppendLine(
                $"""
                        if (source.Length < {summedLength})
                            return false;
                """
            );

            if (memberInfoGroup is IVariableMemberGroup arrayMemberInfo) { }
            else if (memberInfoGroup is ConstantBinaryMemberGroup constantGroup)
            {
                foreach (IConstantMember memberInfo in constantGroup.Members)
                {
                    var variableName = $"{Prefix}read{memberInfo.MemberSymbol.Name}";
                    constructorParameters.Add(variableName);
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
                    StringBuilder.AppendLine(writeString);
                    currentByteIndex += bytesRead;
                }
            }
        }
        StringBuilder.AppendLine($"        bytesRead += {currentByteIndex};");

        // Method end
        StringBuilder.AppendLine(
            $$"""

        value = new {{_symbol.Name}}({{string.Join(", ", constructorParameters)}});
        return true;
    }
"""
        );
    }
}

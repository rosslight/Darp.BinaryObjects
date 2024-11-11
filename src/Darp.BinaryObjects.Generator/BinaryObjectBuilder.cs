namespace Darp.BinaryObjects.Generator;

using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static BuilderHelper;

internal sealed class BinaryObjectBuilder(
    INamedTypeSymbol symbol,
    TypeDeclarationSyntax syntax,
    IReadOnlyList<BinaryMemberInfo> members,
    List<Diagnostic> diagnostics
)
{
    public StringBuilder StringBuilder { get; } = new();
    private readonly INamedTypeSymbol _symbol = symbol;
    private readonly TypeDeclarationSyntax _syntax = syntax;
    private readonly IReadOnlyList<BinaryMemberInfo> _members = members;

    public List<Diagnostic> Diagnostics { get; } = diagnostics;

    public static BinaryObjectBuilder Create(INamedTypeSymbol symbol, TypeDeclarationSyntax syntax)
    {
        List<Diagnostic> diagnostics = [];
        List<BinaryMemberInfo> members = [];
        foreach (
            ISymbol memberSymbol in symbol
                .GetMembers()
                .Where(x => x.Kind is SymbolKind.Field or SymbolKind.Property)
                .Where(x => !x.IsImplicitlyDeclared)
        )
        {
            if (!TryGet(diagnostics, members, memberSymbol, out BinaryMemberInfo? info))
                continue;
            members.Add(info);
        }
        return new BinaryObjectBuilder(symbol, syntax, members, diagnostics);
    }

    public static bool TryGet(
        List<Diagnostic> diagnostics,
        IReadOnlyList<BinaryMemberInfo> previousMembers,
        ISymbol symbol,
        [NotNullWhen(true)] out BinaryMemberInfo? info
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
            var diagnostic = Diagnostic.Create(
                descriptor: DiagnosticDescriptors.MemberTypeNotSupported,
                location: symbol.GetSourceLocation()
            );
            diagnostics.Add(diagnostic);
            return false;
        }

        int? arrayMinLength = null;
        BinaryMemberInfo? arrayLengthMember = null;
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
                            BinaryMemberInfo? previousMember = previousMembers.FirstOrDefault(x =>
                                x.Symbol.Name.Equals(memberName, StringComparison.Ordinal)
                            );
                            if (previousMember is null)
                            {
                                var diagnostic = Diagnostic.Create(
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
                                var diagnostic = Diagnostic.Create(
                                    descriptor: DiagnosticDescriptors.MemberDefiningLengthDataInvalidType,
                                    location: previousMember.Symbol.GetSourceLocation(),
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

        if (arrayKind is ArrayKind.None)
        {
            info = new BinaryMemberInfo
            {
                Symbol = symbol,
                TypeByteLength = length,
                TypeSymbol = typeSymbol,
            };
            return true;
        }
        info = new BinaryArrayMemberInfo
        {
            Symbol = symbol,
            TypeByteLength = length,
            TypeSymbol = typeSymbol,
            ArrayTypeSymbol = arrayTypeSymbol,
            ArrayKind = arrayKind,
            ArrayMinimumLength = arrayMinLength,
            ArrayLengthMember = arrayLengthMember,
            ArrayAbsoluteLength = arrayLength,
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
        IEnumerable<string> memberDocs = _members.Select(memberInfo =>
        {
            var length = memberInfo switch
            {
                BinaryArrayMemberInfo arrayInfo => GetLengthDoc(arrayInfo),
                _ => "",
            };
            return $"""/// <item> <term><see cref="{memberInfo.Symbol.Name}"/></term> <description>{memberInfo.TypeByteLength}{length}</description> </item>""";
        });
        var summedLength = _members.ComputeLength();
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

        string GetLengthDoc(BinaryArrayMemberInfo info)
        {
            if (info.ArrayLengthMember is not null)
                return $""" * <see cref="{info.ArrayLengthMember.Symbol.Name}"/>""";
            if (info.ArrayAbsoluteLength is not null)
                return $" * {info.ArrayAbsoluteLength}";
            var minLength = info.ArrayMinimumLength ?? 0;
            return minLength > 0 ? $" * ({minLength} + k)" : " * k";
        }
    }

    public override string ToString() => StringBuilder.ToString().Replace("\r", "");

    public void AddGetByteCountMethod()
    {
        var summedLength = _members.ComputeLength();
        StringBuilder.AppendLine(
            $"""
    /// <inheritdoc />
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
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
    public bool TryWrite{{methodNameEndianness}}(global::System.Span<byte> destination) => TryWrite{{methodNameEndianness}}(destination, out _);
    /// <inheritdoc />
    public bool TryWrite{{methodNameEndianness}}(global::System.Span<byte> destination, out int bytesWritten)
    {
        bytesWritten = 0;

"""
        );
        // Ensure length of destination
        var summedLength = _members.ComputeLength();
        StringBuilder.AppendLine(
            $"""
        if (destination.Length < {summedLength})
            return false;
"""
        );
        // All the members in the group
        var currentByteIndex = 0;
        foreach (BinaryMemberInfo memberInfo in _members)
        {
            string? write;
            if (memberInfo is BinaryArrayMemberInfo arrayMemberInfo)
            {
                var maxLength = arrayMemberInfo.ArrayAbsoluteLength ?? 0;
                (string MethodName, Func<string, string>? Func)? x = (
                    arrayMemberInfo.ArrayKind,
                    arrayMemberInfo.TypeSymbol.ToString()
                ) switch
                {
                    (ArrayKind.Memory, "byte") => ("WriteUInt8Span", s => $"{s}.Span"),
                    (ArrayKind.Memory, "ushort") => ($"WriteUInt16Span{methodNameEndianness}", s => $"{s}.Span"),
                    (ArrayKind.Array, "byte") => ("WriteUInt8Span", null),
                    (ArrayKind.Array, "ushort") => ($"WriteUInt16Span{methodNameEndianness}", null),
                    (ArrayKind.List, "byte") => ("WriteUInt8List", null),
                    (ArrayKind.List, "ushort") => ($"WriteUInt16List{methodNameEndianness}", null),
                    (ArrayKind.Enumerable, "byte") => ("WriteUInt8Enumerable", null),
                    (ArrayKind.Enumerable, "ushort") => ($"WriteUInt16Enumerable{methodNameEndianness}", null),
                    _ => null,
                };
                if (x is null)
                    continue;
                write = arrayMemberInfo.GetWriteArrayString(
                    x.Value.MethodName,
                    currentByteIndex,
                    maxLength,
                    x.Value.Func
                );
            }
            else
            {
                var methodName = memberInfo.TypeSymbol.ToDisplayString() switch
                {
                    "bool" => "WriteBool",
                    "sbyte" => "WriteInt8",
                    "byte" => "WriteUInt8",
                    "short" => $"WriteInt16{methodNameEndianness}",
                    "ushort" => $"WriteUInt16{methodNameEndianness}",
                    "System.Half" => $"WriteHalf{methodNameEndianness}",
                    "char" => $"WriteChar{methodNameEndianness}",
                    "int" => $"WriteInt32{methodNameEndianness}",
                    "uint" => $"WriteUInt32{methodNameEndianness}",
                    "float" => $"WriteSingle{methodNameEndianness}",
                    "long" => $"WriteInt64{methodNameEndianness}",
                    "ulong" => $"WriteUInt64{methodNameEndianness}",
                    "double" => $"WriteDouble{methodNameEndianness}",
                    "System.Int128" => $"WriteInt128{methodNameEndianness}",
                    "System.UInt128" => $"WriteUInt128{methodNameEndianness}",
                    _ => null,
                };
                if (methodName is null)
                    continue;
                write = memberInfo.GetWriteString(methodName, currentByteIndex);
            }
            StringBuilder.AppendLine(write);
            currentByteIndex += memberInfo.ComputeLength();
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
        const string prefix = "___";

        // Method start
        var methodNameEndianness = littleEndian ? "LittleEndian" : "BigEndian";

        var valueParameter = _symbol.IsValueType ? $"out {_symbol.Name}" : $"[NotNullWhen(true)] out {_symbol.Name}?";
        StringBuilder.AppendLine(
            $$"""
    /// <inheritdoc />
    public static bool TryRead{{methodNameEndianness}}(global::System.ReadOnlySpan<byte> source, {{valueParameter}} value) => TryRead{{methodNameEndianness}}(source, out value, out _);
    /// <inheritdoc />
    public static bool TryRead{{methodNameEndianness}}(global::System.ReadOnlySpan<byte> source, {{valueParameter}} value, out int bytesRead)
    {
        bytesRead = 0;
        value = default;

"""
        );
        // Ensure length of source
        var summedLength = _members.ComputeLength();
        StringBuilder.AppendLine(
            $"""
        if (source.Length < {summedLength})
            return false;
"""
        );
        List<string> constructorParameters = [];
        var currentByteIndex = 0;
        foreach (BinaryMemberInfo memberInfo in _members)
        {
            var variableName = $"{prefix}read{memberInfo.Symbol.Name}";
            constructorParameters.Add(variableName);
            string write;
            if (memberInfo is BinaryArrayMemberInfo arrayMemberInfo)
            {
                var maxLength = arrayMemberInfo.ComputeLength();
                var methodName = (arrayMemberInfo.ArrayKind, arrayMemberInfo.TypeSymbol.ToString()) switch
                {
                    (ArrayKind.Array or ArrayKind.Memory or ArrayKind.Enumerable, "byte") => "ReadUInt8Array",
                    (ArrayKind.Array or ArrayKind.Memory or ArrayKind.Enumerable, "ushort") =>
                        $"ReadUInt16Array{methodNameEndianness}",
                    (ArrayKind.List, "byte") => "ReadUInt8List",
                    (ArrayKind.List, "ushort") => $"ReadUInt16List{methodNameEndianness}",
                    _ => null,
                };
                if (methodName is null)
                    continue;
                write = GetReadArrayString(variableName, methodName, currentByteIndex, maxLength);
            }
            else
            {
                var methodName = memberInfo.TypeSymbol.ToDisplayString() switch
                {
                    "bool" => "ReadBool",
                    "sbyte" => "ReadInt8",
                    "byte" => "ReadUInt8",
                    "short" => $"ReadInt16{methodNameEndianness}",
                    "ushort" => $"ReadUInt16{methodNameEndianness}",
                    "System.Half" => $"ReadHalf{methodNameEndianness}",
                    "char" => $"ReadChar{methodNameEndianness}",
                    "int" => $"ReadInt32{methodNameEndianness}",
                    "uint" => $"ReadUInt32{methodNameEndianness}",
                    "float" => $"ReadSingle{methodNameEndianness}",
                    "long" => $"ReadInt64{methodNameEndianness}",
                    "ulong" => $"ReadUInt64{methodNameEndianness}",
                    "double" => $"ReadDouble{methodNameEndianness}",
                    "System.Int128" => $"ReadInt128{methodNameEndianness}",
                    "System.UInt128" => $"ReadUInt128{methodNameEndianness}",
                    _ => null,
                };
                if (methodName is null)
                    continue;
                write = GetReadString(variableName, methodName, currentByteIndex);
            }
            StringBuilder.AppendLine(write);
            currentByteIndex += memberInfo.ComputeLength();
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

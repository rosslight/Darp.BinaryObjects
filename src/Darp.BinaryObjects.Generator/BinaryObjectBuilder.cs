namespace Darp.BinaryObjects.Generator;

using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

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
                case "Darp.BinaryObjects.BinaryArrayLengthAttribute":
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
        StringBuilder.Append(
            """
            // <auto-generated/>
            #nullable enable

            """
        );
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
        var summedLength = _members.Aggregate(0, (a, b) => a + b.TypeByteLength);
        StringBuilder.AppendLine(
            $"""
/// <remarks> <list type="table">
/// <item> <term><b>Field</b></term> <description><b>Byte Length</b></description> </item>
{string.Join("\n", memberDocs)}
/// <item> <term> --- </term> <description>{summedLength}</description> </item>
/// </list> </remarks>
"""
        );
        // Add actual type declaration
        StringBuilder.AppendLine(
            $"{_syntax.Modifiers} {_syntax.Keyword} {_syntax.Identifier} : global::Darp.BinaryObjects.IWritable"
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
        var summedLength = _members.Aggregate(0, (a, b) => a + b.TypeByteLength);
        StringBuilder.AppendLine(
            $"""
    /// <inheritdoc />
    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public int GetByteCount() => {summedLength};
"""
        );
    }

    public void AddWriteImplementationMethod()
    {
        StringBuilder.AppendLine(
            """
    private bool TryWrite(global::System.Span<byte> destination, out int bytesWritten, bool writeLittleEndian)
    {
        if (destination.Length < GetByteCount())
        {
            bytesWritten = 0;
            return false;
        }
"""
        );
        var currentByteIndex = 0;
        foreach (BinaryMemberInfo memberInfo in _members)
        {
            if (memberInfo is BinaryArrayMemberInfo arrayMemberInfo)
                continue;
            var write = memberInfo.TypeSymbol.ToDisplayString() switch
            {
                "bool" =>
                    $"        destination[{currentByteIndex}] = {memberInfo.Symbol.Name} ? (byte)0b1 : (byte)0b0;",
                _ => null,
            };
            StringBuilder.AppendLine(write);
            currentByteIndex += memberInfo.TypeByteLength;
        }
        StringBuilder.AppendLine(
            """
        bytesWritten = 1;
        return true;
    }
"""
        );
    }

    public void AddWriteBoilerplate()
    {
        StringBuilder.AppendLine(
            """
    /// <inheritdoc />
    public bool TryWriteLittleEndian(global::System.Span<byte> destination) => TryWrite(destination, out _, true);
    /// <inheritdoc />
    public bool TryWriteLittleEndian(global::System.Span<byte> destination, out int bytesWritten) => TryWrite(destination, out bytesWritten, true);
    /// <inheritdoc />
    public bool TryWriteBigEndian(global::System.Span<byte> destination) => TryWrite(destination, out _, false);
    /// <inheritdoc />
    public bool TryWriteBigEndian(global::System.Span<byte> destination, out int bytesWritten) => TryWrite(destination, out bytesWritten, false);
"""
        );
    }

    public void AddReadImplementationMethod()
    {
        const string prefix = "___";
        var summedLength = _members.Aggregate(0, (a, b) => a + b.TypeByteLength);

        var valueParameter = _symbol.IsValueType
            ? $"out {_symbol.Name}"
            : $"[global::System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out {_symbol.Name}?";
        StringBuilder.AppendLine(
            $$"""
    private static bool TryRead(global::System.ReadOnlySpan<byte> source, {{valueParameter}} value, out int bytesRead, bool readLittleEndian)
    {
        if (source.Length < {{summedLength}})
        {
            value = default;
            bytesRead = 0;
            return false;
        }
"""
        );
        List<string> constructorParameters = [];
        var currentByteIndex = 0;
        foreach (BinaryMemberInfo memberInfo in _members)
        {
            var variableName = $"{prefix}read{memberInfo.Symbol.Name}";
            constructorParameters.Add(variableName);
            if (memberInfo is BinaryArrayMemberInfo arrayMemberInfo)
                continue;
            var write = memberInfo.TypeSymbol.ToDisplayString() switch
            {
                "bool" => $"        var {variableName} = source[{currentByteIndex}] > 0;",
                _ => null,
            };
            StringBuilder.AppendLine(write);
            currentByteIndex += memberInfo.TypeByteLength;
        }
        StringBuilder.AppendLine(
            $$"""
        value = new {{_symbol.Name}}({{string.Join(", ", constructorParameters)}});
        bytesRead = {{currentByteIndex}};
        return true;
    }
"""
        );
    }

    public void AddReadBoilerplate()
    {
        StringBuilder.AppendLine(
            """
    /// <inheritdoc />
    public static bool TryReadLittleEndian(global::System.ReadOnlySpan<byte> source, [global::System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out OneBool? value) => TryRead(source, out value, out _, true);
    /// <inheritdoc />
    public static bool TryReadLittleEndian(global::System.ReadOnlySpan<byte> source, [global::System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out OneBool? value, out int bytesRead) => TryRead(source, out value, out bytesRead, true);
    /// <inheritdoc />
    public static bool TryReadBigEndian(global::System.ReadOnlySpan<byte> source, [global::System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out OneBool? value) => TryRead(source, out value, out _, false);
    /// <inheritdoc />
    public static bool TryReadBigEndian(global::System.ReadOnlySpan<byte> source, [global::System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out OneBool? value, out int bytesRead) => TryRead(source, out value, out bytesRead, false);
"""
        );
    }
}

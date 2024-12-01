namespace Darp.BinaryObjects.Generator;

using System.CodeDom.Compiler;
using System.Diagnostics.CodeAnalysis;
using System.Web;
using Microsoft.CodeAnalysis;

internal interface IMember
{
    public ISymbol MemberSymbol { get; }
    public ITypeSymbol TypeSymbol { get; }
    public WellKnownCollectionKind CollectionKind { get; }
    public WellKnownTypeKind TypeKind { get; }
    public int ConstantByteLength { get; }
    public string GetDocCommentLength();
}

internal interface IGroup
{
    public int ConstantByteLength { get; }
    public string GetLengthCodeString();
    public string? GetVariableByteLength();
    public string? GetVariableDocCommentLength();
}

internal interface IConstantMember : IMember
{
    public int TypeByteLength { get; }
    public bool TryGetWriteString(
        bool isLittleEndian,
        int currentByteIndex,
        [NotNullWhen(true)] out string? writeString,
        out int bytesWritten
    );

    public bool TryGetReadString(
        bool isLittleEndian,
        int currentByteIndex,
        [NotNullWhen(true)] out string? readString,
        out int bytesRead
    );
}

internal sealed class ConstantBinaryMemberGroup(IReadOnlyList<IConstantMember> members) : IGroup
{
    public IReadOnlyList<IConstantMember> Members { get; } = members;
    public int ConstantByteLength { get; } = members.Sum(x => x.ConstantByteLength);

    public string GetLengthCodeString() => $"{ConstantByteLength}";

    public string? GetVariableByteLength() => null;

    public string? GetVariableDocCommentLength() => null;
}

internal sealed class ConstantPrimitiveMember : IConstantMember
{
    public WellKnownCollectionKind CollectionKind => WellKnownCollectionKind.None;
    public required WellKnownTypeKind TypeKind { get; init; }
    public required ISymbol MemberSymbol { get; init; }
    public required ITypeSymbol TypeSymbol { get; init; }
    public required int TypeByteLength { get; init; }

    public int ConstantByteLength => TypeByteLength;

    public string GetDocCommentLength() => $"{TypeByteLength}";

    public bool TryGetWriteString(
        bool isLittleEndian,
        int currentByteIndex,
        [NotNullWhen(true)] out string? writeString,
        out int bytesWritten
    )
    {
        var methodName = BinaryObjectsGenerator.GetWriteMethodName(CollectionKind, TypeKind, isLittleEndian);
        var optionalCast = BinaryObjectsGenerator.GetOptionalCastToUnderlyingEnumValue(TypeSymbol);
        writeString =
            $"global::Darp.BinaryObjects.Generated.Utilities.{methodName}(destination[{currentByteIndex}..], {optionalCast}this.{MemberSymbol.Name});";
        bytesWritten = ConstantByteLength;
        return true;
    }

    public bool TryGetReadString(
        bool isLittleEndian,
        int currentByteIndex,
        [NotNullWhen(true)] out string? readString,
        out int bytesRead
    )
    {
        var variableName = $"{BinaryObjectsGenerator.Prefix}read{MemberSymbol.Name}";
        var methodName = BinaryObjectsGenerator.GetReadMethodName(CollectionKind, TypeKind, isLittleEndian);
        var optionalCast = BinaryObjectsGenerator.GetOptionalCastToEnum(TypeSymbol);
        readString =
            $"var {variableName} = {optionalCast}global::Darp.BinaryObjects.Generated.Utilities.{methodName}(source[{currentByteIndex}..]);";
        bytesRead = ConstantByteLength;
        return true;
    }
}

internal sealed class ConstantArrayMember : IConstantMember
{
    public required WellKnownCollectionKind CollectionKind { get; init; }
    public required WellKnownTypeKind TypeKind { get; init; }
    public required ISymbol MemberSymbol { get; init; }
    public required ITypeSymbol TypeSymbol { get; init; }
    public required int TypeByteLength { get; init; }

    public required int ArrayLength { get; init; }
    public required ITypeSymbol ArrayTypeSymbol { get; init; }

    public int ConstantByteLength => TypeByteLength * ArrayLength;

    public string GetDocCommentLength() => $"{TypeByteLength} * {ArrayLength}";

    public bool TryGetWriteString(
        bool isLittleEndian,
        int currentByteIndex,
        [NotNullWhen(true)] out string? writeString,
        out int bytesWritten
    )
    {
        var optionalCast = BinaryObjectsGenerator.GetOptionalCastToUnderlyingEnumValue(TypeSymbol);
        var memberName = $"{optionalCast}this.{MemberSymbol.Name}";
        if (CollectionKind is WellKnownCollectionKind.Memory)
            memberName += ".Span";
        var methodName = BinaryObjectsGenerator.GetWriteMethodName(CollectionKind, TypeKind, isLittleEndian);
        writeString =
            $"global::Darp.BinaryObjects.Generated.Utilities.{methodName}(destination[{currentByteIndex}..], {memberName}, {ConstantByteLength / TypeByteLength});";
        bytesWritten = ConstantByteLength;
        return true;
    }

    public bool TryGetReadString(
        bool isLittleEndian,
        int currentByteIndex,
        [NotNullWhen(true)] out string? readString,
        out int bytesRead
    )
    {
        var variableName = $"{BinaryObjectsGenerator.Prefix}read{MemberSymbol.Name}";
        var methodName = BinaryObjectsGenerator.GetReadMethodName(CollectionKind, TypeKind, isLittleEndian);
        var optionalCast = BinaryObjectsGenerator.GetOptionalCastToEnum(TypeSymbol);
        readString =
            $"var {variableName} = {optionalCast}global::Darp.BinaryObjects.Generated.Utilities.{methodName}(source[{currentByteIndex}..{currentByteIndex + ConstantByteLength}]);";
        bytesRead = ConstantByteLength;
        return true;
    }
}

internal interface IVariableMemberGroup : IMember, IGroup
{
    public new int ConstantByteLength { get; }
    public int TypeByteLength { get; }
    public bool TryGetWriteString(
        bool isLittleEndian,
        int currentByteIndex,
        [NotNullWhen(true)] out string? writeString
    );

    public bool TryGetReadString(bool isLittleEndian, int currentByteIndex, [NotNullWhen(true)] out string? readString);
}

internal sealed class VariableArrayMemberGroup : IVariableMemberGroup
{
    public required WellKnownCollectionKind CollectionKind { get; init; }
    public required WellKnownTypeKind TypeKind { get; init; }
    public required ISymbol MemberSymbol { get; init; }
    public required ITypeSymbol TypeSymbol { get; init; }
    public required int TypeByteLength { get; init; }

    public required string ArrayLengthMemberName { get; init; }
    public required int ArrayMinLength { get; init; }
    public required ITypeSymbol ArrayTypeSymbol { get; init; }

    public int ConstantByteLength => TypeByteLength * ArrayMinLength;

    public string GetVariableByteLength()
    {
        if (ArrayMinLength > 0)
            return $"{TypeByteLength} * (this.{ArrayLengthMemberName} - {ArrayMinLength})";
        return $"{TypeByteLength} * this.{ArrayLengthMemberName}";
    }

    public string GetVariableDocCommentLength() => GetDocCommentLength();

    public string GetDocCommentLength() => $"""{TypeByteLength} * <see cref="{ArrayLengthMemberName}"/>""";

    public string GetLengthCodeString() => $"{TypeByteLength} * this.{ArrayLengthMemberName}";

    public bool TryGetWriteString(
        bool isLittleEndian,
        int currentByteIndex,
        [NotNullWhen(true)] out string? writeString
    )
    {
        var bytesWrittenOffset = currentByteIndex > 0 ? "bytesWritten + " : "";
        var optionalCast = BinaryObjectsGenerator.GetOptionalCastToUnderlyingEnumValue(TypeSymbol);
        var memberName = $"{optionalCast}this.{MemberSymbol.Name}";
        if (CollectionKind is WellKnownCollectionKind.Memory)
            memberName += ".Span";
        var methodName = BinaryObjectsGenerator.GetWriteMethodName(CollectionKind, TypeKind, isLittleEndian);
        writeString = $"""
            if (destination.Length < {bytesWrittenOffset}this.{ArrayLengthMemberName})
                return false;
            global::Darp.BinaryObjects.Generated.Utilities.{methodName}(destination[{currentByteIndex}..], {memberName}, this.{ArrayLengthMemberName});
            bytesWritten += this.{ArrayLengthMemberName};
            """;
        return true;
    }

    public bool TryGetReadString(bool isLittleEndian, int currentByteIndex, [NotNullWhen(true)] out string? readString)
    {
        var bytesReadOffset = currentByteIndex > 0 ? "bytesRead + " : "";
        var variableName = $"{BinaryObjectsGenerator.Prefix}read{MemberSymbol.Name}";
        var methodName = BinaryObjectsGenerator.GetReadMethodName(CollectionKind, TypeKind, isLittleEndian);
        var lengthVariableName = $"{BinaryObjectsGenerator.Prefix}read{ArrayLengthMemberName}";
        var optionalCast = BinaryObjectsGenerator.GetOptionalCastToEnum(TypeSymbol);
        readString = $"""
            if (source.Length < {bytesReadOffset}{lengthVariableName})
                return false;
            var {variableName} = {optionalCast}global::Darp.BinaryObjects.Generated.Utilities.{methodName}(source.Slice({currentByteIndex}, {lengthVariableName}));
            bytesRead += {lengthVariableName};
            """;
        return true;
    }
}

internal sealed class BinaryObjectMemberGroup : IMember, IGroup
{
    public required ISymbol MemberSymbol { get; init; }
    public required ITypeSymbol TypeSymbol { get; init; }

    public WellKnownCollectionKind CollectionKind => WellKnownCollectionKind.None;
    public WellKnownTypeKind TypeKind => WellKnownTypeKind.BinaryObject;
    public int ConstantByteLength => 0;

    public string GetLengthCodeString() => $"this.{TypeSymbol.ToDisplayString()}.GetByteCount()";

    public string? GetVariableByteLength() => $"this.{MemberSymbol.Name}.GetByteCount()";

    public string? GetVariableDocCommentLength() => $"""<see cref="{TypeSymbol.ToDisplayString()}.GetByteCount()"/>""";

    public string GetDocCommentLength() => $"""<see cref="{TypeSymbol.ToDisplayString()}.GetByteCount()"/>""";
}

partial class BinaryObjectsGenerator
{
    internal static string GetOptionalCastToEnum(ITypeSymbol symbol)
    {
        return symbol.TypeKind is TypeKind.Enum ? $"({symbol.Name}) " : string.Empty;
    }

    internal static string GetOptionalCastToUnderlyingEnumValue(ITypeSymbol symbol)
    {
        return
            symbol.TypeKind is TypeKind.Enum && symbol is INamedTypeSymbol { EnumUnderlyingType: not null } namedSymbol
            ? $"({namedSymbol.EnumUnderlyingType.ToDisplayString()}) "
            : string.Empty;
    }

    internal static WellKnownTypeKind GetWellKnownTypeKind(ITypeSymbol symbol)
    {
        if (
            symbol
                .GetAttributes()
                .Any(x => x.AttributeClass?.ToDisplayString() == "Darp.BinaryObjects.BinaryObjectAttribute")
        )
        {
            return WellKnownTypeKind.BinaryObject;
        }

        if (symbol.TypeKind is TypeKind.Enum && symbol is INamedTypeSymbol { EnumUnderlyingType: not null } namedSymbol)
        {
            symbol = namedSymbol.EnumUnderlyingType;
        }
        return symbol.ToDisplayString() switch
        {
            "bool" => WellKnownTypeKind.Bool,
            "sbyte" => WellKnownTypeKind.SByte,
            "byte" => WellKnownTypeKind.Byte,
            "short" => WellKnownTypeKind.Short,
            "ushort" => WellKnownTypeKind.UShort,
            "System.Half" => WellKnownTypeKind.Half,
            "char" => WellKnownTypeKind.Char,
            "int" => WellKnownTypeKind.Int,
            "uint" => WellKnownTypeKind.UInt,
            "float" => WellKnownTypeKind.Float,
            "long" => WellKnownTypeKind.Long,
            "ulong" => WellKnownTypeKind.ULong,
            "double" => WellKnownTypeKind.Double,
            "System.Int128" => WellKnownTypeKind.Int128,
            "System.UInt128" => WellKnownTypeKind.UInt128,
            _ => throw new ArgumentOutOfRangeException(nameof(symbol)),
        };
    }

    private static string GetWellKnownName(WellKnownTypeKind typeKind)
    {
        return typeKind switch
        {
            WellKnownTypeKind.Bool => "Bool",
            WellKnownTypeKind.Byte => "UInt8",
            WellKnownTypeKind.SByte => "Int8",
            WellKnownTypeKind.UShort => "UInt16",
            WellKnownTypeKind.Short => "Int16",
            WellKnownTypeKind.Half => "Half",
            WellKnownTypeKind.Char => "Char",
            WellKnownTypeKind.UInt => "UInt32",
            WellKnownTypeKind.Int => "Int32",
            WellKnownTypeKind.Float => "Single",
            WellKnownTypeKind.ULong => "UInt64",
            WellKnownTypeKind.Long => "Int64",
            WellKnownTypeKind.Double => "Double",
            WellKnownTypeKind.UInt128 => "UInt128",
            WellKnownTypeKind.Int128 => "Int128",
            _ => throw new ArgumentOutOfRangeException(nameof(typeKind)),
        };
    }

    private static string GetWellKnownDisplayName(WellKnownCollectionKind collectionKind, WellKnownTypeKind typeKind)
    {
        var typeKindDisplayName = typeKind switch
        {
            WellKnownTypeKind.Bool => "bool",
            WellKnownTypeKind.Byte => "byte",
            WellKnownTypeKind.SByte => "sbyte",
            WellKnownTypeKind.UShort => "ushort",
            WellKnownTypeKind.Short => "short",
            WellKnownTypeKind.Half => "System.Half",
            WellKnownTypeKind.Char => "char",
            WellKnownTypeKind.UInt => "uint",
            WellKnownTypeKind.Int => "int",
            WellKnownTypeKind.Float => "float",
            WellKnownTypeKind.ULong => "ulong",
            WellKnownTypeKind.Long => "long",
            WellKnownTypeKind.Double => "double",
            WellKnownTypeKind.UInt128 => "System.UInt128",
            WellKnownTypeKind.Int128 => "System.Int128",
            _ => throw new ArgumentOutOfRangeException(nameof(typeKind)),
        };
        return collectionKind switch
        {
            WellKnownCollectionKind.None => typeKindDisplayName,
            WellKnownCollectionKind.Span => $"ReadOnlySpan<{typeKindDisplayName}>",
            WellKnownCollectionKind.Memory => $"ReadOnlyMemory<{typeKindDisplayName}>",
            WellKnownCollectionKind.Array => $"{typeKindDisplayName}[]",
            WellKnownCollectionKind.List => $"List<{typeKindDisplayName}>",
            WellKnownCollectionKind.Enumerable => $"IEnumerable<{typeKindDisplayName}>",
            _ => throw new ArgumentOutOfRangeException(nameof(collectionKind)),
        };
    }

    private static string GetEndiannessName(WellKnownTypeKind typeKind, bool isLittleEndian)
    {
        return (typeKind, isLittleEndian) switch
        {
            (WellKnownTypeKind.Bool or WellKnownTypeKind.SByte or WellKnownTypeKind.Byte, _) => string.Empty,
            (_, true) => "LittleEndian",
            (_, false) => "BigEndian",
        };
    }

    internal static string GetWriteMethodName(
        WellKnownCollectionKind collectionKind,
        WellKnownTypeKind typeKind,
        bool isLittleEndian
    )
    {
        var typeName = GetWellKnownName(typeKind);
        var endianness = GetEndiannessName(typeKind, isLittleEndian);
        return collectionKind switch
        {
            WellKnownCollectionKind.None => $"Write{typeName}{endianness}",
            WellKnownCollectionKind.List => $"Write{typeName}List{endianness}",
            WellKnownCollectionKind.Span or WellKnownCollectionKind.Memory or WellKnownCollectionKind.Array =>
                $"Write{typeName}Span{endianness}",
            WellKnownCollectionKind.Enumerable => $"Write{typeName}Enumerable{endianness}",
            _ => throw new ArgumentOutOfRangeException(nameof(collectionKind)),
        };
    }

    internal static string GetReadMethodName(
        WellKnownCollectionKind collectionKind,
        WellKnownTypeKind typeKind,
        bool isLittleEndian
    )
    {
        var typeName = GetWellKnownName(typeKind);
        var endianness = GetEndiannessName(typeKind, isLittleEndian);
        return collectionKind switch
        {
            WellKnownCollectionKind.None => $"Read{typeName}{endianness}",
            WellKnownCollectionKind.List => $"Read{typeName}List{endianness}",
            WellKnownCollectionKind.Memory or WellKnownCollectionKind.Array or WellKnownCollectionKind.Enumerable =>
                $"Read{typeName}Array{endianness}",
            _ => throw new ArgumentOutOfRangeException(nameof(collectionKind)),
        };
    }

    internal static UtilityData[] GetWriteUtilities(WellKnownCollectionKind collectionKind, WellKnownTypeKind typeKind)
    {
        if (typeKind is WellKnownTypeKind.BinaryObject)
            return [];
        var emitLittleAndBigEndianMethods =
            typeKind is not WellKnownTypeKind.Bool and not WellKnownTypeKind.SByte and not WellKnownTypeKind.Byte;
        return collectionKind switch
        {
            WellKnownCollectionKind.None =>
            [
                new UtilityData(false, collectionKind, typeKind, emitLittleAndBigEndianMethods),
            ],
            WellKnownCollectionKind.Span or WellKnownCollectionKind.Memory or WellKnownCollectionKind.Array =>
            [
                new UtilityData(false, WellKnownCollectionKind.Span, typeKind, emitLittleAndBigEndianMethods),
            ],
            WellKnownCollectionKind.List =>
            [
                new UtilityData(false, WellKnownCollectionKind.Span, typeKind, emitLittleAndBigEndianMethods),
                new UtilityData(false, WellKnownCollectionKind.List, typeKind, emitLittleAndBigEndianMethods),
            ],
            WellKnownCollectionKind.Enumerable =>
            [
                new UtilityData(false, WellKnownCollectionKind.Span, typeKind, emitLittleAndBigEndianMethods),
                new UtilityData(false, WellKnownCollectionKind.List, typeKind, emitLittleAndBigEndianMethods),
                new UtilityData(false, WellKnownCollectionKind.Enumerable, typeKind, emitLittleAndBigEndianMethods),
            ],
            _ => throw new ArgumentOutOfRangeException(nameof(collectionKind)),
        };
    }

    internal static UtilityData[] GetReadUtilities(WellKnownCollectionKind collectionKind, WellKnownTypeKind typeKind)
    {
        if (typeKind is WellKnownTypeKind.BinaryObject)
            return [];
        var emitLittleAndBigEndianMethods =
            typeKind is not WellKnownTypeKind.Bool and not WellKnownTypeKind.SByte and not WellKnownTypeKind.Byte;
        return collectionKind switch
        {
            WellKnownCollectionKind.None =>
            [
                new UtilityData(true, collectionKind, typeKind, emitLittleAndBigEndianMethods),
            ],
            WellKnownCollectionKind.Span =>
            [
                new UtilityData(true, WellKnownCollectionKind.Span, typeKind, emitLittleAndBigEndianMethods),
            ],
            WellKnownCollectionKind.Memory or WellKnownCollectionKind.Array or WellKnownCollectionKind.Enumerable =>
            [
                new UtilityData(true, WellKnownCollectionKind.Array, typeKind, emitLittleAndBigEndianMethods),
            ],
            WellKnownCollectionKind.List =>
            [
                new UtilityData(true, WellKnownCollectionKind.List, typeKind, emitLittleAndBigEndianMethods),
            ],
            _ => throw new ArgumentOutOfRangeException(nameof(collectionKind)),
        };
    }

    private static void EmitWriteUtility(
        IndentedTextWriter writer,
        WellKnownCollectionKind collectionKind,
        WellKnownTypeKind typeKind,
        bool emitLittleAndBigEndian
    )
    {
        if (collectionKind is WellKnownCollectionKind.None)
        {
            GetWriteMethodBody methodBodyGetter = typeKind switch
            {
                WellKnownTypeKind.Bool => (_, _, _) => "destination[0] = value ? (byte)0b1 : (byte)0b0;",
                WellKnownTypeKind.SByte => (_, _, _) => "destination[0] = (byte)value;",
                WellKnownTypeKind.Byte => (_, _, _) => "destination[0] = value;",
                _ => (methodName, _, _) => $"BinaryPrimitives.{methodName}(destination, value);",
            };
            if (emitLittleAndBigEndian)
            {
                EmitWriteAnyValueUtility(writer, methodBodyGetter, typeKind, true);
                EmitWriteAnyValueUtility(writer, methodBodyGetter, typeKind, false);
            }
            else
            {
                EmitWriteAnyValueUtility(writer, methodBodyGetter, typeKind, default);
            }
        }
        else
        {
            GetWriteMethodBody methodBodyGetter = (collectionKind, typeKind) switch
            {
                (WellKnownCollectionKind.Span, WellKnownTypeKind.Byte) => (_, _, _) =>
                    """
                        var length = Math.Min(value.Length, maxElementLength);
                        value.Slice(0, length).CopyTo(destination);
                        """,
                (WellKnownCollectionKind.Span, _) => (_, typeName, isLittleEndian) =>
                    $$"""
                        var length = Math.Min(value.Length, maxElementLength);
                        if ({{CheckForReverseEndianness(isLittleEndian)}})
                        {
                            Span<{{typeName}}> reinterpretedDestination = MemoryMarshal.Cast<byte, {{typeName}}>(destination);
                            BinaryPrimitives.ReverseEndianness(value[..length], reinterpretedDestination);
                            return;
                        }
                        MemoryMarshal.Cast<{{typeName}}, byte>(value[..length]).CopyTo(destination);
                        """,
                (WellKnownCollectionKind.List, WellKnownTypeKind.Byte) => (_, _, _) =>
                    "WriteUInt8Span(destination, CollectionsMarshal.AsSpan(value), maxElementLength);",
                (WellKnownCollectionKind.List, _) => (_, _, isLittleEndian) =>
                    $"{GetWriteMethodName(WellKnownCollectionKind.Span, typeKind, isLittleEndian)}(destination, CollectionsMarshal.AsSpan(value), maxElementLength);",
                (WellKnownCollectionKind.Enumerable, WellKnownTypeKind.Byte) => (_, _, isLittleEndian) =>
                    $$"""
                        switch (value)
                        {
                            case byte[] arrayValue:
                                {{GetWriteMethodName(
                            WellKnownCollectionKind.Span,
                            typeKind,
                            isLittleEndian
                        )}}(destination, arrayValue, maxElementLength);
                                return;
                            case List<byte> listValue:
                                {{GetWriteMethodName(
                            WellKnownCollectionKind.List,
                            typeKind,
                            isLittleEndian
                        )}}(destination, listValue, maxElementLength);
                                return;
                        }
                        var index = 0;
                        foreach (var val in value)
                        {
                            destination[index++] = val;
                            if (index >= maxElementLength)
                                return;
                        }
                        """,
                (WellKnownCollectionKind.Enumerable, _) => (_, typeName, isLittleEndian) =>
                    $$"""
                        switch (value)
                        {
                            case {{typeName}}[] arrayValue:
                                {{GetWriteMethodName(
                            WellKnownCollectionKind.Span,
                            typeKind,
                            isLittleEndian
                        )}}(destination, arrayValue, maxElementLength);
                                return;
                            case List<{{typeName}}> listValue:
                                {{GetWriteMethodName(
                            WellKnownCollectionKind.List,
                            typeKind,
                            isLittleEndian
                        )}}(destination, listValue, maxElementLength);
                                return;
                        }
                        var index = 0;
                        foreach (var val in value)
                        {
                            BinaryPrimitives.{{GetWriteMethodName(
                            WellKnownCollectionKind.None,
                            typeKind,
                            isLittleEndian
                        )}}(destination[({{typeKind.GetLength()}} * index++)..], val);
                            if (index >= maxElementLength)
                                return;
                        }
                        """,
                _ => throw new ArgumentOutOfRangeException(nameof(collectionKind)),
            };
            if (emitLittleAndBigEndian)
            {
                EmitWriteAnyCollectionUtility(writer, methodBodyGetter, collectionKind, typeKind, true);
                EmitWriteAnyCollectionUtility(writer, methodBodyGetter, collectionKind, typeKind, false);
            }
            else
            {
                EmitWriteAnyCollectionUtility(writer, methodBodyGetter, collectionKind, typeKind, default);
            }
        }
    }

    private delegate string GetWriteMethodBody(string methodName, string typeName, bool isLittleEndian);
    private delegate string GetReadMethodBody(string methodName, string typeName, bool isLittleEndian);

    private static void EmitWriteAnyValueUtility(
        IndentedTextWriter writer,
        GetWriteMethodBody getter,
        WellKnownTypeKind typeKind,
        bool isLittleEndian
    )
    {
        var typeName = GetWellKnownDisplayName(WellKnownCollectionKind.None, typeKind);
        var methodName = GetWriteMethodName(WellKnownCollectionKind.None, typeKind, isLittleEndian);
        writer.WriteLine(
            $"/// <summary> Writes a <c>{HttpUtility.HtmlEncode(typeName)}</c> to the destination </summary>"
        );
        writer.WriteLine("[MethodImpl(MethodImplOptions.AggressiveInlining)]");
        writer.WriteLine($"public static void {methodName}(Span<byte> destination, {typeName} value)");
        writer.WriteLine("{");
        writer.Indent++;
        writer.WriteLine(getter(methodName, typeName, isLittleEndian));
        writer.Indent--;
        writer.WriteLine("}");
    }

    private static void EmitWriteAnyCollectionUtility(
        IndentedTextWriter writer,
        GetWriteMethodBody getter,
        WellKnownCollectionKind collectionKind,
        WellKnownTypeKind typeKind,
        bool isLittleEndian
    )
    {
        var collectionName = GetWellKnownDisplayName(collectionKind, typeKind);
        var typeName = GetWellKnownDisplayName(WellKnownCollectionKind.None, typeKind);
        var methodName = GetWriteMethodName(collectionKind, typeKind, isLittleEndian);

        var endiannessName = GetEndiannessName(typeKind, isLittleEndian);
        if (!string.IsNullOrEmpty(endiannessName))
        {
            endiannessName = $", as {endiannessName}";
        }
        writer.WriteLine(
            $"/// <summary> Writes a <c>{HttpUtility.HtmlEncode(collectionName)}</c> with a <c>maxElementLength</c> to the destination{endiannessName} </summary>"
        );
        writer.WriteLine("[MethodImpl(MethodImplOptions.AggressiveInlining)]");
        writer.WriteLine(
            $"public static void {methodName}(Span<byte> destination, {collectionName} value, int maxElementLength)"
        );
        writer.WriteLine("{");
        writer.Indent++;
        writer.WriteMultiLine(getter(methodName, typeName, isLittleEndian));
        writer.Indent--;
        writer.WriteLine("}");
    }

    private static void EmitReadUtility(
        IndentedTextWriter writer,
        WellKnownCollectionKind collectionKind,
        WellKnownTypeKind typeKind,
        bool emitLittleAndBigEndian
    )
    {
        if (collectionKind is WellKnownCollectionKind.None)
        {
            GetReadMethodBody methodBodyGetter = typeKind switch
            {
                WellKnownTypeKind.Bool => (_, _, _) => "return source[0] > 0;",
                WellKnownTypeKind.SByte => (_, _, _) => "return (sbyte)source[0];",
                WellKnownTypeKind.Byte => (_, _, _) => "return source[0];",
                _ => (methodName, _, _) => $"return BinaryPrimitives.{methodName}(source);",
            };
            if (emitLittleAndBigEndian)
            {
                EmitReadAnyValueUtility(writer, methodBodyGetter, typeKind, true);
                EmitReadAnyValueUtility(writer, methodBodyGetter, typeKind, false);
            }
            else
            {
                EmitReadAnyValueUtility(writer, methodBodyGetter, typeKind, default);
            }
        }
        else
        {
            GetReadMethodBody methodBodyGetter = (collectionKind, typeKind) switch
            {
                (WellKnownCollectionKind.Memory or WellKnownCollectionKind.Array, WellKnownTypeKind.Byte) => (
                    _,
                    _,
                    _
                ) => "return source.ToArray();",
                (
                    WellKnownCollectionKind.Memory
                        or WellKnownCollectionKind.Array,
                    WellKnownTypeKind.Bool
                        or WellKnownTypeKind.SByte
                ) => (_, typeName, _) => $"return MemoryMarshal.Cast<byte, {typeName}>(source).ToArray();",
                (
                    WellKnownCollectionKind.Memory
                        or WellKnownCollectionKind.Array
                        or WellKnownCollectionKind.Enumerable,
                    _
                ) => (_, typeName, isLittleEndian) =>
                    $"""
                        var array = MemoryMarshal.Cast<byte, {typeName}>(source).ToArray();
                        if ({CheckForReverseEndianness(isLittleEndian)})
                            BinaryPrimitives.ReverseEndianness(array, array);
                        return array;
                        """,
                (WellKnownCollectionKind.List, WellKnownTypeKind.Byte) => (_, _, _) =>
                    """
                        var list = new List<byte>(source.Length);
                        list.AddRange(source);
                        return list;
                        """,
                (WellKnownCollectionKind.List, _) => (_, typeName, isLittleEndian) =>
                    $$"""
                        ReadOnlySpan<{{typeName}}> span = MemoryMarshal.Cast<byte, {{typeName}}>(source);
                        var list = new List<{{typeName}}>(span.Length);
                        list.AddRange(span);
                        if ({{CheckForReverseEndianness(isLittleEndian)}})
                        {
                            Span<{{typeName}}> listSpan = CollectionsMarshal.AsSpan(list);
                            BinaryPrimitives.ReverseEndianness(span, listSpan);
                        }
                        return list;
                        """,
                _ => throw new ArgumentOutOfRangeException(nameof(collectionKind)),
            };
            if (emitLittleAndBigEndian)
            {
                EmitReadAnyCollectionUtility(writer, methodBodyGetter, collectionKind, typeKind, true);
                EmitReadAnyCollectionUtility(writer, methodBodyGetter, collectionKind, typeKind, false);
            }
            else
            {
                EmitReadAnyCollectionUtility(writer, methodBodyGetter, collectionKind, typeKind, default);
            }
        }
    }

    private static string CheckForReverseEndianness(bool isLittleEndian) =>
        isLittleEndian ? "!BitConverter.IsLittleEndian" : "BitConverter.IsLittleEndian";

    private static void EmitReadAnyValueUtility(
        IndentedTextWriter writer,
        GetReadMethodBody getter,
        WellKnownTypeKind typeKind,
        bool isLittleEndian
    )
    {
        EmitReadAnyCollectionUtility(writer, getter, WellKnownCollectionKind.None, typeKind, isLittleEndian);
    }

    private static void EmitReadAnyCollectionUtility(
        IndentedTextWriter writer,
        GetReadMethodBody getter,
        WellKnownCollectionKind collectionKind,
        WellKnownTypeKind typeKind,
        bool isLittleEndian
    )
    {
        var collectionName = GetWellKnownDisplayName(collectionKind, typeKind);
        var typeName = GetWellKnownDisplayName(WellKnownCollectionKind.None, typeKind);
        var methodName = GetReadMethodName(collectionKind, typeKind, isLittleEndian);

        var endiannessName = GetEndiannessName(typeKind, isLittleEndian);
        if (!string.IsNullOrEmpty(endiannessName))
        {
            endiannessName = $", as {endiannessName}";
        }
        writer.WriteLine(
            $"/// <summary> Reads a <c>{HttpUtility.HtmlEncode(collectionName)}</c> from the given source{endiannessName} </summary>"
        );
        writer.WriteLine("[MethodImpl(MethodImplOptions.AggressiveInlining)]");
        writer.WriteLine($"public static {collectionName} {methodName}(ReadOnlySpan<byte> source)");
        writer.WriteLine("{");
        writer.Indent++;
        writer.WriteMultiLine(getter(methodName, typeName, isLittleEndian));
        writer.Indent--;
        writer.WriteLine("}");
    }
}

internal enum RequestedEndianness
{
    None,
    Little,
    Big,
}

internal enum WellKnownTypeKind
{
    Bool,
    Byte,
    SByte,
    UShort,
    Short,
    Half,
    Char,
    UInt,
    Int,
    Float,
    ULong,
    Long,
    Double,
    UInt128,
    Int128,
    BinaryObject,
}

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

internal sealed class ConstantWellKnownMember : IConstantMember
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
        var optionalCast = BinaryObjectsGenerator.GetOptionalCastToEnum(TypeKind, TypeSymbol);
        var optionalGeneric = BinaryObjectsGenerator.GetOptionalGenericTypeParameter(
            CollectionKind,
            TypeKind,
            TypeSymbol
        );
        readString =
            $"var {variableName} = {optionalCast}global::Darp.BinaryObjects.Generated.Utilities.{methodName}{optionalGeneric}(source[{currentByteIndex}..{currentByteIndex + ConstantByteLength}]);";
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
        var memberName = $"this.{MemberSymbol.Name}";
        if (CollectionKind is WellKnownCollectionKind.Memory)
            memberName += ".Span";
        var methodName = BinaryObjectsGenerator.GetWriteMethodName(CollectionKind, TypeKind, isLittleEndian);
        var optionalGeneric = BinaryObjectsGenerator.GetOptionalGenericTypeParameter(
            CollectionKind,
            TypeKind,
            TypeSymbol
        );
        writeString =
            $"global::Darp.BinaryObjects.Generated.Utilities.{methodName}{optionalGeneric}(destination.Slice({currentByteIndex}, {ConstantByteLength}), {memberName});";
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
        var optionalNumberOfElements =
            TypeKind is WellKnownTypeKind.BinaryObject ? $", {ConstantByteLength / TypeByteLength}" : string.Empty;
        var optionalGeneric = BinaryObjectsGenerator.GetOptionalGenericTypeParameter(
            CollectionKind,
            TypeKind,
            TypeSymbol
        );
        readString =
            $"var {variableName} = global::Darp.BinaryObjects.Generated.Utilities.{methodName}{optionalGeneric}(source[{currentByteIndex}..{currentByteIndex + ConstantByteLength}]{optionalNumberOfElements}, out _);";
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
            global::Darp.BinaryObjects.Generated.Utilities.{methodName}(destination.Slice({currentByteIndex}, this.{ArrayLengthMemberName}), {memberName});
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
        var optionalCast = BinaryObjectsGenerator.GetOptionalCastToEnum(TypeKind, TypeSymbol);
        readString = $"""
            if (source.Length < {bytesReadOffset}{lengthVariableName})
                return false;
            var {variableName} = {optionalCast}global::Darp.BinaryObjects.Generated.Utilities.{methodName}(source.Slice({currentByteIndex}, {lengthVariableName}), out _);
            bytesRead += {lengthVariableName};
            """;
        return true;
    }
}

internal sealed class ReadRemainingArrayMemberGroup : IVariableMemberGroup
{
    public required WellKnownCollectionKind CollectionKind { get; init; }
    public required WellKnownTypeKind TypeKind { get; init; }
    public required ISymbol MemberSymbol { get; init; }
    public required ITypeSymbol TypeSymbol { get; init; }
    public required int TypeByteLength { get; init; }

    public required int ArrayMinLength { get; init; }

    public int ConstantByteLength => TypeByteLength * ArrayMinLength;

    private string GetVariableLength() =>
        CollectionKind switch
        {
            WellKnownCollectionKind.Span or WellKnownCollectionKind.Memory or WellKnownCollectionKind.Array =>
                $"this.{MemberSymbol.Name}.Length",
            WellKnownCollectionKind.List => $"this.{MemberSymbol.Name}.Count",
            WellKnownCollectionKind.Enumerable => $"this.{MemberSymbol.Name}.Count()",
            _ => throw new ArgumentException($"Could not get variable length because {CollectionKind} is unknown"),
        };

    public string GetVariableByteLength()
    {
        return $"{TypeByteLength} * {GetVariableLength()}";
    }

    public string GetVariableDocCommentLength() => GetDocCommentLength();

    public string GetDocCommentLength() =>
        ArrayMinLength > 0 ? $"{TypeByteLength} * ({ArrayMinLength} + n)" : $"{TypeByteLength} * n";

    public string GetLengthCodeString() => $"{TypeByteLength} * {GetVariableLength()}";

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
            bytesWritten += global::Darp.BinaryObjects.Generated.Utilities.{methodName}(destination[{currentByteIndex}..], {memberName});
            """;
        if (ArrayMinLength > 0)
        {
            writeString = $"""
                if (source.Length < {bytesWrittenOffset}{TypeByteLength * ArrayMinLength})
                    return false;
                {writeString}
                """;
        }
        return true;
    }

    public bool TryGetReadString(bool isLittleEndian, int currentByteIndex, [NotNullWhen(true)] out string? readString)
    {
        var variableBytesReadName = $"{BinaryObjectsGenerator.Prefix}bytesRead{MemberSymbol.Name}";
        var variableName = $"{BinaryObjectsGenerator.Prefix}read{MemberSymbol.Name}";
        var methodName = BinaryObjectsGenerator.GetReadMethodName(CollectionKind, TypeKind, isLittleEndian);
        var optionalCast = BinaryObjectsGenerator.GetOptionalCastToEnum(TypeKind, TypeSymbol);
        readString = $"""
            var {variableName} = {optionalCast}global::Darp.BinaryObjects.Generated.Utilities.{methodName}(source.Slice({currentByteIndex}), out int {variableBytesReadName});
            bytesRead += {variableBytesReadName};
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

    public string GetVariableDocCommentLength() => $"""<see cref="{TypeSymbol.ToDisplayString()}.GetByteCount()"/>""";

    public string GetDocCommentLength() => $"""<see cref="{TypeSymbol.ToDisplayString()}.GetByteCount()"/>""";
}

partial class BinaryObjectsGenerator
{
    internal static string GetOptionalCastToEnum(WellKnownTypeKind typeKind, ITypeSymbol symbol)
    {
        return
            typeKind
                is WellKnownTypeKind.EnumByte
                    or WellKnownTypeKind.EnumSByte
                    or WellKnownTypeKind.EnumUShort
                    or WellKnownTypeKind.EnumShort
                    or WellKnownTypeKind.EnumUInt
                    or WellKnownTypeKind.EnumInt
                    or WellKnownTypeKind.EnumULong
                    or WellKnownTypeKind.EnumLong
            ? $"({symbol.Name}) "
            : string.Empty;
    }

    internal static string GetOptionalCastToUnderlyingEnumValue(ITypeSymbol symbol)
    {
        return
            symbol.TypeKind is TypeKind.Enum && symbol is INamedTypeSymbol { EnumUnderlyingType: not null } namedSymbol
            ? $"({namedSymbol.EnumUnderlyingType.ToDisplayString()}) "
            : string.Empty;
    }

    internal static string GetOptionalGenericTypeParameter(
        WellKnownCollectionKind collectionKind,
        WellKnownTypeKind typeKind,
        ITypeSymbol typeSymbol
    )
    {
        if (
            collectionKind is not WellKnownCollectionKind.None
            && typeKind
                is WellKnownTypeKind.EnumByte
                    or WellKnownTypeKind.EnumSByte
                    or WellKnownTypeKind.EnumUShort
                    or WellKnownTypeKind.EnumShort
                    or WellKnownTypeKind.EnumUInt
                    or WellKnownTypeKind.EnumInt
                    or WellKnownTypeKind.EnumULong
                    or WellKnownTypeKind.EnumLong
        )
            return $"<{typeSymbol.ToDisplayString()}>";
        return typeKind is WellKnownTypeKind.BinaryObject ? $"<{typeSymbol.ToDisplayString()}>" : string.Empty;
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
            return namedSymbol.EnumUnderlyingType.ToDisplayString() switch
            {
                "byte" => WellKnownTypeKind.EnumByte,
                "sbyte" => WellKnownTypeKind.EnumSByte,
                "ushort" => WellKnownTypeKind.EnumUShort,
                "short" => WellKnownTypeKind.EnumShort,
                "uint" => WellKnownTypeKind.EnumUInt,
                "int" => WellKnownTypeKind.EnumInt,
                "ulong" => WellKnownTypeKind.EnumULong,
                "long" => WellKnownTypeKind.EnumLong,
                _ => throw new ArgumentException($"Could get well known type kind for enum {symbol.ToDisplayString()}"),
            };
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
            _ => throw new ArgumentException($"Could get well known type kind for {symbol.ToDisplayString()}"),
        };
    }

    private static string GetWellKnownName(WellKnownCollectionKind collectionKind, WellKnownTypeKind typeKind)
    {
        return (collectionKind, typeKind) switch
        {
            (_, WellKnownTypeKind.Bool) => "Bool",
            (_, WellKnownTypeKind.Byte) => "UInt8",
            (_, WellKnownTypeKind.SByte) => "Int8",
            (_, WellKnownTypeKind.UShort) => "UInt16",
            (_, WellKnownTypeKind.Short) => "Int16",
            (_, WellKnownTypeKind.Half) => "Half",
            (_, WellKnownTypeKind.Char) => "Char",
            (_, WellKnownTypeKind.UInt) => "UInt32",
            (_, WellKnownTypeKind.Int) => "Int32",
            (_, WellKnownTypeKind.Float) => "Single",
            (_, WellKnownTypeKind.ULong) => "UInt64",
            (_, WellKnownTypeKind.Long) => "Int64",
            (_, WellKnownTypeKind.Double) => "Double",
            (_, WellKnownTypeKind.UInt128) => "UInt128",
            (_, WellKnownTypeKind.Int128) => "Int128",
            (WellKnownCollectionKind.None, WellKnownTypeKind.EnumByte) => "UInt8",
            (_, WellKnownTypeKind.EnumByte) => "UInt8Enum",
            (WellKnownCollectionKind.None, WellKnownTypeKind.EnumSByte) => "Int8",
            (_, WellKnownTypeKind.EnumSByte) => "Int8Enum",
            (WellKnownCollectionKind.None, WellKnownTypeKind.EnumUShort) => "UInt16",
            (_, WellKnownTypeKind.EnumUShort) => "UInt16Enum",
            (WellKnownCollectionKind.None, WellKnownTypeKind.EnumShort) => "Int16",
            (_, WellKnownTypeKind.EnumShort) => "Int16Enum",
            (WellKnownCollectionKind.None, WellKnownTypeKind.EnumUInt) => "UInt32",
            (_, WellKnownTypeKind.EnumUInt) => "UInt32Enum",
            (WellKnownCollectionKind.None, WellKnownTypeKind.EnumInt) => "Int32",
            (_, WellKnownTypeKind.EnumInt) => "Int32Enum",
            (WellKnownCollectionKind.None, WellKnownTypeKind.EnumULong) => "UInt64",
            (_, WellKnownTypeKind.EnumULong) => "UInt64Enum",
            (WellKnownCollectionKind.None, WellKnownTypeKind.EnumLong) => "Int64",
            (_, WellKnownTypeKind.EnumLong) => "Int64Enum",
            (_, WellKnownTypeKind.BinaryObject) => "BinaryObject",
            _ => throw new ArgumentException($"Could get well known name for {typeKind}"),
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
            WellKnownTypeKind.EnumByte
            or WellKnownTypeKind.EnumSByte
            or WellKnownTypeKind.EnumUShort
            or WellKnownTypeKind.EnumShort
            or WellKnownTypeKind.EnumUInt
            or WellKnownTypeKind.EnumInt
            or WellKnownTypeKind.EnumULong
            or WellKnownTypeKind.EnumLong => "TEnum",
            WellKnownTypeKind.BinaryObject => "T",
            _ => throw new ArgumentException($"Could get well known display name for {typeKind}"),
        };
        return collectionKind switch
        {
            WellKnownCollectionKind.None => typeKindDisplayName,
            WellKnownCollectionKind.Span => $"ReadOnlySpan<{typeKindDisplayName}>",
            WellKnownCollectionKind.Memory => $"ReadOnlyMemory<{typeKindDisplayName}>",
            WellKnownCollectionKind.Array => $"{typeKindDisplayName}[]",
            WellKnownCollectionKind.List => $"List<{typeKindDisplayName}>",
            WellKnownCollectionKind.Enumerable => $"IEnumerable<{typeKindDisplayName}>",
            _ => throw new ArgumentException($"Could get well known display name for {collectionKind}"),
        };
    }

    private static string GetWellKnownEnumIntegerDisplayName(WellKnownTypeKind typeKind)
    {
        return typeKind switch
        {
            WellKnownTypeKind.EnumByte => "byte",
            WellKnownTypeKind.EnumSByte => "sbyte",
            WellKnownTypeKind.EnumUShort => "ushort",
            WellKnownTypeKind.EnumShort => "short",
            WellKnownTypeKind.EnumUInt => "uint",
            WellKnownTypeKind.EnumInt => "int",
            WellKnownTypeKind.EnumULong => "ulong",
            WellKnownTypeKind.EnumLong => "long",
            _ => throw new ArgumentException($"Could get well known display name for enum {typeKind}"),
        };
    }

    private static string GetEndiannessName(WellKnownTypeKind typeKind, bool isLittleEndian)
    {
        return (typeKind, isLittleEndian) switch
        {
            (
                WellKnownTypeKind.Bool
                    or WellKnownTypeKind.SByte
                    or WellKnownTypeKind.Byte
                    or WellKnownTypeKind.EnumSByte
                    or WellKnownTypeKind.EnumByte,
                _
            ) => string.Empty,
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
        var typeName = GetWellKnownName(collectionKind, typeKind);
        var endianness = GetEndiannessName(typeKind, isLittleEndian);
        return collectionKind switch
        {
            WellKnownCollectionKind.None => $"Write{typeName}{endianness}",
            WellKnownCollectionKind.List => $"Write{typeName}List{endianness}",
            WellKnownCollectionKind.Span or WellKnownCollectionKind.Memory or WellKnownCollectionKind.Array =>
                $"Write{typeName}Span{endianness}",
            WellKnownCollectionKind.Enumerable => $"Write{typeName}Enumerable{endianness}",
            _ => throw new ArgumentException(
                $"Could create write method name for {collectionKind} and {typeKind} (littleEndian={isLittleEndian})"
            ),
        };
    }

    internal static string GetReadMethodName(
        WellKnownCollectionKind collectionKind,
        WellKnownTypeKind typeKind,
        bool isLittleEndian
    )
    {
        var typeName = GetWellKnownName(collectionKind, typeKind);
        var endianness = GetEndiannessName(typeKind, isLittleEndian);
        return collectionKind switch
        {
            WellKnownCollectionKind.None => $"Read{typeName}{endianness}",
            WellKnownCollectionKind.List => $"Read{typeName}List{endianness}",
            WellKnownCollectionKind.Memory or WellKnownCollectionKind.Array or WellKnownCollectionKind.Enumerable =>
                $"Read{typeName}Array{endianness}",
            _ => throw new ArgumentException(
                $"Could create read method name for {collectionKind} and {typeKind} (littleEndian={isLittleEndian})"
            ),
        };
    }

    internal static UtilityData[] GetWriteUtilities(WellKnownCollectionKind collectionKind, WellKnownTypeKind typeKind)
    {
        // For normal enums, we just cast the value and do not need a specialized utility
        if (collectionKind is WellKnownCollectionKind.None)
        {
            typeKind = typeKind switch
            {
                WellKnownTypeKind.EnumByte => WellKnownTypeKind.Byte,
                WellKnownTypeKind.EnumSByte => WellKnownTypeKind.SByte,
                WellKnownTypeKind.EnumUShort => WellKnownTypeKind.UShort,
                WellKnownTypeKind.EnumShort => WellKnownTypeKind.Short,
                WellKnownTypeKind.EnumUInt => WellKnownTypeKind.UInt,
                WellKnownTypeKind.EnumInt => WellKnownTypeKind.Int,
                WellKnownTypeKind.EnumULong => WellKnownTypeKind.ULong,
                WellKnownTypeKind.EnumLong => WellKnownTypeKind.Long,
                _ => typeKind,
            };
        }
        var emitLittleAndBigEndianMethods =
            typeKind
                is not WellKnownTypeKind.Bool
                    and not WellKnownTypeKind.SByte
                    and not WellKnownTypeKind.Byte
                    and not WellKnownTypeKind.EnumSByte
                    and not WellKnownTypeKind.EnumByte;
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
            _ => throw new ArgumentException($"Could create write utilities for {collectionKind} and {typeKind}"),
        };
    }

    internal static UtilityData[] GetReadUtilities(WellKnownCollectionKind collectionKind, WellKnownTypeKind typeKind)
    {
        // For normal enums, we just cast the value and do not need a specialized utility
        if (collectionKind is WellKnownCollectionKind.None)
        {
            typeKind = typeKind switch
            {
                WellKnownTypeKind.EnumByte => WellKnownTypeKind.Byte,
                WellKnownTypeKind.EnumSByte => WellKnownTypeKind.SByte,
                WellKnownTypeKind.EnumUShort => WellKnownTypeKind.UShort,
                WellKnownTypeKind.EnumShort => WellKnownTypeKind.Short,
                WellKnownTypeKind.EnumUInt => WellKnownTypeKind.UInt,
                WellKnownTypeKind.EnumInt => WellKnownTypeKind.Int,
                WellKnownTypeKind.EnumULong => WellKnownTypeKind.ULong,
                WellKnownTypeKind.EnumLong => WellKnownTypeKind.Long,
                _ => typeKind,
            };
        }
        var emitLittleAndBigEndianMethods =
            typeKind
                is not WellKnownTypeKind.Bool
                    and not WellKnownTypeKind.SByte
                    and not WellKnownTypeKind.Byte
                    and not WellKnownTypeKind.EnumSByte
                    and not WellKnownTypeKind.EnumByte;
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
            _ => throw new ArgumentException($"Could create read utilities for {collectionKind} and {typeKind}"),
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
                WellKnownTypeKind.BinaryObject => (_, _, isLittleEndian) =>
                    $"""
                        if (!value.TryWrite{GetEndiannessName(typeKind, isLittleEndian)}(destination))
                            throw new ArgumentOutOfRangeException(nameof(value));
                        """,
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
            var byteLength = typeKind.GetLength();
            GetWriteMethodBody methodBodyGetter = (collectionKind, typeKind) switch
            {
                (WellKnownCollectionKind.Span, WellKnownTypeKind.Byte) => (_, _, _) =>
                    $"""
                        var length = Math.Min(value.Length, destination.Length);
                        value.Slice(0, length).CopyTo(destination);
                        return length;
                        """,
                (WellKnownCollectionKind.Span, WellKnownTypeKind.SByte or WellKnownTypeKind.Bool) => (_, typeName, _) =>
                    $"""
                        var length = Math.Min(value.Length, destination.Length);
                        MemoryMarshal.Cast<{typeName}, byte>(value.Slice(0, length)).CopyTo(destination);
                        return length;
                        """,
                (WellKnownCollectionKind.Span, WellKnownTypeKind.EnumByte or WellKnownTypeKind.EnumSByte) => (
                    _,
                    _,
                    _
                ) =>
                {
                    var underlyingTypeName = GetWellKnownEnumIntegerDisplayName(typeKind);
                    return $"""
                        var length = Math.Min(value.Length, destination.Length);
                        MemoryMarshal.Cast<TEnum, {underlyingTypeName}>(value.Slice(0, length)).CopyTo(destination);
                        return length;
                        """;
                },
                (WellKnownCollectionKind.Span, WellKnownTypeKind.BinaryObject) => (_, _, isLittleEndian) =>
                    $$"""
                        if (value.Length == 0)
                            return 0;
                        var elementLength = value[0].GetByteCount();
                        var maxNumberOfElements = destination.Length / elementLength;
                        for (var i = 0; i < maxNumberOfElements; i++)
                        {
                            if (!value[i].TryWrite{{GetEndiannessName(
                            typeKind,
                            isLittleEndian
                        )}}(destination.Slice(i * elementLength, elementLength)))
                                throw new ArgumentException($"Could not write {typeof(T).Name} to destination");
                        }
                        return elementLength * maxNumberOfElements;
                        """,
                (
                    WellKnownCollectionKind.Span,
                    WellKnownTypeKind.EnumUShort
                        or WellKnownTypeKind.EnumShort
                        or WellKnownTypeKind.EnumUInt
                        or WellKnownTypeKind.EnumInt
                        or WellKnownTypeKind.EnumULong
                        or WellKnownTypeKind.EnumLong
                ) => (_, _, isLittleEndian) =>
                {
                    var underlyingTypeName = GetWellKnownEnumIntegerDisplayName(typeKind);
                    return $$"""
                        var length = Math.Min(value.Length, destination.Length / {{byteLength}});
                        if ({{CheckForReverseEndianness(isLittleEndian)}})
                        {
                            ReadOnlySpan<{{underlyingTypeName}}> reinterpretedValue = MemoryMarshal.Cast<TEnum, {{underlyingTypeName}}>(value);
                            Span<{{underlyingTypeName}}> reinterpretedDestination = MemoryMarshal.Cast<byte, {{underlyingTypeName}}>(destination);
                            BinaryPrimitives.ReverseEndianness(reinterpretedValue[..length], reinterpretedDestination);
                            return length * {{byteLength}};
                        }
                        MemoryMarshal.Cast<TEnum, byte>(value[..length]).CopyTo(destination);
                        return length * {{byteLength}};
                        """;
                },
                (WellKnownCollectionKind.Span, _) => (_, typeName, isLittleEndian) =>
                    $$"""
                        var length = Math.Min(value.Length, destination.Length / {{byteLength}});
                        if ({{CheckForReverseEndianness(isLittleEndian)}})
                        {
                            Span<{{typeName}}> reinterpretedDestination = MemoryMarshal.Cast<byte, {{typeName}}>(destination);
                            BinaryPrimitives.ReverseEndianness(value[..length], reinterpretedDestination);
                            return length * {{byteLength}};
                        }
                        MemoryMarshal.Cast<{{typeName}}, byte>(value[..length]).CopyTo(destination);
                        return length * {{byteLength}};
                        """,
                (WellKnownCollectionKind.List, WellKnownTypeKind.Byte) => (_, _, _) =>
                    "return WriteUInt8Span(destination, CollectionsMarshal.AsSpan(value));",
                (WellKnownCollectionKind.List, _) => (_, _, isLittleEndian) =>
                    $"return {GetWriteMethodName(WellKnownCollectionKind.Span, typeKind, isLittleEndian)}(destination, CollectionsMarshal.AsSpan(value));",
                (WellKnownCollectionKind.Enumerable, WellKnownTypeKind.Byte) => (_, _, isLittleEndian) =>
                    $$"""
                        switch (value)
                        {
                            case byte[] arrayValue:
                                return {{GetWriteMethodName(
                            WellKnownCollectionKind.Span,
                            typeKind,
                            isLittleEndian
                        )}}(destination, arrayValue);
                            case List<byte> listValue:
                                return {{GetWriteMethodName(
                            WellKnownCollectionKind.List,
                            typeKind,
                            isLittleEndian
                        )}}(destination, listValue);
                        }
                        var maxElementLength = destination.Length;
                        var index = 0;
                        foreach (var val in value)
                        {
                            destination[index++] = val;
                            if (index >= maxElementLength)
                                return index;
                        }
                        return index;
                        """,
                (WellKnownCollectionKind.Enumerable, _) => (_, typeName, isLittleEndian) =>
                    $$"""
                        switch (value)
                        {
                            case {{typeName}}[] arrayValue:
                                return {{GetWriteMethodName(
                            WellKnownCollectionKind.Span,
                            typeKind,
                            isLittleEndian
                        )}}(destination, arrayValue);
                            case List<{{typeName}}> listValue:
                                return {{GetWriteMethodName(
                            WellKnownCollectionKind.List,
                            typeKind,
                            isLittleEndian
                        )}}(destination, listValue);
                        }
                        var maxElementLength = destination.Length / {{byteLength}};
                        var index = 0;
                        foreach (var val in value)
                        {
                            BinaryPrimitives.{{GetWriteMethodName(
                            WellKnownCollectionKind.None,
                            typeKind,
                            isLittleEndian
                        )}}(destination[({{typeKind.GetLength()}} * index++)..], val);
                            if (index >= maxElementLength)
                                return index * {{byteLength}};
                        }
                        return index * {{byteLength}};
                        """,
                _ => throw new ArgumentException($"Could not emit write utility for {collectionKind} and {typeKind}"),
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
        var typeParameter = typeKind is WellKnownTypeKind.BinaryObject ? "<T>" : string.Empty;
        var typeParameterConstraint =
            typeKind is WellKnownTypeKind.BinaryObject ? "    where T : IBinaryWritable" : string.Empty;
        writer.WriteLine(
            $"/// <summary> Writes a <c>{HttpUtility.HtmlEncode(typeName)}</c> to the destination </summary>"
        );
        writer.WriteLine("[MethodImpl(MethodImplOptions.AggressiveInlining)]");
        writer.WriteLine($"public static void {methodName}{typeParameter}(Span<byte> destination, {typeName} value)");
        if (!string.IsNullOrEmpty(typeParameterConstraint))
            writer.WriteLine(typeParameterConstraint);
        writer.WriteLine("{");
        writer.Indent++;
        writer.WriteMultiLine(getter(methodName, typeName, isLittleEndian));
        writer.Indent--;
        writer.WriteLine("}");
    }

    private static string GetTypeParameter(WellKnownTypeKind typeKind) =>
        typeKind switch
        {
            WellKnownTypeKind.BinaryObject => "<T>",
            WellKnownTypeKind.EnumByte
            or WellKnownTypeKind.EnumSByte
            or WellKnownTypeKind.EnumUShort
            or WellKnownTypeKind.EnumShort
            or WellKnownTypeKind.EnumUInt
            or WellKnownTypeKind.EnumInt
            or WellKnownTypeKind.EnumULong
            or WellKnownTypeKind.EnumLong => "<TEnum>",
            _ => string.Empty,
        };

    private static string GetWriteTypeParameterConstraint(WellKnownTypeKind typeKind) =>
        typeKind switch
        {
            WellKnownTypeKind.BinaryObject => "    where T : IBinaryWritable",
            WellKnownTypeKind.EnumByte
            or WellKnownTypeKind.EnumSByte
            or WellKnownTypeKind.EnumUShort
            or WellKnownTypeKind.EnumShort
            or WellKnownTypeKind.EnumUInt
            or WellKnownTypeKind.EnumInt
            or WellKnownTypeKind.EnumULong
            or WellKnownTypeKind.EnumLong => "    where TEnum : unmanaged, Enum",
            _ => string.Empty,
        };

    private static string GetReadTypeParameterConstraint(WellKnownTypeKind typeKind) =>
        typeKind switch
        {
            WellKnownTypeKind.BinaryObject => "    where T : IBinaryReadable<T>",
            WellKnownTypeKind.EnumByte
            or WellKnownTypeKind.EnumSByte
            or WellKnownTypeKind.EnumUShort
            or WellKnownTypeKind.EnumShort
            or WellKnownTypeKind.EnumUInt
            or WellKnownTypeKind.EnumInt
            or WellKnownTypeKind.EnumULong
            or WellKnownTypeKind.EnumLong => "    where TEnum : unmanaged, Enum",
            _ => string.Empty,
        };

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

        var typeParameter = GetTypeParameter(typeKind);
        var typeParameterConstraint = GetWriteTypeParameterConstraint(typeKind);
        writer.WriteLine(
            $"/// <summary> Writes a <c>{HttpUtility.HtmlEncode(collectionName)}</c> with a <c>maxElementLength</c> to the destination{endiannessName} </summary>"
        );
        writer.WriteLine("[MethodImpl(MethodImplOptions.AggressiveInlining)]");
        writer.WriteLine(
            $"public static int {methodName}{typeParameter}(Span<byte> destination, {collectionName} value)"
        );
        if (!string.IsNullOrEmpty(typeParameterConstraint))
            writer.WriteLine(typeParameterConstraint);
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
                WellKnownTypeKind.BinaryObject => (_, _, isLittleEndian) =>
                    $"""
                        if (!T.TryRead{GetEndiannessName(typeKind, isLittleEndian)}(source, out var value))
                            throw new ArgumentOutOfRangeException(nameof(source));
                        return value;
                        """,
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
            var byteLength = typeKind.GetLength();
            GetReadMethodBody methodBodyGetter = (collectionKind, typeKind) switch
            {
                (WellKnownCollectionKind.Memory or WellKnownCollectionKind.Array, WellKnownTypeKind.Byte) => (
                    _,
                    _,
                    _
                ) =>
                    """
                        bytesRead = source.Length;
                        return source.ToArray();
                        """,
                (
                    WellKnownCollectionKind.Memory
                        or WellKnownCollectionKind.Array,
                    WellKnownTypeKind.EnumByte
                        or WellKnownTypeKind.EnumSByte
                ) => (_, _, _) =>
                {
                    var underlyingTypeName = GetWellKnownEnumIntegerDisplayName(typeKind);
                    return $"""
                        bytesRead = source.Length;
                        return MemoryMarshal.Cast<{underlyingTypeName}, TEnum>(source).ToArray();
                        """;
                },
                (
                    WellKnownCollectionKind.Memory
                        or WellKnownCollectionKind.Array,
                    WellKnownTypeKind.Bool
                        or WellKnownTypeKind.SByte
                ) => (_, typeName, _) =>
                    $"""
                        bytesRead = source.Length;
                        return MemoryMarshal.Cast<byte, {typeName}>(source).ToArray();
                        """,
                (
                    WellKnownCollectionKind.Memory
                        or WellKnownCollectionKind.Array
                        or WellKnownCollectionKind.Enumerable,
                    WellKnownTypeKind.BinaryObject
                ) => (_, _, isLittleEndian) =>
                    $$"""
                          var elementLength = source.Length / numberOfElements;
                          var array = new T[numberOfElements];
                          for (var i = 0; i < numberOfElements; i++)
                          {
                              if (!T.TryRead{{GetEndiannessName(
                                  typeKind,
                                  isLittleEndian
                              )}}(source.Slice(i * elementLength, elementLength), out T? value, out var tempBytesRead))
                                  throw new ArgumentException($"Could not read {typeof(T).Name} from source");
                              array[i] = value;
                          }
                          bytesRead = numberOfElements * elementLength;
                          return array;
                          """,
                (
                    WellKnownCollectionKind.Memory
                        or WellKnownCollectionKind.Array
                        or WellKnownCollectionKind.Enumerable,
                    WellKnownTypeKind.EnumUShort
                        or WellKnownTypeKind.EnumShort
                        or WellKnownTypeKind.EnumUInt
                        or WellKnownTypeKind.EnumInt
                        or WellKnownTypeKind.EnumULong
                        or WellKnownTypeKind.EnumLong
                ) => (_, _, isLittleEndian) =>
                {
                    var underlyingTypeName = GetWellKnownEnumIntegerDisplayName(typeKind);
                    return $$"""
                        var array = MemoryMarshal.Cast<byte, TEnum>(source).ToArray();
                        if ({{CheckForReverseEndianness(isLittleEndian)}})
                        {
                            var reinterpretedArray = MemoryMarshal.Cast<TEnum, {{underlyingTypeName}}>(array);
                            BinaryPrimitives.ReverseEndianness(reinterpretedArray, reinterpretedArray);
                        }
                        bytesRead = array.Length * {{byteLength}};
                        return array;
                        """;
                },
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
                        bytesRead = array.Length * {byteLength};
                        return array;
                        """,
                (WellKnownCollectionKind.List, WellKnownTypeKind.Byte) => (_, _, _) =>
                    $"""
                        var list = new List<byte>(source.Length);
                        list.AddRange(source);
                        bytesRead = list.Count * {byteLength};
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
                        bytesRead = list.Count * {{byteLength}};
                        return list;
                        """,
                _ => throw new ArgumentException($"Could not emit read utility for {collectionKind} and {typeKind}"),
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
        var typeParameter = GetTypeParameter(typeKind);
        var typeParameterConstraint = GetReadTypeParameterConstraint(typeKind);
        var numberOfElementsParameter =
            collectionKind is not WellKnownCollectionKind.None && typeKind is WellKnownTypeKind.BinaryObject
                ? ", int numberOfElements"
                : string.Empty;
        var optionalReadBytesParameter = collectionKind is not WellKnownCollectionKind.None
            ? ", out int bytesRead"
            : string.Empty;
        writer.WriteLine(
            $"/// <summary> Reads a <c>{HttpUtility.HtmlEncode(collectionName)}</c> from the given source{endiannessName} </summary>"
        );
        writer.WriteLine("[MethodImpl(MethodImplOptions.AggressiveInlining)]");
        writer.WriteLine(
            $"public static {collectionName} {methodName}{typeParameter}(ReadOnlySpan<byte> source{numberOfElementsParameter}{optionalReadBytesParameter})"
        );
        if (!string.IsNullOrEmpty(typeParameterConstraint))
            writer.WriteLine(typeParameterConstraint);
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
    EnumByte,
    EnumSByte,
    EnumUShort,
    EnumShort,
    EnumUInt,
    EnumInt,
    EnumULong,
    EnumLong,
    BinaryObject,
}

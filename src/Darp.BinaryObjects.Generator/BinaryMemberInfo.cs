namespace Darp.BinaryObjects.Generator;

using System.CodeDom.Compiler;
using System.Diagnostics.CodeAnalysis;
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
        [NotNullWhen(true)] out string? writeString,
        out int bytesRead
    );
}

internal sealed class ConstantBinaryMemberGroup(IReadOnlyList<IConstantMember> members) : IGroup
{
    public IReadOnlyList<IConstantMember> Members { get; } = members;
    public int ConstantByteLength { get; } = members.Sum(x => x.ConstantByteLength);

    public string GetLengthCodeString() => $"{ConstantByteLength}";
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
        writeString =
            $"global::Darp.BinaryObjects.Generated.Utilities.{methodName}(destination[{currentByteIndex}..], this.{MemberSymbol.Name});";
        bytesWritten = ConstantByteLength;
        return true;
    }

    public bool TryGetReadString(
        bool isLittleEndian,
        int currentByteIndex,
        [NotNullWhen(true)] out string? writeString,
        out int bytesRead
    )
    {
        var variableName = $"{BinaryObjectsGenerator.Prefix}read{MemberSymbol.Name}";
        var methodName = BinaryObjectsGenerator.GetReadMethodName(CollectionKind, TypeKind, isLittleEndian);
        writeString =
            $"var {variableName} = global::Darp.BinaryObjects.Generated.Utilities.{methodName}(source[{currentByteIndex}..]);";
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

    public required WellKnownCollectionKind WellKnownCollectionKind { get; init; }
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
        (string MethodName, Func<string, string>? Func)? match = (
            ArrayKind: WellKnownCollectionKind,
            TypeSymbol.ToDisplayString()
        ) switch
        {
            (WellKnownCollectionKind.Memory, "byte") => ("WriteUInt8Span", s => $"{s}.Span"),
            (WellKnownCollectionKind.Memory, "ushort") => ($"WriteUInt16Span{isLittleEndian}", s => $"{s}.Span"),
            (WellKnownCollectionKind.Array, "byte") => ("WriteUInt8Span", null),
            (WellKnownCollectionKind.Array, "ushort") => ($"WriteUInt16Span{isLittleEndian}", null),
            (WellKnownCollectionKind.List, "byte") => ("WriteUInt8List", null),
            (WellKnownCollectionKind.List, "ushort") => ($"WriteUInt16List{isLittleEndian}", null),
            (WellKnownCollectionKind.Enumerable, "byte") => ("WriteUInt8Enumerable", null),
            (WellKnownCollectionKind.Enumerable, "ushort") => ($"WriteUInt16Enumerable{isLittleEndian}", null),
            _ => null,
        };
        if (match is null)
        {
            writeString = null;
            bytesWritten = default;
            return false;
        }
        var memberName = $"this.{MemberSymbol.Name}";
        if (match.Value.Func is not null)
            memberName = match.Value.Func(memberName);
        writeString =
            $"global::Darp.BinaryObjects.Generated.Utilities.{match.Value.MethodName}(destination[{currentByteIndex}..], {memberName}, {ConstantByteLength / TypeByteLength});";
        bytesWritten = ConstantByteLength;
        return true;
    }

    public bool TryGetReadString(
        bool isLittleEndian,
        int currentByteIndex,
        [NotNullWhen(true)] out string? writeString,
        out int bytesRead
    )
    {
        var variableName = $"{BinaryObjectsGenerator.Prefix}read{MemberSymbol.Name}";
        var methodName = (ArrayKind: WellKnownCollectionKind, TypeSymbol.ToString()) switch
        {
            (
                WellKnownCollectionKind.Array
                    or WellKnownCollectionKind.Memory
                    or WellKnownCollectionKind.Enumerable,
                "byte"
            ) => "ReadUInt8Array",
            (
                WellKnownCollectionKind.Array
                    or WellKnownCollectionKind.Memory
                    or WellKnownCollectionKind.Enumerable,
                "ushort"
            ) => $"ReadUInt16Array{isLittleEndian}",
            (WellKnownCollectionKind.List, "byte") => "ReadUInt8List",
            (WellKnownCollectionKind.List, "ushort") => $"ReadUInt16List{isLittleEndian}",
            _ => null,
        };
        if (methodName is null)
        {
            writeString = null;
            bytesRead = default;
            return false;
        }
        writeString =
            $"var {variableName} = global::Darp.BinaryObjects.Generated.Utilities.{methodName}(source[{currentByteIndex}..{currentByteIndex + ConstantByteLength}]);";
        bytesRead = ConstantByteLength;
        return true;
    }
}

internal interface IVariableMemberGroup : IMember, IGroup
{
    public new int ConstantByteLength { get; }
    public int TypeByteLength { get; }
    public string GetVariableByteLength();
    public string GetVariableDocCommentLength();
}

internal sealed class VariableArrayMemberGroup : IVariableMemberGroup
{
    public required WellKnownCollectionKind CollectionKind { get; init; }
    public required WellKnownTypeKind TypeKind { get; init; }
    public required ISymbol MemberSymbol { get; init; }
    public required ITypeSymbol TypeSymbol { get; init; }
    public required int TypeByteLength { get; init; }

    public required WellKnownCollectionKind WellKnownCollectionKind { get; init; }
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

    public string GetVariableDocCommentLength() => throw new NotImplementedException();

    public string GetDocCommentLength() => $"""{TypeByteLength} * <see cref="{ArrayLengthMemberName}"/>""";

    public string GetLengthCodeString() => $"{TypeByteLength} * this.{ArrayLengthMemberName}";
}

partial class BinaryObjectsGenerator
{
    internal static WellKnownTypeKind GetWellKnownTypeKind(ITypeSymbol symbol)
    {
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

    private static string GetWellKnownName(WellKnownTypeKind wellKnownTypeKind)
    {
        return wellKnownTypeKind switch
        {
            WellKnownTypeKind.Bool => "Bool",
            WellKnownTypeKind.Byte => "UInt8",
            WellKnownTypeKind.SByte => "Int8",
            WellKnownTypeKind.UShort => "UInt16",
            WellKnownTypeKind.Short => "Int16",
            WellKnownTypeKind.Half => "Half",
            WellKnownTypeKind.UInt => "UInt32",
            WellKnownTypeKind.Int => "Int32",
            WellKnownTypeKind.Float => "Single",
            WellKnownTypeKind.ULong => "UInt64",
            WellKnownTypeKind.Long => "Int64",
            WellKnownTypeKind.Double => "Double",
            WellKnownTypeKind.UInt128 => "UInt128",
            WellKnownTypeKind.Int128 => "Int128",
            _ => throw new ArgumentOutOfRangeException(nameof(wellKnownTypeKind)),
        };
    }

    private static string GetWellKnownDisplayName(WellKnownTypeKind wellKnownTypeKind)
    {
        return wellKnownTypeKind switch
        {
            WellKnownTypeKind.Bool => "bool",
            WellKnownTypeKind.Byte => "byte",
            WellKnownTypeKind.SByte => "sbyte",
            WellKnownTypeKind.UShort => "ushort",
            WellKnownTypeKind.Short => "short",
            WellKnownTypeKind.Half => "System.Half",
            WellKnownTypeKind.UInt => "uint",
            WellKnownTypeKind.Int => "int",
            WellKnownTypeKind.Float => "float",
            WellKnownTypeKind.ULong => "ulong",
            WellKnownTypeKind.Long => "long",
            WellKnownTypeKind.Double => "double",
            WellKnownTypeKind.UInt128 => "System.UInt128",
            WellKnownTypeKind.Int128 => "System.Int128",
            _ => throw new ArgumentOutOfRangeException(nameof(wellKnownTypeKind)),
        };
    }

    internal static string GetWriteMethodName(
        WellKnownCollectionKind collectionKind,
        WellKnownTypeKind typeKind,
        bool isLittleEndian
    ) => GetMethodName(false, collectionKind, typeKind, isLittleEndian);

    internal static string GetReadMethodName(
        WellKnownCollectionKind collectionKind,
        WellKnownTypeKind typeKind,
        bool isLittleEndian
    ) => GetMethodName(true, collectionKind, typeKind, isLittleEndian);

    private static string GetMethodName(
        bool isRead,
        WellKnownCollectionKind collectionKind,
        WellKnownTypeKind typeKind,
        bool isLittleEndian
    )
    {
        var typeName = GetWellKnownName(typeKind);
        var prefix = isRead ? "Read" : "Write";
        var endianness = isLittleEndian ? "LittleEndian" : "BigEndian";
        return (collectionKind, typeKind) switch
        {
            (
                WellKnownCollectionKind.None,
                WellKnownTypeKind.Bool
                    or WellKnownTypeKind.SByte
                    or WellKnownTypeKind.Byte
            ) => $"{prefix}{typeName}",
            (WellKnownCollectionKind.None, _) => $"{prefix}{typeName}{endianness}",
            _ => throw new ArgumentOutOfRangeException(nameof(collectionKind)),
        };
    }

    private static void EmitWriteUtility(
        IndentedTextWriter writer,
        WellKnownCollectionKind collectionKind,
        WellKnownTypeKind typeKind,
        bool isLittleEndian
    )
    {
        switch (collectionKind, typeKind)
        {
            case (WellKnownCollectionKind.None, WellKnownTypeKind.Bool):
                if (isLittleEndian)
                    return;
                EmitWriteAnyUtility(
                    writer,
                    _ => "destination[0] = value ? (byte)0b1 : (byte)0b0;",
                    typeKind,
                    isLittleEndian
                );
                break;
            case (WellKnownCollectionKind.None, WellKnownTypeKind.SByte):
                if (isLittleEndian)
                    return;
                EmitWriteAnyUtility(writer, _ => "destination[0] = (byte)value;", typeKind, isLittleEndian);
                break;
            case (WellKnownCollectionKind.None, WellKnownTypeKind.Byte):
                if (isLittleEndian)
                    return;
                EmitWriteAnyUtility(writer, _ => "destination[0] = value;", typeKind, isLittleEndian);
                break;
            case (WellKnownCollectionKind.None, _):
                EmitWriteAnyUtility(
                    writer,
                    methodName => $"BinaryPrimitives.{methodName}(destination, value);",
                    typeKind,
                    isLittleEndian
                );
                break;
        }
    }

    private static void EmitWriteAnyUtility(
        IndentedTextWriter writer,
        Func<string, string> getter,
        WellKnownTypeKind typeKind,
        bool isLittleEndian
    )
    {
        var typeName = GetWellKnownDisplayName(typeKind);
        var methodName = GetWriteMethodName(WellKnownCollectionKind.None, typeKind, isLittleEndian);
        writer.WriteLine($"/// <summary> Writes a <c>{typeName}</c> to the destination </summary>");
        writer.WriteLine("[MethodImpl(MethodImplOptions.AggressiveInlining)]");
        writer.WriteLine($"public static void {methodName}(Span<byte> destination, {typeName} value)");
        writer.WriteLine("{");
        writer.Indent++;
        writer.WriteLine(getter(methodName));
        writer.Indent--;
        writer.WriteLine("}");
    }

    private static void EmitReadUtility(
        IndentedTextWriter writer,
        WellKnownCollectionKind collectionKind,
        WellKnownTypeKind typeKind,
        bool isLittleEndian
    )
    {
        switch (collectionKind, typeKind)
        {
            case (WellKnownCollectionKind.None, WellKnownTypeKind.Bool):
                if (isLittleEndian)
                    return;
                EmitReadAnyUtility(writer, _ => "return source[0] > 0;", typeKind, default);
                break;
            case (WellKnownCollectionKind.None, WellKnownTypeKind.SByte):
                if (isLittleEndian)
                    return;
                EmitReadAnyUtility(writer, _ => "return (sbyte)source[0];", typeKind, default);
                break;
            case (WellKnownCollectionKind.None, WellKnownTypeKind.Byte):
                if (isLittleEndian)
                    return;
                EmitReadAnyUtility(writer, _ => "return source[0];", typeKind, default);
                break;
            case (WellKnownCollectionKind.None, _):
                EmitReadAnyUtility(
                    writer,
                    methodName => $"return BinaryPrimitives.{methodName}(source);",
                    typeKind,
                    isLittleEndian
                );
                break;
        }
    }

    private static void EmitReadAnyUtility(
        IndentedTextWriter writer,
        Func<string, string> getter,
        WellKnownTypeKind typeKind,
        bool isLittleEndian
    )
    {
        var typeName = GetWellKnownDisplayName(typeKind);
        var methodName = GetReadMethodName(WellKnownCollectionKind.None, typeKind, isLittleEndian);
        writer.WriteLine($"/// <summary> Reads a <c>{typeName}</c> from the given source </summary>");
        writer.WriteLine("[MethodImpl(MethodImplOptions.AggressiveInlining)]");
        writer.WriteLine($"public static {typeName} {methodName}(ReadOnlySpan<byte> source)");
        writer.WriteLine("{");
        writer.Indent++;
        writer.WriteMultiLine(getter(methodName));
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
}

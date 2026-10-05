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
    public bool IsNarrow => TypeKind.IsNarrow(TypeByteLength);

    public string GetDocCommentLength() => $"{TypeByteLength}";

    /// <summary> The condition under which the value does not fit into its byte count, if it is narrower than its type </summary>
    public string? GetWriteOverflowCheck()
    {
        if (!IsNarrow)
            return null;
        var methodName = BinaryObjectsGenerator.GetFitsMethodName(TypeKind);
        var optionalCast = BinaryObjectsGenerator.GetOptionalCastToUnderlyingEnumValue(TypeSymbol);
        return $"!global::Darp.BinaryObjects.Generated.Utilities.{methodName}({optionalCast}this.{MemberSymbol.Name}, {TypeByteLength})";
    }

    public bool TryGetWriteString(
        bool isLittleEndian,
        int currentByteIndex,
        [NotNullWhen(true)] out string? writeString,
        out int bytesWritten
    )
    {
        var methodName = BinaryObjectsGenerator.GetWriteMethodName(CollectionKind, TypeKind, isLittleEndian, IsNarrow);
        var optionalCast = BinaryObjectsGenerator.GetOptionalCastToUnderlyingEnumValue(TypeSymbol);
        writeString =
            $"global::Darp.BinaryObjects.Generated.Utilities.{methodName}(destination[{currentByteIndex}..{currentByteIndex + ConstantByteLength}], {optionalCast}this.{MemberSymbol.Name});";
        if (TypeKind is WellKnownTypeKind.BinaryObject)
        {
            var countName = $"{BinaryObjectsGenerator.Prefix}bytesWritten{MemberSymbol.Name}";
            writeString = $$"""
                if (!global::Darp.BinaryObjects.Generated.Utilities.{{methodName}}(destination[{{currentByteIndex}}..{{currentByteIndex
                    + ConstantByteLength}}], this.{{MemberSymbol.Name}}, out var {{countName}}))
                {
                    bytesWritten += {{countName}};
                    return false;
                }
                """;
        }
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
        var methodName = BinaryObjectsGenerator.GetReadMethodName(CollectionKind, TypeKind, isLittleEndian, IsNarrow);
        // The shared helpers read 64 bit values
        var optionalCast =
            IsNarrow && TypeKind is not (WellKnownTypeKind.Long or WellKnownTypeKind.ULong)
                ? $"({TypeSymbol.ToDisplayString()}) "
                : BinaryObjectsGenerator.GetOptionalCastToEnum(TypeKind, TypeSymbol);
        var optionalGeneric = BinaryObjectsGenerator.GetOptionalGenericTypeParameter(
            CollectionKind,
            TypeKind,
            TypeSymbol
        );
        readString =
            $"var {variableName} = {optionalCast}global::Darp.BinaryObjects.Generated.Utilities.{methodName}{optionalGeneric}(source[{currentByteIndex}..{currentByteIndex + ConstantByteLength}]);";
        if (TypeKind is WellKnownTypeKind.BinaryObject)
        {
            var countName = $"{BinaryObjectsGenerator.Prefix}bytesRead{MemberSymbol.Name}";
            readString = $$"""
                if (!global::Darp.BinaryObjects.Generated.Utilities.{{methodName}}{{optionalGeneric}}(source[{{currentByteIndex}}..{{currentByteIndex
                    + ConstantByteLength}}], out var {{variableName}}, out var {{countName}}))
                {
                    bytesRead += {{countName}};
                    return false;
                }
                """;
        }
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

    public int ConstantByteLength => TypeByteLength * ArrayLength;
    public bool IsNarrow => TypeKind.IsNarrow(TypeByteLength);

    public string GetDocCommentLength() => $"{TypeByteLength} * {ArrayLength}";

    public bool TryGetWriteString(
        bool isLittleEndian,
        int currentByteIndex,
        [NotNullWhen(true)] out string? writeString,
        out int bytesWritten
    )
    {
        var memberName = this.GetWriteValue();
        var methodName = BinaryObjectsGenerator.GetWriteMethodName(CollectionKind, TypeKind, isLittleEndian, IsNarrow);
        var optionalGeneric = BinaryObjectsGenerator.GetOptionalGenericTypeParameter(
            CollectionKind,
            TypeKind,
            TypeSymbol
        );
        writeString =
            $"global::Darp.BinaryObjects.Generated.Utilities.{methodName}{optionalGeneric}(destination[{currentByteIndex}..{currentByteIndex + ConstantByteLength}], {memberName});";
        if (TypeKind is WellKnownTypeKind.BinaryObject || IsNarrow)
        {
            var countName = $"{BinaryObjectsGenerator.Prefix}bytesWritten{MemberSymbol.Name}";
            writeString = $$"""
                if (!global::Darp.BinaryObjects.Generated.Utilities.{{methodName}}{{optionalGeneric}}(destination[{{currentByteIndex}}..{{currentByteIndex
                    + ConstantByteLength}}], {{memberName}}, {{TypeByteLength}}, out var {{countName}}))
                {
                    bytesWritten += {{countName}};
                    return false;
                }
                """;
        }
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
        var methodName = BinaryObjectsGenerator.GetReadMethodName(CollectionKind, TypeKind, isLittleEndian, IsNarrow);
        var optionalElementLength =
            TypeKind is WellKnownTypeKind.BinaryObject || IsNarrow ? $", {TypeByteLength}" : string.Empty;
        var optionalGeneric = BinaryObjectsGenerator.GetOptionalGenericTypeParameter(
            CollectionKind,
            TypeKind,
            TypeSymbol
        );
        readString =
            $"var {variableName} = global::Darp.BinaryObjects.Generated.Utilities.{methodName}{optionalGeneric}(source[{currentByteIndex}..{currentByteIndex + ConstantByteLength}]{optionalElementLength}, out _);";
        if (TypeKind is WellKnownTypeKind.BinaryObject)
        {
            var countName = $"{BinaryObjectsGenerator.Prefix}bytesRead{MemberSymbol.Name}";
            readString = $$"""
                if (!global::Darp.BinaryObjects.Generated.Utilities.{{methodName}}{{optionalGeneric}}(source[{{currentByteIndex}}..{{currentByteIndex
                    + ConstantByteLength}}], {{TypeByteLength}}, out var {{variableName}}, out var {{countName}}))
                {
                    bytesRead += {{countName}};
                    return false;
                }
                """;
        }
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
        [NotNullWhen(true)] out string? writeString,
        out string? bytesWrittenString
    );

    public bool TryGetReadString(
        bool isLittleEndian,
        int currentByteIndex,
        [NotNullWhen(true)] out string? readString,
        [NotNullWhen(true)] out string? bytesReadString
    );
}

internal sealed class VariableArrayMemberGroup : IVariableMemberGroup
{
    public required WellKnownCollectionKind CollectionKind { get; init; }
    public required WellKnownTypeKind TypeKind { get; init; }
    public required ISymbol MemberSymbol { get; init; }
    public required ITypeSymbol TypeSymbol { get; init; }
    public required int TypeByteLength { get; init; }

    public required string ArrayLengthMemberName { get; init; }

    /// <summary> Whether the length member holds the number of bytes of the collection instead of its number of elements </summary>
    public required bool LengthIsInBytes { get; init; }
    public required int ArrayMinLength { get; init; }

    public int ConstantByteLength => TypeByteLength * ArrayMinLength;
    private bool IsNarrow => TypeKind.IsNarrow(TypeByteLength);

    private string GetElementCount(string length) =>
        LengthIsInBytes && TypeByteLength > 1 ? $"{length} / {TypeByteLength}" : length;

    private string GetByteLength(string length) => LengthIsInBytes ? length : $"{TypeByteLength} * {length}";

    /// <summary> The condition under which the length cannot describe a collection in the buffer </summary>
    private string GetLengthCheck(string length, string buffer)
    {
        if (!LengthIsInBytes)
            return $"{length} < 0 || {length} > {buffer}.Length / {TypeByteLength}";
        // A negative length is rejected before it is divided by the element length
        var optionalRemainderCheck = TypeByteLength > 1 ? $" || {length} % {TypeByteLength} != 0" : "";
        return $"{length} < 0 || {length} > {buffer}.Length{optionalRemainderCheck}";
    }

    public string GetVariableByteLength()
    {
        if (ArrayMinLength == 0)
            return GetByteLength($"this.{ArrayLengthMemberName}");
        return LengthIsInBytes
            ? $"global::System.Math.Max((int)this.{ArrayLengthMemberName}, {TypeByteLength * ArrayMinLength})"
            : $"{TypeByteLength} * global::System.Math.Max((int)this.{ArrayLengthMemberName}, {ArrayMinLength})";
    }

    public string GetVariableDocCommentLength() => GetByteLength($"""<see cref="{ArrayLengthMemberName}"/>""");

    public string GetDocCommentLength()
    {
        if (ArrayMinLength > 0 && !LengthIsInBytes)
            return $"""{TypeByteLength} * ({ArrayMinLength} + <see cref="{ArrayLengthMemberName}"/> - {ArrayMinLength})""";
        return GetVariableDocCommentLength();
    }

    public string GetLengthCodeString() => GetByteLength($"this.{ArrayLengthMemberName}");

    public bool TryGetWriteString(
        bool isLittleEndian,
        int currentByteIndex,
        [NotNullWhen(true)] out string? writeString,
        out string? bytesWrittenString
    )
    {
        var memberName = this.GetWriteValue();
        var methodName = BinaryObjectsGenerator.GetWriteMethodName(CollectionKind, TypeKind, isLittleEndian, IsNarrow);
        var optionalGeneric = BinaryObjectsGenerator.GetOptionalGenericTypeParameter(
            CollectionKind,
            TypeKind,
            TypeSymbol
        );
        var length = $"this.{ArrayLengthMemberName}";
        var elementCount = GetElementCount(length);
        var optionalMinLengthCheck = ArrayMinLength > 0 ? $" || {elementCount} < {ArrayMinLength}" : "";
        var collectionLengthCheck = $" || {this.GetCollectionCount()} < {elementCount}";
        var byteLengthVariable = $"{BinaryObjectsGenerator.Prefix}byteLength{MemberSymbol.Name}";
        writeString = $"""
            if ({GetLengthCheck(length, "destination")}{optionalMinLengthCheck}{collectionLengthCheck})
                return false;
            var {byteLengthVariable} = {GetByteLength(length)};
            global::Darp.BinaryObjects.Generated.Utilities.{methodName}{optionalGeneric}(destination[{currentByteIndex}..{byteLengthVariable}], {memberName});
            """;
        if (TypeKind is WellKnownTypeKind.BinaryObject || IsNarrow)
        {
            var countName = $"{BinaryObjectsGenerator.Prefix}bytesWritten{MemberSymbol.Name}";
            writeString = $$"""
                if ({{GetLengthCheck(length, "destination")}}{{optionalMinLengthCheck}}{{collectionLengthCheck}})
                    return false;
                var {{byteLengthVariable}} = {{GetByteLength(length)}};
                if (!global::Darp.BinaryObjects.Generated.Utilities.{{methodName}}{{optionalGeneric}}(destination[{{currentByteIndex}}..{{byteLengthVariable}}], {{memberName}}, {{TypeByteLength}}, out var {{countName}}))
                {
                    bytesWritten += {{countName}};
                    return false;
                }
                """;
        }
        bytesWrittenString = byteLengthVariable;
        return true;
    }

    public bool TryGetReadString(
        bool isLittleEndian,
        int currentByteIndex,
        [NotNullWhen(true)] out string? readString,
        [NotNullWhen(true)] out string? bytesReadString
    )
    {
        var variableName = $"{BinaryObjectsGenerator.Prefix}read{MemberSymbol.Name}";
        var methodName = BinaryObjectsGenerator.GetReadMethodName(CollectionKind, TypeKind, isLittleEndian, IsNarrow);
        var length = $"{BinaryObjectsGenerator.Prefix}read{ArrayLengthMemberName}";
        var lengthVariableName = $"{BinaryObjectsGenerator.Prefix}byteLength{MemberSymbol.Name}";
        var optionalGeneric = BinaryObjectsGenerator.GetOptionalGenericTypeParameter(
            CollectionKind,
            TypeKind,
            TypeSymbol
        );
        var optionalElementLength =
            TypeKind is WellKnownTypeKind.BinaryObject || IsNarrow ? $", {TypeByteLength}" : string.Empty;
        var optionalMinLengthCheck = ArrayMinLength > 0 ? $" || {GetElementCount(length)} < {ArrayMinLength}" : "";
        readString = $"""
            if ({GetLengthCheck(length, "source")}{optionalMinLengthCheck})
                return false;
            var {lengthVariableName} = {GetByteLength(length)};
            var {variableName} = global::Darp.BinaryObjects.Generated.Utilities.{methodName}{optionalGeneric}(source[{currentByteIndex}..{lengthVariableName}]{optionalElementLength}, out _);
            """;
        if (TypeKind is WellKnownTypeKind.BinaryObject)
        {
            var countName = $"{BinaryObjectsGenerator.Prefix}bytesRead{MemberSymbol.Name}";
            readString = $$"""
                if ({{GetLengthCheck(length, "source")}}{{optionalMinLengthCheck}})
                    return false;
                var {{lengthVariableName}} = {{GetByteLength(length)}};
                if (!global::Darp.BinaryObjects.Generated.Utilities.{{methodName}}{{optionalGeneric}}(source[{{currentByteIndex}}..{{lengthVariableName}}], {{TypeByteLength}}, out var {{variableName}}, out var {{countName}}))
                {
                    bytesRead += {{countName}};
                    return false;
                }
                """;
        }
        bytesReadString = lengthVariableName;
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
    private bool IsNarrow => TypeKind.IsNarrow(TypeByteLength);

    public string GetVariableByteLength()
    {
        if (ArrayMinLength > 0)
            return $"{TypeByteLength} * global::System.Math.Max({this.GetCollectionCount()}, {ArrayMinLength})";
        return $"{TypeByteLength} * {this.GetCollectionCount()}";
    }

    public string GetVariableDocCommentLength() => GetDocCommentLength();

    public string GetDocCommentLength() =>
        ArrayMinLength > 0 ? $"{TypeByteLength} * ({ArrayMinLength} + n)" : $"{TypeByteLength} * n";

    public string GetLengthCodeString() => $"{TypeByteLength} * {this.GetCollectionCount()}";

    public bool TryGetWriteString(
        bool isLittleEndian,
        int currentByteIndex,
        [NotNullWhen(true)] out string? writeString,
        out string? bytesWrittenString
    )
    {
        var memberName = this.GetWriteValue();
        var methodName = BinaryObjectsGenerator.GetWriteMethodName(CollectionKind, TypeKind, isLittleEndian, IsNarrow);
        var optionalGeneric = BinaryObjectsGenerator.GetOptionalGenericTypeParameter(
            CollectionKind,
            TypeKind,
            TypeSymbol
        );
        var optionalMinLengthCheck =
            ArrayMinLength > 0 ? $" || destination.Length < {TypeByteLength * ArrayMinLength}" : "";
        var minimumCountCheck =
            ArrayMinLength > 0 ? $" || {this.GetCollectionCount()} < {ArrayMinLength}" : string.Empty;

        writeString = $"""
            if ({this.GetCollectionCount()} > destination.Length / {TypeByteLength}{optionalMinLengthCheck}{minimumCountCheck})
                return false;
            bytesWritten += global::Darp.BinaryObjects.Generated.Utilities.{methodName}{optionalGeneric}(destination, {memberName});
            """;
        if (TypeKind is WellKnownTypeKind.BinaryObject || IsNarrow)
        {
            var countName = $"{BinaryObjectsGenerator.Prefix}bytesWritten{MemberSymbol.Name}";
            writeString = $$"""
                if ({{this.GetCollectionCount()}} > destination.Length / {{TypeByteLength}}{{optionalMinLengthCheck}}{{minimumCountCheck}})
                    return false;
                if (!global::Darp.BinaryObjects.Generated.Utilities.{{methodName}}{{optionalGeneric}}(destination, {{memberName}}, {{TypeByteLength}}, out var {{countName}}))
                {
                    bytesWritten += {{countName}};
                    return false;
                }
                bytesWritten += {{countName}};
                """;
        }
        bytesWrittenString = null;
        return true;
    }

    public bool TryGetReadString(
        bool isLittleEndian,
        int currentByteIndex,
        [NotNullWhen(true)] out string? readString,
        [NotNullWhen(true)] out string? bytesReadString
    )
    {
        var variableBytesReadName = $"{BinaryObjectsGenerator.Prefix}bytesRead{MemberSymbol.Name}";
        var variableName = $"{BinaryObjectsGenerator.Prefix}read{MemberSymbol.Name}";
        var methodName = BinaryObjectsGenerator.GetReadMethodName(CollectionKind, TypeKind, isLittleEndian, IsNarrow);
        var optionalGeneric = BinaryObjectsGenerator.GetOptionalGenericTypeParameter(
            CollectionKind,
            TypeKind,
            TypeSymbol
        );
        var optionalElementLength =
            TypeKind is WellKnownTypeKind.BinaryObject || IsNarrow ? $", {TypeByteLength}" : string.Empty;
        readString = $"""
            var {variableName} = global::Darp.BinaryObjects.Generated.Utilities.{methodName}{optionalGeneric}(source{optionalElementLength}, out int {variableBytesReadName});
            """;
        if (TypeKind is WellKnownTypeKind.BinaryObject)
        {
            readString = $$"""
                if (!global::Darp.BinaryObjects.Generated.Utilities.{{methodName}}{{optionalGeneric}}(source, {{TypeByteLength}}, out var {{variableName}}, out var {{variableBytesReadName}}))
                {
                    bytesRead += {{variableBytesReadName}};
                    return false;
                }
                """;
        }
        if (ArrayMinLength > 0)
        {
            readString = $"""
                if (source.Length < {TypeByteLength * ArrayMinLength})
                    return false;
                {readString}
                """;
        }
        bytesReadString = variableBytesReadName;
        return true;
    }
}

internal sealed class BinaryObjectMemberGroup : IMember, IGroup
{
    public required ISymbol MemberSymbol { get; init; }
    public required ITypeSymbol TypeSymbol { get; init; }
    public required bool UseInterfaceDispatch { get; init; }

    public WellKnownCollectionKind CollectionKind => WellKnownCollectionKind.None;
    public WellKnownTypeKind TypeKind => WellKnownTypeKind.BinaryObject;
    public int ConstantByteLength => 0;

    public string GetLengthCodeString() => $"this.{TypeSymbol.ToDisplayString()}.GetByteCount()";

    public string? GetVariableByteLength() =>
        UseInterfaceDispatch
            ? $"global::Darp.BinaryObjects.Generated.Utilities.GetBinaryObjectByteCount(this.{MemberSymbol.Name})"
            : $"this.{MemberSymbol.Name}.GetByteCount()";

    public string GetVariableDocCommentLength() => GetDocCommentLength();

    public string GetDocCommentLength() =>
        UseInterfaceDispatch
            ? """<see cref="global::Darp.BinaryObjects.IBinaryWritable.GetByteCount()"/>"""
            : $"""<see cref="{TypeSymbol.ToDisplayString()}.GetByteCount()"/>""";
}

partial class BinaryObjectsGenerator
{
    internal static string GetOptionalCastToEnum(WellKnownTypeKind typeKind, ITypeSymbol symbol) =>
        typeKind.IsEnum() ? $"({symbol.ToDisplayString()}) " : string.Empty;

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
        if (collectionKind is not WellKnownCollectionKind.None && typeKind.IsEnum())
            return $"<{typeSymbol.ToDisplayString()}>";
        return typeKind is WellKnownTypeKind.BinaryObject ? $"<{typeSymbol.ToDisplayString()}>" : string.Empty;
    }

    internal static WellKnownTypeKind GetWellKnownTypeKind(ITypeSymbol symbol)
    {
        if (IsBinaryObject(symbol))
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

    private static bool IsBinaryObject(ITypeSymbol symbol)
    {
        var hasBinaryObjectAttribute = symbol
            .GetAttributes()
            .Any(x => x.AttributeClass?.ToDisplayString() == BinaryObjectAttributeName);
        var hasBinaryObjectInterface = symbol.AllInterfaces.Any(x =>
            x.OriginalDefinition.ToDisplayString()
                is "Darp.BinaryObjects.IBinaryWritable"
                    or "Darp.BinaryObjects.IBinaryReadable<TSelf>"
        );
        return hasBinaryObjectAttribute || hasBinaryObjectInterface;
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
            (WellKnownCollectionKind.None, WellKnownTypeKind.EnumSByte) => "Int8",
            (WellKnownCollectionKind.None, WellKnownTypeKind.EnumUShort) => "UInt16",
            (WellKnownCollectionKind.None, WellKnownTypeKind.EnumShort) => "Int16",
            (WellKnownCollectionKind.None, WellKnownTypeKind.EnumUInt) => "UInt32",
            (WellKnownCollectionKind.None, WellKnownTypeKind.EnumInt) => "Int32",
            (WellKnownCollectionKind.None, WellKnownTypeKind.EnumULong) => "UInt64",
            (WellKnownCollectionKind.None, WellKnownTypeKind.EnumLong) => "Int64",
            (_, WellKnownTypeKind.EnumByte) => "UInt8Enum",
            (_, WellKnownTypeKind.EnumSByte) => "Int8Enum",
            (_, WellKnownTypeKind.EnumUShort) => "UInt16Enum",
            (_, WellKnownTypeKind.EnumShort) => "Int16Enum",
            (_, WellKnownTypeKind.EnumUInt) => "UInt32Enum",
            (_, WellKnownTypeKind.EnumInt) => "Int32Enum",
            (_, WellKnownTypeKind.EnumULong) => "UInt64Enum",
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
            // ICollection<T> and IReadOnlyCollection<T> share enumeration; the member writer validates their Count.
            WellKnownCollectionKind.Collection => $"IEnumerable<{typeKindDisplayName}>",
            _ => throw new ArgumentException($"Could get well known display name for {collectionKind}"),
        };
    }

    private static string GetWellKnownEnumIntegerDisplayName(WellKnownTypeKind typeKind) =>
        typeKind switch
        {
            WellKnownTypeKind.EnumUShort => "ushort",
            WellKnownTypeKind.EnumShort => "short",
            WellKnownTypeKind.EnumUInt => "uint",
            WellKnownTypeKind.EnumInt => "int",
            WellKnownTypeKind.EnumULong => "ulong",
            WellKnownTypeKind.EnumLong => "long",
            _ => throw new ArgumentException($"Expected a multi-byte enum, got {typeKind}"),
        };

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
        bool isLittleEndian,
        bool isNarrow = false
    )
    {
        if (isNarrow && collectionKind is WellKnownCollectionKind.None)
            return $"Write{GetSignednessName(typeKind)}{GetEndiannessName(typeKind, isLittleEndian)}";
        var typeName = GetWellKnownName(collectionKind, typeKind);
        var prefix = typeKind is WellKnownTypeKind.BinaryObject || isNarrow ? "Try" : string.Empty;
        var endianness = GetEndiannessName(typeKind, isLittleEndian);
        return prefix
            + (
                collectionKind switch
                {
                    WellKnownCollectionKind.None => $"Write{typeName}{endianness}",
                    WellKnownCollectionKind.Span
                    or WellKnownCollectionKind.Memory
                    or WellKnownCollectionKind.Array
                    or WellKnownCollectionKind.List => $"Write{typeName}Span{endianness}",
                    WellKnownCollectionKind.Collection => $"Write{typeName}Collection{endianness}",
                    _ => throw new ArgumentException(
                        $"Could create write method name for {collectionKind} and {typeKind} (littleEndian={isLittleEndian})"
                    ),
                }
            );
    }

    internal static string GetReadMethodName(
        WellKnownCollectionKind collectionKind,
        WellKnownTypeKind typeKind,
        bool isLittleEndian,
        bool isNarrow = false
    )
    {
        if (isNarrow && collectionKind is WellKnownCollectionKind.None)
            return $"Read{GetSignednessName(typeKind)}{GetEndiannessName(typeKind, isLittleEndian)}";
        var typeName = GetWellKnownName(collectionKind, typeKind);
        var prefix = typeKind is WellKnownTypeKind.BinaryObject ? "Try" : string.Empty;
        var endianness = GetEndiannessName(typeKind, isLittleEndian);
        return prefix
            + (
                collectionKind switch
                {
                    WellKnownCollectionKind.None => $"Read{typeName}{endianness}",
                    WellKnownCollectionKind.List => $"Read{typeName}List{endianness}",
                    WellKnownCollectionKind.Memory
                    or WellKnownCollectionKind.Array
                    or WellKnownCollectionKind.Collection => $"Read{typeName}Array{endianness}",
                    _ => throw new ArgumentException(
                        $"Could create read method name for {collectionKind} and {typeKind} (littleEndian={isLittleEndian})"
                    ),
                }
            );
    }

    internal static UtilityData[] GetUtilities(
        bool isRead,
        WellKnownCollectionKind collectionKind,
        WellKnownTypeKind typeKind,
        int? typeByteLength
    )
    {
        if (typeByteLength is { } byteLength && typeKind.IsNarrow(byteLength))
            return GetNarrowUtilities(isRead, collectionKind, typeKind);
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
        // Binary object generation does not depend on the byte length
        if (typeKind is WellKnownTypeKind.BinaryObject)
            typeByteLength = UtilityData.UnknownLength;
        var utilityKind = (isRead, collectionKind) switch
        {
            (true, WellKnownCollectionKind.Memory or WellKnownCollectionKind.Collection) =>
                WellKnownCollectionKind.Array,
            (false, WellKnownCollectionKind.Memory or WellKnownCollectionKind.Array or WellKnownCollectionKind.List) =>
                WellKnownCollectionKind.Span,
            _ => collectionKind,
        };
        var utility = new UtilityData(isRead, utilityKind, typeKind, typeByteLength, emitLittleAndBigEndianMethods);
        return
            !isRead
            && utilityKind is WellKnownCollectionKind.Collection
            && typeKind is not WellKnownTypeKind.BinaryObject
            ?
            [
                new UtilityData(
                    false,
                    WellKnownCollectionKind.Span,
                    typeKind,
                    typeByteLength,
                    emitLittleAndBigEndianMethods
                ),
                utility,
            ]
            : [utility];
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
                WellKnownTypeKind.SByte => (_, _, _) => "destination[0] = unchecked((byte)value);",
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
                    """
                        var length = Math.Min(value.Length, destination.Length);
                        MemoryMarshal.Cast<TEnum, byte>(value[..length]).CopyTo(destination);
                        return length;
                        """,
                (WellKnownCollectionKind.Span, _) when typeKind.IsEnum() => (_, _, isLittleEndian) =>
                {
                    var integerType = GetWellKnownEnumIntegerDisplayName(typeKind);
                    return $$"""
                        var length = Math.Min(value.Length, destination.Length / {{byteLength}});
                        if ({{CheckForReverseEndianness(isLittleEndian)}})
                        {
                            ReadOnlySpan<{{integerType}}> reinterpretedValue = MemoryMarshal.Cast<TEnum, {{integerType}}>(value);
                            Span<{{integerType}}> reinterpretedDestination = MemoryMarshal.Cast<byte, {{integerType}}>(destination);
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
                (WellKnownCollectionKind.Collection, _) => (_, typeName, isLittleEndian) =>
                    $$"""
                        using var enumerator = value.GetEnumerator();
                        var count = destination.Length / {{byteLength}};
                        var index = 0;
                        for (; index < count && enumerator.MoveNext(); index++)
                        {
                            var item = enumerator.Current;
                            {{GetWriteMethodName(
                            WellKnownCollectionKind.Span,
                            typeKind,
                            isLittleEndian
                        )}}{{GetTypeParameter(
                            typeKind
                        )}}(destination.Slice(index * {{byteLength}}, {{byteLength}}), MemoryMarshal.CreateReadOnlySpan(ref item, 1));
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
            _ when typeKind.IsEnum() => "<TEnum>",
            _ => string.Empty,
        };

    private static string GetWriteTypeParameterConstraint(WellKnownTypeKind typeKind) =>
        typeKind switch
        {
            WellKnownTypeKind.BinaryObject => "    where T : IBinaryWritable",
            _ when typeKind.IsEnum() => "    where TEnum : unmanaged, Enum",
            _ => string.Empty,
        };

    private static string GetReadTypeParameterConstraint(WellKnownTypeKind typeKind) =>
        typeKind switch
        {
            WellKnownTypeKind.BinaryObject => "    where T : IBinaryReadable<T>",
            _ when typeKind.IsEnum() => "    where TEnum : unmanaged, Enum",
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
        int? byteLength,
        bool emitLittleAndBigEndian
    )
    {
        if (collectionKind is WellKnownCollectionKind.None)
        {
            GetReadMethodBody methodBodyGetter = typeKind switch
            {
                WellKnownTypeKind.Bool => (_, _, _) => "return source[0] > 0;",
                WellKnownTypeKind.SByte => (_, _, _) => "return unchecked((sbyte)source[0]);",
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
            GetReadMethodBody methodBodyGetter = (collectionKind, typeKind, byteLength) switch
            {
                (WellKnownCollectionKind.Memory or WellKnownCollectionKind.Array, WellKnownTypeKind.Byte, _) => (
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
                        or WellKnownTypeKind.EnumSByte,
                    _
                ) => (_, _, _) =>
                    """
                        bytesRead = source.Length;
                        return MemoryMarshal.Cast<byte, TEnum>(source).ToArray();
                        """,
                (
                    WellKnownCollectionKind.Memory
                        or WellKnownCollectionKind.Array,
                    WellKnownTypeKind.Bool
                        or WellKnownTypeKind.SByte,
                    _
                ) => (_, typeName, _) =>
                    $"""
                        bytesRead = source.Length;
                        return MemoryMarshal.Cast<byte, {typeName}>(source).ToArray();
                        """,
                (WellKnownCollectionKind.Array, _, not null) when typeKind.IsEnum() => (_, _, isLittleEndian) =>
                {
                    var integerType = GetWellKnownEnumIntegerDisplayName(typeKind);
                    return $$"""
                        var array = MemoryMarshal.Cast<byte, TEnum>(source).ToArray();
                        if ({{CheckForReverseEndianness(isLittleEndian)}})
                        {
                            Span<{{integerType}}> reinterpretedArray = MemoryMarshal.Cast<TEnum, {{integerType}}>(array.AsSpan());
                            BinaryPrimitives.ReverseEndianness(reinterpretedArray, reinterpretedArray);
                        }
                        bytesRead = array.Length * {{byteLength}};
                        return array;
                        """;
                },
                (
                    WellKnownCollectionKind.Memory
                        or WellKnownCollectionKind.Array
                        or WellKnownCollectionKind.Collection,
                    _,
                    not null
                ) => (_, typeName, isLittleEndian) =>
                    $"""
                        var array = MemoryMarshal.Cast<byte, {typeName}>(source).ToArray();
                        if ({CheckForReverseEndianness(isLittleEndian)})
                            BinaryPrimitives.ReverseEndianness(array, array);
                        bytesRead = array.Length * {byteLength};
                        return array;
                        """,
                (WellKnownCollectionKind.List, WellKnownTypeKind.Byte, not null) => (_, _, _) =>
                    $"""
                        var list = new List<byte>(source.Length);
                        list.AddRange(source);
                        bytesRead = list.Count * {byteLength};
                        return list;
                        """,
                (WellKnownCollectionKind.List, WellKnownTypeKind.EnumByte or WellKnownTypeKind.EnumSByte, not null) => (
                    _,
                    _,
                    _
                ) =>
                    """
                        ReadOnlySpan<TEnum> values = MemoryMarshal.Cast<byte, TEnum>(source);
                        var list = new List<TEnum>(values.Length);
                        list.AddRange(values);
                        bytesRead = list.Count;
                        return list;
                        """,
                (WellKnownCollectionKind.List, _, not null) when typeKind.IsEnum() => (_, _, isLittleEndian) =>
                {
                    var integerType = GetWellKnownEnumIntegerDisplayName(typeKind);
                    return $$"""
                        ReadOnlySpan<TEnum> span = MemoryMarshal.Cast<byte, TEnum>(source);
                        var list = new List<TEnum>(span.Length);
                        list.AddRange(span);
                        if ({{CheckForReverseEndianness(isLittleEndian)}})
                        {
                            var reinterpretedList = MemoryMarshal.Cast<TEnum, {{integerType}}>(CollectionsMarshal.AsSpan(list));
                            BinaryPrimitives.ReverseEndianness(reinterpretedList, reinterpretedList);
                        }
                        bytesRead = list.Count * {{byteLength}};
                        return list;
                        """;
                },
                (WellKnownCollectionKind.List, _, not null) => (_, typeName, isLittleEndian) =>
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
        var elementsLengthParameter =
            collectionKind is not WellKnownCollectionKind.None && typeKind is WellKnownTypeKind.BinaryObject
                ? ", int elementLength"
                : string.Empty;
        var optionalReadBytesParameter = collectionKind is not WellKnownCollectionKind.None
            ? ", out int bytesRead"
            : string.Empty;
        writer.WriteLine(
            $"/// <summary> Reads a <c>{HttpUtility.HtmlEncode(collectionName)}</c> from the given source{endiannessName} </summary>"
        );
        writer.WriteLine("[MethodImpl(MethodImplOptions.AggressiveInlining)]");
        writer.WriteLine(
            $"public static {collectionName} {methodName}{typeParameter}(ReadOnlySpan<byte> source{elementsLengthParameter}{optionalReadBytesParameter})"
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

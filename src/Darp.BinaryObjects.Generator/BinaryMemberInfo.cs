namespace Darp.BinaryObjects.Generator;

using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;

internal interface IMember
{
    public ISymbol MemberSymbol { get; }
    public ITypeSymbol TypeSymbol { get; }
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
        string methodNameEndianness,
        int currentByteIndex,
        [NotNullWhen(true)] out string? writeString,
        out int bytesWritten
    );

    public bool TryGetReadString(
        string methodNameEndianness,
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
    public required ISymbol MemberSymbol { get; init; }
    public required ITypeSymbol TypeSymbol { get; init; }
    public required int TypeByteLength { get; init; }

    public int ConstantByteLength => TypeByteLength;

    public string GetDocCommentLength() => $"{TypeByteLength}";

    public bool TryGetWriteString(
        string methodNameEndianness,
        int currentByteIndex,
        [NotNullWhen(true)] out string? writeString,
        out int bytesWritten
    )
    {
        var methodName = TypeSymbol.ToDisplayString() switch
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
        {
            writeString = null;
            bytesWritten = default;
            return false;
        }
        writeString =
            $"global::Darp.BinaryObjects.Generated.Utilities.{methodName}(destination[{currentByteIndex}..], this.{MemberSymbol.Name});";
        bytesWritten = ConstantByteLength;
        return true;
    }

    public bool TryGetReadString(
        string methodNameEndianness,
        int currentByteIndex,
        [NotNullWhen(true)] out string? writeString,
        out int bytesRead
    )
    {
        var variableName = $"{BinaryObjectsGenerator.Prefix}read{MemberSymbol.Name}";
        var methodName = TypeSymbol.ToDisplayString() switch
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
        {
            writeString = null;
            bytesRead = default;
            return false;
        }
        writeString =
            $"var {variableName} = global::Darp.BinaryObjects.Generated.Utilities.{methodName}(source[{currentByteIndex}..]);";
        bytesRead = ConstantByteLength;
        return true;
    }
}

internal sealed class ConstantArrayMember : IConstantMember
{
    public required ISymbol MemberSymbol { get; init; }
    public required ITypeSymbol TypeSymbol { get; init; }
    public required int TypeByteLength { get; init; }

    public required ArrayKind ArrayKind { get; init; }
    public required int ArrayLength { get; init; }
    public required ITypeSymbol ArrayTypeSymbol { get; init; }

    public int ConstantByteLength => TypeByteLength * ArrayLength;

    public string GetDocCommentLength() => $"{TypeByteLength} * {ArrayLength}";

    public bool TryGetWriteString(
        string methodNameEndianness,
        int currentByteIndex,
        [NotNullWhen(true)] out string? writeString,
        out int bytesWritten
    )
    {
        (string MethodName, Func<string, string>? Func)? match = (ArrayKind, TypeSymbol.ToDisplayString()) switch
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
        string methodNameEndianness,
        int currentByteIndex,
        [NotNullWhen(true)] out string? writeString,
        out int bytesRead
    )
    {
        var variableName = $"{BinaryObjectsGenerator.Prefix}read{MemberSymbol.Name}";
        var methodName = (ArrayKind, TypeSymbol.ToString()) switch
        {
            (ArrayKind.Array or ArrayKind.Memory or ArrayKind.Enumerable, "byte") => "ReadUInt8Array",
            (ArrayKind.Array or ArrayKind.Memory or ArrayKind.Enumerable, "ushort") =>
                $"ReadUInt16Array{methodNameEndianness}",
            (ArrayKind.List, "byte") => "ReadUInt8List",
            (ArrayKind.List, "ushort") => $"ReadUInt16List{methodNameEndianness}",
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
    public int TypeByteLength { get; }
    public string GetVariableByteLength();
    public string GetVariableDocCommentLength();
}

internal sealed class VariableArrayMemberGroup : IVariableMemberGroup
{
    public required ISymbol MemberSymbol { get; init; }
    public required ITypeSymbol TypeSymbol { get; init; }
    public required int TypeByteLength { get; init; }

    public required ArrayKind ArrayKind { get; init; }
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

namespace Darp.BinaryObjects.Generator;

using System.CodeDom.Compiler;
using Microsoft.CodeAnalysis;

internal static class EnumCollectionCode
{
    public static bool IsEnum(WellKnownTypeKind typeKind) =>
        typeKind is WellKnownTypeKind.EnumByte
            or WellKnownTypeKind.EnumSByte
            or WellKnownTypeKind.EnumUShort
            or WellKnownTypeKind.EnumShort
            or WellKnownTypeKind.EnumUInt
            or WellKnownTypeKind.EnumInt
            or WellKnownTypeKind.EnumULong
            or WellKnownTypeKind.EnumLong;

    public static string WriteFixedEnumerable(
        WellKnownTypeKind typeKind,
        ITypeSymbol typeSymbol,
        string memberName,
        int destinationOffset,
        string elementCount,
        int elementSize,
        bool littleEndian,
        string identifier
    )
    {
        var enumType = typeSymbol.ToDisplayString();
        var destination = Slice("destination", destinationOffset, elementCount, elementSize);
        var methodName = $"Try{BinaryObjectsGenerator.GetWriteMethodName(WellKnownCollectionKind.Enumerable, typeKind, littleEndian)}";
        var written = $"___enumBytesWritten{identifier}";
        return $$"""
            if (!global::Darp.BinaryObjects.Generated.Utilities.{{methodName}}<{{enumType}}>({{destination}}, {{memberName}}, out var {{written}}))
            {
                bytesWritten += {{written}};
                return false;
            }
            """;
    }

    public static string WriteRemainingEnumerable(
        WellKnownTypeKind typeKind,
        ITypeSymbol typeSymbol,
        string memberName,
        int destinationOffset,
        int minimumCount,
        bool littleEndian,
        string identifier
    )
    {
        var enumType = typeSymbol.ToDisplayString();
        var methodName = $"Try{BinaryObjectsGenerator.GetWriteMethodName(WellKnownCollectionKind.Enumerable, typeKind, littleEndian).Replace("Enumerable", "RemainingEnumerable")}";
        var written = $"___enumBytesWritten{identifier}";
        return $$"""
            if (!global::Darp.BinaryObjects.Generated.Utilities.{{methodName}}<{{enumType}}>(destination[{{destinationOffset}}..], {{memberName}}, {{minimumCount}}, out var {{written}}))
            {
                bytesWritten += {{written}};
                return false;
            }
            bytesWritten += {{written}};
            """;
    }

    public static void EmitWriteEnumerableUtilities(
        IndentedTextWriter writer,
        WellKnownTypeKind typeKind,
        bool emitLittleAndBigEndian
    )
    {
        EmitWriteEnumerable(writer, typeKind, true);
        if (emitLittleAndBigEndian)
            EmitWriteEnumerable(writer, typeKind, false);
    }

    private static void EmitWriteEnumerable(IndentedTextWriter writer, WellKnownTypeKind typeKind, bool littleEndian)
    {
        var methodName = $"Try{BinaryObjectsGenerator.GetWriteMethodName(WellKnownCollectionKind.Enumerable, typeKind, littleEndian)}";
        var remainingMethodName = methodName.Replace("Enumerable", "RemainingEnumerable");
        var elementSize = typeKind.GetLength();
        var write = elementSize is 1
            ? "destination[bytesWritten] = Unsafe.As<TEnum, byte>(ref current);"
            : $"BinaryPrimitives.{BinaryObjectsGenerator.GetWriteMethodName(WellKnownCollectionKind.None, typeKind, littleEndian)}(destination[bytesWritten..], Unsafe.As<TEnum, {GetIntegerType(typeKind)}>(ref current));";
        writer.WriteMultiLine($$"""
            public static bool {{methodName}}<TEnum>(Span<byte> destination, IEnumerable<TEnum> value, out int bytesWritten)
                where TEnum : unmanaged, Enum
            {
                bytesWritten = 0;
                using var enumerator = value.GetEnumerator();
                for (var index = 0; index < destination.Length / {{elementSize}}; index++)
                {
                    if (!enumerator.MoveNext())
                        return false;
                    var current = enumerator.Current;
                    {{write}}
                    bytesWritten += {{elementSize}};
                }
                return true;
            }
            public static bool {{remainingMethodName}}<TEnum>(Span<byte> destination, IEnumerable<TEnum> value, int minimumCount, out int bytesWritten)
                where TEnum : unmanaged, Enum
            {
                bytesWritten = 0;
                if (minimumCount > destination.Length / {{elementSize}})
                    return false;
                foreach (var item in value)
                {
                    if (bytesWritten / {{elementSize}} >= destination.Length / {{elementSize}})
                        return false;
                    var current = item;
                    {{write}}
                    bytesWritten += {{elementSize}};
                }
                return bytesWritten / {{elementSize}} >= minimumCount;
            }
            """);
    }

    private static string Slice(string buffer, int offset, string elementCount, int elementSize)
    {
        if (int.TryParse(elementCount, out var count))
            return $"{buffer}[{offset}..{offset + count * elementSize}]";
        return $"{buffer}.Slice({offset}, {elementCount} * {elementSize})";
    }

    private static string GetIntegerType(WellKnownTypeKind typeKind) =>
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
}

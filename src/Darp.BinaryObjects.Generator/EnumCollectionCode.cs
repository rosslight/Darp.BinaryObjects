namespace Darp.BinaryObjects.Generator;

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

    public static string Write(
        WellKnownCollectionKind collectionKind,
        WellKnownTypeKind typeKind,
        string memberName,
        int destinationOffset,
        string elementCount,
        int elementSize,
        bool littleEndian,
        string identifier
    )
    {
        var index = $"___enumIndex{identifier}";
        if (collectionKind is WellKnownCollectionKind.Enumerable)
        {
            var enumerator = $"___enumEnumerator{identifier}";
            var write = WriteElement(typeKind, $"{enumerator}.Current", index, destinationOffset, elementSize, littleEndian);
            return $$"""
                using var {{enumerator}} = {{memberName}}.GetEnumerator();
                for (var {{index}} = 0; {{index}} < {{elementCount}}; {{index}}++)
                {
                    if (!{{enumerator}}.MoveNext())
                    {
                        bytesWritten += {{index}} * {{elementSize}};
                        return false;
                    }
                    {{write}}
                }
                """;
        }

        var collection = collectionKind is WellKnownCollectionKind.Memory ? $"{memberName}.Span" : memberName;
        var length = collectionKind is WellKnownCollectionKind.List ? "Count" : "Length";
        var writeCollectionElement = WriteElement(typeKind, $"{collection}[{index}]", index, destinationOffset, elementSize, littleEndian);
        return $$"""
            if ({{collection}}.{{length}} < {{elementCount}})
                return false;
            for (var {{index}} = 0; {{index}} < {{elementCount}}; {{index}}++)
            {
                {{writeCollectionElement}}
            }
            """;
    }

    public static string Read(
        WellKnownCollectionKind collectionKind,
        WellKnownTypeKind typeKind,
        ITypeSymbol typeSymbol,
        int sourceOffset,
        string elementCount,
        int elementSize,
        bool littleEndian,
        string variableName,
        string? bytesReadName,
        string identifier
    )
    {
        var enumType = typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        var index = $"___enumIndex{identifier}";
        var count = $"___enumCount{identifier}";
        var read = ReadElement(typeKind, enumType, index, sourceOffset, elementSize, littleEndian);
        var result = collectionKind is WellKnownCollectionKind.List
            ? $"var {variableName} = new global::System.Collections.Generic.List<{enumType}>({count});"
            : $"var {variableName} = new {enumType}[{count}];";
        var assignment = collectionKind is WellKnownCollectionKind.List
            ? $"{variableName}.Add({read});"
            : $"{variableName}[{index}] = {read};";
        var code = $$"""
            var {{count}} = {{elementCount}};
            {{result}}
            for (var {{index}} = 0; {{index}} < {{count}}; {{index}}++)
            {
                {{assignment}}
            }
            """;
        return bytesReadName is null ? code : $"{code}\nvar {bytesReadName} = {count} * {elementSize};";
    }

    public static string WriteRemainingEnumerable(
        WellKnownTypeKind typeKind,
        string memberName,
        int destinationOffset,
        int elementSize,
        int minimumCount,
        bool littleEndian,
        string identifier
    )
    {
        var index = $"___enumIndex{identifier}";
        var enumerator = $"___enumEnumerator{identifier}";
        var write = WriteElement(typeKind, $"{enumerator}.Current", index, destinationOffset, elementSize, littleEndian);
        var minimumCheck = minimumCount > 0
            ? $"if ({index} < {minimumCount})\n{{\n    bytesWritten += {index} * {elementSize};\n    return false;\n}}"
            : string.Empty;
        var code = $$"""
            using var {{enumerator}} = {{memberName}}.GetEnumerator();
            var {{index}} = 0;
            while ({{enumerator}}.MoveNext())
            {
                if ({{index}} >= destination.Length / {{elementSize}})
                {
                    bytesWritten += {{index}} * {{elementSize}};
                    return false;
                }
                {{write}}
                {{index}}++;
            }
            {{minimumCheck}}
            bytesWritten += {{index}} * {{elementSize}};
            """;
        return minimumCount is 0
            ? code
            : $"if (destination.Length < {minimumCount * elementSize})\n    return false;\n{code}";
    }

    private static string WriteElement(
        WellKnownTypeKind typeKind,
        string value,
        string index,
        int destinationOffset,
        int elementSize,
        bool littleEndian
    )
    {
        var offset = destinationOffset is 0 ? $"{index} * {elementSize}" : $"{destinationOffset} + {index} * {elementSize}";
        var byteOffset = destinationOffset is 0 ? index : $"{destinationOffset} + {index}";
        return typeKind switch
        {
            WellKnownTypeKind.EnumByte => $"destination[{byteOffset}] = (byte){value};",
            WellKnownTypeKind.EnumSByte => $"destination[{byteOffset}] = unchecked((byte)(sbyte){value});",
            _ => $"global::System.Buffers.Binary.BinaryPrimitives.{BinaryObjectsGenerator.GetWriteMethodName(WellKnownCollectionKind.None, typeKind, littleEndian)}(destination.Slice({offset}, {elementSize}), ({GetIntegerType(typeKind)}){value});",
        };
    }

    private static string ReadElement(
        WellKnownTypeKind typeKind,
        string enumType,
        string index,
        int sourceOffset,
        int elementSize,
        bool littleEndian
    )
    {
        var offset = sourceOffset is 0 ? $"{index} * {elementSize}" : $"{sourceOffset} + {index} * {elementSize}";
        var byteOffset = sourceOffset is 0 ? index : $"{sourceOffset} + {index}";
        return typeKind switch
        {
            WellKnownTypeKind.EnumByte => $"({enumType})source[{byteOffset}]",
            WellKnownTypeKind.EnumSByte => $"({enumType})unchecked((sbyte)source[{byteOffset}])",
            _ => $"({enumType})global::System.Buffers.Binary.BinaryPrimitives.{BinaryObjectsGenerator.GetReadMethodName(WellKnownCollectionKind.None, typeKind, littleEndian)}(source.Slice({offset}, {elementSize}))",
        };
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

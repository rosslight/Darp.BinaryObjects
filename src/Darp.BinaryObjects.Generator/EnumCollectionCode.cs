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

    public static string Write(
        WellKnownCollectionKind collectionKind,
        WellKnownTypeKind typeKind,
        ITypeSymbol typeSymbol,
        string memberName,
        int destinationOffset,
        string elementCount,
        int elementSize,
        bool littleEndian,
        string identifier,
        bool checkLength = true
    )
    {
        var enumType = typeSymbol.ToDisplayString();
        var destination = Slice("destination", destinationOffset, elementCount, elementSize);
        if (collectionKind is WellKnownCollectionKind.Enumerable)
        {
            var methodName = $"Try{BinaryObjectsGenerator.GetWriteMethodName(collectionKind, typeKind, littleEndian)}";
            var written = $"___enumBytesWritten{identifier}";
            return $$"""
                if (!global::Darp.BinaryObjects.Generated.Utilities.{{methodName}}<{{enumType}}>({{destination}}, {{memberName}}, out var {{written}}))
                {
                    bytesWritten += {{written}};
                    return false;
                }
                """;
        }

        var collection = collectionKind is WellKnownCollectionKind.Memory ? $"{memberName}.Span" : memberName;
        var length = collectionKind is WellKnownCollectionKind.List ? "Count" : "Length";
        var writeMethod = BinaryObjectsGenerator.GetWriteMethodName(collectionKind, typeKind, littleEndian);
        var call = $"global::Darp.BinaryObjects.Generated.Utilities.{writeMethod}<{enumType}>({destination}, {collection});";
        return checkLength ? $"if ({collection}.{length} < {elementCount})\n    return false;\n{call}" : call;
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
        string? bytesReadName
    )
    {
        var enumType = typeSymbol.ToDisplayString();
        var source = Slice("source", sourceOffset, elementCount, elementSize);
        var methodName = BinaryObjectsGenerator.GetReadMethodName(
            collectionKind is WellKnownCollectionKind.Span ? WellKnownCollectionKind.Array : collectionKind,
            typeKind,
            littleEndian
        );
        var code = $"var {variableName} = global::Darp.BinaryObjects.Generated.Utilities.{methodName}<{enumType}>({source}, out _);";
        return bytesReadName is null ? code : $"{code}\nvar {bytesReadName} = {elementCount} * {elementSize};";
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

    public static void EmitUtility(
        IndentedTextWriter writer,
        bool isRead,
        WellKnownCollectionKind collectionKind,
        WellKnownTypeKind typeKind,
        bool emitLittleAndBigEndian
    )
    {
        EmitOneUtility(writer, isRead, collectionKind, typeKind, true);
        if (emitLittleAndBigEndian)
            EmitOneUtility(writer, isRead, collectionKind, typeKind, false);
    }

    private static void EmitOneUtility(
        IndentedTextWriter writer,
        bool isRead,
        WellKnownCollectionKind collectionKind,
        WellKnownTypeKind typeKind,
        bool littleEndian
    )
    {
        if (isRead)
        {
            if (collectionKind is WellKnownCollectionKind.List)
                EmitReadList(writer, typeKind, littleEndian);
            else
                EmitReadArray(writer, typeKind, littleEndian);
            return;
        }

        switch (collectionKind)
        {
            case WellKnownCollectionKind.Span:
                EmitWriteSpan(writer, typeKind, littleEndian);
                break;
            case WellKnownCollectionKind.List:
                EmitWriteList(writer, typeKind, littleEndian);
                break;
            case WellKnownCollectionKind.Enumerable:
                EmitWriteEnumerable(writer, typeKind, littleEndian);
                break;
        }
    }

    private static void EmitWriteSpan(IndentedTextWriter writer, WellKnownTypeKind typeKind, bool littleEndian)
    {
        var methodName = BinaryObjectsGenerator.GetWriteMethodName(WellKnownCollectionKind.Span, typeKind, littleEndian);
        var elementSize = typeKind.GetLength();
        if (elementSize is 1)
        {
            writer.WriteMultiLine($$"""
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public static int {{methodName}}<TEnum>(Span<byte> destination, ReadOnlySpan<TEnum> value)
                    where TEnum : unmanaged, Enum
                {
                    var length = Math.Min(value.Length, destination.Length);
                    MemoryMarshal.Cast<TEnum, byte>(value[..length]).CopyTo(destination);
                    return length;
                }
                """);
            return;
        }

        var integerType = GetIntegerType(typeKind);
        var reverse = littleEndian ? "!BitConverter.IsLittleEndian" : "BitConverter.IsLittleEndian";
        writer.WriteMultiLine($$"""
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static int {{methodName}}<TEnum>(Span<byte> destination, ReadOnlySpan<TEnum> value)
                where TEnum : unmanaged, Enum
            {
                var length = Math.Min(value.Length, destination.Length / {{elementSize}});
                if ({{reverse}})
                {
                    ReadOnlySpan<{{integerType}}> integers = MemoryMarshal.Cast<TEnum, {{integerType}}>(value[..length]);
                    Span<{{integerType}}> output = MemoryMarshal.Cast<byte, {{integerType}}>(destination);
                    BinaryPrimitives.ReverseEndianness(integers, output);
                }
                else
                    MemoryMarshal.Cast<TEnum, byte>(value[..length]).CopyTo(destination);
                return length * {{elementSize}};
            }
            """);
    }

    private static void EmitWriteList(IndentedTextWriter writer, WellKnownTypeKind typeKind, bool littleEndian)
    {
        var methodName = BinaryObjectsGenerator.GetWriteMethodName(WellKnownCollectionKind.List, typeKind, littleEndian);
        var spanMethod = BinaryObjectsGenerator.GetWriteMethodName(WellKnownCollectionKind.Span, typeKind, littleEndian);
        writer.WriteMultiLine($$"""
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static int {{methodName}}<TEnum>(Span<byte> destination, List<TEnum> value)
                where TEnum : unmanaged, Enum => {{spanMethod}}<TEnum>(destination, CollectionsMarshal.AsSpan(value));
            """);
    }

    private static void EmitReadArray(IndentedTextWriter writer, WellKnownTypeKind typeKind, bool littleEndian)
    {
        var methodName = BinaryObjectsGenerator.GetReadMethodName(WellKnownCollectionKind.Array, typeKind, littleEndian);
        var elementSize = typeKind.GetLength();
        writer.WriteLine("[MethodImpl(MethodImplOptions.AggressiveInlining)]");
        writer.WriteLine($"public static TEnum[] {methodName}<TEnum>(ReadOnlySpan<byte> source, out int bytesRead)");
        writer.WriteLine("    where TEnum : unmanaged, Enum");
        writer.WriteLine("{");
        writer.Indent++;
        writer.WriteLine("var array = MemoryMarshal.Cast<byte, TEnum>(source).ToArray();");
        if (elementSize > 1)
        {
            writer.WriteLine($"if ({(littleEndian ? "!BitConverter.IsLittleEndian" : "BitConverter.IsLittleEndian")})");
            writer.WriteLine("{");
            writer.Indent++;
            var integerType = GetIntegerType(typeKind);
            writer.WriteLine($"Span<{integerType}> integers = MemoryMarshal.Cast<TEnum, {integerType}>(array.AsSpan());");
            writer.WriteLine("BinaryPrimitives.ReverseEndianness(integers, integers);");
            writer.Indent--;
            writer.WriteLine("}");
        }
        writer.WriteLine($"bytesRead = array.Length * {elementSize};");
        writer.WriteLine("return array;");
        writer.Indent--;
        writer.WriteLine("}");
    }

    private static void EmitReadList(IndentedTextWriter writer, WellKnownTypeKind typeKind, bool littleEndian)
    {
        var methodName = BinaryObjectsGenerator.GetReadMethodName(WellKnownCollectionKind.List, typeKind, littleEndian);
        var elementSize = typeKind.GetLength();
        writer.WriteLine("[MethodImpl(MethodImplOptions.AggressiveInlining)]");
        writer.WriteLine($"public static List<TEnum> {methodName}<TEnum>(ReadOnlySpan<byte> source, out int bytesRead)");
        writer.WriteLine("    where TEnum : unmanaged, Enum");
        writer.WriteLine("{");
        writer.Indent++;
        writer.WriteLine("ReadOnlySpan<TEnum> values = MemoryMarshal.Cast<byte, TEnum>(source);");
        writer.WriteLine("var list = new List<TEnum>(values.Length);");
        writer.WriteLine("list.AddRange(values);");
        if (elementSize > 1)
        {
            writer.WriteLine($"if ({(littleEndian ? "!BitConverter.IsLittleEndian" : "BitConverter.IsLittleEndian")})");
            writer.WriteLine("{");
            writer.Indent++;
            var integerType = GetIntegerType(typeKind);
            writer.WriteLine($"Span<{integerType}> integers = MemoryMarshal.Cast<TEnum, {integerType}>(CollectionsMarshal.AsSpan(list));");
            writer.WriteLine("BinaryPrimitives.ReverseEndianness(integers, integers);");
            writer.Indent--;
            writer.WriteLine("}");
        }
        writer.WriteLine($"bytesRead = list.Count * {elementSize};");
        writer.WriteLine("return list;");
        writer.Indent--;
        writer.WriteLine("}");
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

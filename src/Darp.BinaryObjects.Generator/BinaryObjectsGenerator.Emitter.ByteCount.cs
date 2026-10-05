namespace Darp.BinaryObjects.Generator;

using System.CodeDom.Compiler;
using System.Web;

partial class BinaryObjectsGenerator
{
    private static string GetSignednessName(WellKnownTypeKind typeKind) => typeKind.IsSigned() ? "Signed" : "Unsigned";

    private static string GetSignednessDescription(WellKnownTypeKind typeKind) =>
        typeKind.IsSigned() ? "a signed" : "an unsigned";

    internal static string GetFitsMethodName(WellKnownTypeKind typeKind) => $"Fits{GetSignednessName(typeKind)}";

    /// <summary> Gets the utilities for values which are serialized with fewer bytes than their type has </summary>
    private static UtilityData[] GetNarrowUtilities(
        bool isRead,
        WellKnownCollectionKind collectionKind,
        WellKnownTypeKind typeKind
    )
    {
        // The scalar helpers work on 64 bit values and are shared by all narrow members of the same signedness
        var scalarUtility = new UtilityData(
            isRead,
            WellKnownCollectionKind.None,
            typeKind.IsSigned() ? WellKnownTypeKind.Long : WellKnownTypeKind.ULong,
            UtilityData.UnknownLength,
            EmitLittleAndBigEndian: true,
            IsNarrow: true
        );
        UtilityData CollectionUtility(WellKnownCollectionKind kind) =>
            new(isRead, kind, typeKind, UtilityData.UnknownLength, EmitLittleAndBigEndian: true, IsNarrow: true);
        return (isRead, collectionKind) switch
        {
            (_, WellKnownCollectionKind.None) => [scalarUtility],
            (true, WellKnownCollectionKind.List) => [scalarUtility, CollectionUtility(WellKnownCollectionKind.List)],
            (true, _) => [scalarUtility, CollectionUtility(WellKnownCollectionKind.Array)],
            (false, WellKnownCollectionKind.Collection) =>
            [
                scalarUtility,
                CollectionUtility(WellKnownCollectionKind.Span),
                CollectionUtility(WellKnownCollectionKind.Collection),
            ],
            (false, _) => [scalarUtility, CollectionUtility(WellKnownCollectionKind.Span)],
        };
    }

    private static void EmitNarrowUtility(
        IndentedTextWriter writer,
        bool isRead,
        WellKnownCollectionKind collectionKind,
        WellKnownTypeKind typeKind
    )
    {
        if (collectionKind is WellKnownCollectionKind.None)
        {
            if (!isRead)
                EmitNarrowFitsUtility(writer, typeKind);
            foreach (var isLittleEndian in new[] { true, false })
            {
                if (isRead)
                    EmitNarrowReadScalarUtility(writer, typeKind, isLittleEndian);
                else
                    EmitNarrowWriteScalarUtility(writer, typeKind, isLittleEndian);
            }
            return;
        }
        foreach (var isLittleEndian in new[] { true, false })
        {
            if (isRead)
                EmitNarrowReadCollectionUtility(writer, collectionKind, typeKind, isLittleEndian);
            else
                EmitNarrowWriteCollectionUtility(writer, collectionKind, typeKind, isLittleEndian);
        }
    }

    private static void EmitNarrowFitsUtility(IndentedTextWriter writer, WellKnownTypeKind typeKind)
    {
        var typeName = GetWellKnownDisplayName(WellKnownCollectionKind.None, typeKind);
        var body = typeKind.IsSigned()
            ? """
                var shift = 64 - 8 * byteCount;
                return ((value << shift) >> shift) == value;
                """
            : "return (value >> (8 * byteCount)) == 0;";
        writer.WriteMultiLine(
            $$"""
            /// <summary> Checks whether {{GetSignednessDescription(
                typeKind
            )}} integer can be written with <c>byteCount</c> bytes </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool {{GetFitsMethodName(typeKind)}}({{typeName}} value, int byteCount)
            {
            """
        );
        writer.Indent++;
        writer.WriteMultiLine(body);
        writer.Indent--;
        writer.WriteLine("}");
    }

    private static void EmitNarrowReadScalarUtility(
        IndentedTextWriter writer,
        WellKnownTypeKind typeKind,
        bool isLittleEndian
    )
    {
        var typeName = GetWellKnownDisplayName(WellKnownCollectionKind.None, typeKind);
        var endianness = GetEndiannessName(typeKind, isLittleEndian);
        var methodName = GetReadMethodName(WellKnownCollectionKind.None, typeKind, isLittleEndian, isNarrow: true);
        var mostSignificantByte = isLittleEndian ? "source[^1]" : "source[0]";
        // The most significant byte carries the sign into the 64 bit value
        if (typeKind.IsSigned())
            mostSignificantByte = $"unchecked((sbyte){mostSignificantByte})";
        var remainingBytes = isLittleEndian
            ? "for (var i = source.Length - 2; i >= 0; i--)"
            : "for (var i = 1; i < source.Length; i++)";
        writer.WriteMultiLine(
            $$"""
            /// <summary> Reads {{GetSignednessDescription(
                typeKind
            )}} integer of <c>source.Length</c> bytes from the given source, as {{endianness}} </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static {{typeName}} {{methodName}}(ReadOnlySpan<byte> source)
            {
                {{typeName}} value = {{mostSignificantByte}};
                {{remainingBytes}}
                    value = (value << 8) | source[i];
                return value;
            }
            """
        );
    }

    private static void EmitNarrowWriteScalarUtility(
        IndentedTextWriter writer,
        WellKnownTypeKind typeKind,
        bool isLittleEndian
    )
    {
        var typeName = GetWellKnownDisplayName(WellKnownCollectionKind.None, typeKind);
        var endianness = GetEndiannessName(typeKind, isLittleEndian);
        var methodName = GetWriteMethodName(WellKnownCollectionKind.None, typeKind, isLittleEndian, isNarrow: true);
        var leastSignificantByteFirst = isLittleEndian
            ? "for (var i = 0; i < destination.Length; i++)"
            : "for (var i = destination.Length - 1; i >= 0; i--)";
        writer.WriteMultiLine(
            $$"""
            /// <summary> Writes the lowest <c>destination.Length</c> bytes of {{GetSignednessDescription(
                typeKind
            )}} integer to the destination, as {{endianness}} </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void {{methodName}}(Span<byte> destination, {{typeName}} value)
            {
                {{leastSignificantByteFirst}}
                {
                    destination[i] = unchecked((byte)value);
                    value >>= 8;
                }
            }
            """
        );
    }

    private static void EmitNarrowWriteCollectionUtility(
        IndentedTextWriter writer,
        WellKnownCollectionKind collectionKind,
        WellKnownTypeKind typeKind,
        bool isLittleEndian
    )
    {
        var collectionName = GetWellKnownDisplayName(collectionKind, typeKind);
        var encodedCollectionName = HttpUtility.HtmlEncode(collectionName);
        var endianness = GetEndiannessName(typeKind, isLittleEndian);
        var methodName = GetWriteMethodName(collectionKind, typeKind, isLittleEndian, isNarrow: true);
        var typeParameter = GetTypeParameter(typeKind);
        writer.WriteMultiLine(
            $"""
            /// <summary> Writes the elements of a <c>{encodedCollectionName}</c> with <c>elementLength</c> bytes each to the destination, as {endianness}. Fails at the first element which does not fit </summary>
            public static bool {methodName}{typeParameter}(Span<byte> destination, {collectionName} value, int elementLength, out int bytesWritten)
            """
        );
        var typeParameterConstraint = GetWriteTypeParameterConstraint(typeKind);
        if (!string.IsNullOrEmpty(typeParameterConstraint))
            writer.WriteLine(typeParameterConstraint);
        if (collectionKind is WellKnownCollectionKind.Collection)
        {
            var writeElement = GetWriteMethodName(WellKnownCollectionKind.Span, typeKind, isLittleEndian, true);
            writer.WriteMultiLine(
                $$"""
                {
                    bytesWritten = 0;
                    using var enumerator = value.GetEnumerator();
                    var count = destination.Length / elementLength;
                    for (var index = 0; index < count && enumerator.MoveNext(); index++)
                    {
                        var item = enumerator.Current;
                        if (!{{writeElement}}{{typeParameter}}(destination.Slice(bytesWritten, elementLength), MemoryMarshal.CreateReadOnlySpan(ref item, 1), elementLength, out _))
                            return false;
                        bytesWritten += elementLength;
                    }
                    return true;
                }
                """
            );
            return;
        }

        var integers = "value";
        writer.WriteLine("{");
        writer.Indent++;
        if (typeKind.IsEnum())
        {
            integers = "integers";
            var integerType = GetWellKnownEnumIntegerDisplayName(typeKind);
            writer.WriteLine(
                $"ReadOnlySpan<{integerType}> integers = MemoryMarshal.Cast<TEnum, {integerType}>(value);"
            );
        }
        var writeScalar = GetWriteMethodName(WellKnownCollectionKind.None, typeKind, isLittleEndian, true);
        writer.WriteMultiLine(
            $$"""
            bytesWritten = 0;
            var numberOfElements = Math.Min(value.Length, destination.Length / elementLength);
            for (var i = 0; i < numberOfElements; i++)
            {
                if (!{{GetFitsMethodName(typeKind)}}({{integers}}[i], elementLength))
                    return false;
                {{writeScalar}}(destination.Slice(bytesWritten, elementLength), {{integers}}[i]);
                bytesWritten += elementLength;
            }
            return true;
            """
        );
        writer.Indent--;
        writer.WriteLine("}");
    }

    private static void EmitNarrowReadCollectionUtility(
        IndentedTextWriter writer,
        WellKnownCollectionKind collectionKind,
        WellKnownTypeKind typeKind,
        bool isLittleEndian
    )
    {
        var collectionName = GetWellKnownDisplayName(collectionKind, typeKind);
        var encodedCollectionName = HttpUtility.HtmlEncode(collectionName);
        var typeName = GetWellKnownDisplayName(WellKnownCollectionKind.None, typeKind);
        var typeParameter = GetTypeParameter(typeKind);
        var integerType = typeKind.IsEnum() ? GetWellKnownEnumIntegerDisplayName(typeKind) : typeName;
        var endianness = GetEndiannessName(typeKind, isLittleEndian);
        var methodName = GetReadMethodName(collectionKind, typeKind, isLittleEndian, isNarrow: true);
        var readScalar = GetReadMethodName(WellKnownCollectionKind.None, typeKind, isLittleEndian, true);
        // The shared helpers read 64 bit values
        var optionalCast = integerType is "long" or "ulong" ? string.Empty : $"({integerType})";
        var isList = collectionKind is WellKnownCollectionKind.List;
        var createResult = isList
            ? $"""
                var result = new List<{typeName}>();
                CollectionsMarshal.SetCount(result, source.Length / elementLength);
                """
            : $"var result = new {typeName}[source.Length / elementLength];";
        var resultSpan = isList ? "CollectionsMarshal.AsSpan(result)" : "result.AsSpan()";
        if (typeKind.IsEnum())
            resultSpan = $"MemoryMarshal.Cast<TEnum, {integerType}>({resultSpan})";
        writer.WriteMultiLine(
            $"""
            /// <summary> Reads a <c>{encodedCollectionName}</c> with <c>elementLength</c> bytes per element from the given source, as {endianness} </summary>
            public static {collectionName} {methodName}{typeParameter}(ReadOnlySpan<byte> source, int elementLength, out int bytesRead)
            """
        );
        var typeParameterConstraint = GetReadTypeParameterConstraint(typeKind);
        if (!string.IsNullOrEmpty(typeParameterConstraint))
            writer.WriteLine(typeParameterConstraint);
        writer.WriteLine("{");
        writer.Indent++;
        writer.WriteMultiLine(createResult);
        writer.WriteMultiLine(
            $"""
            Span<{integerType}> integers = {resultSpan};
            for (var i = 0; i < integers.Length; i++)
                integers[i] = {optionalCast}{readScalar}(source.Slice(i * elementLength, elementLength));
            bytesRead = integers.Length * elementLength;
            return result;
            """
        );
        writer.Indent--;
        writer.WriteLine("}");
    }
}

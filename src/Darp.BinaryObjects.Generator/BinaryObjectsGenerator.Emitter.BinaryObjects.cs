namespace Darp.BinaryObjects.Generator;

using System.CodeDom.Compiler;

partial class BinaryObjectsGenerator
{
    private static void EmitBinaryObjectCollectionUtilities(
        IndentedTextWriter writer,
        WellKnownCollectionKind collectionKind,
        bool generateRead
    )
    {
        foreach (var littleEndian in new[] { true, false })
        {
            var endianness = littleEndian ? "LittleEndian" : "BigEndian";
            if (generateRead)
            {
                var isList = collectionKind is WellKnownCollectionKind.List;
                var collectionName = isList ? "List<T>" : "T[]";
                var methodName = GetReadMethodName(collectionKind, WellKnownTypeKind.BinaryObject, littleEndian);
                var createCollection = isList ? "new List<T>(numberOfElements)" : "new T[numberOfElements]";
                var assignElement = isList ? "result.Add(item);" : "result[i] = item;";
                writer.WriteMultiLine(
                    $$"""
                    public static bool {{methodName}}<T>(ReadOnlySpan<byte> source, int elementLength, [global::System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out {{collectionName}}? value, out int bytesRead)
                        where T : IBinaryReadable<T>
                    {
                        value = null;
                        bytesRead = 0;
                        var numberOfElements = source.Length / elementLength;
                        var result = {{createCollection}};
                        for (var i = 0; i < numberOfElements; i++)
                        {
                            if (!T.TryRead{{endianness}}(source.Slice(i * elementLength, elementLength), out var item, out var itemBytesRead))
                            {
                                bytesRead += itemBytesRead;
                                return false;
                            }
                            bytesRead += elementLength;
                            {{assignElement}}
                        }
                        value = result;
                        return true;
                    }
                    """
                );
                continue;
            }

            var writeMethodName = GetWriteMethodName(collectionKind, WellKnownTypeKind.BinaryObject, littleEndian);
            var spanMethodName = GetWriteMethodName(
                WellKnownCollectionKind.Span,
                WellKnownTypeKind.BinaryObject,
                littleEndian
            );
            var listMethodName = GetWriteMethodName(
                WellKnownCollectionKind.List,
                WellKnownTypeKind.BinaryObject,
                littleEndian
            );
            switch (collectionKind)
            {
                case WellKnownCollectionKind.Span:
                    writer.WriteMultiLine(
                        $$"""
                        public static bool {{writeMethodName}}<T>(Span<byte> destination, ReadOnlySpan<T> value, int elementLength, out int bytesWritten)
                            where T : IBinaryWritable
                        {
                            bytesWritten = 0;
                            var numberOfElements = Math.Min(value.Length, destination.Length / elementLength);
                            for (var i = 0; i < numberOfElements; i++)
                            {
                                if (!value[i].TryWrite{{endianness}}(destination.Slice(i * elementLength, elementLength), out var itemBytesWritten))
                                {
                                    bytesWritten += itemBytesWritten;
                                    return false;
                                }
                                bytesWritten += elementLength;
                            }
                            return true;
                        }
                        """
                    );
                    break;
                case WellKnownCollectionKind.List:
                    writer.WriteMultiLine(
                        $$"""
                        public static bool {{writeMethodName}}<T>(Span<byte> destination, List<T> value, int elementLength, out int bytesWritten)
                            where T : IBinaryWritable => {{spanMethodName}}<T>(destination, CollectionsMarshal.AsSpan(value), elementLength, out bytesWritten);
                        """
                    );
                    break;
                case WellKnownCollectionKind.Enumerable:
                    writer.WriteMultiLine(
                        $$"""
                        public static bool {{writeMethodName}}<T>(Span<byte> destination, IEnumerable<T> value, int elementLength, out int bytesWritten)
                            where T : IBinaryWritable
                        {
                            if (value is T[] array)
                                return {{spanMethodName}}<T>(destination, array, elementLength, out bytesWritten);
                            if (value is List<T> list)
                                return {{listMethodName}}<T>(destination, list, elementLength, out bytesWritten);
                            bytesWritten = 0;
                            foreach (var item in value)
                            {
                                if (destination.Length - bytesWritten < elementLength)
                                    break;
                                if (!item.TryWrite{{endianness}}(destination.Slice(bytesWritten, elementLength), out var itemBytesWritten))
                                {
                                    bytesWritten += itemBytesWritten;
                                    return false;
                                }
                                bytesWritten += elementLength;
                            }
                            return true;
                        }
                        """
                    );
                    break;
            }
        }
    }
}

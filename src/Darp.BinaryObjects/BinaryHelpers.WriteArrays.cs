namespace Darp.BinaryObjects;

using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CA1062 // Validate arguments of public methods
#pragma warning disable CA1002 // Do not expose generic lists

public static partial class BinaryHelpers
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteUInt8List(Span<byte> destination, List<byte> value, int maxElementLength)
    {
        WriteUInt8Span(destination, CollectionsMarshal.AsSpan(value), maxElementLength);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteUInt8Span(Span<byte> destination, ReadOnlySpan<byte> value, int maxElementLength)
    {
        var length = Math.Min(value.Length, maxElementLength);
        value.Slice(0, length).CopyTo(destination);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteUInt8Enumerable(Span<byte> destination, IEnumerable<byte> value, int maxElementLength)
    {
        switch (value)
        {
            case byte[] arrayValue:
                WriteUInt8Span(destination, arrayValue, maxElementLength);
                return;
            case List<byte> listValue:
                WriteUInt8List(destination, listValue, maxElementLength);
                return;
        }
        var index = 0;
        foreach (var b in value)
        {
            destination[index++] = b;
            if (index >= maxElementLength)
                return;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteUInt16ListLittleEndian(Span<byte> destination, List<ushort> value, int maxElementLength)
    {
        WriteUInt16SpanLittleEndian(destination, CollectionsMarshal.AsSpan(value), maxElementLength);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteUInt16SpanLittleEndian(
        Span<byte> destination,
        ReadOnlySpan<ushort> value,
        int maxElementLength
    )
    {
        var length = Math.Min(value.Length, maxElementLength);
        if (!BitConverter.IsLittleEndian)
        {
            Span<ushort> reinterpretedDestination = MemoryMarshal.Cast<byte, ushort>(destination);
            BinaryPrimitives.ReverseEndianness(value[..length], reinterpretedDestination);
            return;
        }
        MemoryMarshal.Cast<ushort, byte>(value[..length]).CopyTo(destination);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteUInt16ListBigEndian(Span<byte> destination, List<ushort> value, int maxElementLength)
    {
        WriteUInt16SpanBigEndian(destination, CollectionsMarshal.AsSpan(value), maxElementLength);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteUInt16SpanBigEndian(
        Span<byte> destination,
        ReadOnlySpan<ushort> value,
        int maxElementLength
    )
    {
        var length = Math.Min(value.Length, maxElementLength);
        if (BitConverter.IsLittleEndian)
        {
            Span<ushort> reinterpretedDestination = MemoryMarshal.Cast<byte, ushort>(destination);
            BinaryPrimitives.ReverseEndianness(value[..length], reinterpretedDestination);
            return;
        }
        MemoryMarshal.Cast<ushort, byte>(value[..length]).CopyTo(destination);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteUInt16EnumerableLittleEndian(
        Span<byte> destination,
        IEnumerable<ushort> value,
        int maxElementLength
    )
    {
        var index = 0;
        foreach (var val in value)
        {
            BinaryPrimitives.WriteUInt16LittleEndian(destination[(2 * index++)..], val);
            if (index >= maxElementLength)
                return;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteUInt16EnumerableBigEndian(
        Span<byte> destination,
        IEnumerable<ushort> value,
        int maxElementLength
    )
    {
        var index = 0;
        foreach (var val in value)
        {
            BinaryPrimitives.WriteUInt16BigEndian(destination[(2 * index++)..], val);
            if (index >= maxElementLength)
                return;
        }
    }

    public static void WriteConstantBinaryObjectSpanLittleEndian<T>(
        Span<byte> destination,
        ReadOnlySpan<T> value,
        int elementLength
    )
        where T : IBinaryWritable
    {
        for (var i = 0; i < elementLength; i++)
        {
            if (!value[i].TryWriteLittleEndian(destination.Slice(i * elementLength, elementLength)))
                throw new ArgumentException($"Could not write {typeof(T).Name} to destination");
        }
    }

    public static void WriteConstantBinaryObjectListLittleEndian<T>(
        Span<byte> destination,
        List<T> value,
        int elementLength
    )
        where T : IBinaryWritable
    {
        WriteConstantBinaryObjectSpanLittleEndian<T>(destination, CollectionsMarshal.AsSpan(value), elementLength);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteConstantBinaryObjectEnumerableLittleEndian<T>(
        Span<byte> destination,
        IEnumerable<T> value,
        int elementLength
    )
        where T : IBinaryWritable
    {
        switch (value)
        {
            case T[] arrayValue:
                WriteConstantBinaryObjectSpanLittleEndian<T>(destination, arrayValue, elementLength);
                return;
            case List<T> listValue:
                WriteConstantBinaryObjectListLittleEndian(destination, listValue, elementLength);
                return;
        }

        var i = 0;
        foreach (T writable in value)
        {
            if (!writable.TryWriteLittleEndian(destination[(i * elementLength)..]))
                throw new ArgumentException($"Could not write {typeof(T).Name} to destination");
            i++;
        }
    }

    /// <summary> Writes a <c>ReadOnlySpan&lt;byte&gt;</c> with a <c>maxElementLength</c> to the destination </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteUInt8EnumSpan<TEnum>(
        Span<byte> destination,
        ReadOnlySpan<TEnum> value,
        int maxElementLength
    )
        where TEnum : unmanaged, Enum
    {
        var length = Math.Min(value.Length, maxElementLength);
        MemoryMarshal.Cast<TEnum, byte>(value.Slice(0, length)).CopyTo(destination);
    }

    /// <summary> Writes a <c>ReadOnlySpan&lt;byte&gt;</c> with a <c>maxElementLength</c> to the destination </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteInt32SpanLittleEndian<TEnum>(
        Span<byte> destination,
        ReadOnlySpan<TEnum> value,
        int maxElementLength
    )
        where TEnum : unmanaged, Enum
    {
        var length = Math.Min(value.Length, maxElementLength);
        if (!BitConverter.IsLittleEndian)
        {
            ReadOnlySpan<int> reinterpretedValue = MemoryMarshal.Cast<TEnum, int>(value);
            Span<int> reinterpretedDestination = MemoryMarshal.Cast<byte, int>(destination);
            BinaryPrimitives.ReverseEndianness(reinterpretedValue[..length], reinterpretedDestination);
            return;
        }
        MemoryMarshal.Cast<TEnum, byte>(value[..length]).CopyTo(destination);
    }
}

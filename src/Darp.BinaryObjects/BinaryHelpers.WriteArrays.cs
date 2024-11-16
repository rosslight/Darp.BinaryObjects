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
}

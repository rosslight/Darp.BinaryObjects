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
    public static void WriteUInt8List(Span<byte> destination, List<byte> value, int maxLength)
    {
        WriteUInt8Span(destination, CollectionsMarshal.AsSpan(value), maxLength);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteUInt8Span(Span<byte> destination, ReadOnlySpan<byte> value, int maxLength)
    {
        var length = Math.Min(value.Length, maxLength);
        value.Slice(0, length).CopyTo(destination);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteUInt8Enumerable(Span<byte> destination, IEnumerable<byte> value, int maxLength)
    {
        switch (value)
        {
            case byte[] arrayValue:
                WriteUInt8Span(destination, arrayValue, maxLength);
                return;
            case List<byte> listValue:
                WriteUInt8List(destination, listValue, maxLength);
                return;
        }
        var index = 0;
        foreach (var b in value)
        {
            destination[index++] = b;
            if (index >= maxLength)
                return;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteUInt16ListLittleEndian(Span<byte> destination, List<ushort> value, int maxLength)
    {
        WriteUInt16SpanLittleEndian(destination, CollectionsMarshal.AsSpan(value), maxLength);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteUInt16SpanLittleEndian(Span<byte> destination, ReadOnlySpan<ushort> value, int maxLength)
    {
        var length = Math.Min(value.Length, maxLength);
        if (!BitConverter.IsLittleEndian)
        {
            Span<ushort> reinterpretedDestination = MemoryMarshal.Cast<byte, ushort>(destination);
            BinaryPrimitives.ReverseEndianness(value[..length], reinterpretedDestination);
            return;
        }
        MemoryMarshal.Cast<ushort, byte>(value[..length]).CopyTo(destination);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteUInt16ListBigEndian(Span<byte> destination, List<ushort> value, int maxLength)
    {
        WriteUInt16SpanBigEndian(destination, CollectionsMarshal.AsSpan(value), maxLength);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteUInt16SpanBigEndian(Span<byte> destination, ReadOnlySpan<ushort> value, int maxLength)
    {
        var length = Math.Min(value.Length, maxLength);
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
        int maxLength
    )
    {
        var index = 0;
        foreach (var val in value)
        {
            BinaryPrimitives.WriteUInt16LittleEndian(destination[(2 * index++)..], val);
            if (index >= maxLength)
                return;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteUInt16EnumerableBigEndian(Span<byte> destination, IEnumerable<ushort> value, int maxLength)
    {
        var index = 0;
        foreach (var val in value)
        {
            BinaryPrimitives.WriteUInt16BigEndian(destination[(2 * index++)..], val);
            if (index >= maxLength)
                return;
        }
    }
}

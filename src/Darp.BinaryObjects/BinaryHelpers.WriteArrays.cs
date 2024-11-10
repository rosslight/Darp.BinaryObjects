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
    public static void WriteUInt8List(List<byte> value, int maxLength, Span<byte> destination)
    {
        WriteUInt8Span(CollectionsMarshal.AsSpan(value), maxLength, destination);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteUInt8Span(ReadOnlySpan<byte> value, int maxLength, Span<byte> destination)
    {
        var length = Math.Min(value.Length, maxLength);
        value.Slice(0, length).CopyTo(destination);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteUInt8Enumerable(IEnumerable<byte> value, int maxLength, Span<byte> destination)
    {
        switch (value)
        {
            case byte[] arrayValue:
                WriteUInt8Span(arrayValue, maxLength, destination);
                return;
            case List<byte> listValue:
                WriteUInt8List(listValue, maxLength, destination);
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
    public static void WriteUInt16ListLittleEndian(List<ushort> value, int maxLength, Span<byte> destination)
    {
        WriteUInt16SpanLittleEndian(CollectionsMarshal.AsSpan(value), maxLength, destination);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteUInt16SpanLittleEndian(ReadOnlySpan<ushort> value, int maxLength, Span<byte> destination)
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
    public static void WriteUInt16ListBigEndian(List<ushort> value, int maxLength, Span<byte> destination)
    {
        WriteUInt16SpanBigEndian(CollectionsMarshal.AsSpan(value), maxLength, destination);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteUInt16SpanBigEndian(ReadOnlySpan<ushort> value, int maxLength, Span<byte> destination)
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
        IEnumerable<ushort> value,
        int maxLength,
        Span<byte> destination
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
    public static void WriteUInt16EnumerableBigEndian(IEnumerable<ushort> value, int maxLength, Span<byte> destination)
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

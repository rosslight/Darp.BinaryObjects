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
    public static byte[] ReadUInt8Array(ReadOnlySpan<byte> source)
    {
        return source.ToArray();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static List<byte> ReadUInt8List(ReadOnlySpan<byte> source)
    {
        var list = new List<byte>(source.Length);
        list.AddRange(source);
        return list;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ushort[] ReadUInt16ArrayLittleEndian(ReadOnlySpan<byte> source)
    {
        var array = MemoryMarshal.Cast<byte, ushort>(source).ToArray();
        if (!BitConverter.IsLittleEndian)
            BinaryPrimitives.ReverseEndianness(array, array);
        return array;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ushort[] ReadUInt16ArrayBigEndian(ReadOnlySpan<byte> source)
    {
        var array = MemoryMarshal.Cast<byte, ushort>(source).ToArray();
        if (BitConverter.IsLittleEndian)
            BinaryPrimitives.ReverseEndianness(array, array);
        return array;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static List<ushort> ReadUInt16ListLittleEndian(ReadOnlySpan<byte> source)
    {
        ReadOnlySpan<ushort> span = MemoryMarshal.Cast<byte, ushort>(source);
        var list = new List<ushort>(span.Length);
        list.AddRange(span);
        if (!BitConverter.IsLittleEndian)
        {
            Span<ushort> listSpan = CollectionsMarshal.AsSpan(list);
            BinaryPrimitives.ReverseEndianness(span, listSpan);
        }
        return list;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static List<ushort> ReadUInt16ListBigEndian(ReadOnlySpan<byte> source)
    {
        ReadOnlySpan<ushort> span = MemoryMarshal.Cast<byte, ushort>(source);
        var list = new List<ushort>(span.Length);
        list.AddRange(span);
        if (BitConverter.IsLittleEndian)
        {
            Span<ushort> listSpan = CollectionsMarshal.AsSpan(list);
            BinaryPrimitives.ReverseEndianness(span, listSpan);
        }
        return list;
    }
}

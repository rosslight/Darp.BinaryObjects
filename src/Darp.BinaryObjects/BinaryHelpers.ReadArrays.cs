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

    public static T[] ReadConstantBinaryObjectArrayLittleEndian<T>(
        ReadOnlySpan<byte> source,
        int numberOfElements,
        int elementLength
    )
        where T : ISpanReadable<T>
    {
        var array = new T[numberOfElements];
        for (var i = 0; i < numberOfElements; i++)
        {
            if (
                !T.TryReadLittleEndian(
                    source.Slice(i * elementLength, elementLength),
                    out T? value,
                    out var tempBytesRead
                )
            )
                throw new ArgumentOutOfRangeException(nameof(source));
            array[i] = value;
        }
        return array;
    }

    public static List<T> ReadBinaryObjectListLittleEndian<T>(
        ReadOnlySpan<byte> source,
        int numberOfElements,
        int elementLength
    )
        where T : ISpanReadable<T>
    {
        var array = new List<T>(numberOfElements);
        for (var i = 0; i < numberOfElements; i++)
        {
            if (
                !T.TryReadLittleEndian(
                    source.Slice(i * elementLength, elementLength),
                    out T? value,
                    out var tempBytesRead
                )
            )
                throw new ArgumentOutOfRangeException(nameof(source));
            array[i] = value;
        }
        return array;
    }

    public static TEnum[] ReadUInt8EnumArray<TEnum>(ReadOnlySpan<byte> source)
        where TEnum : unmanaged, Enum
    {
        return MemoryMarshal.Cast<byte, TEnum>(source).ToArray();
    }

    public static TEnum[] ReadUInt64EnumArrayBigEndian<TEnum>(ReadOnlySpan<byte> source)
        where TEnum : unmanaged, Enum
    {
        var array = MemoryMarshal.Cast<byte, TEnum>(source).ToArray();
        if (BitConverter.IsLittleEndian)
        {
            var underlyingArray = MemoryMarshal.Cast<TEnum, ulong>(array);
            BinaryPrimitives.ReverseEndianness(underlyingArray, underlyingArray);
        }
        return array;
    }

    public static TEnum[] ReadInt32EnumArrayLittleEndian<TEnum>(ReadOnlySpan<byte> source)
        where TEnum : unmanaged, Enum
    {
        var array = MemoryMarshal.Cast<byte, TEnum>(source).ToArray();
        if (!BitConverter.IsLittleEndian)
        {
            var underlyingArray = MemoryMarshal.Cast<TEnum, int>(array);
            BinaryPrimitives.ReverseEndianness(underlyingArray, underlyingArray);
        }
        return array;
    }
}

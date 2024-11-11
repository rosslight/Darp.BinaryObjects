namespace Darp.BinaryObjects;

using System.Buffers.Binary;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

[Obsolete(
    "This class is providing binary helper methods. Methods might not contain any checks and should not be called by user code!"
)]
public static partial class BinaryHelpers
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static T ReadTLittleEndian<T>(ReadOnlySpan<byte> source, bool unsigned)
        where T : IBinaryInteger<T> => T.ReadLittleEndian(source, unsigned);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static T ReadTBigEndian<T>(ReadOnlySpan<byte> source, bool unsigned)
        where T : IBinaryInteger<T> => T.ReadBigEndian(source, unsigned);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ReadBool(ReadOnlySpan<byte> source) => source[0] > 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static sbyte ReadInt8(ReadOnlySpan<byte> source) => (sbyte)source[0];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte ReadUInt8(ReadOnlySpan<byte> source) => (byte)source[0];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static short ReadInt16LittleEndian(ReadOnlySpan<byte> source)
    {
        return BinaryPrimitives.ReadInt16LittleEndian(source);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static short ReadInt16BigEndian(ReadOnlySpan<byte> source)
    {
        return BinaryPrimitives.ReadInt16BigEndian(source);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ushort ReadUInt16LittleEndian(ReadOnlySpan<byte> source)
    {
        return BinaryPrimitives.ReadUInt16LittleEndian(source);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ushort ReadUInt16BigEndian(ReadOnlySpan<byte> source)
    {
        return BinaryPrimitives.ReadUInt16BigEndian(source);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Half ReadHalfLittleEndian(ReadOnlySpan<byte> source)
    {
        return BinaryPrimitives.ReadHalfLittleEndian(source);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static UInt128 ReadCharLittleEndian(ReadOnlySpan<byte> source)
    {
        return ReadTLittleEndian<char>(source, true);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static UInt128 ReadCharBigEndian(ReadOnlySpan<byte> source)
    {
        return ReadTBigEndian<char>(source, true);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Half ReadHalfBigEndian(ReadOnlySpan<byte> source)
    {
        return BinaryPrimitives.ReadHalfBigEndian(source);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ReadInt32LittleEndian(ReadOnlySpan<byte> source)
    {
        return BinaryPrimitives.ReadInt32LittleEndian(source);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ReadInt32BigEndian(ReadOnlySpan<byte> source)
    {
        return BinaryPrimitives.ReadInt32BigEndian(source);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint ReadUInt32LittleEndian(ReadOnlySpan<byte> source)
    {
        return BinaryPrimitives.ReadUInt32LittleEndian(source);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint ReadUInt32BigEndian(ReadOnlySpan<byte> source)
    {
        return BinaryPrimitives.ReadUInt32BigEndian(source);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float ReadSingleLittleEndian(ReadOnlySpan<byte> source)
    {
        return BinaryPrimitives.ReadSingleLittleEndian(source);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float ReadSingleBigEndian(ReadOnlySpan<byte> source)
    {
        return BinaryPrimitives.ReadSingleBigEndian(source);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long ReadInt64LittleEndian(ReadOnlySpan<byte> source)
    {
        return BinaryPrimitives.ReadInt64LittleEndian(source);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long ReadInt64BigEndian(ReadOnlySpan<byte> source)
    {
        return BinaryPrimitives.ReadInt64BigEndian(source);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong ReadUInt64LittleEndian(ReadOnlySpan<byte> source)
    {
        return BinaryPrimitives.ReadUInt64LittleEndian(source);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong ReadUInt64BigEndian(ReadOnlySpan<byte> source)
    {
        return BinaryPrimitives.ReadUInt64BigEndian(source);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double ReadDoubleLittleEndian(ReadOnlySpan<byte> source)
    {
        return BinaryPrimitives.ReadDoubleLittleEndian(source);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double ReadDoubleBigEndian(ReadOnlySpan<byte> source)
    {
        return BinaryPrimitives.ReadDoubleBigEndian(source);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Int128 ReadInt128LittleEndian(ReadOnlySpan<byte> source)
    {
        return BinaryPrimitives.ReadInt128LittleEndian(source);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Int128 ReadInt128BigEndian(ReadOnlySpan<byte> source)
    {
        return BinaryPrimitives.ReadInt128BigEndian(source);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static UInt128 ReadUInt128LittleEndian(ReadOnlySpan<byte> source)
    {
        return BinaryPrimitives.ReadUInt128LittleEndian(source);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static UInt128 ReadUInt128BigEndian(ReadOnlySpan<byte> source)
    {
        return BinaryPrimitives.ReadUInt128BigEndian(source);
    }
}

namespace Darp.BinaryObjects;

using System.Buffers.Binary;
using System.Numerics;
using System.Runtime.CompilerServices;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

public static partial class BinaryHelpers
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void WriteTLittleEndian<T>(Span<byte> destination, T value)
        where T : IBinaryInteger<T>
    {
        value.WriteLittleEndian(destination);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void WriteTBigEndian<T>(Span<byte> destination, T value)
        where T : IBinaryInteger<T>
    {
        value.WriteBigEndian(destination);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteBool(Span<byte> destination, bool value)
    {
        destination[0] = value ? (byte)0b01 : (byte)0b00;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteInt8(Span<byte> destination, sbyte value)
    {
        destination[0] = (byte)value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteUInt8(Span<byte> destination, byte value)
    {
        destination[0] = value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteInt16LittleEndian(Span<byte> destination, short value)
    {
        BinaryPrimitives.WriteInt16LittleEndian(destination, value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteInt16BigEndian(Span<byte> destination, short value)
    {
        BinaryPrimitives.WriteInt16BigEndian(destination, value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteUInt16LittleEndian(Span<byte> destination, ushort value)
    {
        BinaryPrimitives.WriteUInt16LittleEndian(destination, value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteUInt16BigEndian(Span<byte> destination, ushort value)
    {
        BinaryPrimitives.WriteUInt16BigEndian(destination, value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteHalfLittleEndian(Span<byte> destination, Half value)
    {
        BinaryPrimitives.WriteHalfLittleEndian(destination, value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteHalfBigEndian(Span<byte> destination, Half value)
    {
        BinaryPrimitives.WriteHalfBigEndian(destination, value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteCharLittleEndian(Span<byte> destination, char value)
    {
        WriteTLittleEndian(destination, value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteCharBigEndian(Span<byte> destination, char value)
    {
        WriteTBigEndian(destination, value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteInt32LittleEndian(Span<byte> destination, int value)
    {
        BinaryPrimitives.WriteInt32LittleEndian(destination, value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteInt32BigEndian(Span<byte> destination, int value)
    {
        BinaryPrimitives.WriteInt32BigEndian(destination, value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteUInt32LittleEndian(Span<byte> destination, uint value)
    {
        BinaryPrimitives.WriteUInt32LittleEndian(destination, value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteUInt32BigEndian(Span<byte> destination, uint value)
    {
        BinaryPrimitives.WriteUInt32BigEndian(destination, value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteSingleLittleEndian(Span<byte> destination, float value)
    {
        BinaryPrimitives.WriteSingleLittleEndian(destination, value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteSingleBigEndian(Span<byte> destination, float value)
    {
        BinaryPrimitives.WriteSingleBigEndian(destination, value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteInt64LittleEndian(Span<byte> destination, long value)
    {
        BinaryPrimitives.WriteInt64LittleEndian(destination, value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteInt64BigEndian(Span<byte> destination, long value)
    {
        BinaryPrimitives.WriteInt64BigEndian(destination, value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteUInt64LittleEndian(Span<byte> destination, ulong value)
    {
        BinaryPrimitives.WriteUInt64LittleEndian(destination, value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteUInt64BigEndian(Span<byte> destination, ulong value)
    {
        BinaryPrimitives.WriteUInt64BigEndian(destination, value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteDoubleLittleEndian(Span<byte> destination, double value)
    {
        BinaryPrimitives.WriteDoubleLittleEndian(destination, value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteDoubleBigEndian(Span<byte> destination, double value)
    {
        BinaryPrimitives.WriteDoubleBigEndian(destination, value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteInt128LittleEndian(Span<byte> destination, Int128 value)
    {
        BinaryPrimitives.WriteInt128LittleEndian(destination, value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteInt128BigEndian(Span<byte> destination, Int128 value)
    {
        BinaryPrimitives.WriteInt128BigEndian(destination, value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteUInt128LittleEndian(Span<byte> destination, UInt128 value)
    {
        BinaryPrimitives.WriteUInt128LittleEndian(destination, value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WriteUInt128BigEndian(Span<byte> destination, UInt128 value)
    {
        BinaryPrimitives.WriteUInt128BigEndian(destination, value);
    }
}

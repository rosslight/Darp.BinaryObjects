namespace Darp.BinaryObjects.Generator.Tests.Sources;

using System.Buffers.Binary;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

public sealed partial record AllPrimitives(
    bool ValueBool,
    sbyte ValueSByte,
    short ValueShort,
    Half ValueHalf,
    int ValueInt,
    float ValueFloat,
    long ValueLong,
    Int128 ValueInt128,
    UInt128 ValueUInt128,
    ulong ValueULong,
    double ValueDouble,
    uint ValueUInt,
    ushort ValueUShort,
    byte ValueByte
);

/// <remarks> <list type="table">
/// <item> <term><b>Field</b></term> <description><b>Byte Length</b></description> </item>
/// <item> <term><see cref="ValueBool"/></term> <description>1</description> </item>
/// <item> <term><see cref="ValueSByte"/></term> <description>1</description> </item>
/// <item> <term><see cref="ValueShort"/></term> <description>2</description> </item>
/// <item> <term><see cref="ValueHalf"/></term> <description>2</description> </item>
/// <item> <term><see cref="ValueInt"/></term> <description>4</description> </item>
/// <item> <term><see cref="ValueFloat"/></term> <description>4</description> </item>
/// <item> <term><see cref="ValueLong"/></term> <description>8</description> </item>
/// <item> <term><see cref="ValueInt128"/></term> <description>16</description> </item>
/// <item> <term><see cref="ValueUInt128"/></term> <description>16</description> </item>
/// <item> <term><see cref="ValueULong"/></term> <description>8</description> </item>
/// <item> <term><see cref="ValueDouble"/></term> <description>8</description> </item>
/// <item> <term><see cref="ValueUInt"/></term> <description>4</description> </item>
/// <item> <term><see cref="ValueUShort"/></term> <description>2</description> </item>
/// <item> <term><see cref="ValueByte"/></term> <description>1</description> </item>
/// <item> <term> --- </term> <description>77</description> </item>
/// </list> </remarks>
public sealed partial record AllPrimitives : IWritable, ISpanReadable<Sources.AllPrimitives>
{
    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetByteCount() => 77;

    private bool TryWrite(Span<byte> destination, out int bytesWritten, bool writeLittleEndian)
    {
        if (destination.Length < GetByteCount())
        {
            bytesWritten = 0;
            return false;
        }
        destination[0] = ValueBool ? (byte)0x01 : (byte)0x00;
        destination[1] = (byte)ValueSByte;
        if (BitConverter.IsLittleEndian != writeLittleEndian)
        {
            MemoryMarshal.Write(destination[2..], BinaryPrimitives.ReverseEndianness(ValueShort));
            MemoryMarshal.Write(
                destination[4..],
                BinaryPrimitives.ReverseEndianness(BitConverter.HalfToUInt16Bits(ValueHalf))
            );
            MemoryMarshal.Write(destination[6..], BinaryPrimitives.ReverseEndianness(ValueInt));
            MemoryMarshal.Write(
                destination[10..],
                BinaryPrimitives.ReverseEndianness(BitConverter.SingleToUInt32Bits(ValueFloat))
            );
            MemoryMarshal.Write(destination[14..], BinaryPrimitives.ReverseEndianness(ValueLong));
            MemoryMarshal.Write(destination[22..], BinaryPrimitives.ReverseEndianness(ValueInt128));
            MemoryMarshal.Write(destination[38..], BinaryPrimitives.ReverseEndianness(ValueUInt128));
            MemoryMarshal.Write(destination[54..], BinaryPrimitives.ReverseEndianness(ValueULong));
            MemoryMarshal.Write(
                destination[62..],
                BinaryPrimitives.ReverseEndianness(BitConverter.DoubleToUInt64Bits(ValueDouble))
            );
            MemoryMarshal.Write(destination[70..], BinaryPrimitives.ReverseEndianness(ValueUInt));
            MemoryMarshal.Write(destination[74..], BinaryPrimitives.ReverseEndianness(ValueUShort));
        }
        else
        {
            MemoryMarshal.Write(destination[2..], ValueShort);
            MemoryMarshal.Write(destination[4..], ValueHalf);
            MemoryMarshal.Write(destination[6..], ValueInt);
            MemoryMarshal.Write(destination[10..], ValueFloat);
            MemoryMarshal.Write(destination[14..], ValueLong);
            MemoryMarshal.Write(destination[22..], ValueInt128);
            MemoryMarshal.Write(destination[38..], ValueUInt128);
            MemoryMarshal.Write(destination[54..], ValueULong);
            MemoryMarshal.Write(destination[62..], ValueDouble);
            MemoryMarshal.Write(destination[70..], ValueUInt);
            MemoryMarshal.Write(destination[74..], ValueUShort);
        }
        destination[76] = ValueByte;
        bytesWritten = 77;
        return true;
    }

    /// <inheritdoc />
    public bool TryWriteLittleEndian(Span<byte> destination) => TryWrite(destination, out _, true);

    /// <inheritdoc />
    public bool TryWriteLittleEndian(Span<byte> destination, out int bytesWritten) =>
        TryWrite(destination, out bytesWritten, true);

    /// <inheritdoc />
    public bool TryWriteBigEndian(Span<byte> destination) => TryWrite(destination, out _, false);

    /// <inheritdoc />
    public bool TryWriteBigEndian(Span<byte> destination, out int bytesWritten) =>
        TryWrite(destination, out bytesWritten, false);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool TryRead(
        ReadOnlySpan<byte> source,
        [NotNullWhen(true)] out Sources.AllPrimitives? value,
        out int bytesRead,
        bool readLittleEndian
    )
    {
        if (source.Length < 77)
        {
            value = default;
            bytesRead = 0;
            return false;
        }
        var readValueBool = MemoryMarshal.Read<bool>(source);
        var readValueSByte = MemoryMarshal.Read<sbyte>(source);
        var readValueShort = MemoryMarshal.Read<short>(source);
        var readValueHalf = MemoryMarshal.Read<ushort>(source);
        var readValueInt = MemoryMarshal.Read<int>(source);
        var readValueFloat = MemoryMarshal.Read<uint>(source);
        var readValueLong = MemoryMarshal.Read<long>(source);
        var readValueInt128 = MemoryMarshal.Read<Int128>(source);
        var readValueUInt128 = MemoryMarshal.Read<UInt128>(source);
        var readValueULong = MemoryMarshal.Read<ulong>(source);
        var readValueDouble = MemoryMarshal.Read<ulong>(source);
        var readValueUInt = MemoryMarshal.Read<uint>(source);
        var readValueUShort = MemoryMarshal.Read<ushort>(source);
        var readValueByte = MemoryMarshal.Read<byte>(source);
        if (BitConverter.IsLittleEndian != readLittleEndian)
        {
            readValueShort = BinaryPrimitives.ReverseEndianness(readValueShort);
            readValueHalf = BinaryPrimitives.ReverseEndianness(readValueHalf);
            readValueInt = BinaryPrimitives.ReverseEndianness(readValueInt);
            readValueFloat = BinaryPrimitives.ReverseEndianness(readValueFloat);
            readValueLong = BinaryPrimitives.ReverseEndianness(readValueLong);
            readValueInt128 = BinaryPrimitives.ReverseEndianness(readValueInt128);
            readValueUInt128 = BinaryPrimitives.ReverseEndianness(readValueUInt128);
            readValueULong = BinaryPrimitives.ReverseEndianness(readValueULong);
            readValueDouble = BinaryPrimitives.ReverseEndianness(readValueDouble);
            readValueUInt = BinaryPrimitives.ReverseEndianness(readValueUInt);
            readValueUShort = BinaryPrimitives.ReverseEndianness(readValueUShort);
        }
        value = new Sources.AllPrimitives(
            readValueBool,
            readValueSByte,
            readValueShort,
            BitConverter.UInt16BitsToHalf(readValueHalf),
            readValueInt,
            BitConverter.UInt32BitsToSingle(readValueFloat),
            readValueLong,
            readValueInt128,
            readValueUInt128,
            readValueULong,
            BitConverter.UInt64BitsToDouble(readValueDouble),
            readValueUInt,
            readValueUShort,
            readValueByte
        );
        bytesRead = 4;
        return true;
    }

    /// <inheritdoc />
    public static bool TryReadLittleEndian(
        ReadOnlySpan<byte> source,
        [NotNullWhen(true)] out Sources.AllPrimitives? value
    ) => TryRead(source, out value, out _, true);

    /// <inheritdoc />
    public static bool TryReadLittleEndian(
        ReadOnlySpan<byte> source,
        [NotNullWhen(true)] out Sources.AllPrimitives? value,
        out int bytesRead
    ) => TryRead(source, out value, out bytesRead, true);

    /// <inheritdoc />
    public static bool TryReadBigEndian(
        ReadOnlySpan<byte> source,
        [NotNullWhen(true)] out Sources.AllPrimitives? value
    ) => TryRead(source, out value, out _, false);

    /// <inheritdoc />
    public static bool TryReadBigEndian(
        ReadOnlySpan<byte> source,
        [NotNullWhen(true)] out Sources.AllPrimitives? value,
        out int bytesRead
    ) => TryRead(source, out value, out bytesRead, false);
}

namespace Darp.BinaryObjects.Generator.Tests.Sources;

using System.Buffers.Binary;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

public sealed partial record TwoUShorts(ushort Value, ushort ValueTwo);

/// <remarks> <list type="table">
/// <item> <term><b>Field</b></term> <description><b>Byte Length</b></description> </item>
/// <item> <term><see cref="Value"/></term> <description>2</description> </item>
/// <item> <term> --- </term> <description>2</description> </item>
/// </list> </remarks>
public sealed partial record TwoUShorts : IWritable, ISpanReadable<Sources.TwoUShorts>
{
    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetWriteSize() => 4;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool TryWrite(Span<byte> destination, bool writeLittleEndian)
    {
        if (destination.Length < GetWriteSize())
        {
            return false;
        }
        if (BitConverter.IsLittleEndian != writeLittleEndian)
        {
            MemoryMarshal.Write(destination, BinaryPrimitives.ReverseEndianness(Value));
            MemoryMarshal.Write(destination[2..], BinaryPrimitives.ReverseEndianness(ValueTwo));
        }
        else
        {
            MemoryMarshal.Write(destination, Value);
            MemoryMarshal.Write(destination[2..], ValueTwo);
        }
        return true;
    }

    /// <inheritdoc />
    public bool TryWriteLittleEndian(Span<byte> destination) => TryWrite(destination, true);

    /// <inheritdoc />
    public bool TryWriteBigEndian(Span<byte> destination) => TryWrite(destination, false);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool TryRead(
        ReadOnlySpan<byte> source,
        [NotNullWhen(true)] out Sources.TwoUShorts? value,
        out int bytesConsumed,
        bool readLittleEndian
    )
    {
        if (source.Length < 4)
        {
            value = default;
            bytesConsumed = 0;
            return false;
        }
        var readValue = MemoryMarshal.Read<ushort>(source);
        var readValueTwo = MemoryMarshal.Read<ushort>(source[2..]);
        if (BitConverter.IsLittleEndian != readLittleEndian)
        {
            readValue = BinaryPrimitives.ReverseEndianness(readValue);
            readValueTwo = BinaryPrimitives.ReverseEndianness(readValueTwo);
        }
        value = new Sources.TwoUShorts(readValue, readValueTwo);
        bytesConsumed = 4;
        return true;
    }

    /// <inheritdoc />
    public static bool TryReadLittleEndian(
        ReadOnlySpan<byte> source,
        [NotNullWhen(true)] out Sources.TwoUShorts? value
    ) => TryRead(source, out value, out _, true);

    /// <inheritdoc />
    public static bool TryReadLittleEndian(
        ReadOnlySpan<byte> source,
        [NotNullWhen(true)] out Sources.TwoUShorts? value,
        out int bytesConsumed
    ) => TryRead(source, out value, out bytesConsumed, true);

    /// <inheritdoc />
    public static bool TryReadBigEndian(ReadOnlySpan<byte> source, [NotNullWhen(true)] out Sources.TwoUShorts? value) =>
        TryRead(source, out value, out _, false);

    /// <inheritdoc />
    public static bool TryReadBigEndian(
        ReadOnlySpan<byte> source,
        [NotNullWhen(true)] out Sources.TwoUShorts? value,
        out int bytesConsumed
    ) => TryRead(source, out value, out bytesConsumed, false);
}

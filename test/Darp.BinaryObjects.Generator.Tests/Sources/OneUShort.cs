namespace Darp.BinaryObjects.Generator.Tests.Sources;

using System.Buffers.Binary;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[BinaryObject]
public sealed partial record OneUShort(ushort Value);

/// <remarks> <list type="table">
/// <item> <term><b>Field</b></term> <description><b>Byte Length</b></description> </item>
/// <item> <term><see cref="Value"/></term> <description>2</description> </item>
/// <item> <term> --- </term> <description>2</description> </item>
/// </list> </remarks>
public sealed partial record OneUShort : IWritable, ISpanReadable<Sources.OneUShort>
{
    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetWriteSize() => 2;

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
        }
        else
        {
            MemoryMarshal.Write(destination, Value);
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
        [NotNullWhen(true)] out Sources.OneUShort? value,
        out int bytesRead,
        bool readLittleEndian
    )
    {
        if (source.Length < 2)
        {
            value = default;
            bytesRead = 0;
            return false;
        }
        var readValue = MemoryMarshal.Read<ushort>(source);
        if (BitConverter.IsLittleEndian != readLittleEndian)
        {
            readValue = BinaryPrimitives.ReverseEndianness(readValue);
        }
        value = new Sources.OneUShort(readValue);
        bytesRead = 2;
        return true;
    }

    /// <inheritdoc />
    public static bool TryReadLittleEndian(
        ReadOnlySpan<byte> source,
        [NotNullWhen(true)] out Sources.OneUShort? value
    ) => TryRead(source, out value, out _, true);

    /// <inheritdoc />
    public static bool TryReadLittleEndian(
        ReadOnlySpan<byte> source,
        [NotNullWhen(true)] out Sources.OneUShort? value,
        out int bytesRead
    ) => TryRead(source, out value, out bytesRead, true);

    /// <inheritdoc />
    public static bool TryReadBigEndian(ReadOnlySpan<byte> source, [NotNullWhen(true)] out Sources.OneUShort? value) =>
        TryRead(source, out value, out _, false);

    /// <inheritdoc />
    public static bool TryReadBigEndian(
        ReadOnlySpan<byte> source,
        [NotNullWhen(true)] out Sources.OneUShort? value,
        out int bytesRead
    ) => TryRead(source, out value, out bytesRead, false);
}

namespace Darp.BinaryObjects.Generator.Tests.Sources;

using System.Buffers.Binary;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

public sealed partial record ListUShortRemainingLength(
    [property: BinaryReadRemaining(MinElements = 2)] IReadOnlyList<ushort> Value
);

/// <remarks> <list type="table">
/// <item> <term><b>Field</b></term> <description><b>Byte Length</b></description> </item>
/// <item> <term><see cref="Value"/></term> <description>2 * (2 + k)</description> </item>
/// <item> <term> --- </term> <description>4 + 2 * k</description> </item>
/// </list> </remarks>
public sealed partial record ListUShortRemainingLength : IWritable, ISpanReadable<ListUShortRemainingLength>
{
    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetByteCount() => Math.Max(4, Value.Count * 2);

    private bool TryWrite(Span<byte> destination, out int bytesWritten, bool writeLittleEndian)
    {
        if (destination.Length < GetByteCount())
        {
            bytesWritten = 0;
            return false;
        }
        bytesWritten = 0;
        var writeValueIndex = 0;
        foreach (var writeValue in Value)
        {
            MemoryMarshal.Write(
                destination[(2 * writeValueIndex++)..],
                BitConverter.IsLittleEndian != writeLittleEndian
                    ? BinaryPrimitives.ReverseEndianness(writeValue)
                    : writeValue
            );
            bytesWritten += 2;
        }
        return true;
    }

    /// <inheritdoc />
    /// <remarks>This method is not thread safe</remarks>
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
        [NotNullWhen(true)] out ListUShortRemainingLength? value,
        out int bytesRead,
        bool readLittleEndian
    )
    {
        if (source.Length < 4)
        {
            value = default;
            bytesRead = 0;
            return false;
        }
        var readValueLength = (source.Length - 0) / 2;
        var readValue = new ushort[readValueLength];
        ReadOnlySpan<ushort> readValueSpan = MemoryMarshal.Cast<byte, ushort>(source.Slice(0, readValueLength * 2));
        if (BitConverter.IsLittleEndian != readLittleEndian)
        {
            BinaryPrimitives.ReverseEndianness(readValueSpan, readValue);
        }
        else
        {
            readValueSpan.CopyTo(readValue);
        }

        value = new ListUShortRemainingLength(readValue);
        bytesRead = readValueLength * 2;
        return true;
    }

    /// <inheritdoc />
    public static bool TryReadLittleEndian(
        ReadOnlySpan<byte> source,
        [NotNullWhen(true)] out ListUShortRemainingLength? value
    ) => TryRead(source, out value, out _, true);

    /// <inheritdoc />
    public static bool TryReadLittleEndian(
        ReadOnlySpan<byte> source,
        [NotNullWhen(true)] out ListUShortRemainingLength? value,
        out int bytesRead
    ) => TryRead(source, out value, out bytesRead, true);

    /// <inheritdoc />
    public static bool TryReadBigEndian(
        ReadOnlySpan<byte> source,
        [NotNullWhen(true)] out ListUShortRemainingLength? value
    ) => TryRead(source, out value, out _, false);

    /// <inheritdoc />
    public static bool TryReadBigEndian(
        ReadOnlySpan<byte> source,
        [NotNullWhen(true)] out ListUShortRemainingLength? value,
        out int bytesRead
    ) => TryRead(source, out value, out bytesRead, false);
}

namespace Darp.BinaryObjects.Generator.Tests.Sources;

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

[BinaryObject]
public sealed partial record ArrayByteFixedSize([property: BinaryArrayLength(2)] byte[] Value);

/// <remarks> <list type="table">
/// <item> <term><b>Field</b></term> <description><b>Byte Length</b></description> </item>
/// <item> <term><see cref="Value"/></term> <description>1 * 2</description> </item>
/// <item> <term> --- </term> <description>2</description> </item>
/// </list> </remarks>
public sealed partial record ArrayByteFixedSize : IWritable, ISpanReadable<ArrayByteFixedSize>
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

        Value.AsSpan().Slice(0, Math.Min(Value.Length, 2)).CopyTo(destination);
        return true;
    }

    /// <inheritdoc />
    public bool TryWriteLittleEndian(Span<byte> destination) => TryWrite(destination, true);

    /// <inheritdoc />
    public bool TryWriteBigEndian(Span<byte> destination) => TryWrite(destination, false);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool TryRead(
        ReadOnlySpan<byte> source,
        [NotNullWhen(true)] out ArrayByteFixedSize? value,
        out int bytesRead,
        bool readLittleEndian
    )
    {
        if (source.Length < 1)
        {
            value = default;
            bytesRead = 0;
            return false;
        }
        var readValue = source[..2].ToArray();
        value = new ArrayByteFixedSize(readValue);
        bytesRead = 2;
        return true;
    }

    /// <inheritdoc />
    public static bool TryReadLittleEndian(
        ReadOnlySpan<byte> source,
        [NotNullWhen(true)] out ArrayByteFixedSize? value
    ) => TryRead(source, out value, out _, true);

    /// <inheritdoc />
    public static bool TryReadLittleEndian(
        ReadOnlySpan<byte> source,
        [NotNullWhen(true)] out ArrayByteFixedSize? value,
        out int bytesRead
    ) => TryRead(source, out value, out bytesRead, true);

    /// <inheritdoc />
    public static bool TryReadBigEndian(ReadOnlySpan<byte> source, [NotNullWhen(true)] out ArrayByteFixedSize? value) =>
        TryRead(source, out value, out _, false);

    /// <inheritdoc />
    public static bool TryReadBigEndian(
        ReadOnlySpan<byte> source,
        [NotNullWhen(true)] out ArrayByteFixedSize? value,
        out int bytesRead
    ) => TryRead(source, out value, out bytesRead, false);
}

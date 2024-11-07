namespace Darp.BinaryObjects.Generator.Tests.Sources;

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

[BinaryObject]
public sealed partial record OneBool(bool Value);

/// <remarks> <list type="table">
/// <item> <term><b>Field</b></term> <description><b>Byte Length</b></description> </item>
/// <item> <term><see cref="Value"/></term> <description>1</description> </item>
/// <item> <term> --- </term> <description>1</description> </item>
/// </list> </remarks>
public sealed partial record OneBool : IWritable, ISpanReadable<Sources.OneBool>
{
    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetByteCount() => 1;

    private bool TryWrite(Span<byte> destination, out int bytesWritten, bool writeLittleEndian)
    {
        if (destination.Length < GetByteCount())
        {
            bytesWritten = 0;
            return false;
        }
        destination[0] = Value ? (byte)0b1 : (byte)0b0;
        bytesWritten = 1;
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
        [NotNullWhen(true)] out Sources.OneBool? value,
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
        var readValue = source[0] > 0;
        value = new Sources.OneBool(readValue);
        bytesRead = 1;
        return true;
    }

    /// <inheritdoc />
    public static bool TryReadLittleEndian(ReadOnlySpan<byte> source, [NotNullWhen(true)] out Sources.OneBool? value) =>
        TryRead(source, out value, out _, true);

    /// <inheritdoc />
    public static bool TryReadLittleEndian(
        ReadOnlySpan<byte> source,
        [NotNullWhen(true)] out Sources.OneBool? value,
        out int bytesRead
    ) => TryRead(source, out value, out bytesRead, true);

    /// <inheritdoc />
    public static bool TryReadBigEndian(ReadOnlySpan<byte> source, [NotNullWhen(true)] out Sources.OneBool? value) =>
        TryRead(source, out value, out _, false);

    /// <inheritdoc />
    public static bool TryReadBigEndian(
        ReadOnlySpan<byte> source,
        [NotNullWhen(true)] out Sources.OneBool? value,
        out int bytesRead
    ) => TryRead(source, out value, out bytesRead, false);
}

namespace Darp.BinaryObjects.Generator.Tests.Sources;

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

public sealed partial record OneByte(byte Value);

/// <remarks> <list type="table">
/// <item> <term><b>Field</b></term> <description><b>Byte Length</b></description> </item>
/// <item> <term><see cref="Value"/></term> <description>1</description> </item>
/// <item> <term> --- </term> <description>1</description> </item>
/// </list> </remarks>
public sealed partial record OneByte : IWritable, ISpanReadable<Sources.OneByte>
{
    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetWriteSize() => 1;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool TryWrite(Span<byte> destination, bool writeLittleEndian)
    {
        if (destination.Length < GetWriteSize())
        {
            return false;
        }
        destination[0] = Value;
        return true;
    }

    /// <inheritdoc />
    public bool TryWriteLittleEndian(Span<byte> destination) => TryWrite(destination, true);

    /// <inheritdoc />
    public bool TryWriteBigEndian(Span<byte> destination) => TryWrite(destination, false);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool TryRead(
        ReadOnlySpan<byte> source,
        [NotNullWhen(true)] out Sources.OneByte? value,
        out int bytesConsumed,
        bool readLittleEndian
    )
    {
        if (source.Length < 1)
        {
            value = default;
            bytesConsumed = 0;
            return false;
        }
        var readValue = source[0];
        value = new Sources.OneByte(readValue);
        bytesConsumed = 1;
        return true;
    }

    /// <inheritdoc />
    public static bool TryReadLittleEndian(ReadOnlySpan<byte> source, [NotNullWhen(true)] out Sources.OneByte? value) =>
        TryRead(source, out value, out _, true);

    /// <inheritdoc />
    public static bool TryReadLittleEndian(
        ReadOnlySpan<byte> source,
        [NotNullWhen(true)] out Sources.OneByte? value,
        out int bytesConsumed
    ) => TryRead(source, out value, out bytesConsumed, true);

    /// <inheritdoc />
    public static bool TryReadBigEndian(ReadOnlySpan<byte> source, [NotNullWhen(true)] out Sources.OneByte? value) =>
        TryRead(source, out value, out _, false);

    /// <inheritdoc />
    public static bool TryReadBigEndian(
        ReadOnlySpan<byte> source,
        [NotNullWhen(true)] out Sources.OneByte? value,
        out int bytesConsumed
    ) => TryRead(source, out value, out bytesConsumed, false);
}

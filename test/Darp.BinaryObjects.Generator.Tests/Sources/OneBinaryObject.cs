namespace Darp.BinaryObjects.Generator.Tests.Sources;

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

public sealed partial record OneBinaryObject(OneBool Value);

/// <remarks> <list type="table">
/// <item> <term><b>Field</b></term> <description><b>Byte Length</b></description> </item>
/// <item> <term><see cref="Value"/></term> <description>1</description> </item>
/// <item> <term> --- </term> <description>1</description> </item>
/// </list> </remarks>
public sealed partial record OneBinaryObject : IWritable, ISpanReadable<OneBinaryObject>
{
    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetByteCount() => Value.GetByteCount();

    /// <inheritdoc />
    public bool TryWriteLittleEndian(Span<byte> destination) => TryWriteLittleEndian(destination, out _);

    /// <inheritdoc />
    public bool TryWriteLittleEndian(Span<byte> destination, out int bytesWritten)
    {
        bytesWritten = 0;

        if (!this.Value.TryWriteLittleEndian(destination, out var ___valueBytesWritten))
        {
            bytesWritten = ___valueBytesWritten;
            return false;
        }
        bytesWritten += ___valueBytesWritten;

        return true;
    }

    /// <inheritdoc />
    public bool TryWriteBigEndian(Span<byte> destination) => TryWriteBigEndian(destination, out _);

    /// <inheritdoc />
    public bool TryWriteBigEndian(Span<byte> destination, out int bytesWritten)
    {
        bytesWritten = 0;

        if (!this.Value.TryWriteBigEndian(destination, out var ___valueBytesWritten))
        {
            bytesWritten = ___valueBytesWritten;
            return false;
        }
        bytesWritten += ___valueBytesWritten;

        return true;
    }

    /// <inheritdoc />
    public static bool TryReadLittleEndian(ReadOnlySpan<byte> source, [NotNullWhen(true)] out OneBinaryObject? value) =>
        TryReadLittleEndian(source, out value, out _);

    /// <inheritdoc />
    public static bool TryReadLittleEndian(
        ReadOnlySpan<byte> source,
        [NotNullWhen(true)] out OneBinaryObject? value,
        out int bytesRead
    )
    {
        bytesRead = 0;

        if (!OneBool.TryReadLittleEndian(source, out var ___readValue, out var ___bytesReadValue))
        {
            value = default;
            return false;
        }
        bytesRead += ___bytesReadValue;

        value = new OneBinaryObject(___readValue);
        return true;
    }

    /// <inheritdoc />
    public static bool TryReadBigEndian(ReadOnlySpan<byte> source, [NotNullWhen(true)] out OneBinaryObject? value) =>
        TryReadBigEndian(source, out value, out _);

    /// <inheritdoc />
    public static bool TryReadBigEndian(
        ReadOnlySpan<byte> source,
        [NotNullWhen(true)] out OneBinaryObject? value,
        out int bytesRead
    )
    {
        bytesRead = 0;
        if (!OneBool.TryReadBigEndian(source, out var ___readValue, out var ___bytesReadValue))
        {
            value = default;
            return false;
        }
        bytesRead += ___bytesReadValue;

        value = new OneBinaryObject(___readValue);
        return true;
    }
}

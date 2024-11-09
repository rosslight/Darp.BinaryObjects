namespace Darp.BinaryObjects.Generator.Tests.Sources;

using System.Buffers.Binary;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

public sealed partial record MemoryMemberLengthSize(
    [property: BinaryByteLength(2)] int Length,
    [property: BinaryArrayLength("Length")] ReadOnlyMemory<byte> Value
);

/// <remarks> <list type="table">
/// <item> <term><b>Field</b></term> <description><b>Byte Length</b></description> </item>
/// <item> <term><see cref="Length"/></term> <description>2</description> </item>
/// <item> <term><see cref="Value"/></term> <description>1 * <see cref="Length"/></description> </item>
/// <item> <term> --- </term> <description>2 + <see cref="Length"/></description> </item>
/// </list> </remarks>
public sealed partial record MemoryMemberLengthSize : IWritable, ISpanReadable<MemoryMemberLengthSize>
{
    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetByteCount() => 2 + Length;

    private bool TryWrite(Span<byte> destination, out int bytesWritten, bool writeLittleEndian)
    {
        if (destination.Length < GetByteCount())
        {
            bytesWritten = 0;
            return false;
        }

        if (BitConverter.IsLittleEndian != writeLittleEndian)
        {
            MemoryMarshal.Write(destination, BinaryPrimitives.ReverseEndianness((ushort)Length));
        }
        else
        {
            MemoryMarshal.Write(destination, (ushort)Length);
        }

        var valueLength = Math.Min(Value.Length, Length);
        Value.Span.Slice(0, valueLength).CopyTo(destination[2..]);
        bytesWritten = 2 + valueLength;
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
        [NotNullWhen(true)] out MemoryMemberLengthSize? value,
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

        var readLength = MemoryMarshal.Read<ushort>(source);
        if (BitConverter.IsLittleEndian != readLittleEndian)
        {
            readLength = BinaryPrimitives.ReverseEndianness(readLength);
        }
        if (source.Length - 2 < readLength)
        {
            value = default;
            bytesRead = 0;
            return false;
        }
        var readValue = source.Slice(2, readLength).ToArray();
        value = new MemoryMemberLengthSize(readLength, readValue);
        bytesRead = 2 + readLength;
        return true;
    }

    /// <inheritdoc />
    public static bool TryReadLittleEndian(
        ReadOnlySpan<byte> source,
        [NotNullWhen(true)] out MemoryMemberLengthSize? value
    ) => TryRead(source, out value, out _, true);

    /// <inheritdoc />
    public static bool TryReadLittleEndian(
        ReadOnlySpan<byte> source,
        [NotNullWhen(true)] out MemoryMemberLengthSize? value,
        out int bytesRead
    ) => TryRead(source, out value, out bytesRead, true);

    /// <inheritdoc />
    public static bool TryReadBigEndian(
        ReadOnlySpan<byte> source,
        [NotNullWhen(true)] out MemoryMemberLengthSize? value
    ) => TryRead(source, out value, out _, false);

    /// <inheritdoc />
    public static bool TryReadBigEndian(
        ReadOnlySpan<byte> source,
        [NotNullWhen(true)] out MemoryMemberLengthSize? value,
        out int bytesRead
    ) => TryRead(source, out value, out bytesRead, false);
}

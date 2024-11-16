namespace Darp.BinaryObjects.Generator.Tests.Sources;

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

public sealed partial record ArrayByteMemberLength(byte Length, [property: BinaryElementCount("Length")] byte[] Value);

/// <remarks> <list type="table">
/// <item> <term><b>Field</b></term> <description><b>Byte Length</b></description> </item>
/// <item> <term><see cref="Length"/></term> <description>1</description> </item>
/// <item> <term><see cref="Value"/></term> <description>1 * <see cref="Length"/></description> </item>
/// <item> <term> --- </term> <description>1 + <see cref="Length"/></description> </item>
/// </list> </remarks>
public sealed partial record ArrayByteMemberLength : IWritable, ISpanReadable<ArrayByteMemberLength>
{
    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetByteCount() => 1 + this.Length;

    /// <inheritdoc />
    public bool TryWriteLittleEndian(global::System.Span<byte> destination) => TryWriteLittleEndian(destination, out _);

    /// <inheritdoc />
    public bool TryWriteLittleEndian(global::System.Span<byte> destination, out int bytesWritten)
    {
        bytesWritten = 0;

        if (destination.Length < 1)
            return false;
        BinaryHelpers.WriteUInt8(destination[0..], this.Length);
        bytesWritten += 1;
        if (destination.Length < bytesWritten + this.Length)
            return false;
        BinaryHelpers.WriteUInt8Span(destination[1..], this.Value, this.Length);
        bytesWritten += this.Length;

        return true;
    }

    /// <inheritdoc />
    public bool TryWriteBigEndian(global::System.Span<byte> destination) => TryWriteBigEndian(destination, out _);

    /// <inheritdoc />
    public bool TryWriteBigEndian(global::System.Span<byte> destination, out int bytesWritten)
    {
        bytesWritten = 0;

        if (destination.Length < 1)
            return false;
        BinaryHelpers.WriteUInt8(destination[0..], this.Length);
        bytesWritten += 1;
        if (destination.Length < bytesWritten + this.Length)
            return false;
        BinaryHelpers.WriteUInt8Span(destination[1..], this.Value, this.Length);
        bytesWritten += this.Length;

        return true;
    }

    /// <inheritdoc />
    public static bool TryReadLittleEndian(
        global::System.ReadOnlySpan<byte> source,
        [NotNullWhen(true)] out ArrayByteMemberLength? value
    ) => TryReadLittleEndian(source, out value, out _);

    /// <inheritdoc />
    public static bool TryReadLittleEndian(
        global::System.ReadOnlySpan<byte> source,
        [NotNullWhen(true)] out ArrayByteMemberLength? value,
        out int bytesRead
    )
    {
        bytesRead = 0;
        value = default;

        if (source.Length < 1)
            return false;
        var ___readLength = BinaryHelpers.ReadUInt8(source[0..]);
        bytesRead += 1;
        if (source.Length < bytesRead + ___readLength)
            return false;
        var ___readValue = BinaryHelpers.ReadUInt8Array(source.Slice(1, ___readLength));
        bytesRead += ___readLength;

        value = new ArrayByteMemberLength(___readLength, ___readValue);
        return true;
    }

    /// <inheritdoc />
    public static bool TryReadBigEndian(
        global::System.ReadOnlySpan<byte> source,
        [NotNullWhen(true)] out ArrayByteMemberLength? value
    ) => TryReadBigEndian(source, out value, out _);

    /// <inheritdoc />
    public static bool TryReadBigEndian(
        global::System.ReadOnlySpan<byte> source,
        [NotNullWhen(true)] out ArrayByteMemberLength? value,
        out int bytesRead
    )
    {
        bytesRead = 0;
        value = default;

        if (source.Length < 1)
            return false;
        var ___readLength = BinaryHelpers.ReadUInt8(source[0..]);
        bytesRead += 1;
        if (source.Length < bytesRead + ___readLength)
            return false;
        var ___readValue = BinaryHelpers.ReadUInt8Array(source.Slice(1, ___readLength));
        bytesRead += ___readLength;

        value = new ArrayByteMemberLength(___readLength, ___readValue);
        return true;
    }
}

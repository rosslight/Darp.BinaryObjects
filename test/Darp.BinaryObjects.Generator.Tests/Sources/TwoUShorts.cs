namespace Darp.BinaryObjects.Generator.Tests.Sources;

using System.Buffers.Binary;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[BinaryObject]
public sealed partial record TwoUShorts(ushort Value, ushort ValueTwo);

/// <remarks> <list type="table">
/// <item> <term><b>Field</b></term> <description><b>Byte Length</b></description> </item>
/// <item> <term><see cref="Value"/></term> <description>2</description> </item>
/// <item> <term> --- </term> <description>2</description> </item>
/// </list> </remarks>
public sealed partial record TwoUShorts : IWritable, ISpanReadable<Sources.TwoUShorts>
{
    /// <inheritdoc />
    [global::System.Runtime.CompilerServices.MethodImpl(
        global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining
    )]
    public int GetByteCount() => 4;

    private bool TryWrite(global::System.Span<byte> destination, out int bytesWritten, bool writeLittleEndian)
    {
        if (destination.Length < GetByteCount())
        {
            bytesWritten = 0;
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
        bytesWritten = 4;
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
        [global::System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out Sources.TwoUShorts? value,
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
        var readValue = MemoryMarshal.Read<ushort>(source);
        var readValueTwo = MemoryMarshal.Read<ushort>(source[2..]);
        if (BitConverter.IsLittleEndian != readLittleEndian)
        {
            readValue = BinaryPrimitives.ReverseEndianness(readValue);
            readValueTwo = BinaryPrimitives.ReverseEndianness(readValueTwo);
        }
        value = new Sources.TwoUShorts(readValue, readValueTwo);
        bytesRead = 4;
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
        out int bytesRead
    ) => TryRead(source, out value, out bytesRead, true);

    /// <inheritdoc />
    public static bool TryReadBigEndian(ReadOnlySpan<byte> source, [NotNullWhen(true)] out Sources.TwoUShorts? value) =>
        TryRead(source, out value, out _, false);

    /// <inheritdoc />
    public static bool TryReadBigEndian(
        ReadOnlySpan<byte> source,
        [NotNullWhen(true)] out Sources.TwoUShorts? value,
        out int bytesRead
    ) => TryRead(source, out value, out bytesRead, false);
}

namespace Darp.BinaryObjects.Generator.Tests.Sources;

using BinaryHelpers = global::Darp.BinaryObjects.BinaryHelpers;
using NotNullWhenAttribute = global::System.Diagnostics.CodeAnalysis.NotNullWhenAttribute;

public partial record ArraysFixedSize(
    [property: BinaryLength(2)] ReadOnlyMemory<byte> ValueByteMemory,
    [property: BinaryElementCount(2)] byte[] ValueByteArray,
    [property: BinaryElementCount(2)] List<byte> ValueByteList,
    [property: BinaryElementCount(2)] IEnumerable<byte> ValueByteEnumerable,
    [property: BinaryLength(4)] ReadOnlyMemory<ushort> ValueUShortMemory,
    [property: BinaryElementCount(2)] ushort[] ValueUShortArray,
    [property: BinaryElementCount(2)] List<ushort> ValueUShortList,
    [property: BinaryElementCount(2)] IEnumerable<ushort> ValueUShortEnumerable
);

/// <remarks> <list type="table">
/// <item> <term><b>Field</b></term> <description><b>Byte Length</b></description> </item>
/// <item> <term><see cref="ValueByteMemory"/></term> <description>1 * 2</description> </item>
/// <item> <term><see cref="ValueByteArray"/></term> <description>1 * 2</description> </item>
/// <item> <term><see cref="ValueByteList"/></term> <description>1 * 2</description> </item>
/// <item> <term><see cref="ValueByteEnumerable"/></term> <description>1 * 2</description> </item>
/// <item> <term><see cref="ValueUShortMemory"/></term> <description>2 * 2</description> </item>
/// <item> <term><see cref="ValueUShortArray"/></term> <description>2 * 2</description> </item>
/// <item> <term><see cref="ValueUShortList"/></term> <description>2 * 2</description> </item>
/// <item> <term><see cref="ValueUShortEnumerable"/></term> <description>2 * 2</description> </item>
/// <item> <term> --- </term> <description>24</description> </item>
/// </list> </remarks>
public sealed partial record ArraysFixedSize
    : global::Darp.BinaryObjects.IWritable,
        global::Darp.BinaryObjects.ISpanReadable<ArraysFixedSize>
{
    /// <inheritdoc />
    [global::System.Runtime.CompilerServices.MethodImpl(
        global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining
    )]
    public int GetByteCount() => 24;

    /// <inheritdoc />
    public bool TryWriteLittleEndian(global::System.Span<byte> destination) => TryWriteLittleEndian(destination, out _);

    /// <inheritdoc />
    public bool TryWriteLittleEndian(global::System.Span<byte> destination, out int bytesWritten)
    {
        bytesWritten = 0;

        if (destination.Length < 24)
            return false;
        var x = new byte[3, 4];
        BinaryHelpers.WriteUInt8Span(destination[0..], this.ValueByteMemory.Span, 2);
        BinaryHelpers.WriteUInt8Span(destination[2..], this.ValueByteArray, 2);
        BinaryHelpers.WriteUInt8List(destination[4..], this.ValueByteList, 2);
        BinaryHelpers.WriteUInt8Enumerable(destination[6..], this.ValueByteEnumerable, 2);
        BinaryHelpers.WriteUInt16SpanLittleEndian(destination[8..], this.ValueUShortMemory.Span, 2);
        BinaryHelpers.WriteUInt16SpanLittleEndian(destination[12..], this.ValueUShortArray, 2);
        BinaryHelpers.WriteUInt16ListLittleEndian(destination[16..], this.ValueUShortList, 2);
        BinaryHelpers.WriteUInt16EnumerableLittleEndian(destination[20..], this.ValueUShortEnumerable, 2);
        bytesWritten += 24;

        return true;
    }

    /// <inheritdoc />
    public bool TryWriteBigEndian(global::System.Span<byte> destination) => TryWriteBigEndian(destination, out _);

    /// <inheritdoc />
    public bool TryWriteBigEndian(global::System.Span<byte> destination, out int bytesWritten)
    {
        bytesWritten = 0;

        if (destination.Length < 24)
            return false;
        BinaryHelpers.WriteUInt8Span(destination[0..], this.ValueByteMemory.Span, 2);
        BinaryHelpers.WriteUInt8Span(destination[2..], this.ValueByteArray, 2);
        BinaryHelpers.WriteUInt8List(destination[4..], this.ValueByteList, 2);
        BinaryHelpers.WriteUInt8Enumerable(destination[6..], this.ValueByteEnumerable, 2);
        BinaryHelpers.WriteUInt16SpanBigEndian(destination[8..], this.ValueUShortMemory.Span, 2);
        BinaryHelpers.WriteUInt16SpanBigEndian(destination[12..], this.ValueUShortArray, 2);
        BinaryHelpers.WriteUInt16ListBigEndian(destination[16..], this.ValueUShortList, 2);
        BinaryHelpers.WriteUInt16EnumerableBigEndian(destination[20..], this.ValueUShortEnumerable, 2);
        bytesWritten += 24;

        return true;
    }

    /// <inheritdoc />
    public static bool TryReadLittleEndian(
        global::System.ReadOnlySpan<byte> source,
        [NotNullWhen(true)] out ArraysFixedSize? value
    ) => TryReadLittleEndian(source, out value, out _);

    /// <inheritdoc />
    public static bool TryReadLittleEndian(
        global::System.ReadOnlySpan<byte> source,
        [NotNullWhen(true)] out ArraysFixedSize? value,
        out int bytesRead
    )
    {
        bytesRead = 0;
        value = default;

        if (source.Length < 24)
            return false;
        var __readValueByteMemory = BinaryHelpers.ReadUInt8Array(source[0..2]);
        var __readValueByteArray = BinaryHelpers.ReadUInt8Array(source[2..4]);
        var __readValueByteList = BinaryHelpers.ReadUInt8List(source[4..6]);
        var __readValueByteEnumerable = BinaryHelpers.ReadUInt8Array(source[6..8]);
        var __readValueUShortMemory = BinaryHelpers.ReadUInt16ArrayLittleEndian(source[8..12]);
        var __readValueUShortArray = BinaryHelpers.ReadUInt16ArrayLittleEndian(source[12..16]);
        var __readValueUShortList = BinaryHelpers.ReadUInt16ListLittleEndian(source[16..20]);
        var __readValueUShortEnumerable = BinaryHelpers.ReadUInt16ArrayLittleEndian(source[20..24]);
        bytesRead += 24;

        value = new ArraysFixedSize(
            __readValueByteMemory,
            __readValueByteArray,
            __readValueByteList,
            __readValueByteEnumerable,
            __readValueUShortMemory,
            __readValueUShortArray,
            __readValueUShortList,
            __readValueUShortEnumerable
        );
        return true;
    }

    /// <inheritdoc />
    public static bool TryReadBigEndian(
        global::System.ReadOnlySpan<byte> source,
        [NotNullWhen(true)] out ArraysFixedSize? value
    ) => TryReadBigEndian(source, out value, out _);

    /// <inheritdoc />
    public static bool TryReadBigEndian(
        global::System.ReadOnlySpan<byte> source,
        [NotNullWhen(true)] out ArraysFixedSize? value,
        out int bytesRead
    )
    {
        bytesRead = 0;
        value = default;

        if (source.Length < 24)
            return false;
        var __readValueByteMemory = BinaryHelpers.ReadUInt8Array(source[0..2]);
        var __readValueByteArray = BinaryHelpers.ReadUInt8Array(source[2..4]);
        var __readValueByteList = BinaryHelpers.ReadUInt8List(source[4..6]);
        var __readValueByteEnumerable = BinaryHelpers.ReadUInt8Array(source[6..8]);
        var __readValueUShortMemory = BinaryHelpers.ReadUInt16ArrayBigEndian(source[8..12]);
        var __readValueUShortArray = BinaryHelpers.ReadUInt16ArrayBigEndian(source[12..16]);
        var __readValueUShortList = BinaryHelpers.ReadUInt16ListBigEndian(source[16..20]);
        var __readValueUShortEnumerable = BinaryHelpers.ReadUInt16ArrayBigEndian(source[20..24]);
        bytesRead += 24;

        value = new ArraysFixedSize(
            __readValueByteMemory,
            __readValueByteArray,
            __readValueByteList,
            __readValueByteEnumerable,
            __readValueUShortMemory,
            __readValueUShortArray,
            __readValueUShortList,
            __readValueUShortEnumerable
        );
        return true;
    }
}

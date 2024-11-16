namespace Darp.BinaryObjects.Tests.Generated;

using System.Buffers.Binary;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using FluentAssertions;

public sealed partial record ListUShortRemainingLength(
    [property: BinaryReadRemaining(MinElements = 2)] IReadOnlyList<ushort> Value
);

public class ListUShortRemainingLengthTests
{
    [Theory]
    [InlineData("01000001", 4, 0x0001, 0x0100)]
    [InlineData("010001000100", 6, 0x0001, 0x0001, 0x0001)]
    [InlineData("FFAAFFAA", 4, 0xAAFF, 0xAAFF)]
    [InlineData("FFFFFFFF00", 4, 0xFFFF, 0xFFFF)]
    public void TryRead_GoodInputShouldBeValid(string hexString, int expectedBytesRead, params int[] expectedValue)
    {
        var buffer = Convert.FromHexString(hexString);
        var expectedValueBE = new ushort[expectedValue.Length];
        BinaryPrimitives.ReverseEndianness(expectedValue.Select(x => (ushort)x).ToArray(), expectedValueBE);

        var successLE1 = ListUShortRemainingLength.TryReadLittleEndian(buffer, out ListUShortRemainingLength? valueLE1);
        var successLE2 = ListUShortRemainingLength.TryReadLittleEndian(
            buffer,
            out ListUShortRemainingLength? valueLE2,
            out var consumedLE
        );
        var successBE1 = ListUShortRemainingLength.TryReadBigEndian(buffer, out ListUShortRemainingLength? valueBE1);
        var successBE2 = ListUShortRemainingLength.TryReadBigEndian(
            buffer,
            out ListUShortRemainingLength? valueBE2,
            out var consumedBE
        );

        successLE1.Should().BeTrue();
        successLE2.Should().BeTrue();
        successBE1.Should().BeTrue();
        successBE2.Should().BeTrue();
        valueLE1!.Value.Should().BeEquivalentTo(expectedValue);
        valueLE1.GetByteCount().Should().Be(expectedBytesRead);
        valueLE2!.Value.Should().BeEquivalentTo(expectedValue);
        valueLE2.GetByteCount().Should().Be(expectedBytesRead);
        valueBE1!.Value.Should().BeEquivalentTo(expectedValueBE);
        valueBE1.GetByteCount().Should().Be(expectedBytesRead);
        valueBE2!.Value.Should().BeEquivalentTo(expectedValueBE);
        valueBE2.GetByteCount().Should().Be(expectedBytesRead);
        consumedLE.Should().Be(expectedBytesRead);
        consumedBE.Should().Be(expectedBytesRead);
    }

    [Theory]
    [InlineData("")]
    [InlineData("00")]
    [InlineData("000000")]
    public void TryRead_BadInputShouldBeValid(string hexString)
    {
        var buffer = Convert.FromHexString(hexString);

        var successLE1 = ListUShortRemainingLength.TryReadLittleEndian(buffer, out ListUShortRemainingLength? valueLE1);
        var successLE2 = ListUShortRemainingLength.TryReadLittleEndian(
            buffer,
            out ListUShortRemainingLength? valueLE2,
            out var consumedLE
        );
        var successBE1 = ListUShortRemainingLength.TryReadBigEndian(buffer, out ListUShortRemainingLength? valueBE1);
        var successBE2 = ListUShortRemainingLength.TryReadBigEndian(
            buffer,
            out ListUShortRemainingLength? valueBE2,
            out var consumedBE
        );

        successLE1.Should().BeFalse();
        successLE2.Should().BeFalse();
        successBE1.Should().BeFalse();
        successBE2.Should().BeFalse();
        valueLE1.Should().BeNull();
        valueLE2.Should().BeNull();
        valueBE1.Should().BeNull();
        valueBE2.Should().BeNull();
        consumedLE.Should().Be(0);
        consumedBE.Should().Be(0);
    }

    [Theory]
    [InlineData("00000000", 4, 4, 4, "00000000", "00000000")]
    [InlineData("01000100", 4, 4, 4, "01000100", "00010001")]
    [InlineData("00100100", 4, 4, 4, "00100100", "10000001")]
    [InlineData("FFFFAAAA", 4, 4, 4, "FFFFAAAA", "FFFFAAAA")]
    [InlineData("FFFFFFFFFFFF", 7, 6, 6, "FFFFFFFFFFFF00", "FFFFFFFFFFFF00")]
    [InlineData("1234", 4, 4, 2, "12340000", "34120000")]
    public void TryWrite_GoodInputShouldBeValid(
        string valueHexString,
        int bufferSize,
        int expectedWriteCount,
        int expectedBytesWritten,
        string expectedBufferHexStringLE,
        string expectedBufferHexStringBE
    )
    {
        var value = MemoryMarshal.Cast<byte, ushort>(Convert.FromHexString(valueHexString)).ToArray();
        var bufferLE = new byte[bufferSize];
        var bufferBE = new byte[bufferSize];
        var expectedValueLE = Convert.FromHexString(expectedBufferHexStringLE);
        var expectedValueBE = Convert.FromHexString(expectedBufferHexStringBE);

        var writable = new ListUShortRemainingLength(value);

        var successLE1 = writable.TryWriteLittleEndian(bufferLE);
        var successLE2 = writable.TryWriteLittleEndian(bufferLE, out var writtenLE);
        var successBE1 = writable.TryWriteBigEndian(bufferBE);
        var successBE2 = writable.TryWriteBigEndian(bufferBE, out var writtenBE);

        successLE1.Should().BeTrue();
        successLE2.Should().BeTrue();
        successBE1.Should().BeTrue();
        successBE2.Should().BeTrue();
        bufferLE.Should().BeEquivalentTo(expectedValueLE);
        bufferBE.Should().BeEquivalentTo(expectedValueBE);
        writtenLE.Should().Be(expectedBytesWritten);
        writtenBE.Should().Be(expectedBytesWritten);
        writable.GetByteCount().Should().Be(expectedWriteCount);
    }

    [Theory]
    [InlineData("", 0, "")]
    [InlineData("01000004", 3, "000000")]
    [InlineData("0100000400000008", 7, "00000000000000")]
    public void TryWrite_BadInputShouldBeValid(string valueHexString, int bufferSize, string expectedHexString)
    {
        var value = MemoryMarshal.Cast<byte, ushort>(Convert.FromHexString(valueHexString)).ToArray();
        var bufferLE = new byte[bufferSize];
        var bufferBE = new byte[bufferSize];
        var expectedHexBytes = Convert.FromHexString(expectedHexString);
        var writable = new ListUShortRemainingLength(value);

        var successLE1 = writable.TryWriteLittleEndian(bufferLE);
        var successLE2 = writable.TryWriteLittleEndian(bufferLE, out var writtenLE);
        var successBE1 = writable.TryWriteBigEndian(bufferBE);
        var successBE2 = writable.TryWriteBigEndian(bufferBE, out var writtenBE);

        successLE1.Should().BeFalse();
        successLE2.Should().BeFalse();
        successBE1.Should().BeFalse();
        successBE2.Should().BeFalse();
        bufferLE.Should().BeEquivalentTo(expectedHexBytes);
        bufferBE.Should().BeEquivalentTo(expectedHexBytes);
        writtenLE.Should().Be(0);
        writtenBE.Should().Be(0);
    }
}

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

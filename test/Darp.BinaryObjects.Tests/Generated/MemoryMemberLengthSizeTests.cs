namespace Darp.BinaryObjects.Tests.Generated;

using System.Buffers.Binary;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using FluentAssertions;

public sealed partial record MemoryMemberLengthSize(
    [property: BinaryLength(2)] int Length,
    [property: BinaryElementCount("Length")] ReadOnlyMemory<byte> Value
);

public class MemoryMemberLengthSizeTests
{
    [Theory]
    [InlineData("0000", "0000", 0)]
    [InlineData("010004", "000104", 1, 0x04)]
    [InlineData("0300010203", "0003010203", 3, 0x01, 0x02, 0x03)]
    [InlineData("0100FF", "0001FF", 1, 0xFF)]
    public void TryRead_GoodInputShouldBeValid(
        string hexStringLE,
        string hexStringBE,
        int expectedLength,
        params int[] expectedValue
    )
    {
        var bufferLE = Convert.FromHexString(hexStringLE);
        var bufferBE = Convert.FromHexString(hexStringBE);

        var successLE1 = MemoryMemberLengthSize.TryReadLittleEndian(bufferLE, out MemoryMemberLengthSize? valueLE1);
        var successLE2 = MemoryMemberLengthSize.TryReadLittleEndian(
            bufferLE,
            out MemoryMemberLengthSize? valueLE2,
            out var consumedLE
        );
        var successBE1 = MemoryMemberLengthSize.TryReadBigEndian(bufferBE, out MemoryMemberLengthSize? valueBE1);
        var successBE2 = MemoryMemberLengthSize.TryReadBigEndian(
            bufferBE,
            out MemoryMemberLengthSize? valueBE2,
            out var consumedBE
        );

        successLE1.Should().BeTrue();
        successLE2.Should().BeTrue();
        successBE1.Should().BeTrue();
        successBE2.Should().BeTrue();
        valueLE1!.Value.ToArray().Should().BeEquivalentTo(expectedValue);
        valueLE2!.Value.ToArray().Should().BeEquivalentTo(expectedValue);
        valueBE1!.Value.ToArray().Should().BeEquivalentTo(expectedValue);
        valueBE2!.Value.ToArray().Should().BeEquivalentTo(expectedValue);
        consumedLE.Should().Be(2 + expectedLength);
        consumedBE.Should().Be(2 + expectedLength);
    }

    [Theory]
    [InlineData("")]
    [InlineData("00")]
    [InlineData("0100")]
    public void TryRead_BadInputShouldBeValid(string hexString)
    {
        var buffer = Convert.FromHexString(hexString);

        var successLE1 = MemoryMemberLengthSize.TryReadLittleEndian(buffer, out MemoryMemberLengthSize? valueLE1);
        var successLE2 = MemoryMemberLengthSize.TryReadLittleEndian(
            buffer,
            out MemoryMemberLengthSize? valueLE2,
            out var consumedLE
        );
        var successBE1 = MemoryMemberLengthSize.TryReadBigEndian(buffer, out MemoryMemberLengthSize? valueBE1);
        var successBE2 = MemoryMemberLengthSize.TryReadBigEndian(
            buffer,
            out MemoryMemberLengthSize? valueBE2,
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
    [InlineData(1, "", 3, 2, "010000", "000100")]
    [InlineData(1, "01", 3, 3, "010001", "000101")]
    [InlineData(3, "030201", 5, 5, "0300030201", "0003030201")]
    [InlineData(3, "0102030405", 5, 5, "0300010203", "0003010203")]
    [InlineData(3, "01020304", 6, 5, "030001020300", "000301020300")]
    public void TryWrite_GoodInputShouldBeValid(
        int length,
        string valueHexString,
        int bufferSize,
        int expectedBytesWritten,
        string expectedHexStringLE,
        string expectedHexStringBE
    )
    {
        var bufferLE = new byte[bufferSize];
        var bufferBE = new byte[bufferSize];
        var expectedHexBytesLE = Convert.FromHexString(expectedHexStringLE);
        var expectedHexBytesBE = Convert.FromHexString(expectedHexStringBE);
        var writable = new MemoryMemberLengthSize(length, Convert.FromHexString(valueHexString));

        var successLE1 = writable.TryWriteLittleEndian(bufferLE);
        var successLE2 = writable.TryWriteLittleEndian(bufferLE, out var writtenLE);
        var successBE1 = writable.TryWriteBigEndian(bufferBE);
        var successBE2 = writable.TryWriteBigEndian(bufferBE, out var writtenBE);

        successLE1.Should().BeTrue();
        successLE2.Should().BeTrue();
        successBE1.Should().BeTrue();
        successBE2.Should().BeTrue();
        bufferLE.Should().BeEquivalentTo(expectedHexBytesLE);
        bufferBE.Should().BeEquivalentTo(expectedHexBytesBE);
        writtenLE.Should().Be(expectedBytesWritten);
        writtenBE.Should().Be(expectedBytesWritten);
        writable.GetByteCount().Should().Be(2 + length);
    }

    [Theory]
    [InlineData(1, "", 0, "")]
    [InlineData(1, "", 2, "0000")]
    [InlineData(1, "00", 1, "00")]
    [InlineData(3, "0102", 3, "000000")]
    public void TryWrite_BadInputShouldBeValid(
        int length,
        string valueHexString,
        int bufferSize,
        string expectedHexString
    )
    {
        var bufferLE = new byte[bufferSize];
        var bufferBE = new byte[bufferSize];
        var expectedHexBytes = Convert.FromHexString(expectedHexString);
        var writable = new MemoryMemberLengthSize(length, Convert.FromHexString(valueHexString));

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

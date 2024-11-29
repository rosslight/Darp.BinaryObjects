namespace Darp.BinaryObjects.Tests.Generated;

using FluentAssertions;

[BinaryObject]
public sealed partial record MemoryMemberLengthSize(
    [property: BinaryLength(2)] ushort Length,
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
    [InlineData("", 0)]
    [InlineData("00", 0)]
    [InlineData("0100", 2)]
    public void TryRead_BadInputShouldBeValid(string hexString, int expectedBytesRead)
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
        consumedLE.Should().Be(expectedBytesRead);
        consumedBE.Should().Be(expectedBytesRead);
    }

    [Theory]
    [InlineData(1, "", 3, 3, "010000", "000100")]
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
        var writable = new MemoryMemberLengthSize((ushort)length, Convert.FromHexString(valueHexString));

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
    [InlineData(1, "", 0, "", "", 0)]
    [InlineData(1, "", 2, "0100", "0001", 2)]
    [InlineData(1, "00", 1, "00", "00", 0)]
    [InlineData(3, "0102", 3, "030000", "000300", 2)]
    public void TryWrite_BadInputShouldBeValid(
        int length,
        string valueHexString,
        int bufferSize,
        string expectedLEHexString,
        string expectedBEHexString,
        int expectedBytesWritten
    )
    {
        var bufferLE = new byte[bufferSize];
        var bufferBE = new byte[bufferSize];
        var expectedLEHexBytes = Convert.FromHexString(expectedLEHexString);
        var expectedBEHexBytes = Convert.FromHexString(expectedBEHexString);
        var writable = new MemoryMemberLengthSize((ushort)length, Convert.FromHexString(valueHexString));

        var successLE1 = writable.TryWriteLittleEndian(bufferLE);
        var successLE2 = writable.TryWriteLittleEndian(bufferLE, out var writtenLE);
        var successBE1 = writable.TryWriteBigEndian(bufferBE);
        var successBE2 = writable.TryWriteBigEndian(bufferBE, out var writtenBE);

        successLE1.Should().BeFalse();
        successLE2.Should().BeFalse();
        successBE1.Should().BeFalse();
        successBE2.Should().BeFalse();
        bufferLE.Should().BeEquivalentTo(expectedLEHexBytes);
        bufferBE.Should().BeEquivalentTo(expectedBEHexBytes);
        writtenLE.Should().Be(expectedBytesWritten);
        writtenBE.Should().Be(expectedBytesWritten);
    }
}

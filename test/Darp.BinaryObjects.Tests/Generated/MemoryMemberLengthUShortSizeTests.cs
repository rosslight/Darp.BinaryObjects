namespace Darp.BinaryObjects.Tests.Generated;

using FluentAssertions;

[BinaryObject]
public sealed partial record MemoryMemberLengthUShortSize(
    [property: BinaryLength(2)] ushort Length,
    [property: BinaryElementCount("Length")] ReadOnlyMemory<ushort> Value
);

public class MemoryMemberLengthUShortSizeTests
{
    [Theory]
    [InlineData("0000", "0000", 0)]
    [InlineData("01000400", "00010004", 1, 0x04)]
    [InlineData("0300010002000300", "0003000100020003", 3, 0x01, 0x02, 0x03)]
    [InlineData("0100FF00", "000100FF", 1, 0xFF)]
    public void TryRead_GoodInputShouldBeValid(
        string hexStringLE,
        string hexStringBE,
        int expectedLength,
        params int[] expectedValue
    )
    {
        var bufferLE = Convert.FromHexString(hexStringLE);
        var bufferBE = Convert.FromHexString(hexStringBE);

        var successLE1 = MemoryMemberLengthUShortSize.TryReadLittleEndian(
            bufferLE,
            out MemoryMemberLengthUShortSize? valueLE1
        );
        var successLE2 = MemoryMemberLengthUShortSize.TryReadLittleEndian(
            bufferLE,
            out MemoryMemberLengthUShortSize? valueLE2,
            out var consumedLE
        );
        var successBE1 = MemoryMemberLengthUShortSize.TryReadBigEndian(
            bufferBE,
            out MemoryMemberLengthUShortSize? valueBE1
        );
        var successBE2 = MemoryMemberLengthUShortSize.TryReadBigEndian(
            bufferBE,
            out MemoryMemberLengthUShortSize? valueBE2,
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
        consumedLE.Should().Be(2 + (2 * expectedLength));
        consumedBE.Should().Be(2 + (2 * expectedLength));
    }

    [Theory]
    [InlineData("", 0)]
    [InlineData("00", 0)]
    [InlineData("0100", 2)]
    public void TryRead_BadInputShouldBeValid(string hexString, int expectedBytesRead)
    {
        var buffer = Convert.FromHexString(hexString);

        var successLE1 = MemoryMemberLengthUShortSize.TryReadLittleEndian(
            buffer,
            out MemoryMemberLengthUShortSize? valueLE1
        );
        var successLE2 = MemoryMemberLengthUShortSize.TryReadLittleEndian(
            buffer,
            out MemoryMemberLengthUShortSize? valueLE2,
            out var consumedLE
        );
        var successBE1 = MemoryMemberLengthUShortSize.TryReadBigEndian(
            buffer,
            out MemoryMemberLengthUShortSize? valueBE1
        );
        var successBE2 = MemoryMemberLengthUShortSize.TryReadBigEndian(
            buffer,
            out MemoryMemberLengthUShortSize? valueBE2,
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
    [InlineData(0, new ushort[] { }, 2, 2, "0000", "0000")]
    [InlineData(1, new ushort[] { 0x01 }, 4, 4, "01000100", "00010001")]
    [InlineData(3, new ushort[] { 0x03, 0x02, 0x01 }, 8, 8, "0300030002000100", "0003000300020001")]
    [InlineData(3, new ushort[] { 0x01, 0x02, 0x03, 0x04, 0x05 }, 8, 8, "0300010002000300", "0003000100020003")]
    [InlineData(3, new ushort[] { 0x01, 0x02, 0x03, 0x04 }, 9, 8, "030001000200030000", "000300010002000300")]
    public void TryWrite_GoodInputShouldBeValid(
        int length,
        ushort[] values,
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
        var writable = new MemoryMemberLengthUShortSize((ushort)length, values);

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
        writable.GetByteCount().Should().Be(expectedBytesWritten);
    }

    [Theory]
    [InlineData(1, new ushort[] { }, 0, "", "", 0)]
    [InlineData(1, new ushort[] { }, 2, "0100", "0001", 2)]
    [InlineData(1, new ushort[] { 0x00 }, 1, "00", "00", 0)]
    [InlineData(3, new ushort[] { 0x01, 0x02 }, 3, "030000", "000300", 2)]
    public void TryWrite_BadInputShouldBeValid(
        int length,
        ushort[] values,
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
        var writable = new MemoryMemberLengthUShortSize((ushort)length, values);

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

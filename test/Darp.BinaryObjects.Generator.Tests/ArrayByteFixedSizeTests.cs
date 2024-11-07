namespace Darp.BinaryObjects.Generator.Tests;

using FluentAssertions;
using Sources;

public class ArrayByteFixedSizeTests
{
    [Theory]
    [InlineData("0000", 0x00, 0x00)]
    [InlineData("0103", 0x01, 0x03)]
    [InlineData("FFFF", 0xFF, 0xFF)]
    [InlineData("000100", 0x00, 0x01)]
    public void TryRead_GoodInputShouldBeValid(string hexString, params int[] expectedValue)
    {
        var buffer = Convert.FromHexString(hexString);

        var successLE1 = ArrayByteFixedSize.TryReadLittleEndian(buffer, out ArrayByteFixedSize? valueLE1);
        var successLE2 = ArrayByteFixedSize.TryReadLittleEndian(
            buffer,
            out ArrayByteFixedSize? valueLE2,
            out var consumedLE
        );
        var successBE1 = ArrayByteFixedSize.TryReadBigEndian(buffer, out ArrayByteFixedSize? valueBE1);
        var successBE2 = ArrayByteFixedSize.TryReadBigEndian(
            buffer,
            out ArrayByteFixedSize? valueBE2,
            out var consumedBE
        );

        successLE1.Should().BeTrue();
        successLE2.Should().BeTrue();
        successBE1.Should().BeTrue();
        successBE2.Should().BeTrue();
        valueLE1!.Value.Should().BeEquivalentTo(expectedValue);
        valueLE2!.Value.Should().BeEquivalentTo(expectedValue);
        valueBE1!.Value.Should().BeEquivalentTo(expectedValue);
        valueBE2!.Value.Should().BeEquivalentTo(expectedValue);
        consumedLE.Should().Be(2);
        consumedBE.Should().Be(2);
    }

    [Theory]
    [InlineData("")]
    public void TryRead_BadInputShouldBeValid(string hexString)
    {
        var buffer = Convert.FromHexString(hexString);

        var successLE1 = ArrayByteFixedSize.TryReadLittleEndian(buffer, out ArrayByteFixedSize? valueLE1);
        var successLE2 = ArrayByteFixedSize.TryReadLittleEndian(
            buffer,
            out ArrayByteFixedSize? valueLE2,
            out var consumedLE
        );
        var successBE1 = ArrayByteFixedSize.TryReadBigEndian(buffer, out ArrayByteFixedSize? valueBE1);
        var successBE2 = ArrayByteFixedSize.TryReadBigEndian(
            buffer,
            out ArrayByteFixedSize? valueBE2,
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
    [InlineData("", 2, "0000")]
    [InlineData("01", 2, "0100")]
    [InlineData("0000", 2, "0000")]
    [InlineData("0103", 2, "0103")]
    [InlineData("100101", 2, "1001")]
    [InlineData("100102", 3, "100100")]
    public void TryWrite_GoodInputShouldBeValid(string valueHexString, int bufferSize, string expectedHexString)
    {
        var bufferLE = new byte[bufferSize];
        var bufferBE = new byte[bufferSize];
        var expectedHexBytes = Convert.FromHexString(expectedHexString);
        var writable = new ArrayByteFixedSize(Convert.FromHexString(valueHexString));

        var successLE = writable.TryWriteLittleEndian(bufferLE);
        var successBE = writable.TryWriteBigEndian(bufferBE);

        successLE.Should().BeTrue();
        successBE.Should().BeTrue();
        bufferLE.Should().BeEquivalentTo(expectedHexBytes);
        bufferBE.Should().BeEquivalentTo(expectedHexBytes);
        writable.GetWriteSize().Should().Be(2);
    }

    [Theory]
    [InlineData("", 0, "")]
    [InlineData("00", 1, "00")]
    [InlineData("0102", 1, "00")]
    public void TryWrite_BadInputShouldBeValid(string valueHexString, int bufferSize, string expectedHexString)
    {
        var bufferLE = new byte[bufferSize];
        var bufferBE = new byte[bufferSize];
        var expectedHexBytes = Convert.FromHexString(expectedHexString);
        var writable = new ArrayByteFixedSize(Convert.FromHexString(valueHexString));

        var successLE = writable.TryWriteLittleEndian(bufferLE);
        var successBE = writable.TryWriteBigEndian(bufferBE);

        successLE.Should().BeFalse();
        successBE.Should().BeFalse();
        bufferLE.Should().BeEquivalentTo(expectedHexBytes);
        bufferBE.Should().BeEquivalentTo(expectedHexBytes);
    }
}

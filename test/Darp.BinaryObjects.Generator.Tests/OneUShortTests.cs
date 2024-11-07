namespace Darp.BinaryObjects.Generator.Tests;

using FluentAssertions;
using Sources;

public class OneUShortTests
{
    [Theory]
    [InlineData("0000", 0x0000, 0x0000)]
    [InlineData("0100", 0x0001, 0x0100)]
    [InlineData("0010", 0x1000, 0x0010)]
    [InlineData("FFFF", 0xFFFF, 0xFFFF)]
    [InlineData("FFFF00", 0xFFFF, 0xFFFF)]
    public void TryRead_GoodInputShouldBeValid(string hexString, ushort expectedValueLE, ushort expectedValueBE)
    {
        var buffer = Convert.FromHexString(hexString);

        var successLE1 = OneUShort.TryReadLittleEndian(buffer, out OneUShort? valueLE1);
        var successLE2 = OneUShort.TryReadLittleEndian(buffer, out OneUShort? valueLE2, out var consumedLE);
        var successBE1 = OneUShort.TryReadBigEndian(buffer, out OneUShort? valueBE1);
        var successBE2 = OneUShort.TryReadBigEndian(buffer, out OneUShort? valueBE2, out var consumedBE);

        successLE1.Should().BeTrue();
        successLE2.Should().BeTrue();
        successBE1.Should().BeTrue();
        successBE2.Should().BeTrue();
        valueLE1!.Value.Should().Be(expectedValueLE);
        valueLE2!.Value.Should().Be(expectedValueLE);
        valueBE1!.Value.Should().Be(expectedValueBE);
        valueBE2!.Value.Should().Be(expectedValueBE);
        consumedLE.Should().Be(2);
        consumedBE.Should().Be(2);
    }

    [Theory]
    [InlineData("")]
    [InlineData("00")]
    public void TryRead_BadInputShouldBeValid(string hexString)
    {
        var buffer = Convert.FromHexString(hexString);

        var successLE1 = OneUShort.TryReadLittleEndian(buffer, out OneUShort? valueLE1);
        var successLE2 = OneUShort.TryReadLittleEndian(buffer, out OneUShort? valueLE2, out var consumedLE);
        var successBE1 = OneUShort.TryReadBigEndian(buffer, out OneUShort? valueBE1);
        var successBE2 = OneUShort.TryReadBigEndian(buffer, out OneUShort? valueBE2, out var consumedBE);

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
    [InlineData(0x0000, 2, "0000", "0000")]
    [InlineData(0x0100, 2, "0001", "0100")]
    [InlineData(0x0010, 2, "1000", "0010")]
    [InlineData(0xFFFF, 2, "FFFF", "FFFF")]
    [InlineData(0x0100, 3, "000100", "010000")]
    public void TryWrite_GoodInputShouldBeValid(
        ushort value,
        int bufferSize,
        string expectedHexStringLE,
        string expectedHexStringBE
    )
    {
        var bufferLE = new byte[bufferSize];
        var bufferBE = new byte[bufferSize];
        var expectedHexBytesLE = Convert.FromHexString(expectedHexStringLE);
        var expectedHexBytesBE = Convert.FromHexString(expectedHexStringBE);
        var writable = new OneUShort(value);

        var successLE = writable.TryWriteLittleEndian(bufferLE);
        var successBE = writable.TryWriteBigEndian(bufferBE);

        successLE.Should().BeTrue();
        successBE.Should().BeTrue();
        bufferLE.Should().BeEquivalentTo(expectedHexBytesLE);
        bufferBE.Should().BeEquivalentTo(expectedHexBytesBE);
        writable.GetWriteSize().Should().Be(2);
    }

    [Theory]
    [InlineData(0x0000, 0, "")]
    [InlineData(0x0100, 0, "")]
    [InlineData(0x0000, 1, "00")]
    [InlineData(0x0100, 1, "00")]
    public void TryWrite_BadInputShouldBeValid(ushort value, int bufferSize, string expectedHexString)
    {
        var bufferLE = new byte[bufferSize];
        var bufferBE = new byte[bufferSize];
        var expectedHexBytes = Convert.FromHexString(expectedHexString);
        var writable = new OneUShort(value);

        var successLE = writable.TryWriteLittleEndian(bufferLE);
        var successBE = writable.TryWriteBigEndian(bufferBE);

        successLE.Should().BeFalse();
        successBE.Should().BeFalse();
        bufferLE.Should().BeEquivalentTo(expectedHexBytes);
        bufferBE.Should().BeEquivalentTo(expectedHexBytes);
    }
}

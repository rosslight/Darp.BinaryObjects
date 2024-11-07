namespace Darp.BinaryObjects.Generator.Tests;

using FluentAssertions;
using Sources;

public class OneBoolTests
{
    [Theory]
    [InlineData("00", false)]
    [InlineData("01", true)]
    [InlineData("10", true)]
    [InlineData("FF", true)]
    [InlineData("0100", true)]
    public void TryRead_GoodInputShouldBeValid(string hexString, bool expectedValue)
    {
        var buffer = Convert.FromHexString(hexString);

        var successLE1 = OneBool.TryReadLittleEndian(buffer, out OneBool? valueLE1);
        var successLE2 = OneBool.TryReadLittleEndian(buffer, out OneBool? valueLE2, out var consumedLE);
        var successBE1 = OneBool.TryReadBigEndian(buffer, out OneBool? valueBE1);
        var successBE2 = OneBool.TryReadBigEndian(buffer, out OneBool? valueBE2, out var consumedBE);

        successLE1.Should().BeTrue();
        successLE2.Should().BeTrue();
        successBE1.Should().BeTrue();
        successBE2.Should().BeTrue();
        valueLE1!.Value.Should().Be(expectedValue);
        valueLE2!.Value.Should().Be(expectedValue);
        valueBE1!.Value.Should().Be(expectedValue);
        valueBE2!.Value.Should().Be(expectedValue);
        consumedLE.Should().Be(1);
        consumedBE.Should().Be(1);
    }

    [Theory]
    [InlineData("")]
    public void TryRead_BadInputShouldBeValid(string hexString)
    {
        var buffer = Convert.FromHexString(hexString);

        var successLE1 = OneBool.TryReadLittleEndian(buffer, out OneBool? valueLE1);
        var successLE2 = OneBool.TryReadLittleEndian(buffer, out OneBool? valueLE2, out var consumedLE);
        var successBE1 = OneBool.TryReadBigEndian(buffer, out OneBool? valueBE1);
        var successBE2 = OneBool.TryReadBigEndian(buffer, out OneBool? valueBE2, out var consumedBE);

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
    [InlineData(false, 1, "00")]
    [InlineData(true, 1, "01")]
    [InlineData(true, 2, "0100")]
    public void TryWrite_GoodInputShouldBeValid(bool value, int bufferSize, string expectedHexString)
    {
        var bufferLE = new byte[bufferSize];
        var bufferBE = new byte[bufferSize];
        var expectedHexBytes = Convert.FromHexString(expectedHexString);
        var writable = new OneBool(value);

        var successLE1 = writable.TryWriteLittleEndian(bufferLE);
        var successLE2 = writable.TryWriteLittleEndian(bufferLE, out var writtenLE);
        var successBE1 = writable.TryWriteBigEndian(bufferBE);
        var successBE2 = writable.TryWriteBigEndian(bufferBE, out var writtenBE);

        successLE1.Should().BeTrue();
        successLE2.Should().BeTrue();
        successBE1.Should().BeTrue();
        successBE2.Should().BeTrue();
        bufferLE.Should().BeEquivalentTo(expectedHexBytes);
        bufferBE.Should().BeEquivalentTo(expectedHexBytes);
        writtenLE.Should().Be(1);
        writtenBE.Should().Be(1);
        writable.GetByteCount().Should().Be(1);
    }

    [Theory]
    [InlineData(false, 0, "")]
    [InlineData(true, 0, "")]
    public void TryWrite_BadInputShouldBeValid(bool value, int bufferSize, string expectedHexString)
    {
        var bufferLE = new byte[bufferSize];
        var bufferBE = new byte[bufferSize];
        var expectedHexBytes = Convert.FromHexString(expectedHexString);
        var writable = new OneBool(value);

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

namespace Darp.BinaryObjects.Generator.Tests;

using FluentAssertions;
using Sources;

public class UnitTest1
{
    [Theory]
    [InlineData("00", false)]
    [InlineData("01", true)]
    [InlineData("10", true)]
    [InlineData("FF", true)]
    [InlineData("0100", true)]
    public void OneBool_TryRead_GoodInputShouldBeValid(string hexString, bool expectedValue)
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
    public void OneBool_TryRead_BadInputShouldBeValid(string hexString)
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

    [Fact]
    public void OneBool_GetWriteSize_ShouldBeValid()
    {
        var writable = new OneBool(true);

        writable.GetWriteSize().Should().Be(1);
    }

    [Theory]
    [InlineData(false, 1, "00")]
    [InlineData(true, 1, "01")]
    [InlineData(true, 2, "0100")]
    public void OneBool_TryWrite_GoodInputShouldBeValid(bool value, int bufferSize, string expectedHexString)
    {
        var bufferLE = new byte[bufferSize];
        var bufferBE = new byte[bufferSize];
        var expectedHexBytes = Convert.FromHexString(expectedHexString);
        var writable = new OneBool(value);

        var successLE = writable.TryWriteLittleEndian(bufferLE);
        var successBE = writable.TryWriteBigEndian(bufferBE);

        successLE.Should().BeTrue();
        successBE.Should().BeTrue();
        bufferLE.Should().BeEquivalentTo(expectedHexBytes);
        bufferBE.Should().BeEquivalentTo(expectedHexBytes);
    }

    [Theory]
    [InlineData(false, 0, "")]
    [InlineData(true, 0, "")]
    public void OneBool_TryWrite_BadInputShouldBeValid(bool value, int bufferSize, string expectedHexString)
    {
        var bufferLE = new byte[bufferSize];
        var bufferBE = new byte[bufferSize];
        var expectedHexBytes = Convert.FromHexString(expectedHexString);
        var writable = new OneBool(value);

        var successLE = writable.TryWriteLittleEndian(bufferLE);
        var successBE = writable.TryWriteBigEndian(bufferBE);

        successLE.Should().BeFalse();
        successBE.Should().BeFalse();
        bufferLE.Should().BeEquivalentTo(expectedHexBytes);
        bufferBE.Should().BeEquivalentTo(expectedHexBytes);
    }
}

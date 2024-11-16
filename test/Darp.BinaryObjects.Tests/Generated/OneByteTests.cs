namespace Darp.BinaryObjects.Tests.Generated;

using FluentAssertions;

[BinaryObject]
public sealed partial record OneByte(byte Value);

public class OneByteTests
{
    [Theory]
    [InlineData("00", 0x00)]
    [InlineData("01", 0x01)]
    [InlineData("10", 0x10)]
    [InlineData("FF", 0xFF)]
    [InlineData("0100", 0x01)]
    public void TryRead_GoodInputShouldBeValid(string hexString, byte expectedValue)
    {
        var buffer = Convert.FromHexString(hexString);

        var successLE1 = OneByte.TryReadLittleEndian(buffer, out OneByte? valueLE1);
        var successLE2 = OneByte.TryReadLittleEndian(buffer, out OneByte? valueLE2, out var consumedLE);
        var successBE1 = OneByte.TryReadBigEndian(buffer, out OneByte? valueBE1);
        var successBE2 = OneByte.TryReadBigEndian(buffer, out OneByte? valueBE2, out var consumedBE);

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

        var successLE1 = OneByte.TryReadLittleEndian(buffer, out OneByte? valueLE1);
        var successLE2 = OneByte.TryReadLittleEndian(buffer, out OneByte? valueLE2, out var consumedLE);
        var successBE1 = OneByte.TryReadBigEndian(buffer, out OneByte? valueBE1);
        var successBE2 = OneByte.TryReadBigEndian(buffer, out OneByte? valueBE2, out var consumedBE);

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
    [InlineData(0x00, 1, "00")]
    [InlineData(0x01, 1, "01")]
    [InlineData(0x01, 2, "0100")]
    public void TryWrite_GoodInputShouldBeValid(byte value, int bufferSize, string expectedHexString)
    {
        var bufferLE = new byte[bufferSize];
        var bufferBE = new byte[bufferSize];
        var expectedHexBytes = Convert.FromHexString(expectedHexString);
        var writable = new OneByte(value);

        var successLE1 = writable.TryWriteLittleEndian(bufferLE);
        var successLE2 = writable.TryWriteLittleEndian(bufferLE, out var writtenLE);
        var successBE1 = writable.TryWriteBigEndian(bufferBE);
        var successBE2 = writable.TryWriteBigEndian(bufferBE, out var writtenBE);

        AssertionExtensions.Should((bool)successLE1).BeTrue();
        AssertionExtensions.Should((bool)successLE2).BeTrue();
        AssertionExtensions.Should((bool)successBE1).BeTrue();
        AssertionExtensions.Should((bool)successBE2).BeTrue();
        bufferLE.Should().BeEquivalentTo(expectedHexBytes);
        bufferBE.Should().BeEquivalentTo(expectedHexBytes);
        AssertionExtensions.Should((int)writtenLE).Be(1);
        AssertionExtensions.Should((int)writtenBE).Be(1);
        AssertionExtensions.Should((int)writable.GetByteCount()).Be(1);
    }

    [Theory]
    [InlineData(0x00, 0, "")]
    [InlineData(0x01, 0, "")]
    public void TryWrite_BadInputShouldBeValid(byte value, int bufferSize, string expectedHexString)
    {
        var bufferLE = new byte[bufferSize];
        var bufferBE = new byte[bufferSize];
        var expectedHexBytes = Convert.FromHexString(expectedHexString);
        var writable = new OneByte(value);

        var successLE1 = writable.TryWriteLittleEndian(bufferLE);
        var successLE2 = writable.TryWriteLittleEndian(bufferLE, out var writtenLE);
        var successBE1 = writable.TryWriteBigEndian(bufferBE);
        var successBE2 = writable.TryWriteBigEndian(bufferBE, out var writtenBE);

        AssertionExtensions.Should((bool)successLE1).BeFalse();
        AssertionExtensions.Should((bool)successLE2).BeFalse();
        AssertionExtensions.Should((bool)successBE1).BeFalse();
        AssertionExtensions.Should((bool)successBE2).BeFalse();
        bufferLE.Should().BeEquivalentTo(expectedHexBytes);
        bufferBE.Should().BeEquivalentTo(expectedHexBytes);
        AssertionExtensions.Should((int)writtenLE).Be(0);
        AssertionExtensions.Should((int)writtenBE).Be(0);
    }
}

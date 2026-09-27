namespace Darp.BinaryObjects.Tests.Generated;

using FluentAssertions;

[BinaryObject]
public sealed partial record TwoUShorts
{
    public TwoUShorts(ushort valueTwo, ushort value)
    {
        Value = value;
        ValueTwo = valueTwo;
    }

    public ushort Value { get; }
    public ushort ValueTwo { get; }

    [BinaryIgnore]
    public byte Ignored { get; init; }
}

[BinaryObject]
internal sealed partial record NestedTwoUShorts(TwoUShorts Value, byte Tail);

public class TwoUShortsTests
{
    [Theory]
    [InlineData(true, "3412CDABEF")]
    [InlineData(false, "1234ABCDEF")]
    public void NestedObject_ShouldUseSerializedMembersForSize(bool littleEndian, string hexString)
    {
        var source = Convert.FromHexString(hexString);
        NestedTwoUShorts? value;
        int bytesRead;
        var read = littleEndian
            ? NestedTwoUShorts.TryReadLittleEndian(source, out value, out bytesRead)
            : NestedTwoUShorts.TryReadBigEndian(source, out value, out bytesRead);

        read.Should().BeTrue();
        Assert.NotNull(value);
        value.Value.Value.Should().Be(0x1234);
        value.Value.ValueTwo.Should().Be(0xABCD);
        value.Tail.Should().Be(0xEF);
        bytesRead.Should().Be(5);
        value.GetByteCount().Should().Be(5);

        var writable = new NestedTwoUShorts(new TwoUShorts(value: 0x1234, valueTwo: 0xABCD) { Ignored = 0xFF }, 0xEF);
        var destination = Enumerable.Repeat((byte)0xA5, 6).ToArray();
        int bytesWritten;
        var written = littleEndian
            ? writable.TryWriteLittleEndian(destination, out bytesWritten)
            : writable.TryWriteBigEndian(destination, out bytesWritten);

        written.Should().BeTrue();
        bytesWritten.Should().Be(5);
        destination.Should().Equal(source.Concat(new byte[] { 0xA5 }));

        var truncated = source[..4];
        var readTruncated = littleEndian
            ? NestedTwoUShorts.TryReadLittleEndian(truncated, out _, out bytesRead)
            : NestedTwoUShorts.TryReadBigEndian(truncated, out _, out bytesRead);
        readTruncated.Should().BeFalse();
        bytesRead.Should().Be(0);

        var shortDestination = Enumerable.Repeat((byte)0xA5, 4).ToArray();
        var writtenTruncated = littleEndian
            ? writable.TryWriteLittleEndian(shortDestination, out bytesWritten)
            : writable.TryWriteBigEndian(shortDestination, out bytesWritten);
        writtenTruncated.Should().BeFalse();
        bytesWritten.Should().Be(0);
        shortDestination.Should().Equal(0xA5, 0xA5, 0xA5, 0xA5);
    }

    [Theory]
    [InlineData("00000000", 0x0000, 0x0000, 0x0000, 0x0000)]
    [InlineData("01000001", 0x0001, 0x0100, 0x0100, 0x0001)]
    [InlineData("00100100", 0x1000, 0x0001, 0x0010, 0x0100)]
    [InlineData("FFFFFFFF", 0xFFFF, 0xFFFF, 0xFFFF, 0xFFFF)]
    [InlineData("FFFFFFFF00", 0xFFFF, 0xFFFF, 0xFFFF, 0xFFFF)]
    public void TryRead_GoodInputShouldBeValid(
        string hexString,
        ushort expectedValueLE,
        ushort expectedValueTwoLE,
        ushort expectedValueBE,
        ushort expectedValueTwoBE
    )
    {
        var buffer = Convert.FromHexString(hexString);

        var successLE1 = TwoUShorts.TryReadLittleEndian(buffer, out TwoUShorts? valueLE1);
        var successLE2 = TwoUShorts.TryReadLittleEndian(buffer, out TwoUShorts? valueLE2, out var consumedLE);
        var successBE1 = TwoUShorts.TryReadBigEndian(buffer, out TwoUShorts? valueBE1);
        var successBE2 = TwoUShorts.TryReadBigEndian(buffer, out TwoUShorts? valueBE2, out var consumedBE);

        successLE1.Should().BeTrue();
        successLE2.Should().BeTrue();
        successBE1.Should().BeTrue();
        successBE2.Should().BeTrue();
        valueLE1!.Value.Should().Be(expectedValueLE);
        valueLE1.ValueTwo.Should().Be(expectedValueTwoLE);
        valueLE2!.Value.Should().Be(expectedValueLE);
        valueLE2.ValueTwo.Should().Be(expectedValueTwoLE);
        valueBE1!.Value.Should().Be(expectedValueBE);
        valueBE1.ValueTwo.Should().Be(expectedValueTwoBE);
        valueBE2!.Value.Should().Be(expectedValueBE);
        valueBE2.ValueTwo.Should().Be(expectedValueTwoBE);
        consumedLE.Should().Be(4);
        consumedBE.Should().Be(4);
    }

    [Theory]
    [InlineData("")]
    [InlineData("00")]
    [InlineData("000000")]
    public void TryRead_BadInputShouldBeValid(string hexString)
    {
        var buffer = Convert.FromHexString(hexString);

        var successLE1 = TwoUShorts.TryReadLittleEndian(buffer, out TwoUShorts? valueLE1);
        var successLE2 = TwoUShorts.TryReadLittleEndian(buffer, out TwoUShorts? valueLE2, out var consumedLE);
        var successBE1 = TwoUShorts.TryReadBigEndian(buffer, out TwoUShorts? valueBE1);
        var successBE2 = TwoUShorts.TryReadBigEndian(buffer, out TwoUShorts? valueBE2, out var consumedBE);

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
    [InlineData(0x0000, 0x0000, 4, "00000000", "00000000")]
    [InlineData(0x0100, 0x0001, 4, "00010100", "01000001")]
    [InlineData(0x0010, 0x1000, 4, "10000010", "00101000")]
    [InlineData(0xFFFF, 0xFFFF, 4, "FFFFFFFF", "FFFFFFFF")]
    [InlineData(0x0100, 0x0000, 5, "0001000000", "0100000000")]
    public void TryWrite_GoodInputShouldBeValid(
        ushort value,
        ushort valueTwo,
        int bufferSize,
        string expectedHexStringLE,
        string expectedHexStringBE
    )
    {
        var bufferLE = new byte[bufferSize];
        var bufferBE = new byte[bufferSize];
        var expectedHexBytesLE = Convert.FromHexString(expectedHexStringLE);
        var expectedHexBytesBE = Convert.FromHexString(expectedHexStringBE);
        var writable = new TwoUShorts(value: value, valueTwo: valueTwo);

        var successLE1 = writable.TryWriteLittleEndian(bufferLE);
        var successLE2 = writable.TryWriteLittleEndian(bufferLE, out var writtenLE);
        var successBE1 = writable.TryWriteBigEndian(bufferBE);
        var successBE2 = writable.TryWriteBigEndian(bufferBE, out var writtenBE);

        successLE1.Should().BeTrue();
        successLE2.Should().BeTrue();
        successBE1.Should().BeTrue();
        successBE2.Should().BeTrue();
        bufferLE.Should().Equal(expectedHexBytesLE);
        bufferBE.Should().Equal(expectedHexBytesBE);
        writtenLE.Should().Be(4);
        writtenBE.Should().Be(4);
        writable.GetByteCount().Should().Be(4);
    }

    [Theory]
    [InlineData(0x0000, 0x0000, 0, "")]
    [InlineData(0x0100, 0x0100, 0, "")]
    [InlineData(0x0000, 0x0000, 3, "000000")]
    [InlineData(0x0100, 0x0100, 3, "000000")]
    public void TryWrite_BadInputShouldBeValid(ushort value, ushort valueTwo, int bufferSize, string expectedHexString)
    {
        var bufferLE = new byte[bufferSize];
        var bufferBE = new byte[bufferSize];
        var expectedHexBytes = Convert.FromHexString(expectedHexString);
        var writable = new TwoUShorts(value: value, valueTwo: valueTwo);

        var successLE1 = writable.TryWriteLittleEndian(bufferLE);
        var successLE2 = writable.TryWriteLittleEndian(bufferLE, out var writtenLE);
        var successBE1 = writable.TryWriteBigEndian(bufferBE);
        var successBE2 = writable.TryWriteBigEndian(bufferBE, out var writtenBE);

        successLE1.Should().BeFalse();
        successLE2.Should().BeFalse();
        successBE1.Should().BeFalse();
        successBE2.Should().BeFalse();
        bufferLE.Should().Equal(expectedHexBytes);
        bufferBE.Should().Equal(expectedHexBytes);
        writtenLE.Should().Be(0);
        writtenBE.Should().Be(0);
    }
}

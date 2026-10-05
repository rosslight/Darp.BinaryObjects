namespace Darp.BinaryObjects.Tests.Generated;

using FluentAssertions;

[BinaryObject]
public sealed partial record MemoryMemberLengthUShortSize(
    ushort Length,
    [property: BinaryElementCount("Length")] ReadOnlyMemory<ushort> Value
);

[BinaryObject]
internal sealed partial record IntCountedUIntMemory(
    int Length,
    [property: BinaryElementCount("Length")] ReadOnlyMemory<uint> Value,
    byte Tail
);

[BinaryObject]
internal sealed partial record MinimumCountedUShortMemory(
    ushort Count,
    [property: BinaryElementCount("Count"), BinaryMinElementCount(2)] ReadOnlyMemory<ushort> Values,
    ushort Tail
);

public class MemoryMemberLengthUShortSizeTests
{
    [Fact]
    public void MinimumElementCount_ShouldRejectDeclaredCountEvenWhenTailProvidesEnoughBytes()
    {
        var sourceLE = Convert.FromHexString("01003412CDAB");
        var sourceBE = Convert.FromHexString("00011234ABCD");

        MinimumCountedUShortMemory.TryReadLittleEndian(sourceLE, out var valueLE, out var bytesReadLE).Should().BeFalse();
        MinimumCountedUShortMemory.TryReadBigEndian(sourceBE, out var valueBE, out var bytesReadBE).Should().BeFalse();
        valueLE.Should().BeNull();
        valueBE.Should().BeNull();
        bytesReadLE.Should().Be(2);
        bytesReadBE.Should().Be(2);

        var value = new MinimumCountedUShortMemory(1, new ushort[] { 0x1234 }, 0xABCD);
        value.TryWriteLittleEndian(new byte[6], out _).Should().BeFalse();
        value.TryWriteBigEndian(new byte[6], out _).Should().BeFalse();
    }

    [Fact]
    public void MinimumElementCount_ShouldAcceptDeclaredMinimum()
    {
        var sourceLE = Convert.FromHexString("020034127856CDAB");
        var sourceBE = Convert.FromHexString("000212345678ABCD");

        MinimumCountedUShortMemory.TryReadLittleEndian(sourceLE, out var valueLE, out var bytesReadLE).Should().BeTrue();
        MinimumCountedUShortMemory.TryReadBigEndian(sourceBE, out var valueBE, out var bytesReadBE).Should().BeTrue();
        Assert.NotNull(valueLE);
        Assert.NotNull(valueBE);
        valueLE.Values.ToArray().Should().Equal(0x1234, 0x5678);
        valueBE.Values.ToArray().Should().Equal(0x1234, 0x5678);
        valueLE.Tail.Should().Be(0xABCD);
        valueBE.Tail.Should().Be(0xABCD);
        bytesReadLE.Should().Be(sourceLE.Length);
        bytesReadBE.Should().Be(sourceBE.Length);
        valueLE.GetByteCount().Should().Be(sourceLE.Length);
        valueBE.GetByteCount().Should().Be(sourceBE.Length);
    }

    [Theory]
    [InlineData("FFFFFFFF", "FFFFFFFF")]
    [InlineData("FFFFFF7F", "7FFFFFFF")]
    [InlineData("00000020", "20000000")]
    [InlineData("00000040AB", "40000000AB")]
    [InlineData("0100004078563412AB", "4000000112345678AB")]
    [InlineData("010000007856", "000000011234")]
    public void SignedCount_ShouldRejectNegativeOverflowingAndTruncatedPayloads(string hexLE, string hexBE)
    {
        var sourceLE = Convert.FromHexString(hexLE);
        var sourceBE = Convert.FromHexString(hexBE);

        IntCountedUIntMemory.TryReadLittleEndian(sourceLE, out _).Should().BeFalse();
        IntCountedUIntMemory.TryReadBigEndian(sourceBE, out _).Should().BeFalse();
        IntCountedUIntMemory.TryReadLittleEndian(sourceLE, out var valueLE, out var bytesReadLE).Should().BeFalse();
        IntCountedUIntMemory.TryReadBigEndian(sourceBE, out var valueBE, out var bytesReadBE).Should().BeFalse();
        valueLE.Should().BeNull();
        valueBE.Should().BeNull();
        bytesReadLE.Should().Be(4);
        bytesReadBE.Should().Be(4);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(int.MaxValue)]
    [InlineData(536870912)]
    [InlineData(1073741824)]
    [InlineData(1073741825)]
    public void SignedCount_ShouldRejectWritesThatCannotFit(int count)
    {
        var value = new IntCountedUIntMemory(count, new uint[] { 0x12345678 }, 0xAB);
        var destinationLE = Enumerable.Repeat((byte)0xA5, 9).ToArray();
        var destinationBE = Enumerable.Repeat((byte)0xA5, 9).ToArray();

        value.TryWriteLittleEndian(destinationLE, out var bytesWrittenLE).Should().BeFalse();
        value.TryWriteBigEndian(destinationBE, out var bytesWrittenBE).Should().BeFalse();
        bytesWrittenLE.Should().Be(4);
        bytesWrittenBE.Should().Be(4);
        destinationLE.AsSpan(4).ToArray().Should().Equal(0xA5, 0xA5, 0xA5, 0xA5, 0xA5);
        destinationBE.AsSpan(4).ToArray().Should().Equal(0xA5, 0xA5, 0xA5, 0xA5, 0xA5);
    }

    [Theory]
    [InlineData("00000000AB", "00000000AB", 0)]
    [InlineData("0100000078563412AB", "0000000112345678AB", 1, 0x12345678u)]
    public void SignedCount_ShouldPreservePayloadAndTrailingField(
        string hexLE,
        string hexBE,
        int count,
        params uint[] expectedValues
    )
    {
        var sourceLE = Convert.FromHexString(hexLE);
        var sourceBE = Convert.FromHexString(hexBE);

        IntCountedUIntMemory.TryReadLittleEndian(sourceLE, out var valueLE, out var bytesReadLE).Should().BeTrue();
        IntCountedUIntMemory.TryReadBigEndian(sourceBE, out var valueBE, out var bytesReadBE).Should().BeTrue();
        Assert.NotNull(valueLE);
        Assert.NotNull(valueBE);
        valueLE.Length.Should().Be(count);
        valueBE.Length.Should().Be(count);
        valueLE.Value.ToArray().Should().Equal(expectedValues);
        valueBE.Value.ToArray().Should().Equal(expectedValues);
        valueLE.Tail.Should().Be(0xAB);
        valueBE.Tail.Should().Be(0xAB);
        bytesReadLE.Should().Be(sourceLE.Length);
        bytesReadBE.Should().Be(sourceBE.Length);
        valueLE.GetByteCount().Should().Be(sourceLE.Length);
        valueBE.GetByteCount().Should().Be(sourceBE.Length);

        var destinationLE = new byte[sourceLE.Length];
        var destinationBE = new byte[sourceBE.Length];
        valueLE.TryWriteLittleEndian(destinationLE, out var bytesWrittenLE).Should().BeTrue();
        valueBE.TryWriteBigEndian(destinationBE, out var bytesWrittenBE).Should().BeTrue();
        destinationLE.Should().Equal(sourceLE);
        destinationBE.Should().Equal(sourceBE);
        bytesWrittenLE.Should().Be(sourceLE.Length);
        bytesWrittenBE.Should().Be(sourceBE.Length);
    }

    [Theory]
    [InlineData(536870911)]
    [InlineData(536870912)]
    public void GetByteCount_ShouldThrowWhenRequiredSizeExceedsInt32(int count)
    {
        var value = new IntCountedUIntMemory(count, ReadOnlyMemory<uint>.Empty, 0);

        Assert.Throws<OverflowException>(() => value.GetByteCount());
    }

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

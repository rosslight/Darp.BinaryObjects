namespace Darp.BinaryObjects.Tests.Generated;

using FluentAssertions;

[BinaryObject]
internal sealed partial record ByteCountedInts(
    ushort Length,
    [property: BinaryByteCount("Length")] int[] Values,
    byte Tail
);

[BinaryObject]
internal sealed partial record ByteCountedNarrow(
    sbyte Length,
    [property: BinaryByteCount("Length"), BinaryElementByteCount(3), BinaryMinElementCount(1)] List<int> Values,
    byte Tail
);

[BinaryObject]
internal sealed partial record ByteCountedObjects(
    byte Length,
    [property: BinaryByteCount("Length")] IReadOnlyList<OneUShort> Values,
    byte DataLength,
    [property: BinaryByteCount("DataLength")] ReadOnlyMemory<byte> Data
);

[BinaryObject]
internal sealed partial record NarrowByteCount(
    [property: BinaryByteCount(3)] int Length,
    [property: BinaryByteCount("Length")] ushort[] Values
);

public sealed class CollectionByteCountTests
{
    [Theory]
    [InlineData(true, "080001000000FEFFFFFFEF")]
    [InlineData(false, "000800000001FFFFFFFEEF")]
    public void Collection_ShouldEndAfterByteCount(bool littleEndian, string hex)
    {
        var expected = Convert.FromHexString(hex);
        var value = new ByteCountedInts(8, [1, -2], 0xEF);

        var destination = new byte[expected.Length];
        var written = littleEndian
            ? value.TryWriteLittleEndian(destination, out var bytesWritten)
            : value.TryWriteBigEndian(destination, out bytesWritten);
        written.Should().BeTrue();
        bytesWritten.Should().Be(expected.Length);
        value.GetByteCount().Should().Be(expected.Length);
        destination.Should().Equal(expected);

        var read = littleEndian
            ? ByteCountedInts.TryReadLittleEndian(expected, out var parsed, out var bytesRead)
            : ByteCountedInts.TryReadBigEndian(expected, out parsed, out bytesRead);
        read.Should().BeTrue();
        bytesRead.Should().Be(expected.Length);
        Assert.NotNull(parsed);
        parsed.Length.Should().Be(8);
        parsed.Values.Should().Equal(1, -2);
        parsed.Tail.Should().Be(0xEF);
    }

    [Theory]
    [InlineData("0900010000000200000003EF")] // Not a multiple of the element length
    [InlineData("0C000100000002000000EF")] // More bytes than the source holds
    public void Read_ShouldRejectByteCount(string hex)
    {
        var source = Convert.FromHexString(hex);

        ByteCountedInts.TryReadLittleEndian(source, out var value, out var bytesRead).Should().BeFalse();

        value.Should().BeNull();
        bytesRead.Should().Be(2);
    }

    [Theory]
    [InlineData(6)] // Not a multiple of the element length
    [InlineData(12)] // More elements than the collection holds
    public void Write_ShouldRejectByteCount(ushort length)
    {
        var value = new ByteCountedInts(length, [1, 2], 0xEF);
        var destination = Enumerable.Repeat((byte)0xA5, 15).ToArray();

        value.TryWriteLittleEndian(destination, out var bytesWritten).Should().BeFalse();

        bytesWritten.Should().Be(2);
        destination.AsSpan(2).ToArray().Should().OnlyContain(x => x == 0xA5);
    }

    [Fact]
    public void Write_ShouldIgnoreSurplusElements()
    {
        var value = new ByteCountedInts(4, [1, 2], 0xEF);
        var destination = new byte[value.GetByteCount()];

        value.TryWriteLittleEndian(destination, out var bytesWritten).Should().BeTrue();

        bytesWritten.Should().Be(7);
        destination.Should().Equal(Convert.FromHexString("040001000000EF"));
    }

    [Theory]
    [InlineData(true, "06010000FEFFFFEF")]
    [InlineData(false, "06000001FFFFFEEF")]
    public void NarrowElements_ShouldDivideByElementByteCount(bool littleEndian, string hex)
    {
        var expected = Convert.FromHexString(hex);
        var value = new ByteCountedNarrow(6, [1, -2], 0xEF);

        var destination = new byte[expected.Length];
        var written = littleEndian ? value.TryWriteLittleEndian(destination) : value.TryWriteBigEndian(destination);
        written.Should().BeTrue();
        value.GetByteCount().Should().Be(expected.Length);
        destination.Should().Equal(expected);

        var read = littleEndian
            ? ByteCountedNarrow.TryReadLittleEndian(expected, out var parsed, out var bytesRead)
            : ByteCountedNarrow.TryReadBigEndian(expected, out parsed, out bytesRead);
        read.Should().BeTrue();
        bytesRead.Should().Be(expected.Length);
        Assert.NotNull(parsed);
        parsed.Values.Should().Equal(1, -2);
        parsed.Tail.Should().Be(0xEF);
    }

    [Theory]
    [InlineData(0, "00EF")] // Fewer elements than the minimum
    [InlineData(-3, "FD010000EF")] // A negative byte count which is a multiple of the element length
    [InlineData(-1, "FF010000EF")]
    public void NarrowElements_ShouldRejectByteCount(sbyte length, string hex)
    {
        ByteCountedNarrow.TryReadLittleEndian(Convert.FromHexString(hex), out _, out var bytesRead).Should().BeFalse();
        bytesRead.Should().Be(1);

        var value = new ByteCountedNarrow(length, [1, 2], 0xEF);
        value.TryWriteLittleEndian(new byte[16], out var bytesWritten).Should().BeFalse();
        bytesWritten.Should().Be(1);
    }

    [Theory]
    [InlineData(true, "043412CDAB02A1B2")]
    [InlineData(false, "041234ABCD02A1B2")]
    public void ObjectsAndBytes_ShouldEndAfterByteCount(bool littleEndian, string hex)
    {
        var expected = Convert.FromHexString(hex);
        var value = new ByteCountedObjects(
            4,
            [new OneUShort(0x1234), new OneUShort(0xABCD)],
            2,
            new byte[] { 0xA1, 0xB2 }
        );

        var destination = new byte[expected.Length];
        var written = littleEndian ? value.TryWriteLittleEndian(destination) : value.TryWriteBigEndian(destination);
        written.Should().BeTrue();
        value.GetByteCount().Should().Be(expected.Length);
        destination.Should().Equal(expected);

        var read = littleEndian
            ? ByteCountedObjects.TryReadLittleEndian(expected, out var parsed, out var bytesRead)
            : ByteCountedObjects.TryReadBigEndian(expected, out parsed, out bytesRead);
        read.Should().BeTrue();
        bytesRead.Should().Be(expected.Length);
        Assert.NotNull(parsed);
        parsed.Values.Should().Equal(value.Values);
        parsed.Data.ToArray().Should().Equal(0xA1, 0xB2);
    }

    [Fact]
    public void NarrowLengthMember_ShouldBoundCollection()
    {
        var expected = Convert.FromHexString("00000400010002");
        var value = new NarrowByteCount(4, [1, 2]);

        var destination = new byte[expected.Length];
        value.TryWriteBigEndian(destination).Should().BeTrue();
        destination.Should().Equal(expected);

        NarrowByteCount.TryReadBigEndian(expected, out var parsed, out var bytesRead).Should().BeTrue();
        bytesRead.Should().Be(expected.Length);
        Assert.NotNull(parsed);
        parsed.Values.Should().Equal(1, 2);
    }
}

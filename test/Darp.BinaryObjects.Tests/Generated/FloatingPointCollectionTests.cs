namespace Darp.BinaryObjects.Tests.Generated;

using FluentAssertions;

[BinaryObject]
internal sealed partial record FloatingPointCollections(
    [property: BinaryElementCount(2)] Half[] Halves,
    [property: BinaryElementCount(2)] float[] Floats,
    [property: BinaryElementCount(2)] List<double> Doubles
);

public sealed class FloatingPointCollectionTests
{
    [Theory]
    [InlineData(true, "003E00800000C03F00000080000000000000F83F0000000000000080")]
    [InlineData(false, "3E0080003FC00000800000003FF80000000000008000000000000000")]
    public void Collections_ShouldPreserveFloatingPointBits(bool littleEndian, string hexBytes)
    {
        var expected = Convert.FromHexString(hexBytes);
        var value = new FloatingPointCollections(
            [BitConverter.UInt16BitsToHalf(0x3E00), BitConverter.UInt16BitsToHalf(0x8000)],
            [BitConverter.UInt32BitsToSingle(0x3FC00000), BitConverter.UInt32BitsToSingle(0x80000000)],
            [BitConverter.UInt64BitsToDouble(0x3FF8000000000000), BitConverter.UInt64BitsToDouble(0x8000000000000000)]
        );

        var destination = new byte[expected.Length];
        var written = littleEndian
            ? value.TryWriteLittleEndian(destination, out var bytesWritten)
            : value.TryWriteBigEndian(destination, out bytesWritten);
        written.Should().BeTrue();
        bytesWritten.Should().Be(expected.Length);
        value.GetByteCount().Should().Be(expected.Length);
        destination.Should().Equal(expected);

        var read = littleEndian
            ? FloatingPointCollections.TryReadLittleEndian(expected, out var parsed, out var bytesRead)
            : FloatingPointCollections.TryReadBigEndian(expected, out parsed, out bytesRead);
        read.Should().BeTrue();
        bytesRead.Should().Be(expected.Length);
        Assert.NotNull(parsed);
        parsed.Halves.Select(BitConverter.HalfToUInt16Bits).Should().Equal(0x3E00, 0x8000);
        parsed.Floats.Select(BitConverter.SingleToUInt32Bits).Should().Equal(0x3FC00000u, 0x80000000u);
        parsed.Doubles.Select(BitConverter.DoubleToUInt64Bits).Should().Equal(0x3FF8000000000000ul, 0x8000000000000000ul);
    }
}

namespace Darp.BinaryObjects.Tests.Generated;

using System.Collections.ObjectModel;
using FluentAssertions;

[Flags]
internal enum ClassOfDevice : uint
{
    LimitedDiscoverable = 0x002000,
    Audio = 0x200000,
}

internal enum SignedLevel
{
    Low = -2,
    High = 2,
}

[BinaryObject]
internal sealed partial record NarrowScalars(
    [property: BinaryByteCount(3)] uint Unsigned,
    [property: BinaryByteCount(6)] ulong Address,
    [property: BinaryByteCount(3)] int Signed,
    [property: BinaryByteCount(3)] ClassOfDevice Class,
    [property: BinaryByteCount(1)] SignedLevel Level,
    byte Tail
);

[BinaryObject]
internal sealed partial record NarrowBounds(
    [property: BinaryByteCount(3)] uint Unsigned,
    [property: BinaryByteCount(3)] int Signed,
    [property: BinaryByteCount(5)] long Wide,
    [property: BinaryByteCount(3)] ClassOfDevice Class
);

[BinaryObject]
internal sealed partial record NarrowCollections(
    [property: BinaryByteCount(3)] int Count,
    [property: BinaryElementCount("Count"), BinaryElementByteCount(3)] int[] Counted,
    [property: BinaryElementCount(2), BinaryElementByteCount(3)] List<ClassOfDevice> Fixed,
    [property: BinaryElementCount(2), BinaryElementByteCount(6)] IReadOnlyList<ulong> Interface,
    [property: BinaryElementByteCount(3)] ReadOnlyMemory<uint> Remaining
);

[BinaryObject]
internal sealed partial record NarrowCollectionKinds(
    [property: BinaryElementCount(2), BinaryElementByteCount(3)] List<int> List,
    [property: BinaryElementCount(2), BinaryElementByteCount(3)] IReadOnlyCollection<ClassOfDevice> EnumCollection,
    [property: BinaryElementCount(2), BinaryElementByteCount(2)] SignedLevel[] SignedEnums
);

[BinaryObject]
internal sealed partial record ScalarThenNarrowArray(
    ushort Head,
    [property: BinaryElementCount(2), BinaryElementByteCount(3)] uint[] Values
);

public sealed class ByteCountTests
{
    [Theory]
    [InlineData(true, "563412665544332211FEFFFF002020FEEF")]
    [InlineData(false, "123456112233445566FFFFFE202000FEEF")]
    public void Scalars_ShouldUseDeclaredByteCount(bool littleEndian, string hex)
    {
        var expected = Convert.FromHexString(hex);
        var value = new NarrowScalars(
            0x123456,
            0x112233445566,
            -2,
            ClassOfDevice.Audio | ClassOfDevice.LimitedDiscoverable,
            SignedLevel.Low,
            0xEF
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
            ? NarrowScalars.TryReadLittleEndian(expected, out var parsed, out var bytesRead)
            : NarrowScalars.TryReadBigEndian(expected, out parsed, out bytesRead);
        read.Should().BeTrue();
        bytesRead.Should().Be(expected.Length);
        parsed.Should().Be(value);
    }

    [Theory]
    [InlineData(0xFFFFFFu, 8_388_607, 549_755_813_887L, "FFFFFFFFFF7FFFFFFFFF7F000000", "FFFFFF7FFFFF7FFFFFFFFF000000")]
    [InlineData(0u, -8_388_608, -549_755_813_888L, "0000000000800000000080000000", "0000008000008000000000000000")]
    [InlineData(1u, -1, -1L, "010000FFFFFFFFFFFFFFFF000000", "000001FFFFFFFFFFFFFFFF000000")]
    public void Bounds_ShouldRoundTrip(uint unsignedValue, int signedValue, long wideValue, string hexLE, string hexBE)
    {
        var value = new NarrowBounds(unsignedValue, signedValue, wideValue, 0);
        var expectedLE = Convert.FromHexString(hexLE);
        var expectedBE = Convert.FromHexString(hexBE);

        var destinationLE = new byte[expectedLE.Length];
        var destinationBE = new byte[expectedBE.Length];
        value.TryWriteLittleEndian(destinationLE).Should().BeTrue();
        value.TryWriteBigEndian(destinationBE).Should().BeTrue();
        destinationLE.Should().Equal(expectedLE);
        destinationBE.Should().Equal(expectedBE);

        NarrowBounds.TryReadLittleEndian(expectedLE, out var parsedLE).Should().BeTrue();
        NarrowBounds.TryReadBigEndian(expectedBE, out var parsedBE).Should().BeTrue();
        parsedLE.Should().Be(value);
        parsedBE.Should().Be(value);
    }

    [Theory]
    [InlineData(0x1000000u, 0, 0L, 0u)]
    [InlineData(0u, 8_388_608, 0L, 0u)]
    [InlineData(0u, -8_388_609, 0L, 0u)]
    [InlineData(0u, 0, 549_755_813_888L, 0u)]
    [InlineData(0u, 0, -549_755_813_889L, 0u)]
    [InlineData(0u, 0, 0L, 0x1000000u)]
    public void ScalarOutOfRange_ShouldNotWrite(uint unsignedValue, int signedValue, long wideValue, uint classOfDevice)
    {
        var value = new NarrowBounds(unsignedValue, signedValue, wideValue, (ClassOfDevice)classOfDevice);
        var untouched = Enumerable.Repeat((byte)0xA5, 14).ToArray();

        var destinationLE = untouched.ToArray();
        var destinationBE = untouched.ToArray();
        value.TryWriteLittleEndian(destinationLE, out var writtenLE).Should().BeFalse();
        value.TryWriteBigEndian(destinationBE, out var writtenBE).Should().BeFalse();
        writtenLE.Should().Be(0);
        writtenBE.Should().Be(0);
        destinationLE.Should().Equal(untouched);
        destinationBE.Should().Equal(untouched);
    }

    [Theory]
    [InlineData(true, "020000010000FEFFFF000020002000665544332211A6A5A4A3A2A1563412")]
    [InlineData(false, "000002000001FFFFFE200000002000112233445566A1A2A3A4A5A6123456")]
    public void Collections_ShouldUseDeclaredElementByteCount(bool littleEndian, string hex)
    {
        var expected = Convert.FromHexString(hex);
        var value = new NarrowCollections(
            2,
            [1, -2],
            [ClassOfDevice.Audio, ClassOfDevice.LimitedDiscoverable],
            [0x112233445566, 0xA1A2A3A4A5A6],
            new uint[] { 0x123456 }
        );

        var destination = new byte[expected.Length];
        var written = littleEndian
            ? value.TryWriteLittleEndian(destination, out var bytesWritten)
            : value.TryWriteBigEndian(destination, out bytesWritten);
        written.Should().BeTrue();
        bytesWritten.Should().Be(expected.Length);
        value.GetByteCount().Should().Be(expected.Length);
        destination.Should().Equal(expected);

        // An incomplete trailing element is not consumed
        byte[] source = [.. expected, 0xAB, 0xCD];
        var read = littleEndian
            ? NarrowCollections.TryReadLittleEndian(source, out var parsed, out var bytesRead)
            : NarrowCollections.TryReadBigEndian(source, out parsed, out bytesRead);
        read.Should().BeTrue();
        bytesRead.Should().Be(expected.Length);
        Assert.NotNull(parsed);
        parsed.Count.Should().Be(2);
        parsed.Counted.Should().Equal(value.Counted);
        parsed.Fixed.Should().Equal(value.Fixed);
        parsed.Interface.Should().Equal(value.Interface);
        parsed.Remaining.ToArray().Should().Equal(value.Remaining.ToArray());
    }

    [Theory]
    [InlineData(true, "FFFFFF030201000020002000FEFF0200")]
    [InlineData(false, "FFFFFF010203200000002000FFFE0002")]
    public void CollectionKinds_ShouldUseDeclaredElementByteCount(bool littleEndian, string hex)
    {
        var expected = Convert.FromHexString(hex);
        var value = new NarrowCollectionKinds(
            [-1, 0x010203],
            new ReadOnlyCollection<ClassOfDevice>([ClassOfDevice.Audio, ClassOfDevice.LimitedDiscoverable]),
            [SignedLevel.Low, SignedLevel.High]
        );

        var destination = new byte[expected.Length];
        var written = littleEndian
            ? value.TryWriteLittleEndian(destination, out var bytesWritten)
            : value.TryWriteBigEndian(destination, out bytesWritten);
        written.Should().BeTrue();
        bytesWritten.Should().Be(expected.Length);
        destination.Should().Equal(expected);

        var read = littleEndian
            ? NarrowCollectionKinds.TryReadLittleEndian(expected, out var parsed, out var bytesRead)
            : NarrowCollectionKinds.TryReadBigEndian(expected, out parsed, out bytesRead);
        read.Should().BeTrue();
        bytesRead.Should().Be(expected.Length);
        Assert.NotNull(parsed);
        parsed.List.Should().Equal(value.List);
        parsed.EnumCollection.Should().Equal(value.EnumCollection);
        parsed.SignedEnums.Should().Equal(value.SignedEnums);
    }

    [Theory]
    [InlineData(true, "3412010000A5A5A5")]
    [InlineData(false, "1234000001A5A5A5")]
    public void ElementOutOfRange_ShouldStopAtElement(bool littleEndian, string hex)
    {
        var value = new ScalarThenNarrowArray(0x1234, [1, 0x1000000]);
        var destination = Enumerable.Repeat((byte)0xA5, 8).ToArray();

        var written = littleEndian
            ? value.TryWriteLittleEndian(destination, out var bytesWritten)
            : value.TryWriteBigEndian(destination, out bytesWritten);

        written.Should().BeFalse();
        bytesWritten.Should().Be(5);
        destination.Should().Equal(Convert.FromHexString(hex));
    }
}

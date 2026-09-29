namespace Darp.BinaryObjects.Tests.Generated;

using System.Buffers.Binary;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using FluentAssertions;

[BinaryObject]
public sealed partial record ListUShortRemainingLength(
    [property: BinaryMinElementCount(2)] IReadOnlyList<ushort> Value
);

[BinaryObject]
internal sealed partial record ObjectUShortList(List<OneUShort> Value);

[BinaryObject]
internal sealed partial record ObjectUShortArray(OneUShort[] Value);

[BinaryObject]
internal sealed partial record RemainingPairCollection(byte Prefix, IReadOnlyCollection<ManualFixedPair> Values);

public class ListUShortRemainingLengthTests
{
    [Theory]
    [InlineData(true, 0xFF, false, 5, "A11234A5FFA5A5A5", "A1123400FF7800")]
    [InlineData(false, 0xFF, false, 5, "A11234A5FFA5A5A5", "A1123400FF7800")]
    [InlineData(true, 0x56, true, 7, "A11234A55678A5A5", "A1123400567800")]
    [InlineData(false, 0x56, true, 7, "A11234A55678A5A5", "A1123400567800")]
    public void ObjectCollection_ShouldPreserveDeclaredStrideAndChildProgress(
        bool littleEndian,
        byte secondFirst,
        bool expectedSuccess,
        int expectedProgress,
        string destinationHex,
        string sourceHex
    )
    {
        var value = new RemainingPairCollection(
            0xA1,
            new System.Collections.ObjectModel.Collection<ManualFixedPair> { new(0x12, 0x34), new(secondFirst, 0x78) }
        );
        var destination = Enumerable.Repeat((byte)0xA5, 8).ToArray();
        var written = littleEndian
            ? value.TryWriteLittleEndian(destination, out var bytesWritten)
            : value.TryWriteBigEndian(destination, out bytesWritten);
        written.Should().Be(expectedSuccess);
        bytesWritten.Should().Be(expectedProgress);
        value.GetByteCount().Should().Be(7);
        destination.Should().Equal(Convert.FromHexString(destinationHex));

        var source = Convert.FromHexString(sourceHex);
        var read = littleEndian
            ? RemainingPairCollection.TryReadLittleEndian(source, out var parsed, out var bytesRead)
            : RemainingPairCollection.TryReadBigEndian(source, out parsed, out bytesRead);
        read.Should().Be(expectedSuccess);
        bytesRead.Should().Be(expectedProgress);
        if (expectedSuccess)
        {
            Assert.NotNull(parsed);
            parsed.Values.Should().Equal(new ManualFixedPair(0x12, 0x34), new ManualFixedPair(0x56, 0x78));
        }
        else
            parsed.Should().BeNull();
    }

    [Theory]
    [InlineData("", "")]
    [InlineData("3412", "1234", 0x1234)]
    [InlineData("3412CDAB", "1234ABCD", 0x1234, 0xABCD)]
    public void ObjectList_ShouldReadElementsInWireOrder(string hexLE, string hexBE, params int[] expectedValues)
    {
        var sourceLE = Convert.FromHexString(hexLE);
        var sourceBE = Convert.FromHexString(hexBE);

        ObjectUShortList.TryReadLittleEndian(sourceLE, out var valueLE, out var bytesReadLE).Should().BeTrue();
        ObjectUShortList.TryReadBigEndian(sourceBE, out var valueBE, out var bytesReadBE).Should().BeTrue();

        Assert.NotNull(valueLE);
        Assert.NotNull(valueBE);
        valueLE.Value.Select(x => x.Value).Should().Equal(expectedValues.Select(x => (ushort)x));
        valueBE.Value.Select(x => x.Value).Should().Equal(expectedValues.Select(x => (ushort)x));
        bytesReadLE.Should().Be(sourceLE.Length);
        bytesReadBE.Should().Be(sourceBE.Length);
        valueLE.GetByteCount().Should().Be(sourceLE.Length);
        valueBE.GetByteCount().Should().Be(sourceBE.Length);
    }

    [Theory]
    [InlineData(true, "3412CDAB")]
    [InlineData(false, "1234ABCD")]
    public void ObjectCollections_ShouldLeaveSurplusDestinationUntouched(bool littleEndian, string hexString)
    {
        OneUShort[] values = [new(0x1234), new(0xABCD)];
        IBinaryWritable[] writables = [new ObjectUShortArray(values), new ObjectUShortList(values.ToList())];
        var expectedBytes = Convert.FromHexString(hexString).Concat(new byte[] { 0xA5, 0xA5, 0xA5 });
        foreach (IBinaryWritable writable in writables)
        {
            var destination = Enumerable.Repeat((byte)0xA5, 7).ToArray();
            int bytesWritten;
            var written = littleEndian
                ? writable.TryWriteLittleEndian(destination, out bytesWritten)
                : writable.TryWriteBigEndian(destination, out bytesWritten);

            written.Should().BeTrue();
            bytesWritten.Should().Be(4);
            writable.GetByteCount().Should().Be(4);
            destination.Should().Equal(expectedBytes);
        }
    }

    [Theory]
    [InlineData("01000001", 4, 0x0001, 0x0100)]
    [InlineData("010001000100", 6, 0x0001, 0x0001, 0x0001)]
    [InlineData("FFAAFFAA", 4, 0xAAFF, 0xAAFF)]
    [InlineData("FFFFFFFF00", 4, 0xFFFF, 0xFFFF)]
    public void TryRead_GoodInputShouldBeValid(string hexString, int expectedBytesRead, params int[] expectedValue)
    {
        var buffer = Convert.FromHexString(hexString);
        var expectedValueBE = new ushort[expectedValue.Length];
        BinaryPrimitives.ReverseEndianness(expectedValue.Select(x => (ushort)x).ToArray(), expectedValueBE);

        var successLE1 = ListUShortRemainingLength.TryReadLittleEndian(buffer, out ListUShortRemainingLength? valueLE1);
        var successLE2 = ListUShortRemainingLength.TryReadLittleEndian(
            buffer,
            out ListUShortRemainingLength? valueLE2,
            out var consumedLE
        );
        var successBE1 = ListUShortRemainingLength.TryReadBigEndian(buffer, out ListUShortRemainingLength? valueBE1);
        var successBE2 = ListUShortRemainingLength.TryReadBigEndian(
            buffer,
            out ListUShortRemainingLength? valueBE2,
            out var consumedBE
        );

        successLE1.Should().BeTrue();
        successLE2.Should().BeTrue();
        successBE1.Should().BeTrue();
        successBE2.Should().BeTrue();
        valueLE1!.Value.Should().BeEquivalentTo(expectedValue);
        valueLE1.GetByteCount().Should().Be(expectedBytesRead);
        valueLE2!.Value.Should().BeEquivalentTo(expectedValue);
        valueLE2.GetByteCount().Should().Be(expectedBytesRead);
        valueBE1!.Value.Should().BeEquivalentTo(expectedValueBE);
        valueBE1.GetByteCount().Should().Be(expectedBytesRead);
        valueBE2!.Value.Should().BeEquivalentTo(expectedValueBE);
        valueBE2.GetByteCount().Should().Be(expectedBytesRead);
        consumedLE.Should().Be(expectedBytesRead);
        consumedBE.Should().Be(expectedBytesRead);
    }

    [Theory]
    [InlineData("")]
    [InlineData("00")]
    [InlineData("000000")]
    public void TryRead_BadInputShouldBeValid(string hexString)
    {
        var buffer = Convert.FromHexString(hexString);

        var successLE1 = ListUShortRemainingLength.TryReadLittleEndian(buffer, out ListUShortRemainingLength? valueLE1);
        var successLE2 = ListUShortRemainingLength.TryReadLittleEndian(
            buffer,
            out ListUShortRemainingLength? valueLE2,
            out var consumedLE
        );
        var successBE1 = ListUShortRemainingLength.TryReadBigEndian(buffer, out ListUShortRemainingLength? valueBE1);
        var successBE2 = ListUShortRemainingLength.TryReadBigEndian(
            buffer,
            out ListUShortRemainingLength? valueBE2,
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
    [InlineData("00000000", 4, 4, 4, "00000000", "00000000")]
    [InlineData("01000100", 4, 4, 4, "01000100", "00010001")]
    [InlineData("00100100", 4, 4, 4, "00100100", "10000001")]
    [InlineData("FFFFAAAA", 4, 4, 4, "FFFFAAAA", "FFFFAAAA")]
    [InlineData("FFFFFFFFFFFF", 7, 6, 6, "FFFFFFFFFFFF00", "FFFFFFFFFFFF00")]
    public void TryWrite_GoodInputShouldBeValid(
        string valueHexString,
        int bufferSize,
        int expectedWriteCount,
        int expectedBytesWritten,
        string expectedBufferHexStringLE,
        string expectedBufferHexStringBE
    )
    {
        var value = MemoryMarshal.Cast<byte, ushort>(Convert.FromHexString(valueHexString)).ToArray();
        var bufferLE = new byte[bufferSize];
        var bufferBE = new byte[bufferSize];
        var expectedValueLE = Convert.FromHexString(expectedBufferHexStringLE);
        var expectedValueBE = Convert.FromHexString(expectedBufferHexStringBE);

        var writable = new ListUShortRemainingLength(value);

        var successLE1 = writable.TryWriteLittleEndian(bufferLE);
        var successLE2 = writable.TryWriteLittleEndian(bufferLE, out var writtenLE);
        var successBE1 = writable.TryWriteBigEndian(bufferBE);
        var successBE2 = writable.TryWriteBigEndian(bufferBE, out var writtenBE);

        successLE1.Should().BeTrue();
        successLE2.Should().BeTrue();
        successBE1.Should().BeTrue();
        successBE2.Should().BeTrue();
        bufferLE.Should().BeEquivalentTo(expectedValueLE);
        bufferBE.Should().BeEquivalentTo(expectedValueBE);
        writtenLE.Should().Be(expectedBytesWritten);
        writtenBE.Should().Be(expectedBytesWritten);
        writable.GetByteCount().Should().Be(expectedWriteCount);
    }

    [Theory]
    [InlineData("", 0, "")]
    [InlineData("1234", 4, "00000000")]
    [InlineData("01000004", 3, "000000")]
    [InlineData("0100000400000008", 7, "00000000000000")]
    public void TryWrite_BadInputShouldBeValid(string valueHexString, int bufferSize, string expectedHexString)
    {
        var value = MemoryMarshal.Cast<byte, ushort>(Convert.FromHexString(valueHexString)).ToArray();
        var bufferLE = new byte[bufferSize];
        var bufferBE = new byte[bufferSize];
        var expectedHexBytes = Convert.FromHexString(expectedHexString);
        var writable = new ListUShortRemainingLength(value);

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

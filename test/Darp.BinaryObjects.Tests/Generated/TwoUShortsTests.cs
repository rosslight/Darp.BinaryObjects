namespace Darp.BinaryObjects.Tests.Generated;

using System.Diagnostics.CodeAnalysis;
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

internal sealed class ManualUInt24(uint value) : IBinaryObject<ManualUInt24>
{
    public uint Value { get; } = value;

    int IBinaryWritable.GetByteCount() => 3;

    public bool TryWriteLittleEndian(Span<byte> destination) => TryWriteLittleEndian(destination, out _);

    public bool TryWriteLittleEndian(Span<byte> destination, out int bytesWritten)
    {
        bytesWritten = Math.Min(destination.Length, 3);
        for (var i = 0; i < bytesWritten; i++)
            destination[i] = (byte)(Value >> (i * 8));
        return bytesWritten == 3;
    }

    public bool TryWriteBigEndian(Span<byte> destination) =>
        ((IBinaryWritable)this).TryWriteBigEndian(destination, out _);

    bool IBinaryWritable.TryWriteBigEndian(Span<byte> destination, out int bytesWritten)
    {
        bytesWritten = Math.Min(destination.Length, 3);
        for (var i = 0; i < bytesWritten; i++)
            destination[i] = (byte)(Value >> ((2 - i) * 8));
        return bytesWritten == 3;
    }

    public static bool TryReadLittleEndian(ReadOnlySpan<byte> source, [NotNullWhen(true)] out ManualUInt24? value) =>
        ReadLittleEndian(source, out value, out _);

    static bool IBinaryReadable<ManualUInt24>.TryReadLittleEndian(
        ReadOnlySpan<byte> source,
        [NotNullWhen(true)] out ManualUInt24? value,
        out int bytesRead
    ) => ReadLittleEndian(source, out value, out bytesRead);

    private static bool ReadLittleEndian(
        ReadOnlySpan<byte> source,
        [NotNullWhen(true)] out ManualUInt24? value,
        out int bytesRead
    )
    {
        value = null;
        bytesRead = Math.Min(source.Length, 3);
        if (source.Length < 3)
            return false;
        value = new ManualUInt24((uint)(source[0] | source[1] << 8 | source[2] << 16));
        return true;
    }

    public static bool TryReadBigEndian(ReadOnlySpan<byte> source, [NotNullWhen(true)] out ManualUInt24? value) =>
        TryReadBigEndian(source, out value, out _);

    public static bool TryReadBigEndian(
        ReadOnlySpan<byte> source,
        [NotNullWhen(true)] out ManualUInt24? value,
        out int bytesRead
    )
    {
        value = null;
        bytesRead = Math.Min(source.Length, 3);
        if (source.Length < 3)
            return false;
        value = new ManualUInt24((uint)(source[0] << 16 | source[1] << 8 | source[2]));
        return true;
    }
}

[BinaryObject]
internal sealed partial record NestedManualUInt24(byte Prefix, ManualUInt24 Value, byte Tail);

// Two payload bytes in a three-byte fixed slot; the parent owns the reserved byte.
[BinaryConstant(3)]
internal sealed record ManualFixedPair(byte First, byte Second) : IBinaryObject<ManualFixedPair>
{
    public int GetByteCount() => 2;

    public bool TryWriteLittleEndian(Span<byte> destination) => TryWriteLittleEndian(destination, out _);

    public bool TryWriteLittleEndian(Span<byte> destination, out int bytesWritten)
    {
        bytesWritten = 0;
        if (destination.Length < 2)
            return false;
        destination[0] = First;
        bytesWritten = 1;
        if (First == 0xFF)
            return false;
        if (First == 0xEE)
            throw new InvalidOperationException("Rejected pair");
        destination[1] = Second;
        bytesWritten = 2;
        return true;
    }

    public bool TryWriteBigEndian(Span<byte> destination) => TryWriteLittleEndian(destination, out _);

    public bool TryWriteBigEndian(Span<byte> destination, out int bytesWritten) =>
        TryWriteLittleEndian(destination, out bytesWritten);

    public static bool TryReadLittleEndian(ReadOnlySpan<byte> source, [NotNullWhen(true)] out ManualFixedPair? value) =>
        TryReadLittleEndian(source, out value, out _);

    public static bool TryReadLittleEndian(
        ReadOnlySpan<byte> source,
        [NotNullWhen(true)] out ManualFixedPair? value,
        out int bytesRead
    )
    {
        value = null;
        bytesRead = 0;
        if (source.Length < 2)
            return false;
        bytesRead = 1;
        if (source[0] == 0xFF)
            return false;
        if (source[0] == 0xEE)
            throw new FormatException("Rejected pair");
        value = new ManualFixedPair(source[0], source[1]);
        bytesRead = 2;
        return true;
    }

    public static bool TryReadBigEndian(ReadOnlySpan<byte> source, [NotNullWhen(true)] out ManualFixedPair? value) =>
        TryReadLittleEndian(source, out value, out _);

    public static bool TryReadBigEndian(
        ReadOnlySpan<byte> source,
        [NotNullWhen(true)] out ManualFixedPair? value,
        out int bytesRead
    ) => TryReadLittleEndian(source, out value, out bytesRead);
}

[BinaryObject]
internal sealed partial record NestedManualFixedPair(byte Prefix, ManualFixedPair Value, byte Tail);

internal readonly ref struct ManualBytes(byte[] data) : IBinaryWritable
{
    public byte[] Data { get; } = data;

    public int GetByteCount() => Data.Length;

    public bool TryWriteLittleEndian(Span<byte> destination) => TryWriteLittleEndian(destination, out _);

    public bool TryWriteLittleEndian(Span<byte> destination, out int bytesWritten)
    {
        bytesWritten = 0;
        if (!Data.AsSpan().TryCopyTo(destination))
            return false;
        bytesWritten = Data.Length;
        return true;
    }

    public bool TryWriteBigEndian(Span<byte> destination) => TryWriteLittleEndian(destination, out _);

    public bool TryWriteBigEndian(Span<byte> destination, out int bytesWritten) =>
        TryWriteLittleEndian(destination, out bytesWritten);
}

[BinaryObject(BinaryOptions.Write)]
internal ref partial struct NestedManualBytes
{
    public byte Prefix { get; init; }
    public ManualBytes Value { get; init; }
    public byte Tail { get; init; }
}

public class TwoUShortsTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void FixedSizeChild_ShouldPropagateFailureAndProgress(bool littleEndian)
    {
        var source = Convert.FromHexString("A1FF3400EF");
        var read = littleEndian
            ? NestedManualFixedPair.TryReadLittleEndian(source, out var value, out var bytesRead)
            : NestedManualFixedPair.TryReadBigEndian(source, out value, out bytesRead);
        read.Should().BeFalse();
        value.Should().BeNull();
        bytesRead.Should().Be(2);

        var writable = new NestedManualFixedPair(0xA1, new ManualFixedPair(0xFF, 0x34), 0xEF);
        var destination = Enumerable.Repeat((byte)0xA5, 5).ToArray();
        var written = littleEndian
            ? writable.TryWriteLittleEndian(destination, out var bytesWritten)
            : writable.TryWriteBigEndian(destination, out bytesWritten);
        written.Should().BeFalse();
        bytesWritten.Should().Be(2);
        destination.Should().Equal(0xA1, 0xFF, 0xA5, 0xA5, 0xA5);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void FixedSizeChild_ShouldPreserveDeclaredOffsetsAndBufferGuard(bool littleEndian)
    {
        var source = Convert.FromHexString("A1123400EF");
        var read = littleEndian
            ? NestedManualFixedPair.TryReadLittleEndian(source, out var value, out var bytesRead)
            : NestedManualFixedPair.TryReadBigEndian(source, out value, out bytesRead);
        read.Should().BeTrue();
        Assert.NotNull(value);
        value.Value.Should().Be(new ManualFixedPair(0x12, 0x34));
        value.Tail.Should().Be(0xEF);
        bytesRead.Should().Be(5);
        value.GetByteCount().Should().Be(5);

        var destination = Enumerable.Repeat((byte)0xA5, 6).ToArray();
        var written = littleEndian
            ? value.TryWriteLittleEndian(destination, out var bytesWritten)
            : value.TryWriteBigEndian(destination, out bytesWritten);
        written.Should().BeTrue();
        bytesWritten.Should().Be(5);
        destination.Should().Equal(0xA1, 0x12, 0x34, 0xA5, 0xEF, 0xA5);

        var shortSource = source[..4];
        var shortRead = littleEndian
            ? NestedManualFixedPair.TryReadLittleEndian(shortSource, out _, out bytesRead)
            : NestedManualFixedPair.TryReadBigEndian(shortSource, out _, out bytesRead);
        shortRead.Should().BeFalse();
        bytesRead.Should().Be(0);
        var shortDestination = Enumerable.Repeat((byte)0xA5, 4).ToArray();
        var shortWrite = littleEndian
            ? value.TryWriteLittleEndian(shortDestination, out bytesWritten)
            : value.TryWriteBigEndian(shortDestination, out bytesWritten);
        shortWrite.Should().BeFalse();
        bytesWritten.Should().Be(0);
        shortDestination.Should().Equal(0xA5, 0xA5, 0xA5, 0xA5);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void FixedSizeChild_ShouldPreserveChildExceptions(bool littleEndian)
    {
        var source = Convert.FromHexString("A1EE3400EF");
        Action read = () =>
        {
            if (littleEndian)
                NestedManualFixedPair.TryReadLittleEndian(source, out _);
            else
                NestedManualFixedPair.TryReadBigEndian(source, out _);
        };
        read.Should().Throw<FormatException>().WithMessage("Rejected pair");

        var value = new NestedManualFixedPair(0xA1, new ManualFixedPair(0xEE, 0x34), 0xEF);
        Action write = () =>
        {
            if (littleEndian)
                value.TryWriteLittleEndian(source);
            else
                value.TryWriteBigEndian(source);
        };
        write.Should().Throw<InvalidOperationException>().WithMessage("Rejected pair");
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ManualRefStruct_ShouldRemainComposable(bool littleEndian)
    {
        var value = new NestedManualBytes
        {
            Prefix = 0xA1,
            Value = new ManualBytes([0x12, 0x34]),
            Tail = 0xEF,
        };
        var destination = Enumerable.Repeat((byte)0xA5, 5).ToArray();
        var written = littleEndian
            ? value.TryWriteLittleEndian(destination, out var bytesWritten)
            : value.TryWriteBigEndian(destination, out bytesWritten);

        value.GetByteCount().Should().Be(4);
        written.Should().BeTrue();
        bytesWritten.Should().Be(4);
        destination.Should().Equal(0xA1, 0x12, 0x34, 0xEF, 0xA5);
    }

    [Theory]
    [InlineData(true, "A1563412EF")]
    [InlineData(false, "A1123456EF")]
    public void ManualObject_ShouldUseReportedCountsAndPreserveTrailingField(bool littleEndian, string hexString)
    {
        var source = Convert.FromHexString(hexString);
        NestedManualUInt24? value;
        int bytesRead;
        var read = littleEndian
            ? NestedManualUInt24.TryReadLittleEndian(source, out value, out bytesRead)
            : NestedManualUInt24.TryReadBigEndian(source, out value, out bytesRead);

        read.Should().BeTrue();
        Assert.NotNull(value);
        value.Prefix.Should().Be(0xA1);
        value.Value.Value.Should().Be(0x123456);
        value.Tail.Should().Be(0xEF);
        bytesRead.Should().Be(5);
        value.GetByteCount().Should().Be(5);

        var writable = new NestedManualUInt24(0xA1, new ManualUInt24(0x123456), 0xEF);
        var destination = Enumerable.Repeat((byte)0xA5, 6).ToArray();
        int bytesWritten;
        var written = littleEndian
            ? writable.TryWriteLittleEndian(destination, out bytesWritten)
            : writable.TryWriteBigEndian(destination, out bytesWritten);

        written.Should().BeTrue();
        bytesWritten.Should().Be(5);
        destination.Should().Equal(source.Concat(new byte[] { 0xA5 }));
    }

    [Theory]
    [InlineData(true, "", 0)]
    [InlineData(false, "", 0)]
    [InlineData(true, "A1", 1)]
    [InlineData(false, "A1", 1)]
    [InlineData(true, "A15634", 3)]
    [InlineData(false, "A11234", 3)]
    [InlineData(true, "A1563412", 4)]
    [InlineData(false, "A1123456", 4)]
    public void ManualObject_ShouldReturnFalseForTruncatedBuffers(
        bool littleEndian,
        string hexString,
        int expectedProgress
    )
    {
        var source = Convert.FromHexString(hexString);
        NestedManualUInt24? value;
        int bytesRead;
        var read = littleEndian
            ? NestedManualUInt24.TryReadLittleEndian(source, out value, out bytesRead)
            : NestedManualUInt24.TryReadBigEndian(source, out value, out bytesRead);

        read.Should().BeFalse();
        value.Should().BeNull();
        bytesRead.Should().Be(expectedProgress);

        var writable = new NestedManualUInt24(0xA1, new ManualUInt24(0x123456), 0xEF);
        var destination = Enumerable.Repeat((byte)0xA5, source.Length).ToArray();
        int bytesWritten;
        var written = littleEndian
            ? writable.TryWriteLittleEndian(destination, out bytesWritten)
            : writable.TryWriteBigEndian(destination, out bytesWritten);

        written.Should().BeFalse();
        bytesWritten.Should().Be(expectedProgress);
        destination.Take(expectedProgress).Should().Equal(source.Take(expectedProgress));
        destination
            .Skip(expectedProgress)
            .Should()
            .Equal(Enumerable.Repeat((byte)0xA5, source.Length - expectedProgress));
    }

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

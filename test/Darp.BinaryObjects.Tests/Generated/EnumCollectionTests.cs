namespace Darp.BinaryObjects.Tests.Generated;

using FluentAssertions;

internal enum UInt64EnumValue : ulong
{
    First = 0x0102030405060708,
    Second = 0x1112131415161718,
}

internal enum UInt16EnumValue : ushort
{
    First = 0x1234,
    Second = 0xABCD,
}

internal enum SignedByteEnumValue : sbyte
{
    Negative = -1,
    Positive = 1,
}

[BinaryObject]
internal sealed partial record UInt64EnumPayload([property: BinaryElementCount(2)] UInt64EnumValue[] Values);

[BinaryObject]
internal sealed partial record CountedAndRemainingEnums(
    byte Count,
    [property: BinaryElementCount("Count")] UInt16EnumValue[] Counted,
    SignedByteEnumValue[] Remaining
);

[BinaryObject]
internal sealed partial record FixedEnumEnumerable([property: BinaryElementCount(2)] IEnumerable<UInt16EnumValue> Values);

[BinaryObject]
internal sealed partial record RemainingEnumEnumerable(IEnumerable<UInt16EnumValue> Values);

public sealed class EnumCollectionTests
{
    [Theory]
    [InlineData(true, "023412CDABFF01")]
    [InlineData(false, "021234ABCDFF01")]
    public void CountedAndRemainingEnums_ShouldRoundTrip(bool littleEndian, string hexBytes)
    {
        var expected = Convert.FromHexString(hexBytes);
        var value = new CountedAndRemainingEnums(
            2,
            [UInt16EnumValue.First, UInt16EnumValue.Second],
            [SignedByteEnumValue.Negative, SignedByteEnumValue.Positive]
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
            ? CountedAndRemainingEnums.TryReadLittleEndian(expected, out var parsed, out var bytesRead)
            : CountedAndRemainingEnums.TryReadBigEndian(expected, out parsed, out bytesRead);
        read.Should().BeTrue();
        bytesRead.Should().Be(expected.Length);
        Assert.NotNull(parsed);
        parsed.Counted.Should().Equal(UInt16EnumValue.First, UInt16EnumValue.Second);
        parsed.Remaining.Should().Equal(SignedByteEnumValue.Negative, SignedByteEnumValue.Positive);
    }

    [Fact]
    public void FixedEnumEnumerable_ShouldNotAdvancePastItsCount()
    {
        var value = new FixedEnumEnumerable(EnumerateValues());
        var destination = new byte[4];

        value.TryWriteLittleEndian(destination, out var bytesWritten).Should().BeTrue();
        bytesWritten.Should().Be(4);
        destination.Should().Equal(Convert.FromHexString("3412CDAB"));
    }

    [Fact]
    public void FixedEnumEnumerable_ShouldFailWhenAnElementIsMissing()
    {
        var value = new FixedEnumEnumerable([UInt16EnumValue.First]);
        var destination = new byte[4];

        value.TryWriteLittleEndian(destination, out var bytesWritten).Should().BeFalse();
        bytesWritten.Should().Be(2);
        destination.Should().Equal(Convert.FromHexString("34120000"));
    }

    private static IEnumerable<UInt16EnumValue> EnumerateValues()
    {
        yield return UInt16EnumValue.First;
        yield return UInt16EnumValue.Second;
        throw new InvalidOperationException("The writer advanced past the declared element count.");
    }

    [Fact]
    public void RemainingEnumEnumerable_ShouldWriteSinglePassSequence()
    {
        var value = new RemainingEnumEnumerable(new SinglePassEnumSequence());
        var destination = new byte[4];

        value.TryWriteLittleEndian(destination, out var bytesWritten).Should().BeTrue();
        bytesWritten.Should().Be(4);
        destination.Should().Equal(Convert.FromHexString("3412CDAB"));
    }

    private sealed class SinglePassEnumSequence : IEnumerable<UInt16EnumValue>
    {
        private bool _used;

        public IEnumerator<UInt16EnumValue> GetEnumerator()
        {
            if (_used)
                throw new InvalidOperationException("The sequence was enumerated twice.");
            _used = true;
            return ((IEnumerable<UInt16EnumValue>)new[] { UInt16EnumValue.First, UInt16EnumValue.Second }).GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }

    [Theory]
    [InlineData(true, "08070605040302011817161514131211")]
    [InlineData(false, "01020304050607081112131415161718")]
    public void UInt64EnumArray_ShouldRoundTrip(bool littleEndian, string hexBytes)
    {
        var bytes = Convert.FromHexString(hexBytes);
        UInt64EnumPayload? value;
        int bytesRead;
        var success = littleEndian
            ? UInt64EnumPayload.TryReadLittleEndian(bytes, out value, out bytesRead)
            : UInt64EnumPayload.TryReadBigEndian(bytes, out value, out bytesRead);

        success.Should().BeTrue();
        bytesRead.Should().Be(bytes.Length);
        value!.Values.Should().Equal(UInt64EnumValue.First, UInt64EnumValue.Second);
        value.GetByteCount().Should().Be(bytes.Length);

        var destination = new byte[bytes.Length];
        int bytesWritten;
        var written = littleEndian
            ? value.TryWriteLittleEndian(destination, out bytesWritten)
            : value.TryWriteBigEndian(destination, out bytesWritten);

        written.Should().BeTrue();
        bytesWritten.Should().Be(bytes.Length);
        destination.Should().Equal(bytes);
    }

    [Fact]
    public void UInt64EnumArray_ShouldRejectMissingElementBeforeWriting()
    {
        var value = new UInt64EnumPayload([UInt64EnumValue.First]);
        var destination = new byte[16];

        value.TryWriteLittleEndian(destination, out var bytesWritten).Should().BeFalse();
        bytesWritten.Should().Be(0);
        destination.Should().Equal(new byte[16]);
    }
}

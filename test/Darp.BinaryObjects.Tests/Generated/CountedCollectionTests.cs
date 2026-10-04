namespace Darp.BinaryObjects.Tests.Generated;

using System.Collections;
using System.Collections.ObjectModel;
using FluentAssertions;

[BinaryObject]
internal sealed partial record CollectionInterfaces(
    [property: BinaryElementCount(2)] ICollection<byte> Bytes,
    [property: BinaryElementCount(2)] IReadOnlyCollection<UInt16EnumValue> Enums,
    [property: BinaryElementCount(2)] IList<bool> Flags,
    [property: BinaryElementCount(2)] IReadOnlyList<OneUShort> Objects
);

[BinaryObject]
internal sealed partial record VariableCollections(
    byte Count,
    [property: BinaryElementCount("Count")] IReadOnlyCollection<ushort> Values,
    [property: BinaryElementCount("Count")] ICollection<UInt16EnumValue> Enums,
    [property: BinaryElementCount("Count")] IReadOnlyList<OneUShort> Objects,
    byte Suffix
);

[BinaryObject]
internal sealed partial record RemainingCollection(IReadOnlyCollection<ushort> Values);

[BinaryObject]
internal sealed partial record FixedCollection([property: BinaryElementCount(2)] IReadOnlyCollection<ushort> Values);

[BinaryObject]
internal sealed partial record EmptyCollection([property: BinaryElementCount(0)] IReadOnlyCollection<ushort> Values);

public sealed class CountedCollectionTests
{
    [Theory]
    [InlineData(true, "01023412CDAB01003412CDAB")]
    [InlineData(false, "01021234ABCD01001234ABCD")]
    public void AllInterfaces_ShouldRoundTrip(bool littleEndian, string hex)
    {
        var value = new CollectionInterfaces(
            new Collection<byte> { 1, 2 },
            new ReadOnlyCollection<UInt16EnumValue>([UInt16EnumValue.First, UInt16EnumValue.Second]),
            new Collection<bool> { true, false },
            new ReadOnlyCollection<OneUShort>([new(0x1234), new(0xABCD)])
        );
        var bytes = new byte[value.GetByteCount()];
        var success = littleEndian
            ? value.TryWriteLittleEndian(bytes, out var written)
            : value.TryWriteBigEndian(bytes, out written);
        success.Should().BeTrue();
        written.Should().Be(bytes.Length);
        bytes.Should().Equal(Convert.FromHexString(hex));

        var read = littleEndian
            ? CollectionInterfaces.TryReadLittleEndian(bytes, out var parsed, out var consumed)
            : CollectionInterfaces.TryReadBigEndian(bytes, out parsed, out consumed);
        read.Should().BeTrue();
        consumed.Should().Be(bytes.Length);
        Assert.NotNull(parsed);
        parsed.Bytes.Should().Equal(value.Bytes);
        parsed.Enums.Should().Equal(value.Enums);
        parsed.Flags.Should().Equal(value.Flags);
        parsed.Objects.Should().Equal(value.Objects);
    }

    [Theory]
    [InlineData(true, "01341234123412AA")]
    [InlineData(false, "01123412341234AA")]
    public void MemberCounts_ShouldBoundEachCollection(bool littleEndian, string hex)
    {
        var value = new VariableCollections(
            1,
            new ReadOnlyCollection<ushort>([0x1234, 0xABCD]),
            new Collection<UInt16EnumValue> { UInt16EnumValue.First, UInt16EnumValue.Second },
            new ReadOnlyCollection<OneUShort>([new(0x1234), new(0xABCD)]),
            0xAA
        );
        var bytes = new byte[value.GetByteCount()];
        var success = littleEndian ? value.TryWriteLittleEndian(bytes) : value.TryWriteBigEndian(bytes);
        success.Should().BeTrue();
        bytes.Should().Equal(Convert.FromHexString(hex));
        var read = littleEndian
            ? VariableCollections.TryReadLittleEndian(bytes, out var parsed)
            : VariableCollections.TryReadBigEndian(bytes, out parsed);
        read.Should().BeTrue();
        Assert.NotNull(parsed);
        parsed.Values.Should().Equal(0x1234);
        parsed.Enums.Should().Equal(UInt16EnumValue.First);
        parsed.Objects.Should().Equal(new OneUShort(0x1234));
        parsed.Suffix.Should().Be(0xAA);
    }

    [Fact]
    public void Sizing_ShouldUseCountAndWritingShouldEnumerateOnce()
    {
        var collection = new ObservedCollection<ushort>([0x1234, 0xABCD]);
        var value = new RemainingCollection(collection);
        value.GetByteCount().Should().Be(4);
        collection.Enumerations.Should().Be(0);
        value.ToArrayLittleEndian().Should().Equal(0x34, 0x12, 0xCD, 0xAB);
        collection.Enumerations.Should().Be(1);
    }

    [Fact]
    public void ZeroCount_ShouldNotAdvanceTheCollection()
    {
        var collection = new ObservedCollection<ushort>([0x1234]);
        new EmptyCollection(collection).TryWriteLittleEndian([]).Should().BeTrue();
        collection.Moves.Should().Be(0);
    }

    [Fact]
    public void FixedCount_ShouldNotAdvancePastItsBoundary()
    {
        var collection = new ObservedCollection<ushort>([0x1234, 0xABCD, 0xFFFF]);
        new FixedCollection(collection).ToArrayLittleEndian().Should().Equal(0x34, 0x12, 0xCD, 0xAB);
        collection.Moves.Should().Be(2);
    }

    [Fact]
    public void ShortCollections_ShouldFailBeforeWritingTheGroup()
    {
        IBinaryWritable[] values =
        [
            new FixedCollection(new ReadOnlyCollection<ushort>([0x1234])),
            new CollectionInterfaces(
                new Collection<byte> { 1 },
                new ReadOnlyCollection<UInt16EnumValue>([UInt16EnumValue.First, UInt16EnumValue.Second]),
                new Collection<bool> { true, false },
                [new(1), new(2)]
            ),
            new CollectionInterfaces(
                new Collection<byte> { 1, 2 },
                new ReadOnlyCollection<UInt16EnumValue>([UInt16EnumValue.First, UInt16EnumValue.Second]),
                new Collection<bool> { true, false },
                [new(1)]
            ),
        ];
        foreach (var value in values)
        {
            var bytes = Enumerable.Repeat((byte)0xA5, value.GetByteCount()).ToArray();
            value.TryWriteLittleEndian(bytes, out var written).Should().BeFalse();
            written.Should().Be(0);
            bytes.Should().OnlyContain(x => x == 0xA5);
        }
    }

    private sealed class ObservedCollection<T>(T[] values) : IReadOnlyCollection<T>
    {
        private readonly T[] _values = values;
        public int Count => _values.Length;
        public int Enumerations { get; private set; }
        public int Moves { get; private set; }

        public IEnumerator<T> GetEnumerator()
        {
            Enumerations++;
            foreach (var value in _values)
            {
                Moves++;
                yield return value;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}

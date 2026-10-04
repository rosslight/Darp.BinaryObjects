namespace Darp.BinaryObjects.Tests.Generated;

using FluentAssertions;

[BinaryObject]
internal sealed partial class SelectedConstructor
{
    public SelectedConstructor() => Value = 0xFFFF;

    [BinaryConstructor]
    private SelectedConstructor(ushort value) => Value = value;

    public ushort Value { get; }
}

[BinaryObject]
internal sealed partial record SelectedConstructorEnvelope(
    [property: BinaryElementCount(2)] SelectedConstructor[] Values,
    byte Tail
);

[BinaryObject(BinaryOptions.Read)]
internal sealed partial record SelectedRecord(ushort Value)
{
    [BinaryConstructor]
    private SelectedRecord(byte tail, ushort value)
        : this(value) => Tail = tail;

    public byte Tail { get; }
}

[BinaryObject]
internal readonly partial struct SingleConstructorStruct
{
    static SingleConstructorStruct() { }

    private SingleConstructorStruct(byte value) => Value = value;

    public byte Value { get; }
}

public sealed class ConstructorSelectionTests
{
    [Theory]
    [InlineData(true, "3412EF")]
    [InlineData(false, "1234EF")]
    public void SelectedRecordConstructor_ShouldOverridePrimaryAndUseParameterOrder(bool littleEndian, string hex)
    {
        var source = Convert.FromHexString(hex);
        var success = littleEndian
            ? SelectedRecord.TryReadLittleEndian(source, out var value, out var consumed)
            : SelectedRecord.TryReadBigEndian(source, out value, out consumed);

        success.Should().BeTrue();
        Assert.NotNull(value);
        value.Value.Should().Be(0x1234);
        value.Tail.Should().Be(0xEF);
        consumed.Should().Be(3);
    }

    [Fact]
    public void SingleConstructor_ShouldIgnoreStaticAndImplicitStructConstructors()
    {
        SingleConstructorStruct.TryReadLittleEndian([42], out var value).Should().BeTrue();
        value.Value.Should().Be(42);
        value.ToArrayLittleEndian().Should().Equal(42);
    }

    [Theory]
    [InlineData(true, "3412CDABEF")]
    [InlineData(false, "1234ABCDEF")]
    public void SelectedConstructor_ShouldDefineReaderAndNestedElementSize(bool littleEndian, string hex)
    {
        var source = Convert.FromHexString(hex);
        var success = littleEndian
            ? SelectedConstructorEnvelope.TryReadLittleEndian(source, out var value, out var consumed)
            : SelectedConstructorEnvelope.TryReadBigEndian(source, out value, out consumed);

        success.Should().BeTrue();
        Assert.NotNull(value);
        consumed.Should().Be(5);
        value.Values.Select(x => x.Value).Should().Equal(0x1234, 0xABCD);
        value.Tail.Should().Be(0xEF);
        value.GetByteCount().Should().Be(5);
        var destination = new byte[5];
        var written = littleEndian ? value.TryWriteLittleEndian(destination) : value.TryWriteBigEndian(destination);
        written.Should().BeTrue();
        destination.Should().Equal(source);
    }
}

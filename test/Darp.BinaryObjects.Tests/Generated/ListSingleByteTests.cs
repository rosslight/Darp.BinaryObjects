namespace Darp.BinaryObjects.Tests.Generated;

using FluentAssertions;

[BinaryObject]
internal sealed partial record ListSingleByteElements(
    byte Count,
    [property: BinaryElementCount("Count")] List<sbyte> Signed,
    List<bool> Flags
);

public sealed class ListSingleByteTests
{
    [Fact]
    public void SByteAndBoolLists_ShouldRoundTrip()
    {
        var expected = Convert.FromHexString("02FF7F0100");
        var value = new ListSingleByteElements(2, [-1, 127], [true, false]);

        var destination = new byte[expected.Length];
        value.TryWriteLittleEndian(destination, out var bytesWritten).Should().BeTrue();
        bytesWritten.Should().Be(expected.Length);
        destination.Should().Equal(expected);

        ListSingleByteElements.TryReadBigEndian(expected, out var parsed, out var bytesRead).Should().BeTrue();
        bytesRead.Should().Be(expected.Length);
        Assert.NotNull(parsed);
        parsed.Signed.Should().Equal(-1, 127);
        parsed.Flags.Should().Equal(true, false);
    }
}

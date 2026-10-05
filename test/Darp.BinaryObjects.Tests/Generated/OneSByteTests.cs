namespace Darp.BinaryObjects.Tests.Generated;

using FluentAssertions;

[BinaryObject]
internal sealed partial record OneSByte(sbyte Value);

public sealed class OneSByteTests
{
    [Fact]
    public void NegativeValue_ShouldRoundTripWithOverflowChecking()
    {
        OneSByte.TryReadLittleEndian([0xFF], out var value).Should().BeTrue();
        Assert.NotNull(value);
        value.Value.Should().Be(-1);

        var destination = new byte[1];
        new OneSByte(-1).TryWriteLittleEndian(destination).Should().BeTrue();
        destination.Should().Equal(0xFF);
    }
}

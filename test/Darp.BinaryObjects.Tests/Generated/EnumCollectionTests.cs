namespace Darp.BinaryObjects.Tests.Generated;

using FluentAssertions;

internal enum UInt64EnumValue : ulong
{
    First = 0x0102030405060708,
    Second = 0x1112131415161718,
}

[BinaryObject]
internal sealed partial record UInt64EnumPayload([property: BinaryElementCount(2)] UInt64EnumValue[] Values);

public sealed class EnumCollectionTests
{
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
}

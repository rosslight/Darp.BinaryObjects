namespace Darp.BinaryObjects.Tests;

using BinaryObjects.Generated;
using FluentAssertions;

[BinaryObject]
internal readonly partial struct TestStruct()
{
    public required byte[] Value2 { get; init; }
    public required List<byte> Value3 { get; init; }
    public required IEnumerable<byte> Value4 { get; init; }
    public required ushort[] Value12 { get; init; }
    public required List<ushort> Value13 { get; init; }
    public required IEnumerable<ushort> Value14 { get; init; }
}

public class UtilitiesTests
{
    [Theory]
    [InlineData("010203", 0x01, 0x02, 0x03)]
    public void ReadUInt8_ShouldBeValid(string hexBytes, params int[] expectedResult)
    {
        var bytes = Convert.FromHexString(hexBytes);
        var expectedLength = bytes.Length;

        var arrayResult = Utilities.ReadUInt8Array(bytes, out var bytesRead1);
        List<byte> listResult = Utilities.ReadUInt8List(bytes, out var bytesRead2);

        arrayResult.Should().BeEquivalentTo(expectedResult);
        listResult.Should().BeEquivalentTo(expectedResult);
        bytesRead1.Should().Be(expectedLength);
        bytesRead2.Should().Be(expectedLength);
    }

    [Theory]
    [InlineData("01020304", 0x0201, 0x0403)]
    public void ReadUInt16ArrayLittleEndian_ShouldBeValid(string hexBytes, params int[] expectedResult)
    {
        var bytes = Convert.FromHexString(hexBytes);
        var expectedLength = bytes.Length;

        var arrayResult = Utilities.ReadUInt16ArrayLittleEndian(bytes, out var bytesRead1);
        List<ushort> listResult = Utilities.ReadUInt16ListLittleEndian(bytes, out var bytesRead2);

        arrayResult.Should().BeEquivalentTo(expectedResult);
        listResult.Should().BeEquivalentTo(expectedResult);
        bytesRead1.Should().Be(expectedLength);
        bytesRead2.Should().Be(expectedLength);
    }

    [Theory]
    [InlineData("01020304", 0x0102, 0x0304)]
    public void ReadUInt16ArrayBigEndian_ShouldBeValid(string hexBytes, params int[] expectedResult)
    {
        var bytes = Convert.FromHexString(hexBytes);
        var expectedLength = bytes.Length;

        var arrayResult = Utilities.ReadUInt16ArrayBigEndian(bytes, out var bytesRead1);
        List<ushort> listResult = Utilities.ReadUInt16ListBigEndian(bytes, out var bytesRead2);

        arrayResult.Should().BeEquivalentTo(expectedResult);
        listResult.Should().BeEquivalentTo(expectedResult);
        bytesRead1.Should().Be(expectedLength);
        bytesRead2.Should().Be(expectedLength);
    }

    [Theory]
    [InlineData("010203", 3, 0x01, 0x02, 0x03)]
    [InlineData("0102", 2, 0x01, 0x02, 0x03)]
    [InlineData("010200", 3, 0x01, 0x02)]
    [InlineData("000000", 3)]
    public void WriteUInt8_ShouldBeValid(string expectedHexBytes, int maxLength, params int[] value)
    {
        var expectedBytes = Convert.FromHexString(expectedHexBytes);
        var array = value.Select(x => (byte)x).ToArray();
        var list = value.Select(x => (byte)x).ToList();
        var enumerable = value.Select(x => (byte)x).ToHashSet();
        var bufferArray = new byte[maxLength];
        var bufferList = new byte[maxLength];
        var bufferEnumerableArray = new byte[maxLength];
        var bufferEnumerableList = new byte[maxLength];
        var bufferEnumerable = new byte[maxLength];

        Utilities.WriteUInt8Span(bufferArray, array);
        Utilities.WriteUInt8List(bufferList, list);
        Utilities.WriteUInt8Enumerable(bufferEnumerableArray, array);
        Utilities.WriteUInt8Enumerable(bufferEnumerableList, list);
        Utilities.WriteUInt8Enumerable(bufferEnumerable, enumerable);

        bufferArray.Should().BeEquivalentTo(expectedBytes);
        bufferList.Should().BeEquivalentTo(expectedBytes);
        bufferEnumerableArray.Should().BeEquivalentTo(expectedBytes);
        bufferEnumerableList.Should().BeEquivalentTo(expectedBytes);
        bufferEnumerable.Should().BeEquivalentTo(expectedBytes);
    }

    [Theory]
    [InlineData("01020304", 2, 0x0201, 0x0403)]
    [InlineData("0102", 1, 0x0201, 0x0403)]
    [InlineData("010203040000", 3, 0x0201, 0x0403)]
    [InlineData("00000000", 2)]
    public void WriteUInt16LittleEndian_ShouldBeValid(string expectedHexBytes, int maxLength, params int[] value)
    {
        var expectedBytes = Convert.FromHexString(expectedHexBytes);
        var array = value.Select(x => (ushort)x).ToArray();
        var list = value.Select(x => (ushort)x).ToList();
        var enumerable = value.Select(x => (ushort)x).ToHashSet();
        var bufferArray = new byte[maxLength * 2];
        var bufferList = new byte[maxLength * 2];
        var bufferEnumerableArray = new byte[maxLength * 2];
        var bufferEnumerableList = new byte[maxLength * 2];
        var bufferEnumerable = new byte[maxLength * 2];

        Utilities.WriteUInt16SpanLittleEndian(bufferArray, array);
        Utilities.WriteUInt16ListLittleEndian(bufferList, list);
        Utilities.WriteUInt16EnumerableLittleEndian(bufferEnumerableArray, array);
        Utilities.WriteUInt16EnumerableLittleEndian(bufferEnumerableList, list);
        Utilities.WriteUInt16EnumerableLittleEndian(bufferEnumerable, enumerable);

        bufferArray.Should().BeEquivalentTo(expectedBytes);
        bufferList.Should().BeEquivalentTo(expectedBytes);
        bufferEnumerableArray.Should().BeEquivalentTo(expectedBytes);
        bufferEnumerableList.Should().BeEquivalentTo(expectedBytes);
        bufferEnumerable.Should().BeEquivalentTo(expectedBytes);
    }

    [Theory]
    [InlineData("02010403", 2, 0x0201, 0x0403)]
    [InlineData("0201", 1, 0x0201, 0x0403)]
    [InlineData("020104030000", 3, 0x0201, 0x0403)]
    [InlineData("00000000", 2)]
    public void WriteUInt16BigEndian_ShouldBeValid(string expectedHexBytes, int maxLength, params int[] value)
    {
        var expectedBytes = Convert.FromHexString(expectedHexBytes);
        var array = value.Select(x => (ushort)x).ToArray();
        var list = value.Select(x => (ushort)x).ToList();
        var enumerable = value.Select(x => (ushort)x).ToHashSet();
        var bufferArray = new byte[maxLength * 2];
        var bufferList = new byte[maxLength * 2];
        var bufferEnumerableArray = new byte[maxLength * 2];
        var bufferEnumerableList = new byte[maxLength * 2];
        var bufferEnumerable = new byte[maxLength * 2];

        Utilities.WriteUInt16SpanBigEndian(bufferArray, array);
        Utilities.WriteUInt16ListBigEndian(bufferList, list);
        Utilities.WriteUInt16EnumerableBigEndian(bufferEnumerableArray, array);
        Utilities.WriteUInt16EnumerableBigEndian(bufferEnumerableList, list);
        Utilities.WriteUInt16EnumerableBigEndian(bufferEnumerable, enumerable);

        bufferArray.Should().BeEquivalentTo(expectedBytes);
        bufferList.Should().BeEquivalentTo(expectedBytes);
        bufferEnumerableArray.Should().BeEquivalentTo(expectedBytes);
        bufferEnumerableList.Should().BeEquivalentTo(expectedBytes);
        bufferEnumerable.Should().BeEquivalentTo(expectedBytes);
    }
}

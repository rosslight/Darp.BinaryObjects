namespace Darp.BinaryObjects.Tests;

using FluentAssertions;

#pragma warning disable CS0618 // Type or member is obsolete

public class BinaryHelpersTests
{
    [Theory]
    [InlineData("010203", 0x01, 0x02, 0x03)]
    public void ReadUInt8_ShouldBeValid(string hexBytes, params int[] expectedResult)
    {
        var bytes = Convert.FromHexString(hexBytes);

        var arrayResult = BinaryHelpers.ReadUInt8Array(bytes);
        List<byte> listResult = BinaryHelpers.ReadUInt8List(bytes);

        arrayResult.Should().BeEquivalentTo(expectedResult);
        listResult.Should().BeEquivalentTo(expectedResult);
    }

    [Theory]
    [InlineData("01020304", 0x0201, 0x0403)]
    public void ReadUInt16ArrayLittleEndian_ShouldBeValid(string hexBytes, params int[] expectedResult)
    {
        var bytes = Convert.FromHexString(hexBytes);

        var arrayResult = BinaryHelpers.ReadUInt16ArrayLittleEndian(bytes);
        List<ushort> listResult = BinaryHelpers.ReadUInt16ListLittleEndian(bytes);

        arrayResult.Should().BeEquivalentTo(expectedResult);
        listResult.Should().BeEquivalentTo(expectedResult);
    }

    [Theory]
    [InlineData("01020304", 0x0102, 0x0304)]
    public void ReadUInt16ArrayBigEndian_ShouldBeValid(string hexBytes, params int[] expectedResult)
    {
        var bytes = Convert.FromHexString(hexBytes);

        var arrayResult = BinaryHelpers.ReadUInt16ArrayBigEndian(bytes);
        List<ushort> listResult = BinaryHelpers.ReadUInt16ListBigEndian(bytes);

        arrayResult.Should().BeEquivalentTo(expectedResult);
        listResult.Should().BeEquivalentTo(expectedResult);
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

        BinaryHelpers.WriteUInt8Span(bufferArray, array, maxLength);
        BinaryHelpers.WriteUInt8List(bufferList, list, maxLength);
        BinaryHelpers.WriteUInt8Enumerable(bufferEnumerableArray, array, maxLength);
        BinaryHelpers.WriteUInt8Enumerable(bufferEnumerableList, list, maxLength);
        BinaryHelpers.WriteUInt8Enumerable(bufferEnumerable, enumerable, maxLength);

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

        BinaryHelpers.WriteUInt16SpanLittleEndian(bufferArray, array, maxLength);
        BinaryHelpers.WriteUInt16ListLittleEndian(bufferList, list, maxLength);
        BinaryHelpers.WriteUInt16EnumerableLittleEndian(bufferEnumerableArray, array, maxLength);
        BinaryHelpers.WriteUInt16EnumerableLittleEndian(bufferEnumerableList, list, maxLength);
        BinaryHelpers.WriteUInt16EnumerableLittleEndian(bufferEnumerable, enumerable, maxLength);

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

        BinaryHelpers.WriteUInt16SpanBigEndian(bufferArray, array, maxLength);
        BinaryHelpers.WriteUInt16ListBigEndian(bufferList, list, maxLength);
        BinaryHelpers.WriteUInt16EnumerableBigEndian(bufferEnumerableArray, array, maxLength);
        BinaryHelpers.WriteUInt16EnumerableBigEndian(bufferEnumerableList, list, maxLength);
        BinaryHelpers.WriteUInt16EnumerableBigEndian(bufferEnumerable, enumerable, maxLength);

        bufferArray.Should().BeEquivalentTo(expectedBytes);
        bufferList.Should().BeEquivalentTo(expectedBytes);
        bufferEnumerableArray.Should().BeEquivalentTo(expectedBytes);
        bufferEnumerableList.Should().BeEquivalentTo(expectedBytes);
        bufferEnumerable.Should().BeEquivalentTo(expectedBytes);
    }
}

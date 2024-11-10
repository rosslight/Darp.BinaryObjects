namespace Darp.BinaryObjects.Tests;

using FluentAssertions;

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

        BinaryHelpers.WriteUInt8Span(array, maxLength, bufferArray);
        BinaryHelpers.WriteUInt8List(list, maxLength, bufferList);
        BinaryHelpers.WriteUInt8Enumerable(array, maxLength, bufferEnumerableArray);
        BinaryHelpers.WriteUInt8Enumerable(list, maxLength, bufferEnumerableList);
        BinaryHelpers.WriteUInt8Enumerable(enumerable, maxLength, bufferEnumerable);

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

        BinaryHelpers.WriteUInt16SpanLittleEndian(array, maxLength, bufferArray);
        BinaryHelpers.WriteUInt16ListLittleEndian(list, maxLength, bufferList);
        BinaryHelpers.WriteUInt16EnumerableLittleEndian(array, maxLength, bufferEnumerableArray);
        BinaryHelpers.WriteUInt16EnumerableLittleEndian(list, maxLength, bufferEnumerableList);
        BinaryHelpers.WriteUInt16EnumerableLittleEndian(enumerable, maxLength, bufferEnumerable);

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

        BinaryHelpers.WriteUInt16SpanBigEndian(array, maxLength, bufferArray);
        BinaryHelpers.WriteUInt16ListBigEndian(list, maxLength, bufferList);
        BinaryHelpers.WriteUInt16EnumerableBigEndian(array, maxLength, bufferEnumerableArray);
        BinaryHelpers.WriteUInt16EnumerableBigEndian(list, maxLength, bufferEnumerableList);
        BinaryHelpers.WriteUInt16EnumerableBigEndian(enumerable, maxLength, bufferEnumerable);

        bufferArray.Should().BeEquivalentTo(expectedBytes);
        bufferList.Should().BeEquivalentTo(expectedBytes);
        bufferEnumerableArray.Should().BeEquivalentTo(expectedBytes);
        bufferEnumerableList.Should().BeEquivalentTo(expectedBytes);
        bufferEnumerable.Should().BeEquivalentTo(expectedBytes);
    }
}

namespace Darp.BinaryObjects.Tests;

using BinaryObjects.Generated;
using FluentAssertions;

[BinaryObject]
internal readonly partial struct TestStruct()
{
    [BinaryElementCount(1)]
    public required byte[] Value2 { get; init; }

    [BinaryElementCount(1)]
    public required List<byte> Value3 { get; init; }

    [BinaryElementCount(1)]
    public required IReadOnlyCollection<byte> Value4 { get; init; }

    [BinaryElementCount(1)]
    public required ushort[] Value12 { get; init; }

    [BinaryElementCount(1)]
    public required List<ushort> Value13 { get; init; }
    public required IReadOnlyCollection<ushort> Value14 { get; init; }
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
        var collection = value.Select(x => (byte)x).ToHashSet();
        var bufferArray = new byte[maxLength];
        var bufferList = new byte[maxLength];
        var bufferCollectionArray = new byte[maxLength];
        var bufferCollectionList = new byte[maxLength];
        var bufferCollection = new byte[maxLength];

        Utilities.WriteUInt8Span(bufferArray, array);
        Utilities.WriteUInt8Span(bufferList, System.Runtime.InteropServices.CollectionsMarshal.AsSpan(list));
        Utilities.WriteUInt8Collection(bufferCollectionArray, array);
        Utilities.WriteUInt8Collection(bufferCollectionList, list);
        Utilities.WriteUInt8Collection(bufferCollection, collection);

        bufferArray.Should().BeEquivalentTo(expectedBytes);
        bufferList.Should().BeEquivalentTo(expectedBytes);
        bufferCollectionArray.Should().BeEquivalentTo(expectedBytes);
        bufferCollectionList.Should().BeEquivalentTo(expectedBytes);
        bufferCollection.Should().BeEquivalentTo(expectedBytes);
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
        var collection = value.Select(x => (ushort)x).ToHashSet();
        var bufferArray = new byte[maxLength * 2];
        var bufferList = new byte[maxLength * 2];
        var bufferCollectionArray = new byte[maxLength * 2];
        var bufferCollectionList = new byte[maxLength * 2];
        var bufferCollection = new byte[maxLength * 2];

        Utilities.WriteUInt16SpanLittleEndian(bufferArray, array);
        Utilities.WriteUInt16SpanLittleEndian(
            bufferList,
            System.Runtime.InteropServices.CollectionsMarshal.AsSpan(list)
        );
        Utilities.WriteUInt16CollectionLittleEndian(bufferCollectionArray, array);
        Utilities.WriteUInt16CollectionLittleEndian(bufferCollectionList, list);
        Utilities.WriteUInt16CollectionLittleEndian(bufferCollection, collection);

        bufferArray.Should().BeEquivalentTo(expectedBytes);
        bufferList.Should().BeEquivalentTo(expectedBytes);
        bufferCollectionArray.Should().BeEquivalentTo(expectedBytes);
        bufferCollectionList.Should().BeEquivalentTo(expectedBytes);
        bufferCollection.Should().BeEquivalentTo(expectedBytes);
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
        var collection = value.Select(x => (ushort)x).ToHashSet();
        var bufferArray = new byte[maxLength * 2];
        var bufferList = new byte[maxLength * 2];
        var bufferCollectionArray = new byte[maxLength * 2];
        var bufferCollectionList = new byte[maxLength * 2];
        var bufferCollection = new byte[maxLength * 2];

        Utilities.WriteUInt16SpanBigEndian(bufferArray, array);
        Utilities.WriteUInt16SpanBigEndian(bufferList, System.Runtime.InteropServices.CollectionsMarshal.AsSpan(list));
        Utilities.WriteUInt16CollectionBigEndian(bufferCollectionArray, array);
        Utilities.WriteUInt16CollectionBigEndian(bufferCollectionList, list);
        Utilities.WriteUInt16CollectionBigEndian(bufferCollection, collection);

        bufferArray.Should().BeEquivalentTo(expectedBytes);
        bufferList.Should().BeEquivalentTo(expectedBytes);
        bufferCollectionArray.Should().BeEquivalentTo(expectedBytes);
        bufferCollectionList.Should().BeEquivalentTo(expectedBytes);
        bufferCollection.Should().BeEquivalentTo(expectedBytes);
    }
}

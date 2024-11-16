namespace Darp.BinaryObjects.Generator.Tests.Sources;

[BinaryObject]
public partial record ArraysFixedSize(
    [property: BinaryElementCount(1)] ReadOnlyMemory<byte> ValueByteMemory,
    [property: BinaryElementCount(1)] byte[] ValueByteArray,
    [property: BinaryElementCount(1)] List<byte> ValueByteList,
    [property: BinaryElementCount(1)] IEnumerable<byte> ValueByteEnumerable,
    [property: BinaryElementCount(1)] ICollection<byte> ValueByteCollection,
    [property: BinaryElementCount(1)] IReadOnlyCollection<byte> ValueByteReadOnlyCollection,
    [property: BinaryElementCount(1)] IList<byte> ValueByteIList,
    [property: BinaryElementCount(1)] IReadOnlyList<byte> ValueByteReadOnlyList,
    [property: BinaryElementCount(1)] ReadOnlyMemory<ushort> ValueUShortMemory,
    [property: BinaryElementCount(1)] ushort[] ValueUShortArray,
    [property: BinaryElementCount(1)] List<ushort> ValueUShortList,
    [property: BinaryElementCount(1)] IEnumerable<ushort> ValueUShortEnumerable
);

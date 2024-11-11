namespace Darp.BinaryObjects.Generator.Tests.Sources;

[BinaryObject]
public sealed partial record ArrayByteFixedSize([property: BinaryElementCount(2)] byte[] Value);

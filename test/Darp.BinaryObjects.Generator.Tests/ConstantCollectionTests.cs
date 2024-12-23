namespace Darp.BinaryObjects.Generator.Tests;

public sealed class ConstantCollectionTests
{
    [Fact]
    public async Task Primitives_ByteArray()
    {
        const string code = """
            using Darp.BinaryObjects;

            [BinaryObject]
            public sealed partial record TestObject([property: BinaryElementCount(2)] byte[] Value);
            """;
        await VerifyHelper.VerifyBinaryObjectsGenerator(code);
    }

    [Fact]
    public async Task Primitives_AllCollectionTypes()
    {
        const string code = """
            using Darp.BinaryObjects;
            using System;
            using System.Collections.Generic;

            [BinaryObject]
            public partial record TestObject(
                [property: BinaryElementCount(2)] ReadOnlyMemory<byte> ValueByteMemory,
                [property: BinaryElementCount(2)] byte[] ValueByteArray,
                [property: BinaryElementCount(2)] List<byte> ValueByteList,
                [property: BinaryElementCount(2)] IEnumerable<byte> ValueByteEnumerable,
                [property: BinaryElementCount(2)] ReadOnlyMemory<ushort> ValueUShortMemory,
                [property: BinaryElementCount(2)] ushort[] ValueUShortArray,
                [property: BinaryElementCount(2)] List<ushort> ValueUShortList,
                [property: BinaryElementCount(2)] IEnumerable<ushort> ValueUShortEnumerable
            );
            """;
        await VerifyHelper.VerifyBinaryObjectsGenerator(code);
    }

    [Fact]
    public async Task Enum_AllCollectionTypes()
    {
        const string code = """
            using Darp.BinaryObjects;
            using System;
            using System.Collections.Generic;

            public enum IntEnum {}

            [BinaryObject]
            public sealed partial record TestObject(
                [property: BinaryElementCount(2)] ReadOnlyMemory<IntEnum> ValueMemory,
                [property: BinaryElementCount(2)] IntEnum[] ValueArray,
                [property: BinaryElementCount(2)] List<IntEnum> ValueList);
            """;
        await VerifyHelper.VerifyBinaryObjectsGenerator(code);
    }

    [Fact]
    public async Task BinaryObjects()
    {
        const string code = """
            using Darp.BinaryObjects;

            [BinaryObject]
            public sealed partial record TestObjectNested([property: BinaryElementCount(2)] bool[] Value);

            [BinaryObject]
            public sealed partial record TestObject(TestObjectNested Array);
            """;
        await VerifyHelper.VerifyBinaryObjectsGenerator(code);
    }

    [Fact]
    public async Task BinaryObjects_CollectionTypes()
    {
        const string code = """
            using Darp.BinaryObjects;

            [BinaryObject]
            public sealed partial record TestObjectNested(bool Value);

            [BinaryObject]
            public sealed partial record TestObject(
                [property: BinaryElementCount(2)] System.ReadOnlyMemory<TestObjectNested> Value1,
                [property: BinaryElementCount(2)] TestObjectNested[] Value2
            );
            """;
        await VerifyHelper.VerifyBinaryObjectsGenerator(code);
    }
}

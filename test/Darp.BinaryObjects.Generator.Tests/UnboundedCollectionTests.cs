namespace Darp.BinaryObjects.Generator.Tests;

public sealed class UnboundedCollectionTests
{
    [Fact]
    public async Task Primitives()
    {
        const string code = """
            using Darp.BinaryObjects;

            [BinaryObject]
            public sealed partial record TestObject(System.ReadOnlyMemory<byte> Value);

            [BinaryObject]
            public sealed partial record TestObjectWithOffset(byte Offset, uint[] Value);
            """;
        await VerifyHelper.VerifyBinaryObjectsGenerator(code);
    }

    [Fact]
    public async Task BinaryObjects_Constant()
    {
        const string code = """
            using Darp.BinaryObjects;

            [BinaryObject]
            public sealed partial record TestObjectNested(byte Value);

            [BinaryObject]
            public sealed partial record TestObject(TestObjectNested[] Value);
            """;
        await VerifyHelper.VerifyBinaryObjectsGenerator(code);
    }
}

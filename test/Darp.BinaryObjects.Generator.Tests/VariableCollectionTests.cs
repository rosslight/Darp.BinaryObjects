namespace Darp.BinaryObjects.Generator.Tests;

public sealed class VariableCollectionTests
{
    [Fact]
    public async Task Primitives()
    {
        const string code = """
            using Darp.BinaryObjects;

            [BinaryObject]
            public sealed partial record TestObject(byte Length, [property: BinaryElementCount("Length")] byte[] Value);
            """;
        await VerifyHelper.VerifyBinaryObjectsGenerator(code);
    }

    [Fact]
    public async Task TestObject_WithOffset()
    {
        const string code = """
            using Darp.BinaryObjects;

            [BinaryObject]
            public sealed partial record TestObject(
                [property: BinaryLength(3)] int Value,
                int Length,
                [property: BinaryLength("Length")] int Value2
                );
            """;
        await VerifyHelper.VerifyBinaryObjectsGenerator(code);
    }

    [Fact]
    public async Task Primitives_MinElementCount()
    {
        const string code = """
            using Darp.BinaryObjects;
            using System;

            [BinaryObject]
            public sealed partial record TestObject(
                int Length,
                [property: BinaryElementCount("Length"), BinaryMinElementCount(1)] ReadOnlyMemory<short> Value,
                [property: BinaryMinElementCount(3)] int[] Value2);
            """;
        await VerifyHelper.VerifyBinaryObjectsGenerator(code);
    }
}

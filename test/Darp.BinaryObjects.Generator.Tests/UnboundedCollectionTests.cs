namespace Darp.BinaryObjects.Generator.Tests;

public sealed class UnboundedCollectionTests
{
    [Fact]
    public async Task ReadOnly_ManualObjectArrayRequiresFixedElementLength()
    {
        const string code = """
            using Darp.BinaryObjects;
            using System;
            using System.Diagnostics.CodeAnalysis;

            public sealed record ManualObject(ushort Value) : IBinaryReadable<ManualObject>
            {
                public static bool TryReadLittleEndian(ReadOnlySpan<byte> source, [NotNullWhen(true)] out ManualObject? value) => throw new NotImplementedException();
                public static bool TryReadLittleEndian(ReadOnlySpan<byte> source, [NotNullWhen(true)] out ManualObject? value, out int bytesRead) => throw new NotImplementedException();
                public static bool TryReadBigEndian(ReadOnlySpan<byte> source, [NotNullWhen(true)] out ManualObject? value) => throw new NotImplementedException();
                public static bool TryReadBigEndian(ReadOnlySpan<byte> source, [NotNullWhen(true)] out ManualObject? value, out int bytesRead) => throw new NotImplementedException();
            }

            [BinaryObject(BinaryOptions.Read)]
            public sealed partial record Parent
            {
                public ManualObject[] Values { get; init; } = Array.Empty<ManualObject>();
            }
            """;
        await VerifyHelper.VerifyBinaryObjectsGenerator(code);
    }

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

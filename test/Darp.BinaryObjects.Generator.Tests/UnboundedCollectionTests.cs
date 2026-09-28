namespace Darp.BinaryObjects.Generator.Tests;

public sealed class UnboundedCollectionTests
{
    [Theory]
    [InlineData(BinaryOptions.All)]
    [InlineData(BinaryOptions.Read)]
    [InlineData(BinaryOptions.Write)]
    public async Task RemainingCollectionMustBeLast(BinaryOptions options)
    {
        var code = $$"""
            using Darp.BinaryObjects;

            [BinaryObject(BinaryOptions.{{options}})]
            public sealed partial record TestObject(byte[] Values, ushort Tail);
            """;
        await VerifyHelper.VerifyBinaryObjectsGenerator(code).UseParameters(options);
    }

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
            public sealed partial record TestObjectWithOffset(byte Offset, uint[] Value)
            {
                [BinaryIgnore] public byte IgnoredTail { get; init; }
                public int ComputedTail => Value.Length;
            }
            """;
        await VerifyHelper.VerifyBinaryObjectsGenerator(code);
    }

    [Fact]
    public async Task EnumEnumerable_CompilesWithoutConsumerLinqUsing()
    {
        const string code = """
            using Darp.BinaryObjects;
            using System.Collections.Generic;

            public enum Status : ushort { First = 0x1234, Second = 0xABCD }

            [BinaryObject]
            public sealed partial record TestObject(IEnumerable<Status> Values);
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

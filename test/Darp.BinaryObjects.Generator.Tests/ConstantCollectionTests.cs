namespace Darp.BinaryObjects.Generator.Tests;

public sealed class ConstantCollectionTests
{
    [Fact]
    public async Task ManualObjectArrayRequiresExplicitElementLength()
    {
        const string code = """
            using Darp.BinaryObjects;
            using System;
            using System.Diagnostics.CodeAnalysis;

            public sealed record ManualObject : IBinaryObject<ManualObject>
            {
                public ushort Value { get; set; }
                public int GetByteCount() => 3;
                public bool TryWriteLittleEndian(Span<byte> destination) => throw new NotImplementedException();
                public bool TryWriteLittleEndian(Span<byte> destination, out int bytesWritten) => throw new NotImplementedException();
                public bool TryWriteBigEndian(Span<byte> destination) => throw new NotImplementedException();
                public bool TryWriteBigEndian(Span<byte> destination, out int bytesWritten) => throw new NotImplementedException();
                public static bool TryReadLittleEndian(ReadOnlySpan<byte> source, [NotNullWhen(true)] out ManualObject? value) => throw new NotImplementedException();
                public static bool TryReadLittleEndian(ReadOnlySpan<byte> source, [NotNullWhen(true)] out ManualObject? value, out int bytesRead) => throw new NotImplementedException();
                public static bool TryReadBigEndian(ReadOnlySpan<byte> source, [NotNullWhen(true)] out ManualObject? value) => throw new NotImplementedException();
                public static bool TryReadBigEndian(ReadOnlySpan<byte> source, [NotNullWhen(true)] out ManualObject? value, out int bytesRead) => throw new NotImplementedException();
            }

            [BinaryObject]
            public sealed partial record Parent
            {
                [BinaryElementCount(2)]
                public ManualObject[] Values { get; init; } = Array.Empty<ManualObject>();
            }
            """;
        await VerifyHelper.VerifyBinaryObjectsGenerator(code);
    }

    [Fact]
    public async Task InvalidElementCounts_ShouldReportDiagnostics()
    {
        const string code = """
            using Darp.BinaryObjects;

            [BinaryObject]
            public sealed partial record NegativeCount
            {
                [BinaryElementCount(-1)] public ushort[] Values { get; init; }
            }

            [BinaryObject]
            public sealed partial record OverflowingCount
            {
                [BinaryElementCount(int.MaxValue)] public ushort[] Values { get; init; }
            }

            [BinaryObject]
            public sealed partial record NegativeMinimum
            {
                [BinaryMinElementCount(-1)] public byte[] Values { get; init; }
            }

            [BinaryObject]
            public sealed partial record OverflowingMinimum
            {
                [BinaryMinElementCount(int.MaxValue)] public ushort[] Values { get; init; }
            }

            [BinaryObject]
            public sealed partial record OverflowingObjectLength
            {
                [BinaryElementCount(1073741824)] public byte[] First { get; init; }
                [BinaryElementCount(1073741824)] public byte[] Second { get; init; }
            }
            """;
        await VerifyHelper.VerifyBinaryObjectsGenerator(code);
    }

    [Fact]
    public async Task ZeroLengthBinaryObjectArrayIsUnsupported()
    {
        const string code = """
            using Darp.BinaryObjects;
            using System;

            [BinaryObject(BinaryOptions.Write)]
            public sealed partial record Empty;

            [BinaryObject(BinaryOptions.Write)]
            public sealed partial record Parent
            {
                [BinaryElementCount(2)]
                public Empty[] Values { get; init; } = Array.Empty<Empty>();
            }
            """;
        await VerifyHelper.VerifyBinaryObjectsGenerator(code);
    }

    [Fact]
    public async Task WriteOnly_ManualObjectArrayRequiresFixedElementLength()
    {
        const string code = """
            using Darp.BinaryObjects;
            using System;

            public sealed record ManualObject(ushort Value) : IBinaryWritable
            {
                public int GetByteCount() => 3;
                public bool TryWriteLittleEndian(Span<byte> destination) => throw new NotImplementedException();
                public bool TryWriteLittleEndian(Span<byte> destination, out int bytesWritten) => throw new NotImplementedException();
                public bool TryWriteBigEndian(Span<byte> destination) => throw new NotImplementedException();
                public bool TryWriteBigEndian(Span<byte> destination, out int bytesWritten) => throw new NotImplementedException();
            }

            [BinaryObject(BinaryOptions.Write)]
            public sealed partial record Parent
            {
                [BinaryElementCount(2)]
                public ManualObject[] Values { get; init; } = Array.Empty<ManualObject>();
            }
            """;
        await VerifyHelper.VerifyBinaryObjectsGenerator(code);
    }

    [Fact]
    public async Task ReadOnly_ManualObjectArray()
    {
        const string code = """
            using Darp.BinaryObjects;
            using System;
            using System.Diagnostics.CodeAnalysis;

            [BinaryConstant(2)]
            public sealed record ManualObject : IBinaryReadable<ManualObject>
            {
                public static bool TryReadLittleEndian(ReadOnlySpan<byte> source, [NotNullWhen(true)] out ManualObject? value) => throw new NotImplementedException();
                public static bool TryReadLittleEndian(ReadOnlySpan<byte> source, [NotNullWhen(true)] out ManualObject? value, out int bytesRead) => throw new NotImplementedException();
                public static bool TryReadBigEndian(ReadOnlySpan<byte> source, [NotNullWhen(true)] out ManualObject? value) => throw new NotImplementedException();
                public static bool TryReadBigEndian(ReadOnlySpan<byte> source, [NotNullWhen(true)] out ManualObject? value, out int bytesRead) => throw new NotImplementedException();
            }

            [BinaryObject(BinaryOptions.Read)]
            public sealed partial record Parent([property: BinaryElementCount(2)] ManualObject[] Values);
            """;
        await VerifyHelper.VerifyBinaryObjectsGenerator(code);
    }

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
                [property: BinaryElementCount(2)] IReadOnlyCollection<byte> ValueByteCollection,
                [property: BinaryElementCount(2)] ReadOnlyMemory<ushort> ValueUShortMemory,
                [property: BinaryElementCount(2)] ushort[] ValueUShortArray,
                [property: BinaryElementCount(2)] List<ushort> ValueUShortList,
                [property: BinaryElementCount(2)] ICollection<ushort> ValueUShortCollection
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
            public sealed partial record TestObjectNested(bool Value)
            {
                public TestObjectNested IgnoredSelfReference => this;
            }

            [BinaryObject]
            public sealed partial record TestObject(
                [property: BinaryElementCount(2)] System.ReadOnlyMemory<TestObjectNested> Value1,
                [property: BinaryElementCount(2)] TestObjectNested[] Value2,
                [property: BinaryElementCount(2)] System.Collections.Generic.List<TestObjectNested> Value3
            );
            """;
        await VerifyHelper.VerifyBinaryObjectsGenerator(code);
    }
}

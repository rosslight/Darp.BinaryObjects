namespace Darp.BinaryObjects.Generator.Tests;

public sealed class ByteCountTests
{
    [Fact]
    public async Task Scalars()
    {
        const string code = """
            using Darp.BinaryObjects;

            public enum UIntEnum : uint {}
            public enum ShortEnum : short {}

            [BinaryObject]
            public sealed partial record TestObject(
                [property: BinaryByteCount(3)] uint Unsigned,
                [property: BinaryByteCount(6)] long Signed,
                ushort Unchanged,
                [property: BinaryByteCount(3)] UIntEnum UnsignedEnum,
                [property: BinaryByteCount(1)] ShortEnum SignedEnum);
            """;
        await VerifyHelper.VerifyBinaryObjectsGenerator(code);
    }

    [Fact]
    public async Task Collections()
    {
        const string code = """
            using System;
            using System.Collections.Generic;
            using Darp.BinaryObjects;

            public enum UIntEnum : uint {}

            [BinaryObject]
            public sealed partial record TestObject(
                [property: BinaryByteCount(3)] int Count,
                [property: BinaryElementCount("Count"), BinaryElementByteCount(3)] int[] Counted,
                [property: BinaryElementCount(2), BinaryElementByteCount(3)] List<UIntEnum> Fixed,
                [property: BinaryElementCount(2), BinaryElementByteCount(6)] IReadOnlyList<ulong> Interface,
                [property: BinaryElementByteCount(3)] ReadOnlyMemory<uint> Remaining);
            """;
        await VerifyHelper.VerifyBinaryObjectsGenerator(code);
    }

    [Fact]
    public async Task InvalidByteCounts_ShouldReportDiagnostics()
    {
        const string code = """
            using System;
            using Darp.BinaryObjects;

            [BinaryObject]
            public sealed partial record Inner(ushort Value);

            [BinaryObject]
            public sealed partial class TestObject
            {
                [BinaryByteCount(0)] public uint Zero { get; set; }
                [BinaryByteCount(5)] public uint TooWide { get; set; }
                [BinaryElementByteCount(3)] public ushort[] TooWideElements { get; set; } = Array.Empty<ushort>();
                [BinaryByteCount(3)] public double Floating { get; set; }
                [BinaryByteCount(1)] public char Character { get; set; }
                [BinaryByteCount(8)] public Int128 Large { get; set; }
                [BinaryByteCount(1)] public Inner Nested { get; set; } = new(0);
                [BinaryElementByteCount(1)] public Inner[] NestedElements { get; set; } = Array.Empty<Inner>();
            }
            """;
        await VerifyHelper.VerifyBinaryObjectsGenerator(code);
    }

    [Fact]
    public async Task MisplacedByteCounts_ShouldReportDiagnostics()
    {
        const string code = """
            using System;
            using Darp.BinaryObjects;

            [BinaryObject]
            public sealed partial class TestObject
            {
                [BinaryByteCount(3)] public uint[] Collection { get; set; } = Array.Empty<uint>();
                [BinaryElementByteCount(3)] public uint Scalar { get; set; }
                [BinaryElementByteCount(3), BinaryByteCount(3)] public uint[] CollectionWithBoth { get; set; } = Array.Empty<uint>();
                [BinaryByteCount(3), BinaryElementByteCount(3)] public uint ScalarWithBoth { get; set; }
            }
            """;
        await VerifyHelper.VerifyBinaryObjectsGenerator(code);
    }

    [Fact]
    public async Task RedundantByteCount_ShouldReportInfo()
    {
        const string code = """
            using Darp.BinaryObjects;

            [BinaryObject]
            public sealed partial record TestObject([property: BinaryByteCount(2)] ushort Value);
            """;
        await VerifyHelper.VerifyBinaryObjectsGenerator(code);
    }
}

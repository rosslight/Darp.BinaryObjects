namespace Darp.BinaryObjects.Generator.Tests;

public sealed class ByteWidthTests
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
                [property: BinaryByteWidth(3)] uint Unsigned,
                [property: BinaryByteWidth(6)] long Signed,
                ushort Unchanged,
                [property: BinaryByteWidth(3)] UIntEnum UnsignedEnum,
                [property: BinaryByteWidth(1)] ShortEnum SignedEnum);
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
                [property: BinaryByteWidth(3)] int Count,
                [property: BinaryElementCount("Count"), BinaryByteWidth(3)] int[] Counted,
                [property: BinaryElementCount(2), BinaryByteWidth(3)] List<UIntEnum> Fixed,
                [property: BinaryElementCount(2), BinaryByteWidth(6)] IReadOnlyList<ulong> Interface,
                [property: BinaryByteWidth(3)] ReadOnlyMemory<uint> Remaining);
            """;
        await VerifyHelper.VerifyBinaryObjectsGenerator(code);
    }

    [Fact]
    public async Task InvalidByteWidths_ShouldReportDiagnostics()
    {
        const string code = """
            using System;
            using Darp.BinaryObjects;

            [BinaryObject]
            public sealed partial record Inner(ushort Value);

            [BinaryObject]
            public sealed partial class TestObject
            {
                [BinaryByteWidth(0)] public uint Zero { get; set; }
                [BinaryByteWidth(5)] public uint TooWide { get; set; }
                [BinaryByteWidth(3)] public ushort[] TooWideElements { get; set; } = Array.Empty<ushort>();
                [BinaryByteWidth(3)] public double Floating { get; set; }
                [BinaryByteWidth(1)] public char Character { get; set; }
                [BinaryByteWidth(8)] public Int128 Large { get; set; }
                [BinaryByteWidth(1)] public Inner Nested { get; set; } = new(0);
            }
            """;
        await VerifyHelper.VerifyBinaryObjectsGenerator(code);
    }

    [Fact]
    public async Task RedundantByteWidth_ShouldReportInfo()
    {
        const string code = """
            using Darp.BinaryObjects;

            [BinaryObject]
            public sealed partial record TestObject([property: BinaryByteWidth(2)] ushort Value);
            """;
        await VerifyHelper.VerifyBinaryObjectsGenerator(code);
    }
}

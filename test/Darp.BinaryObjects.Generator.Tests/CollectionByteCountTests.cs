namespace Darp.BinaryObjects.Generator.Tests;

public sealed class CollectionByteCountTests
{
    [Fact]
    public async Task Collections()
    {
        const string code = """
            using System;
            using System.Collections.Generic;
            using Darp.BinaryObjects;

            [BinaryObject]
            public sealed partial record Pair(byte First, byte Second);

            [BinaryObject]
            public sealed partial record TestObject(
                ushort Length,
                [property: BinaryByteCount("Length")] int[] Values,
                [property: BinaryByteCount("Length"), BinaryElementByteCount(3), BinaryMinElementCount(1)] List<uint> Narrow,
                [property: BinaryByteCount("Length")] ReadOnlyMemory<Pair> Objects,
                [property: BinaryByteCount("Length")] byte[] Bytes,
                byte Tail);
            """;
        await VerifyHelper.VerifyBinaryObjectsGenerator(code);
    }

    [Fact]
    public async Task InvalidCollectionByteCounts_ShouldReportDiagnostics()
    {
        const string code = """
            using System;
            using Darp.BinaryObjects;

            [BinaryObject]
            public sealed partial record Variable(byte Count, [property: BinaryElementCount("Count")] byte[] Data);

            [BinaryObject]
            public sealed partial class TestObject
            {
                public ushort Length { get; set; }
                public uint Unsigned { get; set; }
                [BinaryByteCount("Length"), BinaryElementCount(2)] public uint[] WithConstantElementCount { get; set; } = Array.Empty<uint>();
                [BinaryElementCount("Length"), BinaryByteCount("Length")] public uint[] WithMemberElementCount { get; set; } = Array.Empty<uint>();
                [BinaryByteCount("Length")] public uint SingleValue { get; set; }
                [BinaryByteCount("Missing")] public uint[] MissingMember { get; set; } = Array.Empty<uint>();
                [BinaryByteCount("Unsigned")] public uint[] UnsupportedMemberType { get; set; } = Array.Empty<uint>();
                [BinaryByteCount("Length")] public Variable[] VariableElements { get; set; } = Array.Empty<Variable>();
            }
            """;
        await VerifyHelper.VerifyBinaryObjectsGenerator(code);
    }
}

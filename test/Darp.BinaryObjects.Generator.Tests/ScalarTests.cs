namespace Darp.BinaryObjects.Generator.Tests;

using static VerifyHelper;

public sealed class ScalarTests
{
    [Theory]
    [InlineData(BinaryOptions.All)]
    [InlineData(BinaryOptions.Read)]
    [InlineData(BinaryOptions.Write)]
    public async Task Primitives_OneBool(BinaryOptions options)
    {
        var code = $$"""
            using Darp.BinaryObjects;

            [BinaryObject(BinaryOptions.{{options}})]
            public sealed partial record TestObject(bool Value);
            """;
        await VerifyBinaryObjectsGenerator(code).UseParameters(options);
    }

    [Fact]
    public async Task Primitives_All()
    {
        const string code = """
            using System;
            using Darp.BinaryObjects;

            [BinaryObject]
            public sealed partial record TestObject(
                bool ValueBool,
                sbyte ValueSByte,
                short ValueShort,
                Half ValueHalf,
                int ValueInt,
                float ValueFloat,
                long ValueLong,
                Int128 ValueInt128,
                UInt128 ValueUInt128,
                ulong ValueULong,
                double ValueDouble,
                uint ValueUInt,
                ushort ValueUShort,
                byte ValueByte
            );
            """;
        await VerifyBinaryObjectsGenerator(code);
    }

    [Fact]
    public async Task Enum_DefaultDefinition()
    {
        const string code = """
            using Darp.BinaryObjects;

            public enum IntEnum {}

            [BinaryObject]
            public sealed partial record TestObject(IntEnum Value);
            """;
        await VerifyBinaryObjectsGenerator(code);
    }

    [Fact]
    public async Task Enum_WithNamespaceDefinition()
    {
        const string code = """
            using Darp.BinaryObjects;

            namespace Test.Test1
            {
                public enum IntEnum {}
            }

            [BinaryObject]
            public sealed partial record TestObject(Test.Test1.IntEnum Value);
            """;
        await VerifyBinaryObjectsGenerator(code);
    }

    [Fact]
    public async Task Enum_AllSizes()
    {
        const string code = """
            using Darp.BinaryObjects;

            public enum SByteEnum : sbyte {}
            public enum ByteEnum : byte {}
            public enum ShortEnum : short {}
            public enum UShortEnum : ushort {}
            public enum IntEnum : int {}
            public enum UIntEnum : uint {}
            public enum LongEnum : long {}
            public enum ULongEnum : ulong {}

            [BinaryObject]
            public sealed partial record TestObject(SByteEnum ValueSByte,
                ByteEnum ValueByte,
                ShortEnum ValueShort,
                UShortEnum ValueUShort,
                IntEnum ValueInt,
                UIntEnum ValueUInt,
                LongEnum ValueSLong,
                ULongEnum ValueULong
                );
            """;
        await VerifyBinaryObjectsGenerator(code);
    }

    [Fact]
    public async Task NestedBinaryObject_ConstantObject()
    {
        const string code = """
            using Darp.BinaryObjects;

            [BinaryObject]
            public sealed partial record TestObjectNested
            {
                public TestObjectNested(bool value) => Value = value;

                public bool Value { get; }
                [BinaryIgnore] public int Ignored { get; init; }
                public int IgnoredProperty => 1;
            }

            [BinaryObject]
            public sealed partial record TestObject(TestObjectNested Value, byte Tail);
            """;
        await VerifyBinaryObjectsGenerator(code);
    }

    [Fact]
    public async Task NestedBinaryObject_ConstantObject_ManualDefinition()
    {
        const string code = """
            using Darp.BinaryObjects;
            using System;
            using System.Diagnostics.CodeAnalysis;
            using System.Collections.Generic;

            [BinaryConstant(1)]
            public sealed record TestObjectManual : IBinaryObject<TestObjectManual>
            {
                public int GetByteCount() => throw new NotImplementedException();
                public bool TryWriteLittleEndian(Span<byte> destination) => TryReadLittleEndian(destination, out _);
                public bool TryWriteLittleEndian(Span<byte> destination, out int bytesWritten) => throw new NotImplementedException();
                public bool TryWriteBigEndian(Span<byte> destination) => TryWriteBigEndian(destination, out _);
                public bool TryWriteBigEndian(Span<byte> destination, out int bytesWritten) => throw new NotImplementedException();
                public static bool TryReadLittleEndian(ReadOnlySpan<byte> source,[NotNullWhen(true)] out TestObjectManual? value) => TryReadLittleEndian(source, out value, out _);
                public static bool TryReadLittleEndian(ReadOnlySpan<byte> source,[NotNullWhen(true)] out TestObjectManual? value,out int bytesRead) => throw new NotImplementedException();
                public static bool TryReadBigEndian(ReadOnlySpan<byte> source,[NotNullWhen(true)] out TestObjectManual? value) => TryReadBigEndian(source, out value, out _);
                public static bool TryReadBigEndian(ReadOnlySpan<byte> source,[NotNullWhen(true)] out TestObjectManual? value,out int bytesRead) => throw new NotSupportedException();
            }

            [BinaryObject]
            public sealed partial record TestObject(TestObjectManual Value);
            """;
        await VerifyBinaryObjectsGenerator(code);
    }

    [Fact]
    public async Task ReadOnly_NestedObjectWithRemainingBytes()
    {
        const string code = """
            using Darp.BinaryObjects;
            using System;

            [BinaryObject(BinaryOptions.Read)]
            public sealed partial record Child(byte[] Data)
            {
                public bool TryWriteLittleEndian(Span<byte> destination) => throw new NotImplementedException();
            }

            [BinaryObject(BinaryOptions.Read)]
            public sealed partial record Parent(Child Value);
            """;
        await VerifyBinaryObjectsGenerator(code);
    }

    [Fact]
    public async Task NestedManualObject_UsesReportedByteCounts()
    {
        const string code = """
            using Darp.BinaryObjects;
            using System;
            using System.Diagnostics.CodeAnalysis;

            public sealed record ManualObject(ushort Value) : IBinaryObject<ManualObject>
            {
                int IBinaryWritable.GetByteCount() => 3;
                bool IBinaryWritable.TryWriteLittleEndian(Span<byte> destination) => throw new NotImplementedException();
                bool IBinaryWritable.TryWriteLittleEndian(Span<byte> destination, out int bytesWritten) => throw new NotImplementedException();
                bool IBinaryWritable.TryWriteBigEndian(Span<byte> destination) => throw new NotImplementedException();
                bool IBinaryWritable.TryWriteBigEndian(Span<byte> destination, out int bytesWritten) => throw new NotImplementedException();
                static bool IBinaryReadable<ManualObject>.TryReadLittleEndian(ReadOnlySpan<byte> source, [NotNullWhen(true)] out ManualObject? value) => throw new NotImplementedException();
                static bool IBinaryReadable<ManualObject>.TryReadLittleEndian(ReadOnlySpan<byte> source, [NotNullWhen(true)] out ManualObject? value, out int bytesRead) => throw new NotImplementedException();
                static bool IBinaryReadable<ManualObject>.TryReadBigEndian(ReadOnlySpan<byte> source, [NotNullWhen(true)] out ManualObject? value) => throw new NotImplementedException();
                static bool IBinaryReadable<ManualObject>.TryReadBigEndian(ReadOnlySpan<byte> source, [NotNullWhen(true)] out ManualObject? value, out int bytesRead) => throw new NotImplementedException();
            }

            [BinaryObject]
            public sealed partial record Parent(ManualObject Value, byte Tail);
            """;
        await VerifyBinaryObjectsGenerator(code);
    }

    [Fact]
    public async Task WriteOnly_NestedObjectWithUnboundReadonlyProperty()
    {
        const string code = """
            using Darp.BinaryObjects;
            using System;

            [BinaryObject(BinaryOptions.Write)]
            public sealed partial record Child
            {
                public Child(object context) => Value = true;
                public bool Value { get; }
                public int Computed => 42;
                public static readonly byte StaticField = 1;
                public static byte StaticProperty { get; } = 2;
                public static bool TryReadLittleEndian(ReadOnlySpan<byte> source, out Child? value) => throw new NotImplementedException();
            }

            [BinaryObject(BinaryOptions.Write)]
            public sealed partial record Parent(Child Value, byte Tail);
            """;
        await VerifyBinaryObjectsGenerator(code);
    }

    [Fact]
    public async Task ReadOnly_NestedManualObjectUsesConsumedByteCount()
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
            public sealed partial record Parent(ManualObject Value);
            """;
        await VerifyBinaryObjectsGenerator(code);
    }

    [Fact]
    public async Task WriteOnly_NestedManualObjectUsesReportedByteCount()
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
            public sealed partial record Parent(ManualObject Value);
            """;
        await VerifyBinaryObjectsGenerator(code);
    }
}

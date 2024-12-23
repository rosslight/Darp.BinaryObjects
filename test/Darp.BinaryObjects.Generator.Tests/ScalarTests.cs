namespace Darp.BinaryObjects.Generator.Tests;

using static VerifyHelper;

public sealed class ScalarTests
{
    [Fact]
    public async Task Primitives_OneBool()
    {
        const string code = """
            using Darp.BinaryObjects;

            [BinaryObject]
            public sealed partial record TestObject(bool Value);
            """;
        await VerifyBinaryObjectsGenerator(code);
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
            public sealed partial record TestObjectNested(bool Value)
            {
                public int IgnoredProperty => 1;
            }

            [BinaryObject]
            public sealed partial record TestObject(TestObjectNested Value);
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
}

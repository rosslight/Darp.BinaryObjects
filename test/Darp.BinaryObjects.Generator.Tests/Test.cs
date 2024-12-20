namespace Darp.BinaryObjects.Generator.Tests;

using static VerifyHelper;

public sealed class IntegrationTest
{
    [Fact]
    public async Task OneBool_DefaultAsync()
    {
        const string code = """
using Darp.BinaryObjects;

[BinaryObject]
public sealed partial record OneBool(bool Value);
""";
        await VerifyBinaryObjectsGenerator(code);
    }

    [Fact]
    public async Task TwoUShorts_DefaultAsync()
    {
        const string code = """
using Darp.BinaryObjects;

[BinaryObject]
public sealed partial record TwoUShorts(ushort ValueOne, ushort ValueTwo);
""";
        await VerifyBinaryObjectsGenerator(code);
    }

    [Fact]
    public async Task AllPrimitives_DefaultAsync()
    {
        const string code = """
using System;
using Darp.BinaryObjects;

[BinaryObject]
public sealed partial record AllPrimitives(
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
    public async Task BinaryObject_DefaultAsync()
    {
        const string code = """
using Darp.BinaryObjects;

[BinaryObject]
public sealed partial record OneBool(bool Value);

[BinaryObject]
public sealed partial record OneArray([property: BinaryElementCount(2)] bool[] Value);

[BinaryObject]
public sealed partial record OneBinaryObject(OneBool Value, OneArray Array);
""";
        await VerifyBinaryObjectsGenerator(code);
    }

    [Fact]
    public async Task ArrayByteFixedSize_DefaultAsync()
    {
        const string code = """
using Darp.BinaryObjects;

[BinaryObject]
public sealed partial record ArrayByteFixedSize([property: BinaryElementCount(2)] byte[] Value);
""";
        await VerifyBinaryObjectsGenerator(code);
    }

    [Fact]
    public async Task ArraysFixedSize_DefaultAsync()
    {
        const string code = """
using Darp.BinaryObjects;
using System;
using System.Collections.Generic;

[BinaryObject]
public partial record ArraysFixedSize(
    [property: BinaryElementCount(2)] ReadOnlyMemory<byte> ValueByteMemory,
    [property: BinaryElementCount(2)] byte[] ValueByteArray,
    [property: BinaryElementCount(2)] List<byte> ValueByteList,
    [property: BinaryElementCount(2)] IEnumerable<byte> ValueByteEnumerable,
    [property: BinaryElementCount(2)] ReadOnlyMemory<ushort> ValueUShortMemory,
    [property: BinaryElementCount(2)] ushort[] ValueUShortArray,
    [property: BinaryElementCount(2)] List<ushort> ValueUShortList,
    [property: BinaryElementCount(2)] IEnumerable<ushort> ValueUShortEnumerable
);
""";
        await VerifyBinaryObjectsGenerator(code);
    }

    [Fact]
    public async Task ArrayByteLength_DefaultAsync()
    {
        const string code = """
            using Darp.BinaryObjects;

            [BinaryObject]
            public sealed partial record ArrayByteLength(byte Length, [property: BinaryElementCount("Length")] byte[] Value);
            """;
        await VerifyBinaryObjectsGenerator(code);
    }

    [Fact]
    public async Task Asd()
    {
        const string code = """
using Darp.BinaryObjects;

[BinaryObject]
public sealed partial record OneBool1(bool Value);

[BinaryObject]
public sealed partial record OneBool2
{
    public bool Value { get; set; }
    public bool Value2 { get; init; }
}

[BinaryObject]
public sealed partial record OneBool3
{
    private OneBool3(bool value3, bool value)
    {
        Value = value;
        Value3 = value3;
    }
    public bool Value2 { get; init; }
    public readonly bool Value3;
}

public sealed partial record OneBool3
{
    public bool Value;
}

[BinaryObject]
public sealed partial record OneBool4
{
    public readonly bool Value;
}

[BinaryObject]
public sealed partial class OneBool5
{
    public OneBool5(bool value1, bool value2)
    {
        _value1 = value1;
        _value2 = value2;
    }

    private readonly bool _value1;
    private bool _value2;
}
""";
        await VerifyBinaryObjectsGenerator(code);
    }

    [Fact]
    public async Task PossibleMembers()
    {
        const string code = """
using Darp.BinaryObjects;
using System;
using System.Collections.Generic;

[BinaryObject]
public partial record Members1(byte ValueOne)
{
    private byte valueEight;
    public byte ValueTwo { get; } // Expecting warning
    private readonly byte ValueThree; // Expecting warning
    public byte ValueFour { get; set; }
    public byte ValueFive { get; init; }
    protected byte ValueSix;
    [BinaryIgnore] public readonly byte ValueSeven;
    public byte ValueEight { get => valueEight; set => valueEight = value; }
}

[BinaryObject]
public partial record OneBool2;

[BinaryObject]
public partial record OneBool
{
    private bool Value;
}

[BinaryObject]
public partial record Members2
{
    private readonly byte valueTwo;
    public byte ValueTwo { get; } // Expecting warning
    private readonly byte ValueThree; // Expecting warning
    public byte ValueFour { get; set; }
    public byte ValueFive { get; init; }
    protected byte ValueSix;

    private Members2(bool valueSix, byte valueTwo, byte valueSeven) { } // Warning1: invalid type bool for valueSix, Warning2: Invalid parameter valueSeven
}
""";
        await VerifyBinaryObjectsGenerator(code);
    }

    [Fact]
    public async Task OneByteEnum_DefaultAsync()
    {
        const string code = """
            using Darp.BinaryObjects;
            namespace Test.Space
            {
                public enum DefaultEnum {}
                public enum ByteEnum : byte {}
                public enum LongEnum : long {}
            }
            [BinaryObject]
            public sealed partial record OneByteEnum(Test.Space.DefaultEnum Value1,
                Test.Space.ByteEnum Value2,
                Test.Space.LongEnum Value3,
                [property: BinaryElementCount(2)] System.ReadOnlyMemory<Test.Space.ByteEnum> Value4,
                [property: BinaryElementCount(2)] System.ReadOnlyMemory<Test.Space.DefaultEnum> Value5);
            """;
        await VerifyBinaryObjectsGenerator(code);
    }

    [Fact]
    public async Task BinaryObjectArray_DefaultAsync()
    {
        const string code = """
using Darp.BinaryObjects;

[BinaryObject]
public sealed partial record OneBool(bool Value);

[BinaryObject]
public sealed partial record BinaryObjectArrays(
    [property: BinaryElementCount(2)] System.ReadOnlyMemory<OneBool> Value1,
    [property: BinaryElementCount(2)] OneBool[] Value2
);
""";
        await VerifyBinaryObjectsGenerator(code);
    }

    [Fact]
    public async Task ArrayReadRemaining_DefaultAsync()
    {
        const string code = """
            using Darp.BinaryObjects;

            [BinaryObject]
            public sealed partial record Unlimited(System.ReadOnlyMemory<byte> Value);

            [BinaryObject]
            public sealed partial record UnlimitedWithOffset(byte Offset, uint[] Value);
            """;
        await VerifyBinaryObjectsGenerator(code);
    }

    //[Fact]
    public async Task ArrayMinElementCount_DefaultAsync()
    {
        const string code = """
            using Darp.BinaryObjects;
            using System;

            [BinaryObject]
            public sealed partial record UnlimitedWithMinLength(
                int Length,
                [property: BinaryElementCount("Length"), BinaryMinElementCount(1)] ReadOnlyMemory<byte> Value,
                [property: BinaryMinElementCount(3)] int[] Value2);
            """;
        await VerifyBinaryObjectsGenerator(code);
    }

    [Fact]
    public async Task ManualBinaryObject_ConstantAsync()
    {
        const string code = """
using Darp.BinaryObjects;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Collections.Generic;

[BinaryConstant(1)]
public sealed record ManualConstantObject : IBinaryObject<ManualConstantObject>
{
    public int GetByteCount() => throw new NotImplementedException();
    public bool TryWriteLittleEndian(Span<byte> destination) => TryReadLittleEndian(destination, out _);
    public bool TryWriteLittleEndian(Span<byte> destination, out int bytesWritten) => throw new NotImplementedException();
    public bool TryWriteBigEndian(Span<byte> destination) => TryWriteBigEndian(destination, out _);
    public bool TryWriteBigEndian(Span<byte> destination, out int bytesWritten) => throw new NotImplementedException();
    public static bool TryReadLittleEndian(ReadOnlySpan<byte> source,[NotNullWhen(true)] out ManualConstantObject? value) => TryReadLittleEndian(source, out value, out _);
    public static bool TryReadLittleEndian(ReadOnlySpan<byte> source,[NotNullWhen(true)] out ManualConstantObject? value,out int bytesRead) => throw new NotImplementedException();
    public static bool TryReadBigEndian(ReadOnlySpan<byte> source,[NotNullWhen(true)] out ManualConstantObject? value) => TryReadBigEndian(source, out value, out _);
    public static bool TryReadBigEndian(ReadOnlySpan<byte> source,[NotNullWhen(true)] out ManualConstantObject? value,out int bytesRead) => throw new NotSupportedException();
}

[BinaryObject]
public sealed partial record UnlimitedWithMinLength(ManualConstantObject Value,
    [property: BinaryElementCount(2)] ManualConstantObject[] Values,
    byte Length,
    [property: BinaryElementCount("Length")] ManualConstantObject[] LengthValues,
    List<ManualConstantObject> RemainingValue);
""";
        await VerifyBinaryObjectsGenerator(code);
    }
}

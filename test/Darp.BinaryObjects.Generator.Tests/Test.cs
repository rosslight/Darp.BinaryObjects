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

            public enum DefaultEnum {}
            public enum ByteEnum : byte {}
            public enum LongEnum : long {}

            [BinaryObject]
            public sealed partial record OneByteEnum(DefaultEnum Value1,
                ByteEnum Value2,
                LongEnum Value3,
                [property: BinaryElementCount(2)] System.ReadOnlyMemory<ByteEnum> Value4,
                [property: BinaryElementCount(2)] System.ReadOnlyMemory<DefaultEnum> Value5);
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
}

namespace Darp.BinaryObjects.Generator.Tests;

using System.Text.RegularExpressions;
using static VerifyHelper;

public sealed partial class IntegrationTest
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

    [GeneratedRegex("A1")]
    public partial Regex A1();

    [GeneratedRegex("A2")]
    public partial Regex A2();

    [GeneratedRegex("A3")]
    public partial Regex A3();

    [GeneratedRegex("A4")]
    public partial Regex A4();

    [GeneratedRegex("A5")]
    public partial Regex A5();

    [GeneratedRegex("A6")]
    public partial Regex A6();

    [GeneratedRegex("A7")]
    public partial Regex A7();

    [GeneratedRegex("A8")]
    public partial Regex A8();

    [GeneratedRegex("A9")]
    public partial Regex A9();
}

namespace Darp.BinaryObjects.Generator.Tests;

using VerifyCS = Verifier.CSharpSourceGeneratorVerifier<BinaryObjectsGenerator>;

public class IntegrationTest
{
    [Fact]
    public async Task OneBool_DefaultAsync()
    {
        const string code = """
using Darp.BinaryObjects;

[BinaryObject]
public sealed partial record OneBool(bool Value);
""";
        await new VerifyCS.Test { TestState = { Sources = { code } } }
            .AddGeneratedSources()
            .RunAsync();
    }

    [Fact]
    public async Task TwoUShorts_DefaultAsync()
    {
        const string code = """
using Darp.BinaryObjects;

[BinaryObject]
public sealed partial record TwoUShorts(ushort ValueOne, ushort ValueTwo);
""";
        await new VerifyCS.Test { TestState = { Sources = { code } } }
            .AddGeneratedSources()
            .RunAsync();
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
        await new VerifyCS.Test { TestState = { Sources = { code } } }
            .AddGeneratedSources()
            .RunAsync();
    }

    [Fact]
    public async Task ArrayByteFixedSize_DefaultAsync()
    {
        const string code = """
using Darp.BinaryObjects;

[BinaryObject]
public sealed partial record ArrayByteFixedSize([property: BinaryElementCount(2)] byte[] Value);
""";
        await new VerifyCS.Test { TestState = { Sources = { code } } }
            .AddGeneratedSources()
            .RunAsync();
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
        await new VerifyCS.Test { TestState = { Sources = { code } } }
            .AddGeneratedSources()
            .RunAsync();
    }
}

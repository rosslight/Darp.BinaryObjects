namespace Darp.BinaryObjects.Generator.Tests;

using System.Text.Json;
using System.Text.Json.Serialization;
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
    public async Task OneBool_TwoClasses_NoNamespaces()
    {
        const string code = """
using Darp.BinaryObjects;

[BinaryObject]
public sealed partial record OneBool1(bool Value);

[BinaryObject]
public sealed partial record OneBool2(bool Value);
""";
        await VerifyBinaryObjectsGenerator(code);
    }

    [Fact]
    public async Task OneBool_TwoClasses_SameNamespace()
    {
        const string code = """
namespace Test;

using Darp.BinaryObjects;

[BinaryObject]
public sealed partial record OneBool1(bool Value);

[BinaryObject]
public sealed partial record OneBool2(bool Value);
""";
        await VerifyBinaryObjectsGenerator(code);
    }

    [Fact]
    public async Task OneBool_TwoClasses_DifferentNamespace()
    {
        const string code = """
using Darp.BinaryObjects;

namespace Test1
{
    [BinaryObject]
    public sealed partial record OneBool(bool Value);
}

namespace Test2
{
    [BinaryObject]
    public sealed partial record OneBool(bool Value);
}
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
    public bool Value { get; init; }
}

[BinaryObject]
public sealed partial record OneBool3
{
    private OneBool3(bool value3, bool value)
    {
        Value = value;
        Value3 = value3;
    }
    public bool Value { get; }
    public bool Value2 { get; init; }
    public readonly bool Value3;
}

[BinaryObject]
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
public sealed partial record OneBool4
{
    public readonly bool Value;
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
    public void X()
    {
        const string json = """
{
  "TemperatureCelsius" : 123,
  "Summary" : "asdasdasd"
}
""";
        var x = JsonSerializer.Deserialize<WeatherForecast>(json, SourceGenerationContext.Default.WeatherForecast);
        int i = 0;
    }
}

public partial record Members2
{
    private readonly byte valueTwo;
    public byte ValueTwo { get; }
    private readonly byte ValueThree;
    public byte ValueFour { get; set; }
    public byte ValueFive { get; init; }
    protected byte ValueSix;

    public Members2(byte valueSix, byte valueTwo) { }
}

public struct WeatherForecast(int temperatureCelsius)
{
    public int TemperatureCelsius { get; } = temperatureCelsius;
}

[JsonSourceGenerationOptions(
    WriteIndented = true,
    IncludeFields = true,
    IgnoreReadOnlyFields = false,
    IgnoreReadOnlyProperties = false
)]
[JsonSerializable(typeof(WeatherForecast))]
internal partial class SourceGenerationContext : JsonSerializerContext;

// [BinaryObject] Warning: Binary object without valid constructor. Provide only one or add BinaryConstructorAttribute
public readonly struct Asd
{
    public readonly byte A; // Warning: Member found but no corresponding parameter in a constructor found

    [BinaryIgnore]
    public readonly byte B;
    public readonly byte C;

    private Asd(byte c, byte d)
    {
        C = c;
    }
}

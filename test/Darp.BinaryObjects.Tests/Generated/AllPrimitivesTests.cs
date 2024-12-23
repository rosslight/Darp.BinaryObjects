namespace Darp.BinaryObjects.Tests.Generated;

using FluentAssertions;

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

public class AllPrimitivesTests
{
    [Theory]
    [InlineData(
        "0000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000",
        false,
        false,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0
    )]
    [InlineData(
        "0100010000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000",
        true,
        true,
        0,
        0,
        0x0001,
        0x0100,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0
    )]
    public void TryRead_GoodInputShouldBeValid(
        string hexString,
        bool expectedValueBoolLE,
        bool expectedValueBoolBE,
        sbyte expectedValueSByteLE,
        sbyte expectedValueSByteBE,
        short expectedValueShortLE,
        short expectedValueShortBE,
        ushort expectedValueHalfLE,
        ushort expectedValueHalfBE,
        int expectedValueIntLE,
        int expectedValueIntBE,
        float expectedValueFloatLE,
        float expectedValueFloatBE,
        long expectedValueLongLE,
        long expectedValueLongBE,
        long expectedValueInt128LE,
        long expectedValueInt128BE,
        ulong expectedValueUInt128LE,
        ulong expectedValueUInt128BE,
        ulong expectedValueULongLE,
        ulong expectedValueULongBE,
        double expectedValueDoubleLE,
        double expectedValueDoubleBE,
        uint expectedValueUIntLE,
        uint expectedValueUIntBE,
        ushort expectedValueUShortLE,
        ushort expectedValueUShortBE,
        byte expectedValueByteLE,
        byte expectedValueByteBE
    )
    {
        var buffer = Convert.FromHexString(hexString);

        var successLE1 = AllPrimitives.TryReadLittleEndian(buffer, out AllPrimitives? valueLE1);
        var successLE2 = AllPrimitives.TryReadLittleEndian(buffer, out AllPrimitives? valueLE2, out var consumedLE);
        var successBE1 = AllPrimitives.TryReadBigEndian(buffer, out AllPrimitives? valueBE1);
        var successBE2 = AllPrimitives.TryReadBigEndian(buffer, out AllPrimitives? valueBE2, out var consumedBE);

        successLE1.Should().BeTrue();
        successLE2.Should().BeTrue();
        successBE1.Should().BeTrue();
        successBE2.Should().BeTrue();
        valueLE1!.ValueBool.Should().Be(expectedValueBoolLE);
        valueLE1.ValueSByte.Should().Be(expectedValueSByteLE);
        valueLE1.ValueShort.Should().Be(expectedValueShortLE);
        valueLE1.ValueHalf.Should().Be((Half)expectedValueHalfLE);
        valueLE1.ValueInt.Should().Be(expectedValueIntLE);
        valueLE1.ValueFloat.Should().Be(expectedValueFloatLE);
        valueLE1.ValueLong.Should().Be(expectedValueLongLE);
        valueLE1.ValueInt128.Should().Be(expectedValueInt128LE);
        valueLE1.ValueUInt128.Should().Be(expectedValueUInt128LE);
        valueLE1.ValueULong.Should().Be(expectedValueULongLE);
        valueLE1.ValueDouble.Should().Be(expectedValueDoubleLE);
        valueLE1.ValueUInt.Should().Be(expectedValueUIntLE);
        valueLE1.ValueUShort.Should().Be(expectedValueUShortLE);
        valueLE1.ValueByte.Should().Be(expectedValueByteLE);
        valueLE2!.ValueBool.Should().Be(expectedValueBoolLE);
        valueLE2.ValueSByte.Should().Be(expectedValueSByteLE);
        valueLE2.ValueShort.Should().Be(expectedValueShortLE);
        valueLE2.ValueHalf.Should().Be((Half)expectedValueHalfLE);
        valueLE2.ValueInt.Should().Be(expectedValueIntLE);
        valueLE2.ValueFloat.Should().Be(expectedValueFloatLE);
        valueLE2.ValueLong.Should().Be(expectedValueLongLE);
        valueLE2.ValueInt128.Should().Be(expectedValueInt128LE);
        valueLE2.ValueUInt128.Should().Be(expectedValueUInt128LE);
        valueLE2.ValueULong.Should().Be(expectedValueULongLE);
        valueLE2.ValueDouble.Should().Be(expectedValueDoubleLE);
        valueLE2.ValueUInt.Should().Be(expectedValueUIntLE);
        valueLE2.ValueUShort.Should().Be(expectedValueUShortLE);
        valueLE2.ValueByte.Should().Be(expectedValueByteLE);
        valueBE1!.ValueBool.Should().Be(expectedValueBoolBE);
        valueBE1.ValueSByte.Should().Be(expectedValueSByteBE);
        valueBE1.ValueShort.Should().Be(expectedValueShortBE);
        valueBE1.ValueHalf.Should().Be((Half)expectedValueHalfBE);
        valueBE1.ValueInt.Should().Be(expectedValueIntBE);
        valueBE1.ValueFloat.Should().Be(expectedValueFloatBE);
        valueBE1.ValueLong.Should().Be(expectedValueLongBE);
        valueBE1.ValueInt128.Should().Be(expectedValueInt128BE);
        valueBE1.ValueUInt128.Should().Be(expectedValueUInt128BE);
        valueBE1.ValueULong.Should().Be(expectedValueULongBE);
        valueBE1.ValueDouble.Should().Be(expectedValueDoubleBE);
        valueBE1.ValueUInt.Should().Be(expectedValueUIntBE);
        valueBE1.ValueUShort.Should().Be(expectedValueUShortBE);
        valueBE1.ValueByte.Should().Be(expectedValueByteBE);
        valueBE2!.ValueBool.Should().Be(expectedValueBoolBE);
        valueBE2.ValueSByte.Should().Be(expectedValueSByteBE);
        valueBE2.ValueShort.Should().Be(expectedValueShortBE);
        valueBE2.ValueHalf.Should().Be((Half)expectedValueHalfBE);
        valueBE2.ValueInt.Should().Be(expectedValueIntBE);
        valueBE2.ValueFloat.Should().Be(expectedValueFloatBE);
        valueBE2.ValueLong.Should().Be(expectedValueLongBE);
        valueBE2.ValueInt128.Should().Be(expectedValueInt128BE);
        valueBE2.ValueUInt128.Should().Be(expectedValueUInt128BE);
        valueBE2.ValueULong.Should().Be(expectedValueULongBE);
        valueBE2.ValueDouble.Should().Be(expectedValueDoubleBE);
        valueBE2.ValueUInt.Should().Be(expectedValueUIntBE);
        valueBE2.ValueUShort.Should().Be(expectedValueUShortBE);
        valueBE2.ValueByte.Should().Be(expectedValueByteBE);
        consumedLE.Should().Be(77);
        consumedBE.Should().Be(77);
    }

    [Theory]
    [InlineData("")]
    [InlineData("00")]
    [InlineData("000000")]
    public void TryRead_BadInputShouldBeValid(string hexString)
    {
        var buffer = Convert.FromHexString(hexString);

        var successLE1 = AllPrimitives.TryReadLittleEndian(buffer, out AllPrimitives? valueLE1);
        var successLE2 = AllPrimitives.TryReadLittleEndian(buffer, out AllPrimitives? valueLE2, out var consumedLE);
        var successBE1 = AllPrimitives.TryReadBigEndian(buffer, out AllPrimitives? valueBE1);
        var successBE2 = AllPrimitives.TryReadBigEndian(buffer, out AllPrimitives? valueBE2, out var consumedBE);

        successLE1.Should().BeFalse();
        successLE2.Should().BeFalse();
        successBE1.Should().BeFalse();
        successBE2.Should().BeFalse();
        valueLE1.Should().BeNull();
        valueLE2.Should().BeNull();
        valueBE1.Should().BeNull();
        valueBE2.Should().BeNull();
        consumedLE.Should().Be(0);
        consumedBE.Should().Be(0);
    }

    [Theory]
    [InlineData(
        false,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        77,
        "0000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000",
        "0000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000"
    )]
    [InlineData(
        true,
        1,
        1,
        1,
        1,
        1,
        1,
        1,
        1,
        1,
        1,
        1,
        1,
        1,
        77,
        "01010100003C010000000000803F010000000000000001000000000000000000000000000000010000000000000000000000000000000100000000000000000000000000F03F01000000010001",
        "010100013C00000000013F8000000000000000000001000000000000000000000000000000010000000000000000000000000000000100000000000000013FF000000000000000000001000101"
    )]
    public void TryWrite_GoodInputShouldBeValid(
        bool valueBool,
        sbyte valueSByte,
        short valueShort,
        ushort valueHalf,
        int valueInt,
        float valueFloat,
        long valueLong,
        long valueInt128,
        ulong valueUInt128,
        ulong valueULong,
        double valueDouble,
        uint valueUInt,
        ushort valueUShort,
        byte valueByte,
        int bufferSize,
        string expectedHexStringLE,
        string expectedHexStringBE
    )
    {
        var bufferLE = new byte[bufferSize];
        var bufferBE = new byte[bufferSize];
        var expectedHexBytesLE = Convert.FromHexString(expectedHexStringLE);
        var expectedHexBytesBE = Convert.FromHexString(expectedHexStringBE);
        var writable = new AllPrimitives(
            valueBool,
            valueSByte,
            valueShort,
            (Half)valueHalf,
            valueInt,
            valueFloat,
            valueLong,
            valueInt128,
            valueUInt128,
            valueULong,
            valueDouble,
            valueUInt,
            valueUShort,
            valueByte
        );

        var successLE1 = writable.TryWriteLittleEndian(bufferLE);
        var successLE2 = writable.TryWriteLittleEndian(bufferLE, out var writtenLE);
        var successBE1 = writable.TryWriteBigEndian(bufferBE);
        var successBE2 = writable.TryWriteBigEndian(bufferBE, out var writtenBE);

        successLE1.Should().BeTrue();
        successLE2.Should().BeTrue();
        successBE1.Should().BeTrue();
        successBE2.Should().BeTrue();
        bufferLE.Should().BeEquivalentTo(expectedHexBytesLE);
        bufferBE.Should().BeEquivalentTo(expectedHexBytesBE);
        writtenLE.Should().Be(77);
        writtenBE.Should().Be(77);
        writable.GetByteCount().Should().Be(77);
    }

    [Theory]
    [InlineData(0, "")]
    [InlineData(3, "000000")]
    [InlineData(
        76,
        "00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000"
    )]
    public void TryWrite_BadInputShouldBeValid(int bufferSize, string expectedHexString)
    {
        var bufferLE = new byte[bufferSize];
        var bufferBE = new byte[bufferSize];
        var expectedHexBytes = Convert.FromHexString(expectedHexString);
        var writable = new AllPrimitives(
            default,
            default,
            default,
            default,
            default,
            default,
            default,
            default,
            default,
            default,
            default,
            default,
            default,
            default
        );

        var successLE1 = writable.TryWriteLittleEndian(bufferLE);
        var successLE2 = writable.TryWriteLittleEndian(bufferLE, out var writtenLE);
        var successBE1 = writable.TryWriteBigEndian(bufferBE);
        var successBE2 = writable.TryWriteBigEndian(bufferBE, out var writtenBE);

        successLE1.Should().BeFalse();
        successLE2.Should().BeFalse();
        successBE1.Should().BeFalse();
        successBE2.Should().BeFalse();
        bufferLE.Should().BeEquivalentTo(expectedHexBytes);
        bufferBE.Should().BeEquivalentTo(expectedHexBytes);
        writtenLE.Should().Be(0);
        writtenBE.Should().Be(0);
    }
}

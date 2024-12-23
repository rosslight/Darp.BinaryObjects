namespace Darp.BinaryObjects.Generator.Tests;

public sealed class Tests
{
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
        await VerifyHelper.VerifyBinaryObjectsGenerator(code);
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
        await VerifyHelper.VerifyBinaryObjectsGenerator(code);
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
        await VerifyHelper.VerifyBinaryObjectsGenerator(code);
    }
}

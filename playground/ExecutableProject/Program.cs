// See https://aka.ms/new-console-template for more information

using System.Buffers.Binary;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Darp.BinaryObjects;
using Testi.Asd;

Console.WriteLine("Hello, World!");

/*

Constant(       string Name,                                        Type FieldType,         int FieldLength);
ConstantArray(  string Name,    Type ArrayType,                     Type UnderlyingType,    int FieldLength);
Unknown(        string Name,                                        Type FieldType);
UnknownArray(   string Name,    Type ArrayType, int ArrayCount,     Type UnderlyingType);
VariableArray(  string Name,    Type ArrayType, int MinArrayCount,  Type UnderlyingType);

// Constant BinaryObject (unmanaged types, enums, blittable types, known objects (string, BigInteger, BitArray))
[BinaryConstant(0)]
public record Object0_1();
public record Object0_2(int A, SomeEnum B);
public record Object0_3([property: BinaryLength(3)] int A);
public record Object0_4([property: BinaryLength(8)] int[] B);
public record Object0_5([property: BinaryElementCount(2)] int[] B);
public record Object0_6([property: BinaryElementCount(2, 3)] int[] B);
public record Object0_7([property: BinaryLength(16)] string A);
public record Object0_8([property: BinaryElementCount(8)] string A);

{
    Object0_1: {},
    Object0_2: { Constant(System.Int32, 4), Constant(Namespace.SomeEnum, 2) },
    Object0_3: { Constant(System.Int32, 3) },
    Object0_4: { ConstantArray(System.Array, System.Int32, 8) },
    Object0_5: { ConstantArray(System.Array, System.Int32, 8) },
    Object0_6: { ConstantArray(System.Array, System.Int32, 6) },
    Object0_7: { ConstantArray(System.String, System.Char, 16) },
    Object0_8: { ConstantArray(System.String, System.Char, 16) },
}


// Constant/Variable nested BinaryObject (dependent on nested BinaryObject)
public record Object1_1(Object0_2 A);
public record Object1_2([property: BinaryLength(12)] Object0_2[] A);
public record Object1_3([property: BinaryElementCount(2)] Object0_2[] A);

{
    Object1_1: { Unknown(Namespace.Object0_2);
    Object1_2: { ConstantArray(System.Array, Namespace.Object0_2, 12);
    Object1_2: { UnknownArray(System.Array, 2, Namespace.Object0_2);
}


// Variable BinaryObject
public record Object2_0([property: BinaryElementLength(3)] int[] A);
public record Object2_1(int[] A);
public record Object2_2([property: BinaryMinElementCount(2)] int[] A);
public record Object2_3(int Length, [property: BinaryElementCount("Length")] int[] B);
public record Object2_4(int Length, [property: BinaryLength("Length")] int B);
public record Object2_5(int Length, [property: BinaryLength("Length")] int[] B);
public record Object2_6(int Length, [property: BinaryMinElementCount(2), BinaryLength("Length")] int[] B);
public record Object2_7(int Length, [property: BinaryLength("Length")] string B);
public record Object2_8<T>(T A) where T : unmanaged;

{
    Object2_1: [ VariableArray(System.Array, 0, System.Int32) ];
    Object2_2: [ VariableArray(System.Array, 2, System.Int32) ];
    Object2_3: [ Constant(System.Int32, 4), VariableArray(System.Array, 0, "Length", System.Int32) ];
    Object2_4: [ Constant(System.Int32, 4), Variable(System.Int32, "Length") ];
    Object2_5: [ Constant(System.Int32, 4), VariableArray(System.Array, 0, System.Int32, "Length") ];
    Object2_6: [ Constant(System.Int32, 4), VariableArray(System.Array, 2, System.Int32, "Length") ];
    Object2_7: [ Constant(System.Int32, 4), VariableArray(System.String, 0, System.Char, "Length") ];
    Object2_8: [ Variable(T) ];
}

// Variable nested BinaryObject
public record Object3_1<T>(T[] A) where T : IBinaryObject<T>;
public record Object3_2(int Length, [property: BinaryLength("Length")] Object0_1[] A);
public record Object3_3(int Length, [property: BinaryElementCount("Length")] Object0_1[] A);

{
    Object3_1: [ VariableArray(System.Array, 0, T) ];
    Object2_3: [ Constant(System.Int32, 4), VariableArray(System.Array, 0, Namespace.Object0_1, "Length") ];
    Object2_3: [ Constant(System.Int32, 4), VariableArray(System.Array, 0, "Length", Namespace.Object0_1) ];
}


// Invalid objects but valid parsing
public record Object4_1(SomeRandomObject A);

{
    Object4_1: { Unknown(Namespace.SomeRandomObject);
}

*/

// Read the struct from the buffer using either little or big endian format
var buffer = Convert.FromHexString("AABBCC");

var success = ArrayByteFixedSize.TryReadLittleEndian(source: buffer, out var value);
var success2 = ArrayByteFixedSize.TryReadBigEndian(source: buffer, out var value2, out int bytesRead);

// Get the actual size of the struct
var size = value!.GetByteCount();

// Write the values back to a buffer
var writeBuffer = new byte[size];
value.WriteBigEndian(destination: writeBuffer);
var success4 = value2!.TryWriteLittleEndian(destination: writeBuffer, out int bytesWritten);

var x = ReadUInt64EnumArrayBigEndian<TestEnum>(writeBuffer);

WriteUInt16SpanBigEndian(writeBuffer.AsSpan().Slice(3, 6), new ushort[3]);

static void WriteUInt16SpanBigEndian(Span<byte> destination, ReadOnlySpan<ushort> value)
{
    var length = Math.Min(value.Length, destination.Length / 2);
    if (BitConverter.IsLittleEndian)
    {
        Span<ushort> reinterpretedDestination = MemoryMarshal.Cast<byte, ushort>(destination);
        BinaryPrimitives.ReverseEndianness(value[..length], reinterpretedDestination);
        return;
    }
    MemoryMarshal.Cast<ushort, byte>(value[..length]).CopyTo(destination);
}

static TEnum[] ReadUInt64EnumArrayBigEndian<TEnum>(ReadOnlySpan<byte> source)
    where TEnum : unmanaged, Enum
{
    var array = MemoryMarshal.Cast<byte, TEnum>(source).ToArray();
    if (BitConverter.IsLittleEndian)
    {
        var underlyingArray = MemoryMarshal.Cast<TEnum, ulong>(array);
        BinaryPrimitives.ReverseEndianness(underlyingArray, underlyingArray);
    }
    return array;
}

static uint[] ReadUInt32ArrayLittleEndian(ReadOnlySpan<byte> source, out int bytesRead)
{
    var array = MemoryMarshal.Cast<byte, uint>(source).ToArray();
    if (!BitConverter.IsLittleEndian)
        BinaryPrimitives.ReverseEndianness(array, array);
    bytesRead = array.Length * 4;
    return array;
}

static void WriteUInt32SpanLittleEndian<TEnum>(Span<byte> destination, ReadOnlySpan<int> value)
    where TEnum : unmanaged, Enum
{
    if (!BitConverter.IsLittleEndian)
    {
        Span<int> reinterpretedDestination = MemoryMarshal.Cast<byte, int>(destination);
        BinaryPrimitives.ReverseEndianness(value, reinterpretedDestination);
        return;
    }
    MemoryMarshal.Cast<int, byte>(value).CopyTo(destination);
}

static ulong[] ReadUInt64ArrayBigEndian(ReadOnlySpan<byte> source)
{
    var array = MemoryMarshal.Cast<byte, ulong>(source).ToArray();
    if (BitConverter.IsLittleEndian)
        BinaryPrimitives.ReverseEndianness(array, array);
    return array;
}

var lsit = ReadBinaryObjectListBigEndian<Aa>(buffer, 1, out var _);

int i = 0;
return;

static List<T> ReadBinaryObjectListBigEndian<T>(ReadOnlySpan<byte> source, int numberOfElements, out int bytesRead)
    where T : IBinaryReadable<T>
{
    var elementLength = source.Length / numberOfElements;
    var array = new List<T>(numberOfElements);
    for (var i = 0; i < numberOfElements; i++)
    {
        if (!T.TryReadBigEndian(source.Slice(i * elementLength, elementLength), out T? value, out var tempBytesRead))
            throw new ArgumentException($"Could not read {typeof(T).Name} from source");
        array[i] = value;
    }
    bytesRead = numberOfElements * elementLength;
    return array;
}

static void WriteUInt64LittleEndian(Span<byte> destination, ulong value, int length)
{
    Span<byte> buffer = stackalloc byte[8];
    BinaryPrimitives.WriteUInt64LittleEndian(buffer, value);
    buffer[..length].CopyTo(destination);
}

static void WriteUInt64BigEndian(Span<byte> destination, ulong value, int length)
{
    Span<byte> buffer = stackalloc byte[8];
    BinaryPrimitives.WriteUInt64BigEndian(buffer, value);
    buffer[(8 - length)..].CopyTo(destination);
}

static ulong ReadUInt64LittleEndian(ReadOnlySpan<byte> source, int length)
{
    Span<byte> buffer = stackalloc byte[8];
    source.CopyTo(buffer);
    return BinaryPrimitives.ReadUInt64LittleEndian(buffer);
}

static ulong ReadUInt64BigEndian(ReadOnlySpan<byte> source, int length)
{
    Span<byte> buffer = stackalloc byte[8];
    source.CopyTo(buffer[(8 - length)..]);
    return BinaryPrimitives.ReadUInt64BigEndian(buffer);
}

static ulong[] ReadUInt64ArrayBigEndian2(ReadOnlySpan<byte> source, int length)
{
    var array = new ulong[source.Length / length];
    Span<byte> buffer = stackalloc byte[8];
    Span<byte> bufferToRead = buffer[(8 - length)..];
    for (var i = 0; i < array.Length; i++)
    {
        source.Slice(i * length, length).CopyTo(bufferToRead);
        array[i] = ReadUInt64BigEndian(bufferToRead, length);
    }
    return array;
}

static void WriteUInt64SpanLittleEndian(Span<byte> destination, ReadOnlySpan<ulong> value, int length)
{
    Span<byte> buffer = stackalloc byte[8];
    Span<byte> bufferToWrite = buffer[..length];
    for (var i = 0; i < value.Length; i++)
    {
        BinaryPrimitives.WriteUInt64LittleEndian(buffer, value[1]);
        bufferToWrite.CopyTo(destination.Slice(i * length, length));
    }
}

static TArray ReadLittleEndian<TArray, T>(ReadOnlySpan<byte> source, int size)
    where TArray : struct
    where T : struct
{
    var value = new TArray();
    Span<T> buffer = MemoryMarshal.CreateSpan(ref Unsafe.As<TArray, T>(ref value), size);
    /*
    if (!BitConverter.IsLittleEndian)
    {
        switch (buffer)
        {
            case Span<ushort> typedBuffer:
                BinaryPrimitives.ReverseEndianness(typedBuffer, typedBuffer);
                break;
            case Span<short> typedBuffer:
                BinaryPrimitives.ReverseEndianness(typedBuffer, typedBuffer);
                break;
            case Span<uint> typedBuffer:
                BinaryPrimitives.ReverseEndianness(typedBuffer, typedBuffer);
                break;
            case Span<int> typedBuffer:
                BinaryPrimitives.ReverseEndianness(typedBuffer, typedBuffer);
                break;
            case Span<ulong> typedBuffer:
                BinaryPrimitives.ReverseEndianness(typedBuffer, typedBuffer);
                break;
            case Span<long> typedBuffer:
                BinaryPrimitives.ReverseEndianness(typedBuffer, typedBuffer);
                break;
            case Span<UInt128> typedBuffer:
                BinaryPrimitives.ReverseEndianness(typedBuffer, typedBuffer);
                break;
            case Span<Int128> typedBuffer:
                BinaryPrimitives.ReverseEndianness(typedBuffer, typedBuffer);
                break;
        }
    }*/
    MemoryMarshal.Cast<byte, T>(source).CopyTo(buffer);
    return value;
}

static TArray ReadUInt16InlineArrayLittleEndian<TArray, T>(ReadOnlySpan<byte> source, int size)
    where TArray : struct
    where T : struct
{
    var value = new TArray();
    Span<ushort> buffer = MemoryMarshal.CreateSpan(ref Unsafe.As<TArray, ushort>(ref value), size);
    MemoryMarshal.Cast<byte, ushort>(source).CopyTo(buffer);
    return value;
}

static void Write<TArray, T>(Span<byte> destination, TArray value, int size)
    where TArray : struct
    where T : struct
{
    Span<T> buffer = MemoryMarshal.CreateSpan(ref Unsafe.As<TArray, T>(ref value), size);
    MemoryMarshal.Cast<T, byte>(buffer).CopyTo(destination);
}

static TEnum[] ReadInt32EnumArrayBigEndian<TEnum>(ReadOnlySpan<byte> source, out int bytesRead)
    where TEnum : unmanaged, Enum
{
    var array = MemoryMarshal.Cast<byte, TEnum>(source).ToArray();
    if (BitConverter.IsLittleEndian)
    {
        var reinterpretedArray = MemoryMarshal.Cast<TEnum, int>(array);
        BinaryPrimitives.ReverseEndianness(reinterpretedArray, reinterpretedArray);
    }
    bytesRead = array.Length * 4;
    return array;
}

static List<TEnum> ReadInt32EnumListBigEndian<TEnum>(ReadOnlySpan<byte> source, out int bytesRead)
    where TEnum : unmanaged, Enum
{
    ReadOnlySpan<TEnum> span = MemoryMarshal.Cast<byte, TEnum>(source);
    var list = new List<TEnum>(span.Length);
    list.AddRange(span);
    if (!BitConverter.IsLittleEndian)
    {
        Span<int> reinterpretedList = MemoryMarshal.Cast<TEnum, int>(CollectionsMarshal.AsSpan(list));
        BinaryPrimitives.ReverseEndianness(reinterpretedList, reinterpretedList);
    }
    bytesRead = list.Count * 4;
    return list;
}

static bool TryReadBinaryIntegerLittleEndian<T>(
    ReadOnlySpan<byte> source,
    bool isUnsigned,
    out T value,
    out int bytesRead
)
    where T : struct, IBinaryInteger<T>
{
    if (!T.TryReadLittleEndian(source, isUnsigned, out value))
    {
        bytesRead = 0;
        return false;
    }
    bytesRead = value.GetByteCount();
    return true;
}

static void WriteBinaryIntegerLittleEndian<T>(Span<byte> destination, T value, out int bytesWritten)
    where T : struct, IBinaryInteger<T>
{
    value.WriteLittleEndian(destination);
    bytesWritten = value.GetByteCount();
}

delegate ref Span<T> Aaaa<in TBuffer, T>(TBuffer buffer)
    where TBuffer : struct
    where T : struct;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public sealed class BinarySignAttribute(bool IsUnsigned) : Attribute;

[InlineArray(3)]
public struct UInt24
{
    private byte _field;
}

[BinaryObject]
public readonly partial record struct SomeTestStruct(byte A, ushort B, ReadOnlyMemory<int> Data);

public enum TestEnum : ulong;

public record Test1(Test2 A);

public partial record Test2(Test1 B)
{
    partial byte Length { get; }
}

namespace Testi.Asd
{
    using System.Diagnostics.CodeAnalysis;

    /*
    public static class Ex
    {
        public static void ReverseEndianness<T>(ReadOnlySpan<T> source, Span<T> destination)
            where T : IBinaryWritable
        {
            for (var index = 0; index < source.Length; index++)
            {
                T aa = source[index];
                destination[index] = T.ReverseEndianness(aa);
            }
        }
    }*/

    [BinaryObject]
    public readonly partial struct DeviceAddress(byte byte0, byte byte1, byte byte2, byte byte3, byte byte4, byte byte5)
    {
        private readonly byte _byte0 = byte0;
        private readonly byte _byte1 = byte1;
        private readonly byte _byte2 = byte2;
        private readonly byte _byte3 = byte3;
        private readonly byte _byte4 = byte4;
        private readonly byte _byte5 = byte5;
    }

    [BinaryObject]
    public readonly partial struct Test
    {
        public required int Test2 { get; init; }

        [property: BinaryElementCount(2)]
        public required ReadOnlyMemory<Aa> Aaa { get; init; }
        public required int Test3 { get; init; }
    }

    [BinaryObject]
    public readonly partial record struct Aa(bool I, int Ii);

    [BinaryObject]
    public sealed partial record ArrayByteFixedSize(
        [property: BinaryElementCount(2)] TestEnum[] Value2,
        int Asd,
        [property: BinaryElementCount("Asd")] byte[] Value,
        ArrayByteFixedSize Value3,
        TestEnum Value4,
        DeviceAddress aaaa
    );

    public enum HciOpCode : ushort
    {
        /// <summary> Invalid Hci command </summary>
        None,
    }

    [BinaryObject]
    public readonly partial record struct HciLeExtendedCreateConnectionV1Command
    {
        public static HciOpCode OpCode => HciOpCode.None;

        /// <summary> The Initiator_Filter_Policy parameter is used to determine whether the Filter Accept List is used and whether to process decision PDUs and other advertising PDUs </summary>
        public required byte InitiatorFilterPolicy { get; init; }

        /// <summary> The Own_Address_Type parameter indicates the type of address being used in the connection request packets. </summary>
        public required byte OwnAddressType { get; init; }

        /// <summary> The Peer_Address_Type parameter indicates the type of address used in the connectable advertisement sent by the peer. </summary>
        public required byte PeerAddressType { get; init; }

        /// <summary> The Peer_Address parameter </summary>
        public required DeviceAddress PeerAddress { get; init; }

        /// <summary> The Initiating_PHYs parameter indicates the PHY(s) on which the advertising packets should be received on the primary advertising physical channel and the PHYs for which connection parameters have been specified </summary>
        public required byte InitiatingPhys { get; init; }

        /// <summary> The Scan_Interval[i] and Scan_Window[i] parameters are recommendations from the Host on how long (Scan_Window[i]) and how frequently (Scan_Interval[i]) the Controller should scan </summary>
        public required ushort ScanInterval { get; init; }

        /// <summary> The Scan_Interval[i] and Scan_Window[i] parameters are recommendations from the Host on how long (Scan_Window[i]) and how frequently (Scan_Interval[i]) the Controller should scan </summary>
        public required ushort ScanWindow { get; init; }

        /// <summary> The Connection_Interval_Min[i] and Connection_Interval_Max[i] parameters define the minimum and maximum allowed connection interval </summary>
        public required ushort ConnectionIntervalMin { get; init; }

        /// <summary> The Connection_Interval_Min[i] and Connection_Interval_Max[i] parameters define the minimum and maximum allowed connection interval </summary>
        public required ushort ConnectionIntervalMax { get; init; }

        /// <summary> The Max_Latency[i] parameter defines the maximum allowed Peripheral latency </summary>
        public required ushort MaxLatency { get; init; }

        /// <summary> The Supervision_Timeout[i] parameter defines the link supervision timeout for the connection </summary>
        public required ushort SupervisionTimeout { get; init; }

        /// <summary> The Min_CE_Length[i] and Max_CE_Length[i] parameters provide the Controller with the expected minimum and maximum length of the connection events </summary>
        public required ushort MinCeLength { get; init; }

        /// <summary> The Min_CE_Length[i] and Max_CE_Length[i] parameters provide the Controller with the expected minimum and maximum length of the connection events </summary>
        public required ushort MaxCeLength { get; init; }
    }
}

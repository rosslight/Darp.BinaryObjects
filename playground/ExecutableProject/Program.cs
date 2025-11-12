// See https://aka.ms/new-console-template for more information

using System.Buffers.Binary;
using System.Runtime.InteropServices;
using Darp.BinaryObjects;
using Testi.Asd;

Console.WriteLine("Hello, World!");

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

[BinaryObject]
public readonly partial record struct SomeTestStruct(byte A, ushort B, ReadOnlyMemory<int> Data);

public enum TestEnum : ulong;

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
    public readonly partial record struct Aa(bool I, int Ii);

    [BinaryObject]
    public readonly partial struct Test
    {
        public required int Test2 { get; init; }

        [property: BinaryElementCount(2)]
        public required ReadOnlyMemory<Aa> Aaa { get; init; }
        public required int Test3 { get; init; }
    }

    [BinaryObject]
    public sealed partial record ArrayByteFixedSize(
        [property: BinaryElementCount(2)] TestEnum[] Value2,
        int Asd,
        [property: BinaryElementCount("Asd")] byte[] Value,
        ArrayByteFixedSize Value3,
        TestEnum Value4
    );
}

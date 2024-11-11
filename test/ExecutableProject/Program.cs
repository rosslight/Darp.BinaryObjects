﻿// See https://aka.ms/new-console-template for more information

using Darp.BinaryObjects;
using Test.Asd;

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

int i = 0;

namespace Test.Asd
{
    [BinaryObject]
    public sealed partial record ArrayByteFixedSize([property: BinaryElementCount(2)] byte[] Value);
}

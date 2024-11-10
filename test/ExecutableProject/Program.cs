// See https://aka.ms/new-console-template for more information

using Darp.BinaryObjects;
using Test.Asd;

Console.WriteLine("Hello, World!");

OneBool.TryReadLittleEndian(Convert.FromHexString("010002"), out var value);
int i = 0;

namespace Test.Asd
{
    [BinaryObject]
    public partial record struct OneBool(bool Value, ushort Value2, ushort Value3);
}

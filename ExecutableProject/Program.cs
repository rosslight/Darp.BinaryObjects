// See https://aka.ms/new-console-template for more information

using Darp.BinaryObjects;
using Test;

Console.WriteLine("Hello, World!");

OneBool.TryReadLittleEndian(Convert.FromHexString("01"), out var value);
int i = 0;

namespace Test
{
    [BinaryObject]
    public partial record OneBool(bool Value, bool Value2, bool Value3);
}

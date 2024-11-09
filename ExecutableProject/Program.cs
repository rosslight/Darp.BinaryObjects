// See https://aka.ms/new-console-template for more information

using Darp.BinaryObjects;

Console.WriteLine("Hello, World!");

OneBool.TryReadBigEndian(Convert.FromHexString("01"), out var value);

int i = 0;

[BinaryObject]
public readonly partial record struct OneBool(bool Value, bool Value2, bool Value3);

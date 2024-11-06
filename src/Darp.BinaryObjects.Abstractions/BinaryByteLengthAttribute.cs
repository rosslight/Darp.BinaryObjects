namespace Darp.BinaryObjects.Abstractions;

/// <summary> Sets the number of bytes to be used when reading or writing a primitive </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class BinaryByteLengthAttribute : Attribute;

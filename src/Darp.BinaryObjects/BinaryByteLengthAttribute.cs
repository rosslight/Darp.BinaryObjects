namespace Darp.BinaryObjects;

/// <summary> Sets the number of bytes to be used when reading or writing a primitive </summary>
/// <param name="byteLength"> The number of bytes on the field </param>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class BinaryByteLengthAttribute(int byteLength) : Attribute
{
    /// <summary>The number of bytes on the field </summary>
    public int ByteLength { get; } = byteLength;
}

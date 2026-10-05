namespace Darp.BinaryObjects;

/// <summary> Read and write an integer or enum with fewer bytes than its type has. Applies to each element of a collection </summary>
/// <param name="byteWidth"> The number of bytes of each value. Has to be between 1 and the size of the type </param>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class BinaryByteWidthAttribute(int byteWidth) : Attribute
{
    /// <summary> The number of bytes of each value </summary>
    public int ByteWidth { get; } = byteWidth;
}

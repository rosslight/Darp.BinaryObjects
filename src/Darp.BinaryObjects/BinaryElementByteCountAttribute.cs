namespace Darp.BinaryObjects;

/// <summary> Read and write each integer or enum of a collection with fewer bytes than its type has </summary>
/// <param name="byteCount"> The number of bytes of each element. Has to be between 1 and the size of the element type </param>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class BinaryElementByteCountAttribute(int byteCount) : Attribute
{
    /// <summary> The number of bytes of each element </summary>
    public int ByteCount { get; } = byteCount;
}

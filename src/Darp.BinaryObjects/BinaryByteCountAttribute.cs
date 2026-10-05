namespace Darp.BinaryObjects;

/// <summary> Read and write an integer or enum with fewer bytes than its type has </summary>
/// <param name="byteCount"> The number of bytes of the value. Has to be between 1 and the size of the type </param>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class BinaryByteCountAttribute(int byteCount) : Attribute
{
    /// <summary> The number of bytes of the value </summary>
    public int ByteCount { get; } = byteCount;
}

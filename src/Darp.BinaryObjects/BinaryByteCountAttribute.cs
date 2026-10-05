namespace Darp.BinaryObjects;

/// <summary> Sets the number of bytes a member occupies during binary reading or writing </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class BinaryByteCountAttribute : Attribute
{
    /// <summary> Read and write an integer or enum with fewer bytes than its type has </summary>
    /// <param name="byteCount"> The number of bytes of the value. Has to be between 1 and the size of the type </param>
    public BinaryByteCountAttribute(int byteCount)
    {
        ByteCount = byteCount;
    }

    /// <summary> Use a member which provides the number of bytes of a collection </summary>
    /// <param name="memberWithByteCount"> The name of the member </param>
    public BinaryByteCountAttribute(string memberWithByteCount)
    {
        MemberWithByteCount = memberWithByteCount;
    }

    /// <summary> The name of the member to provide the number of bytes of the collection </summary>
    public string? MemberWithByteCount { get; }

    /// <summary> The number of bytes of the value </summary>
    public int? ByteCount { get; }
}

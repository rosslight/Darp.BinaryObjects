namespace Darp.BinaryObjects;

/// <summary> Read the remaining bytes into the array this attribute is applied on </summary>
/// <param name="minElements"> The minimum number of elements expected to be inside the collection </param>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class BinaryMinElementCountAttribute(int minElements) : Attribute
{
    /// <summary> The minimum number of elements expected to be inside the collection </summary>
    public int MinElements { get; } = minElements;
}

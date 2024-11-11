namespace Darp.BinaryObjects;

/// <summary> Read the remaining bytes into the array this attribute is applied on </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class BinaryReadRemainingAttribute : Attribute
{
    /// <summary> An optional minimum number of elements of the collection </summary>
    public int MinElements { get; set; }
}

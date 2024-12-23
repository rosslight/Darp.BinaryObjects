namespace Darp.BinaryObjects;

/// <summary> Marks the BinaryObject to be of a constant length </summary>
/// <param name="constantLength"> The constant length of the BinaryObject </param>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public sealed class BinaryConstantAttribute(int constantLength) : Attribute
{
    /// <summary> The constant length of the BinaryObject </summary>
    public int ConstantLength { get; } = constantLength;
}

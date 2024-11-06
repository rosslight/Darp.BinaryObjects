namespace Darp.BinaryObjects.Abstractions;

/// <summary> Read the remaining bytes into the array this attribute is applied on </summary>
/// <param name="minLength"> An optional minimum length for the array </param>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class BinaryReadRemainingAttribute(int minLength = 0) : Attribute
{
    /// <summary> An optional minimum length for the array </summary>
    public int MinLength { get; } = minLength;
}

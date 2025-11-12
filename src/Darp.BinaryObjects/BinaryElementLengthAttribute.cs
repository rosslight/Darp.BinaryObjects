namespace Darp.BinaryObjects;

/// <summary> Supply a length to an element of an array during binary reading or writing </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class BinaryElementLengthAttribute : Attribute
{
    /// <summary> Use a defined length for each element of an array </summary>
    /// <param name="elementLength"> The constant length of an element </param>
    public BinaryElementLengthAttribute(int elementLength)
    {
        ElementLength = elementLength;
    }

    /// <summary> The constant length of an element </summary>
    public int? ElementLength { get; }
}

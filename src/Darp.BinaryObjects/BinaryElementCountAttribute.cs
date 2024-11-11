namespace Darp.BinaryObjects;

/// <summary> Supply a length to an array during binary reading or writing </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class BinaryElementCountAttribute : Attribute
{
    /// <summary> Use a member which provides the length </summary>
    /// <param name="memberWithLength"> The name of the member </param>
    public BinaryElementCountAttribute(string memberWithLength)
    {
        MemberWithLength = memberWithLength;
    }

    /// <summary> Use a constant length </summary>
    /// <param name="length"> The constant length </param>
    public BinaryElementCountAttribute(int length)
    {
        Length = length;
    }

    /// <summary> The name of the member to provide the length during reading </summary>
    public string? MemberWithLength { get; }

    /// <summary> The number of items in the array </summary>
    public int? Length { get; }
}

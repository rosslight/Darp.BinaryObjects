namespace Darp.BinaryObjects;

/// <summary> Sets the number of bytes to be used when reading or writing a primitive </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class BinaryLengthAttribute : Attribute
{
    /// <summary> Use a member which provides the length </summary>
    /// <param name="fieldWithLength"> The name of the member </param>
    public BinaryLengthAttribute(string fieldWithLength)
    {
        FieldWithLength = fieldWithLength;
    }

    /// <summary> Use a constant length </summary>
    /// <param name="length"> The constant length </param>
    public BinaryLengthAttribute(int length)
    {
        Length = length;
    }

    /// <summary> The name of the member to provide the length </summary>
    public string? FieldWithLength { get; }

    /// <summary> The number of bytes in the member </summary>
    public int? Length { get; }
}

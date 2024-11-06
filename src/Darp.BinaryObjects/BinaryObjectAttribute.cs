namespace Darp.BinaryObjects;

/// <summary> Marks an object or struct as binary readable or writable </summary>
/// <param name="binaryOptions"> The options on what to generate </param>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public sealed class BinaryObjectAttribute(BinaryOptions binaryOptions = BinaryOptions.Span) : Attribute
{
    /// <summary> The options </summary>
    public BinaryOptions BinaryOptions { get; } = binaryOptions;
}

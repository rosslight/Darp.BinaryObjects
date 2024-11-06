namespace Darp.BinaryObjects;

/// <summary> Ignore the property or field this is set on during binary reading or writing </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class BinaryIgnoreAttribute : Attribute;

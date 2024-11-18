namespace Darp.BinaryObjects;

/// <summary> Use the constructor this is set on for initializing new binary fields </summary>
[AttributeUsage(AttributeTargets.Constructor)]
public sealed class BinaryConstructorAttribute : Attribute;

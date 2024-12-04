namespace Darp.BinaryObjects;

/// <summary>
/// The options to choose from
/// </summary>
[Flags]
public enum BinaryOptions
{
    /// <summary> Implement <see cref="IBinaryWritable"/> </summary>
    Write = 1 << 0,

    /// <summary> Implement <see cref="IBinaryReadable{TSelf}"/> </summary>
    Read = 1 << 1,

    /// <summary> Implement <see cref="IBinaryWritable"/> and <see cref="IBinaryReadable{TSelf}"/> </summary>
    All = Write | Read,
}

namespace Darp.BinaryObjects;

/// <summary>
/// The options to choose from
/// </summary>
[Flags]
public enum BinaryOptions
{
    /// <summary> Implement nothing </summary>
    None = 0,

    /// <summary> Implement <see cref="IWritable"/> </summary>
    Write = 0b00000001,

    /// <summary> Implement <see cref="ISpanReadable{TSelf}"/> </summary>
    ReadSpan = 0b00000010,

    /// <summary> Implement <see cref="IMemoryReadable{TSelf}"/> </summary>
    ReadMemory = 0b00000100,

    /// <summary> Implement <see cref="IWritable"/> and <see cref="ISpanReadable{TSelf}"/> </summary>
    Span = Write | ReadSpan,

    /// <summary> Implement <see cref="IWritable"/> and <see cref="IMemoryReadable{TSelf}"/> </summary>
    Memory = Write | ReadMemory,

    /// <summary> Implement everything </summary>
    All = Write | ReadSpan | ReadMemory,
}

namespace Darp.BinaryObjects;

using System.Diagnostics.Contracts;

/// <summary> Defines a mechanism for reading the object from a <see cref="ReadOnlySpan{T}"/> with a constant length </summary>
/// <typeparam name="TSelf"> The type that implements this interface </typeparam>
public interface IBinaryConstantReadable<TSelf> : IBinaryReadable<TSelf>
#if NET9_0_OR_GREATER
    where TSelf : IBinaryConstantReadable<TSelf>, allows ref struct
#else
    where TSelf : IBinaryConstantReadable<TSelf>
#endif
{
    /// <summary> Gets the constant number of bytes that are required for a read or write operation. </summary>
    /// <returns> The number of bytes required. </returns>
    [Pure]
    public static abstract int ByteCount { get; }
}

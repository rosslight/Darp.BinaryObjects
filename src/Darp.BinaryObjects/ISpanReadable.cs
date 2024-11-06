namespace Darp.BinaryObjects;

using System.Diagnostics.CodeAnalysis;

/// <summary> Defines a mechanism for reading the object from a <see cref="ReadOnlySpan{T}"/> </summary>
/// <typeparam name="TSelf"> The type that implements this interface </typeparam>
public interface ISpanReadable<TSelf>
    where TSelf : ISpanReadable<TSelf>
{
    /// <summary> Attempt to unpack the object from a span </summary>
    /// <param name="source"> The source to get the current object from </param>
    /// <param name="value"> The object that was unpacked </param>
    /// <returns> <see langword="true"/>, when the unpacking was successful, <see langword="false"/> otherwise </returns>
    public static abstract bool TryReadLittleEndian(ReadOnlySpan<byte> source, [NotNullWhen(true)] out TSelf? value);

    /// <summary> Attempt to unpack the object from a span </summary>
    /// <param name="source"> The source to get the current object from </param>
    /// <param name="value"> The object that was unpacked </param>
    /// <param name="bytesConsumed"> The number of bytes used for unpacking </param>
    /// <returns> <see langword="true"/>, when the unpacking was successful, <see langword="false"/> otherwise </returns>
    public static abstract bool TryReadLittleEndian(
        ReadOnlySpan<byte> source,
        [NotNullWhen(true)] out TSelf? value,
        out int bytesConsumed
    );

    /// <summary> Attempt to unpack the object from a span </summary>
    /// <param name="source"> The source to get the current object from </param>
    /// <param name="value"> The object that was unpacked </param>
    /// <returns> <see langword="true"/>, when the unpacking was successful, <see langword="false"/> otherwise </returns>
    public static abstract bool TryReadBigEndian(ReadOnlySpan<byte> source, [NotNullWhen(true)] out TSelf? value);

    /// <summary> Attempt to unpack the object from a span </summary>
    /// <param name="source"> The source to get the current object from </param>
    /// <param name="value"> The object that was unpacked </param>
    /// <param name="bytesConsumed"> The number of bytes used for unpacking </param>
    /// <returns> <see langword="true"/>, when the unpacking was successful, <see langword="false"/> otherwise </returns>
    public static abstract bool TryReadBigEndian(
        ReadOnlySpan<byte> source,
        [NotNullWhen(true)] out TSelf? value,
        out int bytesConsumed
    );
}

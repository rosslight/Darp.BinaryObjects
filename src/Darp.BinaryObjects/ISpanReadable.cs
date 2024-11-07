namespace Darp.BinaryObjects;

using System.Diagnostics.CodeAnalysis;

/// <summary> Defines a mechanism for reading the object from a <see cref="ReadOnlySpan{T}"/> </summary>
/// <typeparam name="TSelf"> The type that implements this interface </typeparam>
public interface ISpanReadable<TSelf>
    where TSelf : ISpanReadable<TSelf>
{
    /// <summary> Tries to read the object from a span, in little-endian format </summary>
    /// <param name="source"> The span from which the object should be read. </param>
    /// <param name="value"> On return, contains the value read from <paramref name="source" /> or <c>default</c> if a value could not be read. </param>
    /// <returns> <c>true</c> if the value was successfully read from <paramref name="source" />; otherwise, <c>false</c>. </returns>
    public static abstract bool TryReadLittleEndian(ReadOnlySpan<byte> source, [NotNullWhen(true)] out TSelf? value);

    /// <summary> Tries to read the object from a span, in little-endian format </summary>
    /// <param name="source"> The span from which the object should be read. </param>
    /// <param name="value"> On return, contains the value read from <paramref name="source" /> or <c>default</c> if a value could not be read. </param>
    /// <param name="bytesRead"> The number of bytes written from the <paramref name="source" />. </param>
    /// <returns> <c>true</c> if the value was successfully read from <paramref name="source" />; otherwise, <c>false</c>. </returns>
    public static abstract bool TryReadLittleEndian(
        ReadOnlySpan<byte> source,
        [NotNullWhen(true)] out TSelf? value,
        out int bytesRead
    );

    /// <summary> Tries to read the object from a span, in big-endian format </summary>
    /// <param name="source"> The span from which the object should be read. </param>
    /// <param name="value"> On return, contains the value read from <paramref name="source" /> or <c>default</c> if a value could not be read. </param>
    /// <returns> <c>true</c> if the value was successfully read from <paramref name="source" />; otherwise, <c>false</c>. </returns>
    public static abstract bool TryReadBigEndian(ReadOnlySpan<byte> source, [NotNullWhen(true)] out TSelf? value);

    /// <summary> Tries to read the object from a span, in big-endian format </summary>
    /// <param name="source"> The span from which the object should be read. </param>
    /// <param name="value"> On return, contains the value read from <paramref name="source" /> or <c>default</c> if a value could not be read. </param>
    /// <param name="bytesRead"> The number of bytes written from the <paramref name="source" />. </param>
    /// <returns> <c>true</c> if the value was successfully read from <paramref name="source" />; otherwise, <c>false</c>. </returns>
    public static abstract bool TryReadBigEndian(
        ReadOnlySpan<byte> source,
        [NotNullWhen(true)] out TSelf? value,
        out int bytesRead
    );
}

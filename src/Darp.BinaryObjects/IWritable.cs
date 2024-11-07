namespace Darp.BinaryObjects;

/// <summary> Defines a mechanism for writing the object to a <see cref="Span{T}"/> </summary>
public interface IWritable
{
    /// <summary> Gets the number of bytes that will be written as part of <see cref="TryWriteLittleEndian(System.Span{byte})" />. </summary>
    /// <returns> The number of bytes that will be written as part of <see cref="TryWriteBigEndian(System.Span{byte})" />. </returns>
    public int GetByteCount();

    /// <summary> Tries to write the current value, in little-endian format, to a given span. </summary>
    /// <param name="destination"> The span to which the current value should be written. </param>
    /// <returns> <c>true</c> if the value was successfully written to <paramref name="destination" />; otherwise, <c>false</c>. </returns>
    public bool TryWriteLittleEndian(Span<byte> destination);

    /// <summary> Tries to write the current value, in little-endian format, to a given span. </summary>
    /// <param name="destination"> The span to which the current value should be written. </param>
    /// <param name="bytesWritten"> The number of bytes written to <paramref name="destination" />. </param>
    /// <returns> <c>true</c> if the value was successfully written to <paramref name="destination" />; otherwise, <c>false</c>. </returns>
    public bool TryWriteLittleEndian(Span<byte> destination, out int bytesWritten);

    /// <summary> Tries to write the current value, in big-endian format, to a given span. </summary>
    /// <param name="destination"> The span to which the current value should be written. </param>
    /// <returns> <c>true</c> if the value was successfully written to <paramref name="destination" />; otherwise, <c>false</c>. </returns>
    public bool TryWriteBigEndian(Span<byte> destination);

    /// <summary> Tries to write the current value, in big-endian format, to a given span. </summary>
    /// <param name="destination"> The span to which the current value should be written. </param>
    /// <param name="bytesWritten"> The number of bytes written to <paramref name="destination" />. </param>
    /// <returns> <c>true</c> if the value was successfully written to <paramref name="destination" />; otherwise, <c>false</c>. </returns>
    public bool TryWriteBigEndian(Span<byte> destination, out int bytesWritten);
}

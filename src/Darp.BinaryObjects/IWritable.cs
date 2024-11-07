namespace Darp.BinaryObjects;

/// <summary> Defines a mechanism for writing the object to a <see cref="Span{T}"/> </summary>
public interface IWritable
{
    /// <summary> Get the size of the current object in bytes </summary>
    /// <returns> The number of bytes used in binary serialization </returns>
    public int GetWriteSize();

    /// <summary> Tries to write the current value, in little-endian format, to a given span. </summary>
    /// <param name="destination"> The span to which the current value should be written. </param>
    /// <returns> <c>true</c> if the value was successfully written to <paramref name="destination" />; otherwise, <c>false</c>. </returns>
    public bool TryWriteLittleEndian(Span<byte> destination);

    /// <summary> Tries to write the current value, in big-endian format, to a given span. </summary>
    /// <param name="destination"> The span to which the current value should be written. </param>
    /// <returns> <c>true</c> if the value was successfully written to <paramref name="destination" />; otherwise, <c>false</c>. </returns>
    public bool TryWriteBigEndian(Span<byte> destination);
}

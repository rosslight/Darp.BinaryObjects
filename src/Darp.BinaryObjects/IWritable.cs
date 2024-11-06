namespace Darp.BinaryObjects;

/// <summary> Defines a mechanism for writing the object to a <see cref="Span{T}"/> </summary>
public interface IWritable
{
    /// <summary> Get the size of the current object in bytes </summary>
    /// <returns> The number of bytes used in binary serialization </returns>
    public int GetWriteSize();

    /// <summary> Attempt to pack the current object to a span </summary>
    /// <param name="destination"> The buffer to hold the packed object </param>
    /// <returns> True, when the packing was successful </returns>
    public bool TryWriteLittleEndian(Span<byte> destination);

    /// <summary> Attempt to pack the current object to a span </summary>
    /// <param name="destination"> The buffer to hold the packed object </param>
    /// <returns> True, when the packing was successful </returns>
    public bool TryWriteBigEndian(Span<byte> destination);
}

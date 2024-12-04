namespace Darp.BinaryObjects;

/// <summary> Provides extension methods to binary objects marked with <see cref="IBinaryWritable"/> interfaces </summary>
public static class BinaryObjectExtensions
{
    /// <summary> Writes the object, in little-endian format, to an array. </summary>
    /// <param name="writable"> The binary object to be written </param>
    /// <typeparam name="T"> The type of the writable binary object </typeparam>
    /// <exception cref="ArgumentException"> Writing the object to the destination failed </exception>
    public static byte[] ToArrayLittleEndian<T>(this T writable)
        where T : IBinaryWritable
    {
        ArgumentNullException.ThrowIfNull(writable);
        var destination = new byte[writable.GetByteCount()];
        writable.WriteLittleEndian(destination);
        return destination;
    }

    /// <summary> Write the current value, in little-endian format, to a given span. </summary>
    /// <param name="writable"> The binary object to be written </param>
    /// <param name="destination"> The span to which the current value should be written. </param>
    /// <typeparam name="T"> The type of the writable binary object </typeparam>
    /// <exception cref="ArgumentException"> Writing the object to the destination failed </exception>
    public static void WriteLittleEndian<T>(this T writable, Span<byte> destination)
        where T : IBinaryWritable
    {
        ArgumentNullException.ThrowIfNull(writable);
        if (!writable.TryWriteLittleEndian(destination))
            throw new ArgumentException($"Could not write {typeof(T).Name} to destination");
    }

    /// <summary> Writes the object, in little-endian format, to an array. </summary>
    /// <param name="writable"> The binary object to be written </param>
    /// <typeparam name="T"> The type of the writable binary object </typeparam>
    /// <exception cref="ArgumentException"> Writing the object to the destination failed </exception>
    public static byte[] ToArrayBigEndian<T>(this T writable)
        where T : IBinaryWritable
    {
        ArgumentNullException.ThrowIfNull(writable);
        var destination = new byte[writable.GetByteCount()];
        writable.WriteBigEndian(destination);
        return destination;
    }

    /// <summary> Write the current value, in big-endian format, to a given span. </summary>
    /// <param name="writable"> The binary object to be written </param>
    /// <param name="destination"> The span to which the current value should be written. </param>
    /// <typeparam name="T"> The type of the writable binary object </typeparam>
    /// <exception cref="ArgumentException"> Writing the object to the destination failed </exception>
    public static void WriteBigEndian<T>(this T writable, Span<byte> destination)
        where T : IBinaryWritable
    {
        ArgumentNullException.ThrowIfNull(writable);
        if (!writable.TryWriteBigEndian(destination))
            throw new ArgumentException($"Could not write {typeof(T).Name} to destination", nameof(destination));
    }
}

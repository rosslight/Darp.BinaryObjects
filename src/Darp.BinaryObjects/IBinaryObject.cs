namespace Darp.BinaryObjects;

/// <summary> Defines a binary object which implements both read and write methods </summary>
/// <typeparam name="TSelf"> The type that implements this interface </typeparam>
public interface IBinaryObject<TSelf> : IBinaryWritable, IBinaryReadable<TSelf>
#if NET9_0_OR_GREATER
    where TSelf : IBinaryObject<TSelf>, allows ref struct;
#else
    where TSelf : IBinaryObject<TSelf>;
#endif

namespace Darp.BinaryObjects;

/// <summary> Defines a binary object which implements both read and write methods and has a constant length </summary>
/// <typeparam name="TSelf"> The type that implements this interface </typeparam>
public interface IBinaryConstantObject<TSelf> : IBinaryObject<TSelf>, IBinaryConstantReadable<TSelf>
#if NET9_0_OR_GREATER
    where TSelf : IBinaryConstantObject<TSelf>, allows ref struct;
#else
    where TSelf : IBinaryConstantObject<TSelf>;
#endif

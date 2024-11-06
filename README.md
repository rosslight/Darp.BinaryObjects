# Darp.BinaryObjects

The goal of this project is to be used in situations when:
- Serialization to a buffer of bytes
- Deserialization from a buffer already completely received

Usage: When you want to use MemoryMarshal.TryWrite / TryRead or BinaryPrimitives but your objects are too complex

# Usage
```csharp
[BinaryObject]
partial record struct YourStruct(ushort A, byte B);


var buffer = Convert.FromHexString("AABBCC");
var success = YourStruct.TryReadLittleEndian(buffer, out var value);

```

# Other projects
- Several binary serializers. e.g. [MemoryPack](https://github.com/Cysharp/MemoryPack), [BinaryPack]()https://github.com/Sergio0694/BinaryPack, ...
  Cool projects, but all having custom serialization formats
- Serialization libraries relying on reflection. e.g. [HyperSerializer](https://github.com/adam-dot-cohen/HyperSerializer)
- [StructPacker](https://github.com/RudolfKurkaMs/StructPacker) - not supporting allocation less packing/unpacking
- [BinarySerializer](https://github.com/jefffhaynes/BinarySerializer?tab=readme-ov-file) - same feature set, however, difficult to understand and no allocation less serializing


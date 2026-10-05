# Project goals

Darp.BinaryObjects exists to provide clean typed representations of binary protocol data and reduce repetitive reader and writer code across multiple internal projects.

Internal projects are the primary consumers. External developers are welcome to benefit from the published library.

The [README supported-properties checklist](../README.md#supported-properties) is an aspirational roadmap of capabilities the author would like to support as time allows. It makes no delivery or completeness commitments. It should describe observable capabilities with examples demonstrating their behavior. Requirements may evolve as additional projects use the library; success is not tied to migrating one particular consumer or completing a fixed release checklist.

## Scope and priorities

The library owns reading and writing externally defined binary layouts. Consumers own transport, message dispatch, protocol state, and semantic protocol validation. Generated semantic checks and validation attributes are outside the current scope; this does not remove consumers' need to validate protocols.

Priorities, in order: correctness, API simplicity, measured optimization, and readable generated code. Avoid unnecessary allocations, while allowing allocations needed to construct owned results. More complex performance optimizations need measurement to justify them.

Breaking API changes are possible during development, as described by the README notice. Changes to the serialized layout of an existing declaration require explicit compatibility attention.

## Layout and extensibility

Member declaration order defines binary order. Keep this convention as long as it serves consumers. If it causes issues, consider explicit layout attributes analogous to `FieldOffset` for explicit struct layouts.

Hand-written reader and writer implementations must remain possible and comfortable to use, including composition with generated objects. Preserve a simple API rather than expanding the generator to cover every unusual layout.

Member byte widths (`BinaryByteWidth`) are primarily intended to support unusual primitive widths, such as a three-byte unsigned integer, including arrays of those primitives. Padding is a possible future use case, rather than the original motivation or a current requirement.

One concrete consumer use case can justify a generator feature when it fits the existing model cleanly and preserves a simple API. Manual implementations remain the appropriate extension mechanism when generation would require substantial special-case behavior.

Insufficient buffers should return `false` from generated `TryRead` and `TryWrite` methods. Partial progress on failure is acceptable and should be documented; callers cannot assume a failed write left the destination untouched.

## Deferred decisions

Collection-count consistency is unresolved: no policy has been chosen for disagreements between a declared count and the actual collection. Partial properties are a possible future approach, not a selected design. Detailed designs for roadmap features will be settled when those features are pursued.

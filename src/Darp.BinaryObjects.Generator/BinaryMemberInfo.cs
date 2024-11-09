namespace Darp.BinaryObjects.Generator;

using Microsoft.CodeAnalysis;

internal class BinaryMemberInfo
{
    public required ISymbol Symbol { get; init; }
    public required int TypeByteLength { get; init; }
    public required ITypeSymbol TypeSymbol { get; init; }
}

internal class BinaryArrayMemberInfo : BinaryMemberInfo
{
    public required ArrayKind ArrayKind { get; init; }
    public required int? ArrayMinimumLength { get; init; }
    public required BinaryMemberInfo? ArrayLengthMember { get; init; }
    public required int? ArrayAbsoluteLength { get; init; }
    public required ITypeSymbol ArrayTypeSymbol { get; init; }
}

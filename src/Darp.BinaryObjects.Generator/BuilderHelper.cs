namespace Darp.BinaryObjects.Generator;

using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;

internal enum ArrayKind
{
    None,
    Array,
    MultiDimensionalArray,
    List,
    Enumerable,
}

internal static class BuilderHelper
{
    /// <summary>
    /// Checks whether the type symbol is a valid array type. Currently supported are 1 dim arrays only
    /// </summary>
    public static bool TryGetArrayType(
        this ITypeSymbol symbol,
        out ArrayKind arrayKind,
        [NotNullWhen(true)] out ITypeSymbol? underlyingTypeSymbol
    )
    {
        if (symbol is IArrayTypeSymbol { Rank: 1 } arrayTypeSymbol)
        {
            underlyingTypeSymbol = arrayTypeSymbol.ElementType;
            arrayKind = ArrayKind.Array;
            return true;
        }
        underlyingTypeSymbol = null;
        arrayKind = ArrayKind.None;
        return false;
    }

    public static bool IsValidLengthInteger(this ITypeSymbol symbol) =>
        symbol.ToDisplayString() switch
        {
            "byte" => true,
            "sbyte" => true,
            "ushort" => true,
            "short" => true,
            "int" => true,
            _ => false,
        };

    public static bool TryGetLength(this ITypeSymbol symbol, out int length)
    {
        int? primitiveLength = symbol.ToDisplayString() switch
        {
            "bool" => 1,
            "byte" => 1,
            "sbyte" => 1,
            "ushort" => 2,
            "short" => 2,
            "System.Half" => 2,
            "uint" => 4,
            "int" => 4,
            "float" => 4,
            "ulong" => 8,
            "long" => 8,
            "double" => 8,
            "System.UInt128" => 16,
            "System.Int128" => 16,
            "decimal" => 16,
            _ => null,
        };
        if (primitiveLength is not null)
        {
            length = primitiveLength.Value;
            return true;
        }
        length = default;
        return false;
    }
}

namespace Darp.BinaryObjects.Generator;

using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;

internal enum ArrayKind
{
    None,
    Memory,
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
        (ArrayKind Kind, ITypeSymbol? Symbol) x = symbol.OriginalDefinition.ToDisplayString() switch
        {
            "System.ReadOnlyMemory<T>" => (
                ArrayKind.Memory,
                (symbol as INamedTypeSymbol)?.TypeArguments.FirstOrDefault()
            ),
            "System.Collections.Generic.List<T>" => (
                ArrayKind.List,
                (symbol as INamedTypeSymbol)?.TypeArguments.FirstOrDefault()
            ),
            "System.Collections.Generic.IEnumerable<T>" => (
                ArrayKind.Enumerable,
                (symbol as INamedTypeSymbol)?.TypeArguments.FirstOrDefault()
            ),
            "System.Collections.Generic.IReadOnlyCollection<T>" => (
                ArrayKind.Enumerable,
                (symbol as INamedTypeSymbol)?.TypeArguments.FirstOrDefault()
            ),
            "System.Collections.Generic.ICollection<T>" => (
                ArrayKind.Enumerable,
                (symbol as INamedTypeSymbol)?.TypeArguments.FirstOrDefault()
            ),
            "System.Collections.Generic.IReadOnlyList<T>" => (
                ArrayKind.Enumerable,
                (symbol as INamedTypeSymbol)?.TypeArguments.FirstOrDefault()
            ),
            "System.Collections.Generic.IList<T>" => (
                ArrayKind.Enumerable,
                (symbol as INamedTypeSymbol)?.TypeArguments.FirstOrDefault()
            ),
            _ => (ArrayKind.None, null),
        };
        underlyingTypeSymbol = x.Symbol;
        arrayKind = x.Kind;
        return arrayKind is not ArrayKind.None && underlyingTypeSymbol is not null;
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

    public static int ComputeLength(this BinaryMemberInfo memberInfo)
    {
        if (memberInfo is not BinaryArrayMemberInfo arrayMemberInfo)
            return memberInfo.TypeByteLength;
        if (arrayMemberInfo.ArrayAbsoluteLength is not null)
            return memberInfo.TypeByteLength * arrayMemberInfo.ArrayAbsoluteLength.Value;
        var minLength = arrayMemberInfo.ArrayMinimumLength ?? 0;
        return memberInfo.TypeByteLength * minLength;
    }

    public static int ComputeLength(this IEnumerable<BinaryMemberInfo> members)
    {
        return members.Sum(memberInfo => memberInfo.ComputeLength());
    }

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

    public static string GetWriteArrayString(
        this BinaryArrayMemberInfo info,
        string methodName,
        int currentIndex,
        int maxLength,
        Func<string, string>? func = null
    )
    {
        var memberName = $"this.{info.Symbol.Name}";
        if (func is not null)
            memberName = func(memberName);
        return $"        BinaryHelpers.{methodName}(destination[{currentIndex}..], {memberName}, {maxLength});";
    }

    public static string GetReadArrayString(string variableName, string methodName, int currentIndex, int maxLength)
    {
        return $"        var {variableName} = BinaryHelpers.{methodName}(source[{currentIndex}..{currentIndex + maxLength}]);";
    }

    public static string GetWriteString(this BinaryMemberInfo info, string methodName, int currentIndex)
    {
        return $"        BinaryHelpers.{methodName}(destination[{currentIndex}..], this.{info.Symbol.Name});";
    }

    public static string GetReadString(string variableName, string methodName, int currentIndex)
    {
        return $"        var {variableName} = BinaryHelpers.{methodName}(source[{currentIndex}..]);";
    }
}

namespace Darp.BinaryObjects.Generator;

using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;

internal enum WellKnownCollectionKind
{
    None,
    Memory,
    Array,
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
        out WellKnownCollectionKind collectionKind,
        [NotNullWhen(true)] out ITypeSymbol? underlyingTypeSymbol
    )
    {
        if (symbol is IArrayTypeSymbol { Rank: 1 } arrayTypeSymbol)
        {
            underlyingTypeSymbol = arrayTypeSymbol.ElementType;
            collectionKind = WellKnownCollectionKind.Array;
            return true;
        }
        (WellKnownCollectionKind Kind, ITypeSymbol? Symbol) x = symbol.OriginalDefinition.ToDisplayString() switch
        {
            "System.ReadOnlyMemory<T>" => (
                WellKnownCollectionKind.Memory,
                (symbol as INamedTypeSymbol)?.TypeArguments.FirstOrDefault()
            ),
            "System.Collections.Generic.List<T>" => (
                WellKnownCollectionKind.List,
                (symbol as INamedTypeSymbol)?.TypeArguments.FirstOrDefault()
            ),
            "System.Collections.Generic.IEnumerable<T>" => (
                WellKnownCollectionKind.Enumerable,
                (symbol as INamedTypeSymbol)?.TypeArguments.FirstOrDefault()
            ),
            "System.Collections.Generic.IReadOnlyCollection<T>" => (
                WellKnownCollectionKind.Enumerable,
                (symbol as INamedTypeSymbol)?.TypeArguments.FirstOrDefault()
            ),
            "System.Collections.Generic.ICollection<T>" => (
                WellKnownCollectionKind.Enumerable,
                (symbol as INamedTypeSymbol)?.TypeArguments.FirstOrDefault()
            ),
            "System.Collections.Generic.IReadOnlyList<T>" => (
                WellKnownCollectionKind.Enumerable,
                (symbol as INamedTypeSymbol)?.TypeArguments.FirstOrDefault()
            ),
            "System.Collections.Generic.IList<T>" => (
                WellKnownCollectionKind.Enumerable,
                (symbol as INamedTypeSymbol)?.TypeArguments.FirstOrDefault()
            ),
            _ => (WellKnownCollectionKind.None, null),
        };
        underlyingTypeSymbol = x.Symbol;
        collectionKind = x.Kind;
        return collectionKind is not WellKnownCollectionKind.None && underlyingTypeSymbol is not null;
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

    public static IEnumerable<IGroup> GroupInfos(this IEnumerable<IMember> enumerable)
    {
        List<IConstantMember>? list = null;
        foreach (IMember member in enumerable)
        {
            if (member is IVariableMemberGroup groupInfo)
            {
                if (list is not null)
                {
                    yield return new ConstantBinaryMemberGroup(list.ToArray());
                    list = null;
                }
                yield return groupInfo;
                continue;
            }

            if (member is not IConstantMember constantMember)
            {
                throw new ArgumentOutOfRangeException(nameof(enumerable));
            }
            list ??= [];
            list.Add(constantMember);
        }
        if (list is not null)
        {
            yield return new ConstantBinaryMemberGroup(list.ToArray());
        }
    }

    public static IEnumerable<IMember> SelectMembers(this IEnumerable<IGroup> groups)
    {
        return groups.SelectMany<IGroup, IMember>(x =>
            x switch
            {
                ConstantBinaryMemberGroup c => c.Members,
                IMember m => [m],
                _ => [],
            }
        );
    }
}

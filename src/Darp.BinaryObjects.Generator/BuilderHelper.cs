namespace Darp.BinaryObjects.Generator;

using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;

internal enum WellKnownCollectionKind
{
    None,
    Span,
    Memory,
    Array,
    List,
    Collection,
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
            "System.Collections.Generic.IReadOnlyCollection<T>" => (
                WellKnownCollectionKind.Collection,
                (symbol as INamedTypeSymbol)?.TypeArguments.FirstOrDefault()
            ),
            "System.Collections.Generic.ICollection<T>" => (
                WellKnownCollectionKind.Collection,
                (symbol as INamedTypeSymbol)?.TypeArguments.FirstOrDefault()
            ),
            "System.Collections.Generic.IReadOnlyList<T>" => (
                WellKnownCollectionKind.Collection,
                (symbol as INamedTypeSymbol)?.TypeArguments.FirstOrDefault()
            ),
            "System.Collections.Generic.IList<T>" => (
                WellKnownCollectionKind.Collection,
                (symbol as INamedTypeSymbol)?.TypeArguments.FirstOrDefault()
            ),
            _ => (WellKnownCollectionKind.None, null),
        };
        underlyingTypeSymbol = x.Symbol;
        collectionKind = x.Kind;
        return collectionKind is not WellKnownCollectionKind.None && underlyingTypeSymbol is not null;
    }

    public static string GetCollectionCount(this IMember member) =>
        $"this.{member.MemberSymbol.Name}.{(member.CollectionKind is WellKnownCollectionKind.List or WellKnownCollectionKind.Collection ? "Count" : "Length")}";

    public static string GetWriteValue(this IMember member) =>
        member.CollectionKind switch
        {
            WellKnownCollectionKind.Memory => $"this.{member.MemberSymbol.Name}.Span",
            WellKnownCollectionKind.List =>
                $"global::System.Runtime.InteropServices.CollectionsMarshal.AsSpan(this.{member.MemberSymbol.Name})",
            _ => $"this.{member.MemberSymbol.Name}",
        };

    public static bool IsEnum(this WellKnownTypeKind typeKind) =>
        typeKind
            is WellKnownTypeKind.EnumByte
                or WellKnownTypeKind.EnumSByte
                or WellKnownTypeKind.EnumUShort
                or WellKnownTypeKind.EnumShort
                or WellKnownTypeKind.EnumUInt
                or WellKnownTypeKind.EnumInt
                or WellKnownTypeKind.EnumULong
                or WellKnownTypeKind.EnumLong;

    public static bool IsSigned(this WellKnownTypeKind typeKind) =>
        typeKind
            is WellKnownTypeKind.SByte
                or WellKnownTypeKind.Short
                or WellKnownTypeKind.Int
                or WellKnownTypeKind.Long
                or WellKnownTypeKind.EnumSByte
                or WellKnownTypeKind.EnumShort
                or WellKnownTypeKind.EnumInt
                or WellKnownTypeKind.EnumLong;

    /// <summary> Whether values of the type can be serialized with fewer bytes than the type has </summary>
    public static bool SupportsByteWidth(this WellKnownTypeKind typeKind) =>
        typeKind.IsEnum()
        || typeKind
            is WellKnownTypeKind.Byte
                or WellKnownTypeKind.SByte
                or WellKnownTypeKind.UShort
                or WellKnownTypeKind.Short
                or WellKnownTypeKind.UInt
                or WellKnownTypeKind.Int
                or WellKnownTypeKind.ULong
                or WellKnownTypeKind.Long;

    /// <summary> Whether a value is serialized with fewer bytes than its type has </summary>
    public static bool IsNarrow(this WellKnownTypeKind typeKind, int typeByteLength) =>
        typeKind.TryGetLength(out var naturalLength) && typeByteLength < naturalLength;

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

    public static int? GetLength(this WellKnownTypeKind typeKind) =>
        typeKind.TryGetLength(out var length) ? length : null;

    public static bool TryGetLength(this WellKnownTypeKind typeKind, out int length)
    {
        int? primitiveLength = typeKind switch
        {
            WellKnownTypeKind.Bool => 1,
            WellKnownTypeKind.Byte or WellKnownTypeKind.EnumByte => 1,
            WellKnownTypeKind.SByte or WellKnownTypeKind.EnumSByte => 1,
            WellKnownTypeKind.UShort or WellKnownTypeKind.EnumUShort => 2,
            WellKnownTypeKind.Short or WellKnownTypeKind.EnumShort => 2,
            WellKnownTypeKind.Half => 2,
            WellKnownTypeKind.UInt or WellKnownTypeKind.EnumUInt => 4,
            WellKnownTypeKind.Int or WellKnownTypeKind.EnumInt => 4,
            WellKnownTypeKind.Float => 4,
            WellKnownTypeKind.ULong or WellKnownTypeKind.EnumULong => 8,
            WellKnownTypeKind.Long or WellKnownTypeKind.EnumLong => 8,
            WellKnownTypeKind.Double => 8,
            WellKnownTypeKind.UInt128 => 16,
            WellKnownTypeKind.Int128 => 16,
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
            if (member is IGroup groupInfo)
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

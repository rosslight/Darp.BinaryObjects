namespace Darp.BinaryObjects.Generator;

using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Dunet;
using Microsoft.CodeAnalysis;
using static BinarySymbol;

internal static class Parser
{
    public static ParserResult Parse(INamedTypeSymbol typeSymbol, Compilation compilation)
    {
        List<DiagnosticData> diagnostics = [];
        List<BinarySymbol> members = [];

        ImmutableArray<ISymbol> typeMembers = typeSymbol.GetMembers();
        var fields = typeMembers
            .Where(x => x.Kind is SymbolKind.Field or SymbolKind.Property)
            .Where(x => !x.IsImplicitlyDeclared)
            .ToImmutableArray();
        foreach (ISymbol fieldSymbol in fields)
        {
            if (TryParseField(fieldSymbol, compilation, diagnostics, members, out BinarySymbol? parsedSymbol))
                members.Add(parsedSymbol);
        }
        return new ParserResult(typeSymbol, members.ToImmutableArray(), diagnostics.ToImmutableArray());
    }

    private static bool TryParseField(
        ISymbol symbol,
        Compilation compilation,
        List<DiagnosticData> diagnotics,
        IReadOnlyList<BinarySymbol> previousMembers,
        [NotNullWhen(true)] out BinarySymbol? binarySymbol
    )
    {
        binarySymbol = null;
        ITypeSymbol fieldType = symbol switch
        {
            IPropertySymbol s => s.Type,
            IFieldSymbol s => s.Type,
            _ => throw new ArgumentException("Invalid symbol type"),
        };

        FieldAttributeData attributeData = ParseFieldAttributes(symbol);
        if (attributeData.IsIgnore)
            return false;

        if (fieldType.IsValidArrayType(compilation, out ITypeSymbol? elementTypeSymbol))
        {
            if (!TryParseTypeData(elementTypeSymbol, out TypeAttributeData? arrayTypeData))
                return false;
            if (arrayTypeData.IsConstant)
            {
                return TryParseConstantArray(
                    symbol,
                    fieldType,
                    elementTypeSymbol,
                    arrayTypeData.ConstantLength,
                    attributeData,
                    diagnotics,
                    out binarySymbol
                );
            }
            return false;
        }
        if (TryParseTypeData(fieldType, out TypeAttributeData? typeData))
        {
            if (typeData.IsConstant)
            {
                if (
                    !DiagnosticGuard.IsAttributeDataInRangeOrAdd(
                        (attributeData.BinaryLength, 1, typeData.ConstantLength),
                        (attributeData.BinaryLengthAttribute, 0),
                        diagnotics
                    )
                )
                {
                    return false;
                }
                binarySymbol = new ConstantBinarySymbol(
                    symbol,
                    fieldType,
                    attributeData.BinaryLength ?? typeData.ConstantLength
                );
                return true;
            }
            if (typeData.IsBinaryObject)
            {
                binarySymbol = new UnknownBinaryObjectSymbol(symbol, fieldType);
                return true;
            }
        }

        diagnotics.Add(
            DiagnosticData.Create(
                DiagnosticDescriptors.InvalidFieldType,
                symbol.GetSourceLocation(),
                [fieldType.Name, symbol.Name]
            )
        );
        return false;
    }

    private static bool TryParseConstantArray(
        ISymbol symbol,
        ITypeSymbol arrayTypeSymbol,
        ITypeSymbol elementTypeSymbol,
        int elementLength,
        FieldAttributeData attributeData,
        List<DiagnosticData> diagnotics,
        [NotNullWhen(true)] out BinarySymbol? binarySymbol
    )
    {
        binarySymbol = null;

        int elementCount;
        if (attributeData.BinaryElementCount is not null)
        {
            if (
                !DiagnosticGuard.IsAttributeDataInRangeOrAdd(
                    (attributeData.BinaryLength, Min: 1, Max: elementLength),
                    (attributeData.BinaryElementCountAttribute, 1),
                    diagnotics
                )
            )
            {
                return false;
            }
            if (
                !DiagnosticGuard.IsAttributeDataAtLeastOrAdd(
                    (attributeData.BinaryElementCount, Min: 1),
                    (attributeData.BinaryElementCountAttribute, 0),
                    diagnotics
                )
            )
            {
                return false;
            }
            elementCount = attributeData.BinaryElementCount.Value;
            if (attributeData.BinaryLength is not null)
                elementLength = attributeData.BinaryLength.Value;
        }
        else if (attributeData.BinaryLength is not null)
        {
            if (attributeData.BinaryLength % elementLength != 0)
            {
                diagnotics.Add(
                    DiagnosticData.Create(
                        DiagnosticDescriptors.InvalidMultipleOfAttributeData,
                        attributeData.BinaryLengthAttribute?.GetLocationOfConstructorArgument(0),
                        ["BinaryLength", attributeData.BinaryLength, elementLength]
                    )
                );
                return false;
            }
            elementCount = attributeData.BinaryLength.Value / elementLength;
        }
        else
        {
            return false;
        }
        binarySymbol = new ConstantArrayBinarySymbol(
            symbol,
            arrayTypeSymbol,
            elementCount,
            elementTypeSymbol,
            elementLength
        );
        return true;
    }

    private static FieldAttributeData ParseFieldAttributes(ISymbol symbol)
    {
        AttributeData? _binaryLengthAttribute = null;
        int? binaryLength = null;
        AttributeData? _binaryElementCountAttribute = null;
        int? binaryElementCount = null;

        ImmutableArray<AttributeData> attributes = symbol.GetAttributes();
        foreach (AttributeData attributeData in attributes)
        {
            if (attributeData.AttributeClass is null)
                continue;
            switch (attributeData.AttributeClass.ToDisplayString())
            {
                case "Darp.BinaryObjects.BinaryIgnoreAttribute":
                    return new FieldAttributeData { IsIgnore = true };
                case "Darp.BinaryObjects.BinaryLengthAttribute":
                    _binaryLengthAttribute = attributeData;
                    foreach (KeyValuePair<string, TypedConstant> pair in attributeData.GetArguments())
                    {
                        // Do not overwrite an already set binary length
                        if (binaryLength is not null)
                            continue;
                        if (pair is { Key: "length", Value.Value: int lengthValue })
                            binaryLength = lengthValue;
                    }
                    break;
                case "Darp.BinaryObjects.BinaryElementCountAttribute":
                    _binaryElementCountAttribute = attributeData;
                    foreach (KeyValuePair<string, TypedConstant> pair in attributeData.GetArguments())
                    {
                        if (pair is { Key: "count", Value.Value: int countValue })
                            binaryElementCount = countValue;
                        else if (pair is { Key: "elementLength", Value.Value: int elementLength })
                            binaryLength = elementLength;
                    }
                    break;
            }
        }
        return new FieldAttributeData
        {
            IsIgnore = false,
            BinaryLengthAttribute = _binaryLengthAttribute,
            BinaryLength = binaryLength,
            BinaryElementCountAttribute = _binaryElementCountAttribute,
            BinaryElementCount = binaryElementCount,
        };
    }

    private static bool IsValidArrayType(
        this ITypeSymbol fieldTypeSymbol,
        Compilation compilation,
        [NotNullWhen(true)] out ITypeSymbol? elementTypeSymbol
    )
    {
        if (fieldTypeSymbol is IArrayTypeSymbol { Rank: 1 } arrayTypeSymbol)
        {
            elementTypeSymbol = arrayTypeSymbol.ElementType;
            return true;
        }

        if (fieldTypeSymbol is not INamedTypeSymbol namedTypeSymbol)
        {
            elementTypeSymbol = null;
            return false;
        }
        switch (namedTypeSymbol.GetNamespace(), namedTypeSymbol.MetadataName)
        {
            case ("System", "String"):
                elementTypeSymbol = compilation.GetSpecialType(SpecialType.System_Char);
                return true;
            case ("System.Collections.Generic", "List`1"):
            case ("System", "ReadOnlyMemory`1"):
                elementTypeSymbol = namedTypeSymbol.TypeArguments[0];
                return true;
            default:
                elementTypeSymbol = null;
                return false;
        }
    }

    private static bool TryParseTypeData(
        ITypeSymbol fieldTypeSymbol,
        [NotNullWhen(true)] out TypeAttributeData? typeData
    )
    {
        var isEnum = false;
        // Handle enums
        if (
            fieldTypeSymbol.TypeKind == TypeKind.Enum
            && fieldTypeSymbol is INamedTypeSymbol { EnumUnderlyingType: not null } namedFieldTypeSymbol
        )
        {
            fieldTypeSymbol = namedFieldTypeSymbol.EnumUnderlyingType;
            isEnum = true;
        }

        // Handle supported primitives
        var primitiveLength = fieldTypeSymbol.SpecialType switch
        {
            SpecialType.System_Boolean or SpecialType.System_Byte or SpecialType.System_SByte => 1,
            SpecialType.System_Int16 or SpecialType.System_UInt16 or SpecialType.System_Char => 2,
            SpecialType.System_Int32 or SpecialType.System_UInt32 => 4,
            SpecialType.System_Int64 or SpecialType.System_UInt64 => 8,
            _ => -1,
        };
        if (primitiveLength >= 0)
        {
            typeData = new TypeAttributeData
            {
                IsConstant = true,
                IsEnum = isEnum,
                IsBinaryObject = false,
                ConstantLength = primitiveLength,
            };
            return true;
        }

        var implementsBinaryObject = fieldTypeSymbol.AllInterfaces.Any(x =>
            x.ToDisplayString() == $"Darp.BinaryObjects.IBinaryObject<{fieldTypeSymbol.Name}>"
        );
        ImmutableArray<AttributeData> attributes = fieldTypeSymbol.GetAttributes();
        var hasBinaryObjectAttribute = false;
        foreach (AttributeData attributeData in attributes)
        {
            if (attributeData.AttributeClass is null)
                continue;
            switch (attributeData.AttributeClass.ToDisplayString())
            {
                case "Darp.BinaryObjects.BinaryConstantAttribute":
                    if (!implementsBinaryObject)
                        continue;
                    foreach (KeyValuePair<string, TypedConstant> pair in attributeData.GetArguments())
                    {
                        if (pair is not { Key: "constantLength", Value.Value: int constantLength })
                            continue;
                        typeData = new TypeAttributeData
                        {
                            IsConstant = true,
                            IsEnum = false,
                            IsBinaryObject = true,
                            ConstantLength = constantLength,
                        };
                        return true;
                    }
                    break;
                case "Darp.BinaryObjects.BinaryObjectAttribute":
                    hasBinaryObjectAttribute = true;
                    break;
            }
        }
        if (hasBinaryObjectAttribute)
        {
            typeData = new TypeAttributeData
            {
                IsConstant = false,
                IsEnum = false,
                IsBinaryObject = true,
                ConstantLength = -1,
            };
            return true;
        }

        typeData = null;
        return false;
    }
}

internal static class DiagnosticGuard
{
    public static bool IsAttributeDataInRangeOrAdd(
        (int? Value, int Min, int Max) valueTuple,
        (AttributeData? Attribute, int ParameterIndex) attribute,
        List<DiagnosticData> diagnostics
    )
    {
        var (value, min, max) = valueTuple;
        if (value is null || (value >= min && value <= max))
            return true;
        (AttributeData? attributeData, var parameterIndex) = attribute;
        diagnostics.Add(
            DiagnosticData.Create(
                DiagnosticDescriptors.InvalidNumericRangeAttributeData,
                attributeData?.GetLocationOfConstructorArgument(parameterIndex),
                [attributeData?.AttributeClass?.Name.Replace("Attribute", string.Empty), value, min, max]
            )
        );
        return false;
    }

    public static bool IsAttributeDataAtLeastOrAdd(
        (int? Value, int Min) valueTuple,
        (AttributeData? Attribute, int ParameterIndex) attribute,
        List<DiagnosticData> diagnostics
    )
    {
        var (value, min) = valueTuple;
        if (value is null || value >= min)
            return true;
        (AttributeData? attributeData, var parameterIndex) = attribute;
        diagnostics.Add(
            DiagnosticData.Create(
                DiagnosticDescriptors.InvalidNumericMinimumAttributeData,
                attributeData?.GetLocationOfConstructorArgument(parameterIndex),
                [attributeData?.AttributeClass?.Name.Replace("Attribute", string.Empty), value, min]
            )
        );
        return false;
    }
}

public sealed record FieldAttributeData
{
    public bool IsIgnore { get; init; }
    public AttributeData? BinaryLengthAttribute { get; init; }
    public int? BinaryLength { get; init; }
    public string? BinaryLengthFieldName { get; init; }
    public AttributeData? BinaryElementCountAttribute { get; init; }
    public int? BinaryElementCount { get; init; }
    public string? BinaryElementCountFieldName { get; init; }
}

public sealed record ArrayTypeData { }

public sealed record TypeAttributeData
{
    public bool IsConstant { get; init; }
    public bool IsEnum { get; init; }
    public bool IsBinaryObject { get; init; }

    public int ConstantLength { get; init; }
}

internal sealed record ParserResult(
    ITypeSymbol TypeName,
    ImmutableArray<BinarySymbol> Symbols,
    ImmutableArray<DiagnosticData> Diagnostics
)
{
    public bool IsError => Diagnostics.Any(x => x.DefaultSeverity >= DiagnosticSeverity.Error);
}

[Union]
internal partial record BinarySymbol
{
    /// <summary> The symbol of the field </summary>
    public abstract ISymbol Symbol { get; init; }

    partial record ConstantBinarySymbol(ISymbol Symbol, ITypeSymbol FieldType, int FieldLength);

    partial record ConstantArrayBinarySymbol(
        ISymbol Symbol,
        ITypeSymbol ArrayType,
        int ElementCount,
        ITypeSymbol UnderlyingType,
        int ElementLength
    )
    {
        public int FieldLength => ElementCount * ElementLength;
    }

    partial record UnknownBinaryObjectSymbol(ISymbol Symbol, ITypeSymbol FieldType);
}

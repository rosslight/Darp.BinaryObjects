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
            switch (typeData)
            {
                case { IsConstant: true } when attributeData.BinaryLengthFieldName is null:
                {
                    if (
                        !DiagnosticGuard.IsAttributeDataInRangeOrAdd(
                            (attributeData.BinaryLength, 1, typeData.ConstantLength),
                            attributeData.BinaryLengthAttribute,
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
                case { IsConstant: true } when attributeData.BinaryLengthFieldName is not null:
                    binarySymbol = new VariableBinarySymbol(
                        symbol,
                        fieldType,
                        typeData.ConstantLength,
                        attributeData.BinaryLengthFieldName
                    );
                    return true;
                case { IsBinaryObject: true, IsGenerated: true }:
                    binarySymbol = new UnknownGeneratedBinaryObjectSymbol(symbol, fieldType);
                    return true;
                case { IsBinaryObject: true }:
                    binarySymbol = new UnknownBinaryObjectSymbol(symbol, fieldType);
                    return true;
            }
        }

        diagnotics.Add(
            DiagnosticData.Create(
                DiagnosticDescriptors.InvalidFieldType,
                symbol.GetSourceLocation(),
                fieldType.Name,
                symbol.Name
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
                    attributeData.BinaryElementLengthAttribute,
                    diagnotics
                )
            )
            {
                return false;
            }
            if (
                !DiagnosticGuard.IsAttributeDataAtLeastOrAdd(
                    (attributeData.BinaryElementCount, Min: 1),
                    attributeData.BinaryElementCountAttribute,
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
        else if (attributeData.BinaryElementCountFieldName is not null)
        {
            binarySymbol = new VariableCountArrayBinarySymbol(
                symbol,
                arrayTypeSymbol,
                attributeData.BinaryElementCountFieldName,
                elementTypeSymbol,
                attributeData.BinaryLength ?? elementLength
            );
            return true;
        }
        else if (attributeData.BinaryLengthFieldName is not null)
        {
            binarySymbol = new ConstantLengthArrayBinarySymbol(
                symbol,
                arrayTypeSymbol,
                elementTypeSymbol,
                attributeData.BinaryLength ?? elementLength,
                attributeData.BinaryLengthFieldName
            );
            return true;
        }
        else if (attributeData.BinaryLength is not null)
        {
            if (attributeData.BinaryElementLengthAttribute is not null)
            {
                binarySymbol = new VariableArrayBinarySymbol(
                    symbol,
                    arrayTypeSymbol,
                    elementTypeSymbol,
                    attributeData.BinaryLength.Value
                );
                return true;
            }
            if (attributeData.BinaryLength % elementLength != 0)
            {
                diagnotics.Add(
                    DiagnosticData.Create(
                        DiagnosticDescriptors.InvalidMultipleOfAttributeData,
                        attributeData.BinaryLengthAttribute?.GetLocationOfConstructorArgument(0),
                        "BinaryLength",
                        attributeData.BinaryLength,
                        elementLength
                    )
                );
                return false;
            }
            elementCount = attributeData.BinaryLength.Value / elementLength;
        }
        else
        {
            binarySymbol = new VariableArrayBinarySymbol(symbol, arrayTypeSymbol, elementTypeSymbol, elementLength);
            return true;
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
        AttributeData? binaryLengthAttribute = null;
        int? binaryLength = null;
        string? binaryLengthFieldName = null;
        AttributeData? binaryElementCountAttribute = null;
        int? binaryElementCount = null;
        string? binaryElementCountFieldName = null;
        AttributeData? binaryElementLengthAttribute = null;

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
                    binaryLengthAttribute = attributeData;
                    foreach (KeyValuePair<string, TypedConstant> pair in attributeData.GetArguments())
                    {
                        // Do not overwrite an already set binary length
                        if (binaryLength is not null)
                            continue;
                        if (pair is { Key: "length", Value.Value: int lengthValue })
                            binaryLength = lengthValue;
                        if (pair is { Key: "fieldWithLength", Value.Value: string fieldWithLength })
                            binaryLengthFieldName = fieldWithLength;
                    }
                    break;
                case "Darp.BinaryObjects.BinaryElementCountAttribute":
                    binaryElementCountAttribute = attributeData;
                    foreach (KeyValuePair<string, TypedConstant> pair in attributeData.GetArguments())
                    {
                        if (pair is { Key: "count", Value.Value: int countValue })
                            binaryElementCount = countValue;
                        else if (pair is { Key: "memberWithLength", Value.Value: string memberWithLength })
                            binaryElementCountFieldName = memberWithLength;
                    }
                    break;
                case "Darp.BinaryObjects.BinaryElementLengthAttribute":
                    binaryElementLengthAttribute = attributeData;
                    foreach (KeyValuePair<string, TypedConstant> pair in attributeData.GetArguments())
                    {
                        if (pair is { Key: "elementLength", Value.Value: int elementLength })
                            binaryLength = elementLength;
                    }
                    break;
            }
        }
        return new FieldAttributeData
        {
            IsIgnore = false,
            BinaryLengthAttribute = binaryLengthAttribute,
            BinaryLength = binaryLength,
            BinaryLengthFieldName = binaryLengthFieldName,
            BinaryElementCountAttribute = binaryElementCountAttribute,
            BinaryElementCount = binaryElementCount,
            BinaryElementCountFieldName = binaryElementCountFieldName,
            BinaryElementLengthAttribute = binaryElementLengthAttribute,
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
        var hasBinaryObjectConstraints =
            fieldTypeSymbol is ITypeParameterSymbol ts
            && ts.ConstraintTypes.Any(x =>
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
                IsGenerated = true,
                ConstantLength = -1,
            };
            return true;
        }

        if (implementsBinaryObject || hasBinaryObjectConstraints)
        {
            typeData = new TypeAttributeData
            {
                IsConstant = false,
                IsEnum = false,
                IsBinaryObject = true,
                IsGenerated = false,
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
        AttributeData? attribute,
        List<DiagnosticData> diagnostics
    )
    {
        var (value, min, max) = valueTuple;
        if (value is null || (value >= min && value <= max))
            return true;
        diagnostics.Add(
            DiagnosticData.Create(
                DiagnosticDescriptors.InvalidNumericRangeAttributeData,
                attribute?.GetLocationOfConstructorArgument(0),
                attribute?.AttributeClass?.Name.Replace("Attribute", string.Empty),
                value,
                min,
                max
            )
        );
        return false;
    }

    public static bool IsAttributeDataAtLeastOrAdd(
        (int? Value, int Min) valueTuple,
        AttributeData? attribute,
        List<DiagnosticData> diagnostics
    )
    {
        var (value, min) = valueTuple;
        if (value is null || value >= min)
            return true;
        diagnostics.Add(
            DiagnosticData.Create(
                DiagnosticDescriptors.InvalidNumericMinimumAttributeData,
                attribute?.GetLocationOfConstructorArgument(0),
                attribute?.AttributeClass?.Name.Replace("Attribute", string.Empty),
                value,
                min
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
    public AttributeData? BinaryElementLengthAttribute { get; init; }
    public int? BinaryElementCount { get; init; }
    public string? BinaryElementCountFieldName { get; init; }
}

public sealed record ArrayTypeData { }

public sealed record TypeAttributeData
{
    public bool IsConstant { get; init; }
    public bool IsEnum { get; init; }
    public bool IsBinaryObject { get; init; }
    public bool IsGenerated { get; init; }

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

    /// <summary> Tracks an object with [ FieldLength: constant ] </summary>
    /// <code> int A </code>
    /// <code>
    /// // An object extending IBinaryObject with BinaryConstant attribute
    /// ConstantBinaryObject A
    /// </code>
    partial record ConstantBinarySymbol(ISymbol Symbol, ITypeSymbol FieldType, int FieldLength);

    /// <summary> Tracks an object with [ FieldLength: constant/unknown ] </summary>
    /// <code>
    /// // An object extending IBinaryObject
    /// SomeBinaryObject A
    /// </code>
    partial record UnknownBinaryObjectSymbol(ISymbol Symbol, ITypeSymbol FieldType);

    /// <summary> Tracks an object with [ FieldLength: constant/unknown ] </summary>
    /// <code>
    /// // An object with BinaryObject attribute
    /// GeneratedBinaryObject A
    /// </code>
    partial record UnknownGeneratedBinaryObjectSymbol(ISymbol Symbol, ITypeSymbol FieldType);

    /// <summary> Tracks an object with [ MaxFieldLength: const FieldLength: variable ] </summary>
    /// <code> int Length, [property: BinaryLength("Length")] int A </code>
    partial record VariableBinarySymbol(
        ISymbol Symbol,
        ITypeSymbol FieldType,
        int MaxFieldLength,
        string FieldLengthName
    );

    /// <summary> Tracks an array with [ ElementCount: constant, ElementLength: constant, FieldLength: constant ] </summary>
    /// <code> [property: BinaryLength(8)] int[] A </code>
    /// <code> [property: BinaryElementCount(2)] int[] A </code>
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

    /// <summary> Tracks an array with [ ElementCount: undefined, ElementLength: constant, FieldLength: undefined ] </summary>
    /// <code> int[] A </code>
    partial record VariableArrayBinarySymbol(
        ISymbol Symbol,
        ITypeSymbol ArrayType,
        ITypeSymbol UnderlyingType,
        int ElementLength
    );

    /// <summary> Tracks an array with [ ElementCount: undefined, ElementLength: constant, FieldLength: variable ] </summary>
    /// <code> int Length, [property: BinaryLength("Length")] int[] A </code>
    partial record ConstantLengthArrayBinarySymbol(
        ISymbol Symbol,
        ITypeSymbol ArrayType,
        ITypeSymbol UnderlyingType,
        int ElementLength,
        string FieldLengthName
    );

    /// <summary> Tracks an array with [ ElementCount: variable, ElementLength: constant, FieldLength: undefined ] </summary>
    /// <code> int Length, [property: BinaryElementCount("Length")] int[] A </code>
    partial record VariableCountArrayBinarySymbol(
        ISymbol Symbol,
        ITypeSymbol ArrayType,
        string ElementCountName,
        ITypeSymbol UnderlyingType,
        int ElementLength
    );
}

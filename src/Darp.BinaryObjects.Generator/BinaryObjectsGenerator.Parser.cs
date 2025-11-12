namespace Darp.BinaryObjects.Generator;

using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

internal readonly record struct ParsedObjectInfo(
    ImmutableArray<DiagnosticData> Diagnostics,
    ImmutableArray<IGroup> MemberGroups,
    ImmutableArray<IMember> MembersInitializedByConstructor
)
{
    public static ParsedObjectInfo Fail(IEnumerable<DiagnosticData> diagnostics)
    {
        return new ParsedObjectInfo(
            diagnostics.ToImmutableArray(),
            ImmutableArray<IGroup>.Empty,
            ImmutableArray<IMember>.Empty
        );
    }
}

internal readonly record struct PreParsedMemberData(bool IsConstructorInitialized, ISymbol MemberSymbol);

internal readonly record struct PreParsedObjectInfo(
    ISymbol Symbol,
    ImmutableArray<DiagnosticData> Diagnostics,
    bool IsValid,
    ImmutableArray<PreParsedMemberData> Members
);

partial class BinaryObjectsGenerator
{
    private static bool CanEmitSource(TargetTypeInfo info)
    {
        if (info.LanguageVersion < LanguageVersion.CSharp10)
        {
            return false;
        }
        return true;
    }

    private static PreParsedObjectInfo ParseType(INamedTypeSymbol typeSymbol)
    {
        List<DiagnosticData> diagnostics = [];
        List<PreParsedMemberData> members = [];

        ImmutableArray<IParameterSymbol> constructorParameters =
            typeSymbol.Constructors.FirstOrDefault(x => !x.IsImplicitlyDeclared)?.Parameters
            ?? ImmutableArray<IParameterSymbol>.Empty;
        ImmutableArray<ISymbol> typeMembers = typeSymbol.GetMembers();
        var fieldOrProperties = typeMembers
            .Where(x => x.Kind is SymbolKind.Field or SymbolKind.Property)
            .ToImmutableArray();
        foreach (ISymbol fieldOrProperty in fieldOrProperties)
        {
            IParameterSymbol? constructorParameter = constructorParameters.FirstOrDefault(c =>
                IsNameEquivalent(fieldOrProperty.Name, c.Name)
            );
            ImmutableArray<AttributeData> attributes = fieldOrProperty.GetAttributes();

            // Should member be ignored?
            if (attributes.ContainsBinaryIgnore())
            {
                CheckConstructorParameterWhenMemberIsIgnored(diagnostics, fieldOrProperty, constructorParameter);
                continue;
            }

            // Check member if it can be initialized when reading?

            var hasDuplicate = members.Any(x =>
                x.MemberSymbol.Name.Equals(fieldOrProperty.Name, StringComparison.OrdinalIgnoreCase)
            );
            if (hasDuplicate)
            {
                var duplicateDiagnostic = DiagnosticData.Create(
                    DiagnosticDescriptors.MemberIgnoredDuplicateName,
                    fieldOrProperty.GetSourceLocation(),
                    fieldOrProperty.Name
                );
                diagnostics.Add(duplicateDiagnostic);
            }

            if (constructorParameter is not null)
            {
                // Check if types match
                if (!IsConstructorTypeValid(fieldOrProperty, constructorParameter, out DiagnosticData? diagnostic))
                {
                    diagnostics.Add(diagnostic.Value);
                    continue;
                }
                // TODO: Check for constructor parameters with equal name
                // Parameter {0} equals other members in the constructor (ignoring the case). This is not allowed for BinaryObjects

                members.Add(new PreParsedMemberData(true, fieldOrProperty));
                continue;
            }

            var isReadOnly = fieldOrProperty switch
            {
                IFieldSymbol fs => fs.IsReadOnly,
                IPropertySymbol ps => ps.IsReadOnly,
                _ => throw new ArgumentOutOfRangeException(nameof(fieldOrProperty)),
            };
            // If the property is initialized via constructor assume everything else fine
            var isAutoProperty = fieldOrProperty switch
            {
                IPropertySymbol ps => typeMembers
                    .Where(x => x.IsImplicitlyDeclared)
                    .OfType<IFieldSymbol>()
                    .Any(x => SymbolEqualityComparer.Default.Equals(x.AssociatedSymbol, ps)),
                _ => false,
            };
            // Ignore non auto properties without a warning
            if (!isAutoProperty)
                continue;
            if (isReadOnly)
            {
                // Ignore readonly properties with a warning
                var readonlyDiagnostic = DiagnosticData.Create(
                    DiagnosticDescriptors.MemberIgnoredReadonly,
                    fieldOrProperty.GetSourceLocation(),
                    fieldOrProperty.Name
                );
                diagnostics.Add(readonlyDiagnostic);
            }

            members.Add(new PreParsedMemberData(false, fieldOrProperty));
        }
        /*
if ((constructor?.Parameters.Length ?? 0) != membersInitializedByConstructor.Count)
        {
            ImmutableArray<IParameterSymbol> parameters =
                constructor?.Parameters ?? ImmutableArray<IParameterSymbol>.Empty;
            IEnumerable<DiagnosticData> parameterDiagnostics = parameters
                .Where(x =>
                    !membersInitializedByConstructor
                        .Select(m => m.MemberSymbol.Name)
                        .Contains(x.Name, StringComparer.OrdinalIgnoreCase)
                )
                .Select(nonDefinedParameter =>
                    DiagnosticData.Create(
                        DiagnosticDescriptors.MemberConstructorParameterUnknown,
                        nonDefinedParameter.GetSourceLocation(),
                        [nonDefinedParameter.Name]
                    )
                );
            diagnostics.AddRange(parameterDiagnostics);
            result = ParsedObjectInfo.Fail(diagnostics);
            return false;
        }
         */
        var isValid = diagnostics.All(x => x.Descriptor.DefaultSeverity <= DiagnosticSeverity.Warning);
        return new PreParsedObjectInfo(typeSymbol, diagnostics.ToImmutableArray(), isValid, members.ToImmutableArray());
    }

    private static bool IsConstructorTypeValid(
        ISymbol fieldOrProperty,
        IParameterSymbol constructorParameter,
        [NotNullWhen(false)] out DiagnosticData? diagnostic
    )
    {
        ITypeSymbol symbolType = fieldOrProperty switch
        {
            IFieldSymbol s => s.Type,
            IPropertySymbol s => s.Type,
            _ => throw new ArgumentOutOfRangeException(nameof(fieldOrProperty)),
        };
        var isLessNullableType =
            constructorParameter.Type.NullableAnnotation == NullableAnnotation.Annotated
            && constructorParameter.Type.Equals(symbolType, SymbolEqualityComparer.Default);
        var isIdenticalType = constructorParameter.Type.Equals(symbolType, SymbolEqualityComparer.IncludeNullability);
        if (!(isLessNullableType || isIdenticalType))
        {
            diagnostic = DiagnosticData.Create(
                DiagnosticDescriptors.MemberConstructorParameterTypeMismatch,
                constructorParameter.GetSourceLocation(),
                fieldOrProperty.Name,
                constructorParameter.Type.Name,
                symbolType.Name
            );
            return false;
        }

        diagnostic = null;
        return true;
    }

    private static void CheckConstructorParameterWhenMemberIsIgnored(
        List<DiagnosticData> diagnostics,
        ISymbol fieldOrProperty,
        IParameterSymbol? constructorParameter
    )
    {
        if (constructorParameter is null)
            return;
        // Parameter {0} is present in constructor but corresponding member {1} is marked with a BinaryIgnoreAttribute
        var duplicateDiagnostic = DiagnosticData.Create(
            DiagnosticDescriptors.ParameterWithIgnoredMember,
            constructorParameter.GetSourceLocation(),
            constructorParameter.Name,
            fieldOrProperty.Name
        );
        diagnostics.Add(duplicateDiagnostic);
    }

    private static ParsedObjectInfo GetConstantLength(PreParsedObjectInfo binaryObject, HashSet<ISymbol> workingSet)
    {
        List<DiagnosticData> diagnostics = [];

        List<IMember> previousMembers = [];
        foreach (PreParsedMemberData data in binaryObject.Members)
        {
            if (!TryGet(diagnostics, previousMembers, data.MemberSymbol, workingSet, out IMember? memberInfo))
                continue;
            previousMembers.Add(memberInfo);
        }
        return ParsedObjectInfo.Fail(diagnostics);
    }

    private static bool IsNameEquivalent(string memberName, string constructorName)
    {
        if (memberName.Equals(constructorName, StringComparison.OrdinalIgnoreCase))
            return true;
        if (memberName.Equals('_' + constructorName, StringComparison.OrdinalIgnoreCase))
            return true;
        return false;
    }

    private static bool TryGet(
        List<DiagnosticData> diagnostics,
        IReadOnlyList<IMember> previousMembers,
        ISymbol symbol,
        HashSet<ISymbol>? workingSet,
        [NotNullWhen(true)] out IMember? info
    )
    {
        info = null;
        ITypeSymbol? typeSymbol = symbol switch
        {
            IPropertySymbol s => s.Type,
            IFieldSymbol s => s.Type,
            _ => default,
        };
        if (typeSymbol is null)
            return false;
        ITypeSymbol arrayTypeSymbol = typeSymbol;
        if (
            typeSymbol.TryGetArrayType(
                out WellKnownCollectionKind collectionKind,
                out ITypeSymbol? underlyingTypeSymbol
            )
        )
            typeSymbol = underlyingTypeSymbol;
        WellKnownTypeKind typeKind = GetWellKnownTypeKind(typeSymbol);
        var length = 0;
        if (typeKind is not WellKnownTypeKind.BinaryObject && !typeKind.TryGetLength(out length))
        {
            var diagnostic = DiagnosticData.Create(
                descriptor: DiagnosticDescriptors.MemberTypeNotSupported,
                location: symbol.GetSourceLocation()
            );
            diagnostics.Add(diagnostic);
            return false;
        }
        int? arrayMinLength = null;
        IMember? arrayLengthMember = null;
        int? arrayLength = null;
        ImmutableArray<AttributeData> attributes = symbol.GetAttributes();
        foreach (AttributeData attributeData in attributes)
        {
            if (attributeData.AttributeClass is null)
                continue;
            switch (attributeData.AttributeClass.ToDisplayString())
            {
                case "Darp.BinaryObjects.BinaryIgnoreAttribute":
                    return false;
                case "Darp.BinaryObjects.BinaryElementCountAttribute":
                    foreach (KeyValuePair<string, TypedConstant> pair in attributeData.GetArguments())
                    {
                        if (pair is { Key: "memberWithLength", Value.Value: string memberName })
                        {
                            IMember? previousMember = previousMembers.FirstOrDefault(x =>
                                x.MemberSymbol.Name.Equals(memberName, StringComparison.Ordinal)
                            );
                            if (previousMember is null)
                            {
                                var diagnostic = DiagnosticData.Create(
                                    descriptor: DiagnosticDescriptors.MemberDefiningLengthNotFound,
                                    location: attributeData.GetLocationOfConstructorArgument(0)
                                        ?? symbol.GetSourceLocation(),
                                    memberName,
                                    symbol.Name
                                );
                                diagnostics.Add(diagnostic);
                                return false;
                            }
                            if (!previousMember.TypeSymbol.IsValidLengthInteger())
                            {
                                var diagnostic = DiagnosticData.Create(
                                    descriptor: DiagnosticDescriptors.MemberDefiningLengthDataInvalidType,
                                    location: previousMember.TypeSymbol.GetSourceLocation(),
                                    memberName,
                                    symbol.Name
                                );
                                diagnostics.Add(diagnostic);
                                return false;
                            }
                            arrayLengthMember = previousMember;
                        }
                        else if (pair is { Key: "length", Value.Value: int lengthValue })
                        {
                            arrayLength = lengthValue;
                        }
                    }
                    continue;
                case "Darp.BinaryObjects.BinaryMinElementCountAttribute":
                    foreach (KeyValuePair<string, TypedConstant> pair in attributeData.GetArguments())
                    {
                        if (pair is { Key: "minElements", Value.Value: int value })
                        {
                            arrayMinLength = value;
                        }
                    }
                    continue;
                case "Darp.BinaryObjects.BinaryByteLengthAttribute":
                    foreach (KeyValuePair<string, TypedConstant> pair in attributeData.GetArguments())
                    {
                        if (pair is { Key: "byteLength", Value.Value: int value })
                        {
                            length = value;
                        }
                    }
                    continue;
                default:
                    continue;
            }
        }

        if (typeKind is WellKnownTypeKind.BinaryObject)
        {
            var isConstant = IsConstant(typeSymbol, out var constantLength);
            if (isConstant)
                length = constantLength;
            info = (collectionKind, arrayLength, arrayLengthMember, isConstant) switch
            {
                (WellKnownCollectionKind.None, _, _, true) => new ConstantWellKnownMember
                {
                    TypeKind = WellKnownTypeKind.BinaryObject,
                    MemberSymbol = symbol,
                    TypeByteLength = length,
                    TypeSymbol = typeSymbol,
                },
                (WellKnownCollectionKind.None, _, _, false) => new BinaryObjectMemberGroup
                {
                    MemberSymbol = symbol,
                    TypeSymbol = typeSymbol,
                },
                (not WellKnownCollectionKind.None, not null, _, true) => info = new ConstantArrayMember
                {
                    TypeKind = WellKnownTypeKind.BinaryObject,
                    MemberSymbol = symbol,
                    TypeSymbol = typeSymbol,
                    CollectionKind = collectionKind,
                    TypeByteLength = length,
                    ArrayTypeSymbol = arrayTypeSymbol,
                    ArrayLength = arrayLength.Value,
                },
                (not WellKnownCollectionKind.None, _, not null, true) => new VariableArrayMemberGroup
                {
                    TypeKind = typeKind,
                    CollectionKind = collectionKind,
                    MemberSymbol = symbol,
                    TypeSymbol = typeSymbol,
                    TypeByteLength = length,
                    ArrayTypeSymbol = arrayTypeSymbol,
                    ArrayMinLength = arrayMinLength ?? 0,
                    ArrayLengthMemberName = arrayLengthMember.MemberSymbol.Name,
                },
                (not WellKnownCollectionKind.None, _, _, _) => new ReadRemainingArrayMemberGroup
                {
                    TypeKind = typeKind,
                    MemberSymbol = symbol,
                    TypeByteLength = length,
                    TypeSymbol = typeSymbol,
                    CollectionKind = collectionKind,
                    ArrayMinLength = arrayMinLength ?? 0,
                },
            };
            return true;
        }
        info = (collectionKind, arrayLength, arrayLengthMember) switch
        {
            (not WellKnownCollectionKind.None, _, not null) => new VariableArrayMemberGroup
            {
                TypeKind = typeKind,
                CollectionKind = collectionKind,
                MemberSymbol = symbol,
                TypeSymbol = typeSymbol,
                TypeByteLength = length,
                ArrayTypeSymbol = arrayTypeSymbol,
                ArrayMinLength = arrayMinLength ?? 0,
                ArrayLengthMemberName = arrayLengthMember.MemberSymbol.Name,
            },
            (not WellKnownCollectionKind.None, not null, _) => new ConstantArrayMember
            {
                TypeKind = typeKind,
                CollectionKind = collectionKind,
                MemberSymbol = symbol,
                TypeSymbol = typeSymbol,
                TypeByteLength = length,
                ArrayTypeSymbol = arrayTypeSymbol,
                ArrayLength = arrayLength.Value,
            },
            (not WellKnownCollectionKind.None, _, _) => new ReadRemainingArrayMemberGroup
            {
                TypeKind = typeKind,
                MemberSymbol = symbol,
                TypeByteLength = length,
                TypeSymbol = typeSymbol,
                CollectionKind = collectionKind,
                ArrayMinLength = arrayMinLength ?? 0,
            },
            (WellKnownCollectionKind.None, _, _) => new ConstantWellKnownMember
            {
                TypeKind = typeKind,
                MemberSymbol = symbol,
                TypeByteLength = length,
                TypeSymbol = typeSymbol,
            },
            //_ => throw new ArgumentException(
            //    $"Could not get info for {symbol.Name} {typeKind} {collectionKind} ({arrayLength}, {arrayMinLength}, {arrayLengthMember?.MemberSymbol.Name})"
            //),
        };
        return true;
    }

    private static bool IsConstant(ITypeSymbol typeSymbol, out int totalLength)
    {
        AttributeData? binaryConstantObjectAttribute = typeSymbol
            .GetAttributes()
            .FirstOrDefault(x => x.AttributeClass?.ToDisplayString() == "Darp.BinaryObjects.BinaryConstantAttribute");
        if (binaryConstantObjectAttribute?.ConstructorArguments[0].Value is int value)
        {
            totalLength = value;
            return true;
        }

        totalLength = 0;
        foreach (
            ISymbol symbol in typeSymbol
                .GetMembers()
                .Where(x => x.Kind is SymbolKind.Field or SymbolKind.Property)
                .Where(x => !x.IsImplicitlyDeclared)
        )
        {
            (bool HasAttribute, int? Length) hasElementCountTuple = symbol
                .GetAttributes()
                .Select(a =>
                {
                    if (a.AttributeClass?.ToDisplayString() != "Darp.BinaryObjects.BinaryElementCountAttribute")
                        return (false, 1);
                    if (
                        !a.GetArguments()
                            .ToDictionary(x => x.Key, x => x.Value)
                            .TryGetValue("length", out TypedConstant lengthValue)
                    )
                        return (false, 1);
                    return (true, (int?)lengthValue.Value);
                })
                .FirstOrDefault(x => x.Item1);
            var arrayLength = hasElementCountTuple.Length ?? 1;
            ITypeSymbol memberTypeSymbol = symbol switch
            {
                IFieldSymbol s => s.Type,
                IPropertySymbol s => s.Type,
                _ => throw new ArgumentException($"Invalid symbol type {symbol.ToDisplayString()}"),
            };
            if (memberTypeSymbol.TryGetArrayType(out _, out ITypeSymbol? arrayTypeSymbol))
            {
                if (!hasElementCountTuple.HasAttribute)
                    return false;
                memberTypeSymbol = arrayTypeSymbol;
            }
            WellKnownTypeKind typeKind = GetWellKnownTypeKind(memberTypeSymbol);
            if (typeKind is WellKnownTypeKind.BinaryObject)
                return false;
            if (!typeKind.TryGetLength(out var length))
                return false;
            totalLength += length * arrayLength;
        }
        return true;
    }
}

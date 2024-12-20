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

    private static bool TryParseType(INamedTypeSymbol typeSymbol, out ParsedObjectInfo result)
    {
        List<IMember> membersInitializedByConstructor = [];

        List<DiagnosticData> diagnostics = [];
        List<IMember> members = [];

        IMethodSymbol? constructor = typeSymbol.Constructors.FirstOrDefault(x => !x.IsImplicitlyDeclared);
        var fieldsOrProperties = typeSymbol
            .GetMembers()
            .Where(x => x.Kind is SymbolKind.Field or SymbolKind.Property)
            .ToImmutableArray();
        foreach (ISymbol memberSymbol in fieldsOrProperties.Where(x => !x.IsImplicitlyDeclared))
        {
            (bool IsValid, bool IsConstructorInitialized) validity = IsValidMember(
                memberSymbol,
                constructor,
                fieldsOrProperties,
                members,
                diagnostics
            );
            if (!validity.IsValid)
                continue;
            if (!TryGet(diagnostics, members, memberSymbol, out IMember? memberInfo))
                continue;
            members.Add(memberInfo);
            if (validity.IsConstructorInitialized)
                membersInitializedByConstructor.Add(memberInfo);
        }

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
        var groupedMembers = members.GroupInfos().ToImmutableArray();
        if (diagnostics.Any(x => x.Descriptor.DefaultSeverity > DiagnosticSeverity.Warning))
        {
            result = ParsedObjectInfo.Fail(diagnostics);
            return false;
        }
        result = new ParsedObjectInfo(
            diagnostics.ToImmutableArray(),
            groupedMembers,
            membersInitializedByConstructor.ToImmutableArray()
        );
        return true;
    }

    /// <summary> Checks a property or field symbol and returns whether it is a valid member which can be written to when constructing the object </summary>
    private static (bool IsValid, bool IsConstructorInitialized) IsValidMember(
        ISymbol propertyOrFieldSymbol,
        IMethodSymbol? constructor,
        ImmutableArray<ISymbol> typeMembers,
        List<IMember> previousMembers,
        List<DiagnosticData> diagnostics
    )
    {
        var shouldBeIgnored = propertyOrFieldSymbol
            .GetAttributes()
            .Any(x =>
                x
                    .AttributeClass?.ToDisplayString()
                    .Equals("Darp.BinaryObjects.BinaryIgnoreAttribute", StringComparison.Ordinal)
                    is true
            );
        if (shouldBeIgnored)
            return (false, default);
        switch (propertyOrFieldSymbol)
        {
            case IPropertySymbol propertySymbol:
            {
                var isAutoProperty = typeMembers
                    .Where(x => x.IsImplicitlyDeclared)
                    .OfType<IFieldSymbol>()
                    .Any(x => SymbolEqualityComparer.Default.Equals(x.AssociatedSymbol, propertySymbol));
                (bool IsValid, bool IsConstructorInitialized) constructorInit = IsConstructorInitialized(
                    propertySymbol,
                    propertySymbol.Type
                );
                // If finding out about constructors failed, return
                if (!constructorInit.IsValid)
                    return (false, default);
                // If the property is initialized via constructor assume everything else fine
                if (constructorInit.IsConstructorInitialized)
                    return (true, true);
                // Ignore non auto properties without a warning
                if (!isAutoProperty)
                    return (false, default);
                if (!propertySymbol.IsReadOnly)
                    return (true, false);
                // Ignore readonly properties with a warning
                var diagnostic = DiagnosticData.Create(
                    DiagnosticDescriptors.MemberIgnoredReadonly,
                    propertySymbol.GetSourceLocation(),
                    [propertySymbol.Name]
                );
                diagnostics.Add(diagnostic);
                return (false, default);
            }
            case IFieldSymbol fieldSymbol:
            {
                (bool IsValid, bool IsConstructorInitialized) constructorInit = IsConstructorInitialized(
                    fieldSymbol,
                    fieldSymbol.Type
                );
                // If finding out about constructors failed, return
                if (!constructorInit.IsValid)
                    return (false, default);
                // If the field is initialized via constructor assume everything else fine
                if (constructorInit.IsConstructorInitialized)
                    return (true, true);
                if (!fieldSymbol.IsReadOnly)
                    return (true, false);
                // Ignore readonly properties with a warning
                var diagnostic = DiagnosticData.Create(
                    DiagnosticDescriptors.MemberIgnoredReadonly,
                    fieldSymbol.GetSourceLocation(),
                    [fieldSymbol.Name]
                );
                diagnostics.Add(diagnostic);
                return (false, default);
            }
            default:
                return (false, default);
        }

        (bool IsValid, bool IsConstructorInitialized) IsConstructorInitialized(ISymbol symbol, ITypeSymbol symbolType)
        {
            IParameterSymbol? constructorParameter = constructor?.Parameters.FirstOrDefault(c =>
                IsNameEquivalent(symbol.Name, c.Name)
            );
            if (constructorParameter is not null)
            {
                var isIdenticalType = constructorParameter.Type.Equals(
                    symbolType,
                    SymbolEqualityComparer.IncludeNullability
                );
                var isLessNullableType =
                    constructorParameter.Type.NullableAnnotation == NullableAnnotation.Annotated
                    && constructorParameter.Type.Equals(symbolType, SymbolEqualityComparer.Default);
                if (isIdenticalType || isLessNullableType)
                {
                    var hasDuplicate = previousMembers.Any(x =>
                        x.MemberSymbol.Name.Equals(symbol.Name, StringComparison.OrdinalIgnoreCase)
                    );
                    if (!hasDuplicate)
                        return (true, true);
                    // Warning duplicate named readonly members
                    var duplicateDiagnostic = DiagnosticData.Create(
                        DiagnosticDescriptors.MemberIgnoredDuplicateName,
                        symbol.GetSourceLocation(),
                        [symbol.Name]
                    );
                    diagnostics.Add(duplicateDiagnostic);
                    return (false, default);
                }

                // Throw error for invalid constructor parameters
                var typeMismatchDiagnostic = DiagnosticData.Create(
                    DiagnosticDescriptors.MemberConstructorParameterTypeMismatch,
                    constructorParameter.GetSourceLocation(),
                    [symbol.Name, constructorParameter.Type.Name, symbolType.Name]
                );
                diagnostics.Add(typeMismatchDiagnostic);
                return (false, default);
            }
            return (true, default);
        }
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
                                    messageArgs: [memberName, symbol.Name]
                                );
                                diagnostics.Add(diagnostic);
                                return false;
                            }
                            if (!previousMember.TypeSymbol.IsValidLengthInteger())
                            {
                                var diagnostic = DiagnosticData.Create(
                                    descriptor: DiagnosticDescriptors.MemberDefiningLengthDataInvalidType,
                                    location: previousMember.TypeSymbol.GetSourceLocation(),
                                    messageArgs: [memberName, symbol.Name]
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
                case "Darp.BinaryObjects.UnlimitedWithMinLengthAttribute":
                    foreach (KeyValuePair<string, TypedConstant> pair in attributeData.GetArguments())
                    {
                        if (pair is { Key: "minLength", Value.Value: int value })
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
            if (memberTypeSymbol.TryGetArrayType(out WellKnownCollectionKind _, out ITypeSymbol? arrayTypeSymbol))
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

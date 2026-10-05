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

    private static bool TryParseType(
        INamedTypeSymbol typeSymbol,
        bool generateRead,
        out ParsedObjectInfo result,
        Location? warningLocation = null
    )
    {
        List<IMember> membersInitializedByConstructor = [];

        List<DiagnosticData> diagnostics = [];
        List<IMember> members = [];

        if (!TrySelectConstructor(typeSymbol, diagnostics, out IMethodSymbol? constructor))
        {
            result = ParsedObjectInfo.Fail(diagnostics);
            return false;
        }
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
                diagnostics,
                generateRead
            );
            if (!validity.IsValid)
                continue;
            if (!TryGet(diagnostics, members, memberSymbol, out IMember? memberInfo))
                continue;
            members.Add(memberInfo);
            if (generateRead && validity.IsConstructorInitialized)
                membersInitializedByConstructor.Add(memberInfo);
        }

        if (
            typeSymbol.TypeKind is TypeKind.Class
            && typeSymbol.BaseType is { SpecialType: not SpecialType.System_Object }
        )
        {
            diagnostics.Add(
                DiagnosticData.Create(
                    DiagnosticDescriptors.BaseClassNotSerialized,
                    warningLocation ?? typeSymbol.GetSourceLocation(),
                    [typeSymbol.Name]
                )
            );
        }

        ImmutableArray<IParameterSymbol> parameters = constructor?.Parameters ?? ImmutableArray<IParameterSymbol>.Empty;
        if (generateRead && parameters.Length != membersInitializedByConstructor.Count)
        {
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
        for (var index = 0; index + 1 < members.Count; index++)
        {
            IMember member = members[index];
            if (member is ReadRemainingArrayMemberGroup)
            {
                diagnostics.Add(
                    DiagnosticData.Create(
                        DiagnosticDescriptors.RemainingCollectionMustBeLast,
                        member.MemberSymbol.GetSourceLocation(),
                        [member.MemberSymbol.Name]
                    )
                );
            }
        }
        if (members.Sum(member => (long)member.ConstantByteLength) > int.MaxValue)
        {
            diagnostics.Add(
                DiagnosticData.Create(DiagnosticDescriptors.ObjectLengthTooLarge, typeSymbol.GetSourceLocation())
            );
        }
        if (diagnostics.Any(x => x.Descriptor.DefaultSeverity > DiagnosticSeverity.Warning))
        {
            result = ParsedObjectInfo.Fail(diagnostics);
            return false;
        }
        var groupedMembers = members.GroupInfos().ToImmutableArray();
        result = new ParsedObjectInfo(
            diagnostics.ToImmutableArray(),
            groupedMembers,
            membersInitializedByConstructor
                .OrderBy(member =>
                    parameters.First(parameter => IsNameEquivalent(member.MemberSymbol.Name, parameter.Name)).Ordinal
                )
                .ToImmutableArray()
        );
        return true;
    }

    private static bool TrySelectConstructor(
        INamedTypeSymbol typeSymbol,
        List<DiagnosticData> diagnostics,
        out IMethodSymbol? constructor
    )
    {
        var constructors = typeSymbol.InstanceConstructors.Where(x => !x.IsImplicitlyDeclared).ToImmutableArray();
        var markedConstructors = constructors
            .Where(x =>
                x.GetAttributes()
                    .Any(attribute =>
                        attribute.AttributeClass?.ToDisplayString() == "Darp.BinaryObjects.BinaryConstructorAttribute"
                    )
            )
            .ToImmutableArray();

        constructor = markedConstructors.Length == 1 ? markedConstructors[0] : constructors.FirstOrDefault();
        if (constructors.Length <= 1 || markedConstructors.Length == 1)
            return true;

        diagnostics.Add(
            DiagnosticData.Create(
                DiagnosticDescriptors.ConstructorSelectionAmbiguous,
                typeSymbol.GetSourceLocation(),
                [typeSymbol.Name]
            )
        );
        return false;
    }

    /// <summary> Checks a property or field symbol and returns whether it is a valid member which can be written to when constructing the object </summary>
    private static (bool IsValid, bool IsConstructorInitialized) IsValidMember(
        ISymbol propertyOrFieldSymbol,
        IMethodSymbol? constructor,
        ImmutableArray<ISymbol> typeMembers,
        List<IMember> previousMembers,
        List<DiagnosticData> diagnostics,
        bool generateRead
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
        if (!generateRead && propertyOrFieldSymbol.IsStatic)
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
                if (!propertySymbol.IsReadOnly || !generateRead)
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
                if (!fieldSymbol.IsReadOnly || !generateRead)
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
                if (!generateRead)
                    return (true, isIdenticalType || isLessNullableType);
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
        if (typeSymbol.OriginalDefinition.SpecialType is SpecialType.System_Collections_Generic_IEnumerable_T)
        {
            diagnostics.Add(
                DiagnosticData.Create(
                    DiagnosticDescriptors.EnumerableMemberNotSupported,
                    symbol.GetSourceLocation(),
                    [symbol.Name]
                )
            );
            return false;
        }
        if (
            typeSymbol.TryGetArrayType(
                out WellKnownCollectionKind collectionKind,
                out ITypeSymbol? underlyingTypeSymbol
            )
        )
            typeSymbol = underlyingTypeSymbol;
        WellKnownTypeKind typeKind = GetWellKnownTypeKind(typeSymbol);
        if (!TryGetByteCount(diagnostics, symbol, typeSymbol, typeKind, collectionKind, out var byteCount))
            return false;
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
        length = byteCount ?? length;
        int? arrayMinLength = null;
        IMember? arrayLengthMember = null;
        IMember? arrayByteCountMember = null;
        AttributeData? arrayByteCountAttribute = null;
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
                            if (!TryGetLengthMember(attributeData, memberName, out arrayLengthMember))
                                return false;
                        }
                        else if (pair is { Key: "length", Value.Value: int lengthValue })
                        {
                            arrayLength = lengthValue;
                        }
                    }
                    continue;
                case "Darp.BinaryObjects.BinaryByteCountAttribute":
                    // A constant narrows a single value and is resolved together with the element byte count
                    if (attributeData.ConstructorArguments is not [{ Value: string byteCountMemberName }])
                        continue;
                    if (collectionKind is WellKnownCollectionKind.None)
                    {
                        diagnostics.Add(
                            DiagnosticData.Create(
                                DiagnosticDescriptors.ByteCountMemberOnScalar,
                                attributeData.GetLocationOfConstructorArgument(0) ?? symbol.GetSourceLocation(),
                                [symbol.Name]
                            )
                        );
                        return false;
                    }
                    if (!TryGetLengthMember(attributeData, byteCountMemberName, out arrayByteCountMember))
                        return false;
                    arrayByteCountAttribute = attributeData;
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
                default:
                    continue;
            }
        }

        if (arrayByteCountAttribute is not null && (arrayLengthMember is not null || arrayLength is not null))
        {
            diagnostics.Add(
                DiagnosticData.Create(
                    DiagnosticDescriptors.ByteCountWithElementCount,
                    arrayByteCountAttribute.GetLocationOfConstructorArgument(0) ?? symbol.GetSourceLocation(),
                    [symbol.Name]
                )
            );
            return false;
        }
        // The collection ends after a number of elements or a number of bytes
        var lengthIsInBytes = arrayByteCountMember is not null;
        arrayLengthMember ??= arrayByteCountMember;

        var isConstant = false;
        if (typeKind is WellKnownTypeKind.BinaryObject)
        {
            isConstant = IsConstant(typeSymbol, out var constantLength);
            if (collectionKind is not WellKnownCollectionKind.None && (!isConstant || constantLength <= 0))
            {
                diagnostics.Add(
                    DiagnosticData.Create(
                        DiagnosticDescriptors.CollectionElementLengthUnknown,
                        symbol.GetSourceLocation(),
                        [symbol.Name]
                    )
                );
                return false;
            }
            if (isConstant)
                length = constantLength;
        }
        if (
            collectionKind is not WellKnownCollectionKind.None
            && (
                arrayLength is < 0
                || arrayMinLength is < 0
                || (long)length * (arrayLength ?? 0) > int.MaxValue
                || (long)length * (arrayMinLength ?? 0) > int.MaxValue
            )
        )
        {
            diagnostics.Add(
                DiagnosticData.Create(
                    DiagnosticDescriptors.CollectionLengthInvalid,
                    symbol.GetSourceLocation(),
                    [symbol.Name]
                )
            );
            return false;
        }
        if (typeKind is WellKnownTypeKind.BinaryObject)
        {
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
                    // Ref-like serializers retain direct calls; generic helpers also serve C# 11 consumers.
                    UseInterfaceDispatch =
                        typeSymbol is not INamedTypeSymbol { IsRefLikeType: true }
                        && !typeSymbol
                            .GetAttributes()
                            .Any(attribute => attribute.AttributeClass?.ToDisplayString() == BinaryObjectAttributeName),
                },
                (not WellKnownCollectionKind.None, not null, _, true) => info = new ConstantArrayMember
                {
                    TypeKind = WellKnownTypeKind.BinaryObject,
                    MemberSymbol = symbol,
                    TypeSymbol = typeSymbol,
                    CollectionKind = collectionKind,
                    TypeByteLength = length,
                    ArrayLength = arrayLength.Value,
                },
                (not WellKnownCollectionKind.None, _, not null, true) => new VariableArrayMemberGroup
                {
                    TypeKind = typeKind,
                    CollectionKind = collectionKind,
                    MemberSymbol = symbol,
                    TypeSymbol = typeSymbol,
                    TypeByteLength = length,
                    ArrayMinLength = arrayMinLength ?? 0,
                    ArrayLengthMemberName = arrayLengthMember.MemberSymbol.Name,
                    LengthIsInBytes = lengthIsInBytes,
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
                ArrayMinLength = arrayMinLength ?? 0,
                ArrayLengthMemberName = arrayLengthMember.MemberSymbol.Name,
                LengthIsInBytes = lengthIsInBytes,
            },
            (not WellKnownCollectionKind.None, not null, _) => new ConstantArrayMember
            {
                TypeKind = typeKind,
                CollectionKind = collectionKind,
                MemberSymbol = symbol,
                TypeSymbol = typeSymbol,
                TypeByteLength = length,
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

        bool TryGetLengthMember(
            AttributeData attributeData,
            string memberName,
            [NotNullWhen(true)] out IMember? lengthMember
        )
        {
            lengthMember = previousMembers.FirstOrDefault(x =>
                x.MemberSymbol.Name.Equals(memberName, StringComparison.Ordinal)
            );
            if (lengthMember is null)
            {
                var diagnostic = DiagnosticData.Create(
                    descriptor: DiagnosticDescriptors.MemberDefiningLengthNotFound,
                    location: attributeData.GetLocationOfConstructorArgument(0) ?? symbol.GetSourceLocation(),
                    messageArgs: [memberName, symbol.Name]
                );
                diagnostics.Add(diagnostic);
                return false;
            }
            if (!lengthMember.TypeSymbol.IsValidLengthInteger())
            {
                var diagnostic = DiagnosticData.Create(
                    descriptor: DiagnosticDescriptors.MemberDefiningLengthDataInvalidType,
                    location: lengthMember.TypeSymbol.GetSourceLocation(),
                    messageArgs: [memberName, symbol.Name]
                );
                diagnostics.Add(diagnostic);
                return false;
            }
            return true;
        }
    }

    /// <summary> Gets the byte count of a value which is narrower than its type. Fails if the member cannot have the declared byte count </summary>
    private static bool TryGetByteCount(
        List<DiagnosticData> diagnostics,
        ISymbol symbol,
        ITypeSymbol typeSymbol,
        WellKnownTypeKind typeKind,
        WellKnownCollectionKind collectionKind,
        out int? byteCount
    )
    {
        byteCount = null;
        ImmutableArray<AttributeData> attributes = symbol.GetAttributes();
        // A member name bounds a collection instead of narrowing a value
        AttributeData? memberAttribute = attributes.FirstOrDefault(x =>
            x.AttributeClass?.ToDisplayString() == "Darp.BinaryObjects.BinaryByteCountAttribute"
            && x.ConstructorArguments is [{ Value: int }]
        );
        AttributeData? elementAttribute = attributes.FirstOrDefault(x =>
            x.AttributeClass?.ToDisplayString() == "Darp.BinaryObjects.BinaryElementByteCountAttribute"
        );
        // A single value takes its byte count from the member, a collection from its elements
        var isCollection = collectionKind is not WellKnownCollectionKind.None;
        AttributeData? attribute = isCollection ? elementAttribute : memberAttribute;
        AttributeData? misplacedAttribute = isCollection ? memberAttribute : elementAttribute;
        if ((attribute ?? misplacedAttribute) is not { } anyAttribute)
            return true;
        var attributeName = ReferenceEquals(anyAttribute, memberAttribute)
            ? "BinaryByteCount"
            : "BinaryElementByteCount";
        if (!typeKind.SupportsByteCount() || !typeKind.TryGetLength(out var naturalLength))
        {
            diagnostics.Add(
                DiagnosticData.Create(
                    DiagnosticDescriptors.ByteCountNotSupported,
                    anyAttribute.GetLocationOfConstructorArgument(0) ?? symbol.GetSourceLocation(),
                    [attributeName, symbol.Name, typeSymbol.ToDisplayString()]
                )
            );
            return false;
        }
        if (misplacedAttribute is not null)
        {
            diagnostics.Add(
                DiagnosticData.Create(
                    isCollection
                        ? DiagnosticDescriptors.ByteCountOnCollection
                        : DiagnosticDescriptors.ElementByteCountOnScalar,
                    misplacedAttribute.GetLocationOfConstructorArgument(0) ?? symbol.GetSourceLocation(),
                    [symbol.Name]
                )
            );
            return false;
        }
        if (attribute?.ConstructorArguments is not [{ Value: int declaredCount }])
            return true;
        Location location = attribute.GetLocationOfConstructorArgument(0) ?? symbol.GetSourceLocation();
        if (declaredCount < 1 || declaredCount > naturalLength)
        {
            diagnostics.Add(
                DiagnosticData.Create(
                    DiagnosticDescriptors.ByteCountInvalid,
                    location,
                    [attributeName, declaredCount, symbol.Name, typeSymbol.ToDisplayString(), naturalLength]
                )
            );
            return false;
        }
        if (declaredCount == naturalLength)
        {
            diagnostics.Add(
                DiagnosticData.Create(
                    DiagnosticDescriptors.ByteCountRedundant,
                    location,
                    [attributeName, declaredCount, symbol.Name, typeSymbol.ToDisplayString()]
                )
            );
            return true;
        }
        byteCount = declaredCount;
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

        if (
            typeSymbol is INamedTypeSymbol namedType
            && typeSymbol.GetAttributes().Any(x => x.AttributeClass?.ToDisplayString() == BinaryObjectAttributeName)
        )
            return TryGetGeneratedConstantLength(namedType, out totalLength);

        totalLength = 0;
        // Handwritten serializers own their layout; only explicit metadata establishes a fixed size.
        return false;
    }

    private static bool TryGetGeneratedConstantLength(INamedTypeSymbol namedType, out int totalLength)
    {
        totalLength = 0;
        AttributeData attribute = namedType
            .GetAttributes()
            .First(x => x.AttributeClass?.ToDisplayString() == BinaryObjectAttributeName);
        BinaryGenerationOptions options = GetGenerationOptions(attribute);
        if ((options & BinaryGenerationOptions.All) == 0)
            return false;
        var generateRead = (options & BinaryGenerationOptions.Read) != 0;
        // Keep nested object graphs variable-sized rather than recursively parsing them.
        if (!TrySelectConstructor(namedType, [], out IMethodSymbol? constructor))
            return false;
        var fieldsOrProperties = namedType
            .GetMembers()
            .Where(x => x.Kind is SymbolKind.Field or SymbolKind.Property)
            .ToImmutableArray();
        foreach (ISymbol member in fieldsOrProperties.Where(x => !x.IsImplicitlyDeclared))
        {
            ITypeSymbol? memberType = member switch
            {
                IFieldSymbol field => field.Type,
                IPropertySymbol property => property.Type,
                _ => null,
            };
            if (memberType is null)
                continue;
            if (memberType.TryGetArrayType(out _, out ITypeSymbol? elementType))
                memberType = elementType;
            if (
                IsBinaryObject(memberType)
                && IsValidMember(member, constructor, fieldsOrProperties, [], [], generateRead).IsValid
            )
                return false;
        }

        if (!TryParseType(namedType, generateRead, out ParsedObjectInfo parsedObject))
            return false;
        if (parsedObject.MemberGroups.SelectMembers().Any(member => member is not IConstantMember))
            return false;
        totalLength = parsedObject.MemberGroups.Sum(group => group.ConstantByteLength);
        return true;
    }
}

namespace Darp.BinaryObjects.Generator;

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

internal readonly record struct Aaa(
    ImmutableArray<DiagnosticData> Diagnostics,
    ImmutableArray<IGroup> MemberGroups,
    ImmutableHashSet<IMember> MembersInitializedByConstructor
)
{
    public static Aaa Fail(IEnumerable<DiagnosticData> diagnostics)
    {
        return new Aaa(diagnostics.ToImmutableArray(), ImmutableArray<IGroup>.Empty, ImmutableHashSet<IMember>.Empty);
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

    private static bool TryParseType(INamedTypeSymbol typeSymbol, out Aaa result)
    {
        HashSet<IMember> membersInitializedByConstructor = [];

        List<DiagnosticData> diagnostics = [];
        List<IMember> members = [];

        IMethodSymbol constructor = typeSymbol.Constructors.First(x => !x.IsImplicitlyDeclared);
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
            if (!BinaryObjectBuilder.TryGet(diagnostics, members, memberSymbol, out IMember? info))
                continue;
            members.Add(info);
            if (validity.IsConstructorInitialized)
                membersInitializedByConstructor.Add(info);
        }

        if (constructor.Parameters.Length != membersInitializedByConstructor.Count)
        {
            result = Aaa.Fail(diagnostics);
            return false;
        }
        var groupedMembers = members.GroupInfos().ToImmutableArray();
        if (diagnostics.Any(x => x.Descriptor.DefaultSeverity > DiagnosticSeverity.Warning))
        {
            result = Aaa.Fail(diagnostics);
            return false;
        }
        result = new Aaa(
            diagnostics.ToImmutableArray(),
            groupedMembers,
            membersInitializedByConstructor.ToImmutableHashSet()
        );
        return true;
    }

    /// <summary> Checks a property or field symbol and returns whether it is a valid member which can be written to when constructing the object </summary>
    private static (bool IsValid, bool IsConstructorInitialized) IsValidMember(
        ISymbol propertyOrFieldSymbol,
        IMethodSymbol constructor,
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
            IParameterSymbol? constructorParameter = constructor.Parameters.FirstOrDefault(c =>
                c.Name.Equals(symbol.Name, StringComparison.OrdinalIgnoreCase)
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
}

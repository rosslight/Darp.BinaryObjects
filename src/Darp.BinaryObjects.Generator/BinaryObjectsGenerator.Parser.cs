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
                diagnostics
            );
            if (!validity.IsValid)
                continue;
            if (!BinaryObjectBuilder.TryGet(diagnostics, members, memberSymbol, out IMember? info))
                continue;
            members.Add(info);
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
                // Ignore non auto properties without a warning
                if (!isAutoProperty)
                    return (false, default);
                if (!propertySymbol.IsReadOnly)
                    return (true, false);
                return IsValidReadonlyMember(propertySymbol, propertySymbol.Type);
            }
            case IFieldSymbol fieldSymbol:
            {
                if (!fieldSymbol.IsReadOnly)
                    return (true, false);
                return IsValidReadonlyMember(fieldSymbol, fieldSymbol.Type);
            }
            default:
                return (false, default);
        }

        (bool IsValid, bool IsConstructorInitialized) IsValidReadonlyMember(ISymbol symbol, ITypeSymbol symbolType)
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
                    return (true, true);

                // Throw error for invalid constructor parameters
                var typeMismatchDiagnostic = DiagnosticData.Create(
                    DiagnosticDescriptors.MemberConstructorParameterTypeMismatch,
                    constructorParameter.GetSourceLocation(),
                    [symbol.Name, constructorParameter.Type.Name, symbolType.Name]
                );
                diagnostics.Add(typeMismatchDiagnostic);
                return (false, default);
            }
            // Ignore readonly properties with a warning
            var diagnostic = DiagnosticData.Create(
                DiagnosticDescriptors.MemberIgnoredReadonly,
                symbol.GetSourceLocation(),
                [symbol.Name]
            );
            diagnostics.Add(diagnostic);
            return (false, default);
        }
    }
}

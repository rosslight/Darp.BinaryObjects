namespace Darp.BinaryObjects.Generator;

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

internal static class RoslynHelper
{
    public static Location GetSourceLocation(this ISymbol symbol) => symbol.Locations.FirstOrDefault() ?? Location.None;

    public static Location? GetLocationOfConstructorArgument(this AttributeData attributeData, int constructorIndex)
    {
        if (attributeData.ApplicationSyntaxReference?.GetSyntax() is not AttributeSyntax x || x.ArgumentList is null)
            return null;
        SeparatedSyntaxList<AttributeArgumentSyntax> arguments = x.ArgumentList.Arguments;
        return arguments.Count < constructorIndex ? null : arguments[constructorIndex].GetLocation();
    }

    public static IEnumerable<KeyValuePair<string, TypedConstant>> GetArguments(this AttributeData attributeData)
    {
        ImmutableArray<KeyValuePair<string, TypedConstant>> arguments = attributeData.NamedArguments;
        if (attributeData.AttributeConstructor is null)
            return arguments;
        IEnumerable<KeyValuePair<string, TypedConstant>> constructorArguments =
            attributeData.AttributeConstructor.Parameters.Zip(
                attributeData.ConstructorArguments,
                (symbol, constant) => new KeyValuePair<string, TypedConstant>(symbol.Name, constant)
            );
        return arguments.Concat(constructorArguments);
    }
}

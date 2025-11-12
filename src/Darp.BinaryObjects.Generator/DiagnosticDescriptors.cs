namespace Darp.BinaryObjects.Generator;

using Microsoft.CodeAnalysis;

internal static class DiagnosticDescriptors
{
    public static readonly DiagnosticDescriptor GeneralError = new(
        id: "DBO001",
        title: "GeneralError",
        messageFormat: "Failed to generate code for this object because of {0}",
        category: "DarpBinaryObjectsGenerator",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );
    public static readonly DiagnosticDescriptor PartialKeywordMissing = new(
        id: "DBO001",
        title: "PartialKeywordMissing",
        messageFormat: "There should not be multiple attributes defining the length of a binary object",
        category: "DarpBinaryObjectsGenerator",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );
    public static readonly DiagnosticDescriptor InheritanceNotSupported = new(
        id: "DBO001",
        title: "InheritanceNotSupported",
        messageFormat: "Properties from base classes are not respected during binary operations",
        category: "DarpBinaryObjectsGenerator",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true
    );
    public static readonly DiagnosticDescriptor MemberTypeNotSupported = new(
        id: "DBO001",
        title: "MemberTypeNotSupported",
        messageFormat: "The type {0} of member {1} is not supported. The member will be skipped.",
        category: "DarpBinaryObjectsGenerator",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true
    );
    public static readonly DiagnosticDescriptor LengthAttributeInvalidCombination = new(
        id: "DBO001",
        title: "LengthAttributeInvalidCombination",
        messageFormat: "There should not be multiple attributes defining the length of a binary object",
        category: "DarpBinaryObjectsGenerator",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );
    public static readonly DiagnosticDescriptor MemberDefiningLengthNotFound = new(
        id: "DBO001",
        title: "MemberDefiningLengthNotFound",
        messageFormat: "Member '{0}' defining length for '{1}' was not found",
        category: "DarpBinaryObjectsGenerator",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );
    public static readonly DiagnosticDescriptor MemberDefiningLengthDataInvalidType = new(
        id: "DBO001",
        title: "MemberDefiningLengthDataInvalidType",
        messageFormat: "Member '{0}' defining length for '{1}' shall be an integer value. Allowed types are [sbyte, byte, short, ushort, int].",
        category: "DarpBinaryObjectsGenerator",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );
    public static readonly DiagnosticDescriptor MemberIgnoredReadonly = new(
        id: "DBO001",
        title: "MemberIgnoredReadonly",
        messageFormat: "Member '{0}' is ignored. Cannot read types with readonly members.",
        category: "DarpBinaryObjectsGenerator",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true
    );
    public static readonly DiagnosticDescriptor MemberIgnoredDuplicateName = new(
        id: "DBO001",
        title: "MemberIgnoredDuplicateName",
        messageFormat: "Member '{0}' is ignored. Cannot have two readonly members with an equal name.",
        category: "DarpBinaryObjectsGenerator",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true
    );
    public static readonly DiagnosticDescriptor ParameterWithIgnoredMember = new(
        id: "DBO001",
        title: "MemberIgnoredDuplicateName",
        messageFormat: "Parameter {0} is present in constructor but corresponding member {1} is marked with a BinaryIgnoreAttribute",
        category: "DarpBinaryObjectsGenerator",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true
    );
    public static readonly DiagnosticDescriptor MemberConstructorParameterTypeMismatch = new(
        id: "DBO001",
        title: "MemberConstructorParameterTypeMismatch",
        messageFormat: "This parameter matches the name of Member '{0}'. However, parameter type '{1}' does not match member type '{2}'.",
        category: "DarpBinaryObjectsGenerator",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );
    public static readonly DiagnosticDescriptor MemberConstructorParameterUnknown = new(
        id: "DBO001",
        title: "MemberConstructorParameterUnknown",
        messageFormat: "This parameter '{0}' does not have a corresponding member with an equal name. However, this is an requirement for binary objects.",
        category: "DarpBinaryObjectsGenerator",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );
    public static readonly DiagnosticDescriptor CollectionParameterInvalidType = new(
        id: "DBO001",
        title: "CollectionParameterInvalidType",
        messageFormat: "The type '{0}' is not allowed for collections",
        category: "DarpBinaryObjectsGenerator",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    public static readonly DiagnosticDescriptor InvalidFieldType = new(
        id: "DBO001",
        title: "CollectionParameterInvalidType",
        messageFormat: "BinaryObjects does not support the type '{0}' of field '{1}'",
        category: "DarpBinaryObjectsGenerator",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    public static readonly DiagnosticDescriptor InvalidNumericRangeAttributeData = new(
        id: "DBO001",
        title: "CollectionParameterInvalidType",
        messageFormat: "Attribute '{0}' has invalid data '{1}'. Has to be at least {2} and at most {3}.",
        category: "DarpBinaryObjectsGenerator",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    public static readonly DiagnosticDescriptor InvalidNumericMinimumAttributeData = new(
        id: "DBO001",
        title: "CollectionParameterInvalidType",
        messageFormat: "Attribute '{0}' has invalid data '{1}'. Has to be at least {2}.",
        category: "DarpBinaryObjectsGenerator",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    public static readonly DiagnosticDescriptor InvalidMultipleOfAttributeData = new(
        id: "DBO001",
        title: "CollectionParameterInvalidType",
        messageFormat: "Attribute '{0}' has invalid data '{1}'. Has to be multiple of {2}.",
        category: "DarpBinaryObjectsGenerator",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );
}

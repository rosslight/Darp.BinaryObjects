namespace Darp.BinaryObjects.Generator;

using Microsoft.CodeAnalysis;

internal static class DiagnosticDescriptors
{
    public static readonly DiagnosticDescriptor ConstructorParameterByReference = new(
        id: "DBO011",
        title: "ConstructorParameterByReference",
        messageFormat: "Constructor parameter '{0}' is passed by reference. Generated readers require a constructor with by-value parameters.",
        category: "DarpBinaryObjectsGenerator",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );
    public static readonly DiagnosticDescriptor ConstructorSelectionAmbiguous = new(
        id: "DBO010",
        title: "ConstructorSelectionAmbiguous",
        messageFormat: "Type '{0}' has multiple explicit instance constructors. Mark exactly one with BinaryConstructor to select the constructor used by generated readers.",
        category: "DarpBinaryObjectsGenerator",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );
    public static readonly DiagnosticDescriptor EnumerableMemberNotSupported = new(
        id: "DBO009",
        title: "EnumerableMemberNotSupported",
        messageFormat: "Enumerable member '{0}' is not supported. Materialize the sequence with ToArray() or ToList() and declare an array, list, or counted collection interface.",
        category: "DarpBinaryObjectsGenerator",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );
    public static readonly DiagnosticDescriptor RemainingCollectionMustBeLast = new(
        id: "DBO002",
        title: "RemainingCollectionMustBeLast",
        messageFormat: "Collection member '{0}' consumes the remaining buffer and must be the last serialized member. Use BinaryElementCount to define its boundary.",
        category: "DarpBinaryObjectsGenerator",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );
    public static readonly DiagnosticDescriptor CollectionLengthInvalid = new(
        id: "DBO003",
        title: "CollectionLengthInvalid",
        messageFormat: "Collection member '{0}' requires nonnegative element counts and a byte length no greater than Int32.MaxValue",
        category: "DarpBinaryObjectsGenerator",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );
    public static readonly DiagnosticDescriptor ObjectLengthTooLarge = new(
        id: "DBO004",
        title: "ObjectLengthTooLarge",
        messageFormat: "The minimum binary object byte length exceeds Int32.MaxValue",
        category: "DarpBinaryObjectsGenerator",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );
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
    public static readonly DiagnosticDescriptor BaseClassNotSerialized = new(
        id: "DBO005",
        title: "BaseClassNotSerialized",
        messageFormat: "Class '{0}' has a base class; only its declared members are serialized. Suppress DBO005 if this is intentional.",
        category: "DarpBinaryObjectsGenerator",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true
    );
    public static readonly DiagnosticDescriptor MemberTypeNotSupported = new(
        id: "DBO006",
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
        id: "DBO007",
        title: "MemberIgnoredReadonly",
        messageFormat: "Member '{0}' is ignored. Cannot read types with readonly members.",
        category: "DarpBinaryObjectsGenerator",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true
    );
    public static readonly DiagnosticDescriptor MemberIgnoredDuplicateName = new(
        id: "DBO008",
        title: "MemberIgnoredDuplicateName",
        messageFormat: "Member '{0}' is ignored. Cannot have two readonly members with an equal name.",
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
    public static readonly DiagnosticDescriptor CollectionElementLengthUnknown = new(
        id: "DBO001",
        title: "CollectionElementLengthUnknown",
        messageFormat: "Collection member '{0}' requires a known, positive binary element length. Use BinaryConstant on manual element types.",
        category: "DarpBinaryObjectsGenerator",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );
}

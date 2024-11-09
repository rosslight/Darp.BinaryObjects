namespace Darp.BinaryObjects.Generator;

using Microsoft.CodeAnalysis;

internal static class DiagnosticDescriptors
{
    public static readonly DiagnosticDescriptor PartialKeywordMissing =
        new(
            id: "DBO001",
            title: "PartialKeywordMissing",
            messageFormat: "There should not be multiple attributes defining the length of a binary object",
            category: "DarpBinaryObjectsGenerator",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true
        );
    public static readonly DiagnosticDescriptor InheritanceNotSupported =
        new(
            id: "DBO001",
            title: "InheritanceNotSupported",
            messageFormat: "Properties from base classes are not respected during binary operations",
            category: "DarpBinaryObjectsGenerator",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true
        );
    public static readonly DiagnosticDescriptor MemberTypeNotSupported =
        new(
            id: "DBO001",
            title: "PartialKeywordMissing",
            messageFormat: "The type {0} of member {1} is not supported. The member will be skipped.",
            category: "DarpBinaryObjectsGenerator",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true
        );
    public static readonly DiagnosticDescriptor LengthAttributeInvalidCombination =
        new(
            id: "DBO001",
            title: "LengthAttributeInvalidCombination",
            messageFormat: "There should not be multiple attributes defining the length of a binary object",
            category: "DarpBinaryObjectsGenerator",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true
        );
    public static readonly DiagnosticDescriptor MemberDefiningLengthNotFound =
        new(
            id: "DBO001",
            title: "MemberDefiningLengthNotFound",
            messageFormat: "Member '{0}' defining length for '{1}' was not found",
            category: "DarpBinaryObjectsGenerator",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true
        );
    public static readonly DiagnosticDescriptor MemberDefiningLengthDataInvalidType =
        new(
            id: "DBO001",
            title: "MemberDefiningLengthDataInvalidType",
            messageFormat: "Member '{0}' defining length for '{1}' shall be an integer value. Allowed types are [sbyte, byte, short, ushort, int].",
            category: "DarpBinaryObjectsGenerator",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true
        );
}

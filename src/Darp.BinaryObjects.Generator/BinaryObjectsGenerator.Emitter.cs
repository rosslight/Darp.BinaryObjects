namespace Darp.BinaryObjects.Generator;

using System.CodeDom.Compiler;

partial class BinaryObjectsGenerator
{
    private static BinaryObjectStruct EmitBinaryObjectData(TargetTypeInfo info)
    {
        var sw = new StringWriter();
        using var writer = new IndentedTextWriter(sw);
        var diagnostics = new List<DiagnosticData>();
        var utilities = new List<UtilityData>();

        return new BinaryObjectStruct(
            diagnostics.ToImmutableEquatableArray(),
            sw.ToString(),
            utilities.ToImmutableEquatableArray()
        );
    }
}

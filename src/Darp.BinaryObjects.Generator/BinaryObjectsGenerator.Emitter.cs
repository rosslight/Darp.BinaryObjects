namespace Darp.BinaryObjects.Generator;

using System.CodeDom.Compiler;
using System.Collections.Immutable;

internal readonly record struct Aaa2(
    ImmutableArray<DiagnosticData> Diagnostics,
    string? Code,
    ImmutableArray<UtilityData> Utilities
)
{
    public static Aaa2 Fail(IEnumerable<DiagnosticData> diagnostics)
    {
        return new Aaa2(diagnostics.ToImmutableArray(), null, ImmutableArray<UtilityData>.Empty);
    }
}

partial class BinaryObjectsGenerator
{
    private static bool TryEmit(
        ImmutableArray<IGroup> resultMemberGroups,
        ImmutableHashSet<IMember> resultMembersInitializedByConstructor,
        out Aaa2 aaa2
    )
    {
        var sw = new StringWriter();
        using var writer = new IndentedTextWriter(sw);
        var diagnostics = new List<DiagnosticData>();
        var utilities = new List<UtilityData>();

        aaa2 = Aaa2.Fail(diagnostics);
        return false;
    }
}

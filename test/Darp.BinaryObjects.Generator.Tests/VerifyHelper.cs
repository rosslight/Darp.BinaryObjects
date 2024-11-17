namespace Darp.BinaryObjects.Generator.Tests;

using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

public static class VerifyHelper
{
    public static Task VerifyBinaryObjectsGenerator(params string[] sources) =>
        VerifyGenerator<BinaryObjectsGenerator>(sources);

    private static async Task VerifyGenerator<TGenerator>(params string[] sources)
        where TGenerator : IIncrementalGenerator, new()
    {
        SyntaxTree[] syntaxTrees = sources.Select(x => CSharpSyntaxTree.ParseText(x)).ToArray();

        // Get all references of the currently loaded assembly
        PortableExecutableReference[] references = AppDomain
            .CurrentDomain.GetAssemblies() // Get currently loaded assemblies
            .Where(a => !a.IsDynamic && !string.IsNullOrEmpty(a.Location))
            .Select(a => MetadataReference.CreateFromFile(a.Location))
            .ToArray();

        var compilation = CSharpCompilation.Create(
            assemblyName: Assembly.GetExecutingAssembly().FullName,
            syntaxTrees: syntaxTrees,
            references: references
        );

        // Verify that there are no compilation errors (except for CS5001 which informs about a missing program entry)
        Assert.Empty(compilation.GetDiagnostics().Where(x => x.Id is not "CS5001"));

        var generator = new TGenerator();

        var driver = CSharpGeneratorDriver.Create(generator);
        await Verify(driver.RunGenerators(compilation)).UseDirectory("Snapshots");
    }
}

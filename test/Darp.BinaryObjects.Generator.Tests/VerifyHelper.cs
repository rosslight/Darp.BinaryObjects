namespace Darp.BinaryObjects.Generator.Tests;

using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

public static partial class VerifyHelper
{
    public static SettingsTask VerifyBinaryObjectsGenerator(params string[] sources) =>
        VerifyGenerator<BinaryObjectsGenerator>(sources).ScrubGeneratedCodeAttribute();

    [GeneratedRegex("""GeneratedCodeAttribute\("[^"\n]+",\s*"(?<version>\d+\.\d+\.\d+\.\d+)"\)""")]
    private static partial Regex GetGeneratedCodeRegex();

    public static SettingsTask ScrubGeneratedCodeAttribute(
        this SettingsTask settingsTask,
        string scrubbedVersionName = "GeneratorVersion"
    )
    {
        return settingsTask.ScrubLinesWithReplace(line =>
        {
            Regex regex = GetGeneratedCodeRegex();
            return regex.Replace(
                line,
                match =>
                {
                    var versionToReplace = match.Groups["version"].Value;
                    return match.Value.Replace(versionToReplace, scrubbedVersionName);
                }
            );
        });
    }

    private static SettingsTask VerifyGenerator<TGenerator>(params string[] sources)
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

        // Assert that there are no compilation errors (except for CS5001 which informs about the missing program entry)
        Assert.Empty(compilation.GetDiagnostics().Where(x => x.Id is not "CS5001"));

        var generator = new TGenerator();

        var driver = CSharpGeneratorDriver.Create(generator);
        return Verify(driver.RunGenerators(compilation)).UseDirectory("Snapshots");
    }
}

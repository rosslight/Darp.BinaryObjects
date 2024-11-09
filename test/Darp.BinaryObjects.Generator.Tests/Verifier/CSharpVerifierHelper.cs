using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

namespace Darp.BinaryObjects.Generator.Tests.Verifier;

using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.CodeAnalysis.Text;

public static class CSharpSourceGeneratorVerifier<TSourceGenerator>
    where TSourceGenerator : IIncrementalGenerator, new()
{
    public class Test : CSharpSourceGeneratorTest<TSourceGenerator, DefaultVerifier>
    {
        private const string DirectoryOfGeneratedSources = "Resources.Generated";

        private readonly string _identifier;
        private readonly string? _testFile;
        private readonly string? _testMethod;

        public Test(
            string identifier = "",
            [CallerFilePath] string? testFile = null,
            [CallerMemberName] string? testMethod = null
        )
        {
            _identifier = identifier;
            _testFile = testFile;
            _testMethod = testMethod;

#if WRITE_EXPECTED
            TestBehaviors |= TestBehaviors.SkipGeneratedSourcesCheck;
#endif
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80;
        }

        private string ResourceName
        {
            get
            {
                if (string.IsNullOrEmpty(_identifier))
                    return _testMethod ?? "";

                return $"{_testMethod}_{_identifier}";
            }
        }

        protected override string DefaultFileExt => "g.cs";

        public LanguageVersion LanguageVersion { get; set; } = LanguageVersion.CSharp11;

        protected override CompilationOptions CreateCompilationOptions()
        {
            CompilationOptions compilationOptions = base.CreateCompilationOptions();
            return compilationOptions.WithSpecificDiagnosticOptions(
                compilationOptions.SpecificDiagnosticOptions.SetItems(GetNullableWarningsFromCompiler())
            );
        }

        private static ImmutableDictionary<string, ReportDiagnostic> GetNullableWarningsFromCompiler()
        {
            string[] args = { "/warnaserror:nullable" };
            CSharpCommandLineArguments commandLineArguments = CSharpCommandLineParser.Default.Parse(
                args,
                baseDirectory: Environment.CurrentDirectory,
                sdkDirectory: Environment.CurrentDirectory
            );
            ImmutableDictionary<string, ReportDiagnostic> nullableWarnings = commandLineArguments
                .CompilationOptions
                .SpecificDiagnosticOptions;

            return nullableWarnings;
        }

        protected override ParseOptions CreateParseOptions() =>
            ((CSharpParseOptions)base.CreateParseOptions()).WithLanguageVersion(LanguageVersion);

        protected override Task RunImplAsync(CancellationToken cancellationToken)
        {
            TestState.AdditionalReferences.Add(typeof(BinaryObjectAttribute).Assembly.Location);
            return base.RunImplAsync(cancellationToken);
        }

        public Test AddGeneratedSources()
        {
            var expectedPrefix =
                $"{Assembly.GetExecutingAssembly().GetName().Name}.{DirectoryOfGeneratedSources}.{ResourceName}.";
            foreach (var resourceName in Assembly.GetExecutingAssembly().GetManifestResourceNames())
            {
                if (!resourceName.StartsWith(expectedPrefix, StringComparison.Ordinal))
                    continue;

                using Stream resourceStream =
                    Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName)
                    ?? throw new InvalidOperationException();
                using var reader = new StreamReader(
                    resourceStream,
                    Encoding.UTF8,
                    detectEncodingFromByteOrderMarks: true,
                    bufferSize: 4096,
                    leaveOpen: true
                );
                var name = resourceName[expectedPrefix.Length..];
                var readData = reader.ReadToEnd();
                readData = readData.ReplaceLineEndings("\n");
                TestState.GeneratedSources.Add(
                    (
                        typeof(TSourceGenerator),
                        name,
                        SourceText.From(readData, Encoding.UTF8, SourceHashAlgorithm.Sha256)
                    )
                );
            }

            return this;
        }
    }
}

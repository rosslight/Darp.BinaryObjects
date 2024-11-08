namespace Darp.BinaryObjects.Generator.Tests;

using System.Text;
using Microsoft.CodeAnalysis.Text;
using VerifyCS = Verifier.CSharpSourceGeneratorVerifier<BinaryObjectsGenerator>;

public class IntegrationTest
{
    [Fact]
    public async Task OneBool_DefaultAsync()
    {
        const string code = """
[Darp.BinaryObjects.BinaryObject]
public sealed partial record OneBool(bool Value);
""";
        await new VerifyCS.Test { TestState = { Sources = { code } } }
            .AddGeneratedSources()
            .RunAsync();
    }
}

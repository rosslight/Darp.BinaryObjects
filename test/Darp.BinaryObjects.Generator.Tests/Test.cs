namespace Darp.BinaryObjects.Generator.Tests;

using VerifyCS = Verifier.CSharpSourceGeneratorVerifier<BinaryObjectsGenerator>;

public class IntegrationTest
{
    [Fact]
    public async Task OneBool_DefaultAsync()
    {
        const string code = """
using Darp.BinaryObjects;

[BinaryObject]
public sealed partial record OneBool(bool Value);
""";
        await new VerifyCS.Test { TestState = { Sources = { code } } }
            .AddGeneratedSources()
            .RunAsync();
    }

    [Fact]
    public async Task TwoUShorts_DefaultAsync()
    {
        const string code = """
using Darp.BinaryObjects;

[BinaryObject]
public sealed partial record TwoUShorts(ushort ValueOne, ushort ValueTwo);
""";
        await new VerifyCS.Test { TestState = { Sources = { code } } }
            .AddGeneratedSources()
            .RunAsync();
    }
}

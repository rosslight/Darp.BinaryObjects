namespace Darp.BinaryObjects.Generator.Tests;

using System.Runtime.CompilerServices;

public static class VerifySettingsInitializer
{
    [ModuleInitializer]
    public static void Initialize()
    {
        VerifySourceGenerators.Initialize();
    }
}

public sealed class VerifySettingsTests
{
    [Fact]
    public Task RunVerifyChecks() => VerifyChecks.Run();
}

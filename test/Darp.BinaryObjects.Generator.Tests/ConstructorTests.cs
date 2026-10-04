namespace Darp.BinaryObjects.Generator.Tests;

public sealed class ConstructorTests
{
    [Theory]
    [InlineData("Read")]
    [InlineData("All")]
    public async Task AmbiguousConstructors_ShouldReportDiagnostic(string options)
    {
        var code = $$"""
            using Darp.BinaryObjects;

            [BinaryObject(BinaryOptions.{{options}})]
            public sealed partial class Unmarked
            {
                public Unmarked() { }
                public Unmarked(byte value) => Value = value;
                public byte Value { get; }
            }

            [BinaryObject(BinaryOptions.{{options}})]
            public sealed partial class MultiplyMarked
            {
                [BinaryConstructor]
                public MultiplyMarked() { }
                [BinaryConstructor]
                public MultiplyMarked(byte value) => Value = value;
                public byte Value { get; }
            }
            """;
        await VerifyHelper.VerifyBinaryObjectsGenerator(code).UseParameters(options);
    }

    [Fact]
    public async Task SelectedConstructor_ShouldStillValidateParameters()
    {
        const string code = """
            using Darp.BinaryObjects;

            [BinaryObject]
            public sealed partial class InvalidConstructor
            {
                public InvalidConstructor() { }
                [BinaryConstructor]
                private InvalidConstructor(int value) { }
                public byte Value { get; }
            }
            """;
        await VerifyHelper.VerifyBinaryObjectsGenerator(code);
    }
}

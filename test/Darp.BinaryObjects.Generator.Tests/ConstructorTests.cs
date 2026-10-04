namespace Darp.BinaryObjects.Generator.Tests;

public sealed class ConstructorTests
{
    [Fact]
    public async Task AmbiguousConstructors_ShouldReportDiagnostic()
    {
        const string code = """
            using Darp.BinaryObjects;

            [BinaryObject]
            public sealed partial class Unmarked
            {
                public Unmarked() { }
                public Unmarked(byte value) => Value = value;
                public byte Value { get; }
            }

            [BinaryObject]
            public sealed partial class MultiplyMarked
            {
                [BinaryConstructor]
                public MultiplyMarked() { }
                [BinaryConstructor]
                public MultiplyMarked(byte value) => Value = value;
                public byte Value { get; }
            }

            [BinaryObject(BinaryOptions.Write)]
            public sealed partial class WriteOnlyUnmarked
            {
                public WriteOnlyUnmarked() { }
                public WriteOnlyUnmarked(byte value) => Value = value;
                public byte Value { get; }
            }
            """;
        await VerifyHelper.VerifyBinaryObjectsGenerator(code);
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

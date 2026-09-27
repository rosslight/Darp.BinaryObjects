namespace Darp.BinaryObjects.Generator.Tests;

using static VerifyHelper;

public class LayoutTests
{
    [Theory]
    [InlineData("Read")]
    [InlineData("Write")]
    [InlineData("All")]
    [InlineData("0")]
    public async Task BinaryOptions_SelectGeneratedContract(string options)
    {
        if (options == "0")
        {
            await VerifyBinaryObjectsGenerator(
                    """
                    using Darp.BinaryObjects;

                    [BinaryObject((BinaryOptions)0)]
                    public sealed partial record TestObject
                    {
                        public TestObject(object unrelated) { }
                        public object Unsupported { get; init; } = new();
                    }
                    """
                )
                .UseParameters(options);
            return;
        }

        var manualInterfaces = options switch
        {
            "Read" => "IBinaryReadable<ManualObject>",
            "Write" => "IBinaryWritable",
            _ => "IBinaryObject<ManualObject>",
        };
        var manualReaders =
            options == "Write"
                ? ""
                : """
                    public static bool TryReadLittleEndian(ReadOnlySpan<byte> source, [NotNullWhen(true)] out ManualObject? value) => throw new NotImplementedException();
                    public static bool TryReadLittleEndian(ReadOnlySpan<byte> source, [NotNullWhen(true)] out ManualObject? value, out int bytesRead) => throw new NotImplementedException();
                    public static bool TryReadBigEndian(ReadOnlySpan<byte> source, [NotNullWhen(true)] out ManualObject? value) => throw new NotImplementedException();
                    public static bool TryReadBigEndian(ReadOnlySpan<byte> source, [NotNullWhen(true)] out ManualObject? value, out int bytesRead) => throw new NotImplementedException();
                    """;
        var manualWriters =
            options == "Read"
                ? ""
                : """
                    public int GetByteCount() => 2;
                    public bool TryWriteLittleEndian(Span<byte> destination) => throw new NotImplementedException();
                    public bool TryWriteLittleEndian(Span<byte> destination, out int bytesWritten) => throw new NotImplementedException();
                    public bool TryWriteBigEndian(Span<byte> destination) => throw new NotImplementedException();
                    public bool TryWriteBigEndian(Span<byte> destination, out int bytesWritten) => throw new NotImplementedException();
                    """;
        var childConstructor =
            options == "Write"
                ? "public Child(object context) => Value = true;"
                : "public Child(bool value) => Value = value;";
        var oppositeMethod = options switch
        {
            "Read" =>
                "public bool TryWriteLittleEndian(Span<byte> destination) => throw new NotImplementedException();",
            "Write" =>
                "public static bool TryReadLittleEndian(ReadOnlySpan<byte> source, out Child? value) => throw new NotImplementedException();",
            _ => "",
        };
        var manualVariableObject =
            options == "All"
                ? ""
                : $$"""
                    public sealed record VariableManualObject(ushort Value) : {{manualInterfaces.Replace(
                        "ManualObject",
                        "VariableManualObject",
                        StringComparison.Ordinal
                    )}}
                    {
                        {{manualReaders.Replace("ManualObject", "VariableManualObject", StringComparison.Ordinal)}}
                        {{manualWriters}}
                    }

                    [BinaryObject(BinaryOptions.{{options}})]
                    public sealed partial record ManualParent(VariableManualObject Value);
                    """;
        var staticMembers =
            options == "Write"
                ? """
                    public static readonly byte StaticField = 1;
                    public static byte StaticProperty { get; } = 2;
                    """
                : "";
        var code = $$"""
            using Darp.BinaryObjects;
            using System;
            using System.Diagnostics.CodeAnalysis;

            [BinaryObject(BinaryOptions.{{options}})]
            public sealed partial record Child
            {
                {{childConstructor}}
                public bool Value { get; }
                [BinaryIgnore] public int Ignored { get; init; }
                public int Computed => 42;
                {{staticMembers}}
                {{oppositeMethod}}
            }

            [BinaryConstant(2)]
            public sealed record ManualObject : {{manualInterfaces}}
            {
                {{manualReaders}}
                {{manualWriters}}
            }

            [BinaryObject(BinaryOptions.{{options}})]
            public sealed partial record VariableChild(byte[] Data);

            [BinaryObject(BinaryOptions.{{options}})]
            public sealed partial record Parent(
                Child Header,
                [property: BinaryElementCount(2)] ManualObject[] Values,
                VariableChild Payload);

            {{manualVariableObject}}
            """;
        await VerifyBinaryObjectsGenerator(code).UseParameters(options);
    }

    [Fact]
    public async Task TwoClasses_NoNamespaces()
    {
        const string code = """
using Darp.BinaryObjects;

[BinaryObject]
public sealed partial record TestObject1;

[BinaryObject]
public sealed partial record TestObject2;
""";
        await VerifyBinaryObjectsGenerator(code);
    }

    [Fact]
    public async Task TwoClasses_SameNamespace()
    {
        const string code = """
namespace Test;

using Darp.BinaryObjects;

[BinaryObject]
public sealed partial record TestObject1;

[BinaryObject]
public sealed partial record TestObject2;
""";
        await VerifyBinaryObjectsGenerator(code);
    }

    [Fact]
    public async Task TwoClasses_DifferentNamespace()
    {
        const string code = """
using Darp.BinaryObjects;

namespace Test1
{
    [BinaryObject]
    public sealed partial record TestObject1;
}

namespace Test2
{
    [BinaryObject]
    public sealed partial record TestObject2;
}
""";
        await VerifyBinaryObjectsGenerator(code);
    }
}

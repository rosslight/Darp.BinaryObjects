namespace Darp.BinaryObjects.Generator.Tests;

using static VerifyHelper;

public class LayoutTests
{
    [Fact]
    public async Task InheritedMembers_WarnUnlessSuppressed()
    {
        const string code = """
            using Darp.BinaryObjects;

            public class Header
            {
                public byte Prefix { get; init; }
            }

            [BinaryObject]
            public partial class Packet : Header
            {
                public byte Data { get; init; }
            }

            #pragma warning disable DBO005
            [BinaryObject]
            public partial class IntentionalPacket : Header
            {
                public byte Data { get; init; }
            }
            #pragma warning restore DBO005

            public class BehaviorBase
            {
                public void Ping() { }
            }

            [BinaryObject]
            public partial class BehaviorPacket : BehaviorBase
            {
                public byte Data { get; init; }
            }

            public class VirtualBase
            {
                public virtual byte Value { get; init; }
            }

            [BinaryObject]
            public partial class OverridePacket : VirtualBase
            {
                public override byte Value { get; init; }
            }
            """;
        await VerifyBinaryObjectsGenerator(code);
    }

    [Fact]
    public async Task InheritedMembers_LocalPragmaWorksOnAnnotatedPartialDeclaration()
    {
        const string code = """
            using Darp.BinaryObjects;

            public class Header
            {
                public byte Prefix { get; init; }
            }

            public partial class SplitPacket : Header { }

            #pragma warning disable DBO005
            [BinaryObject]
            public partial class SplitPacket
            {
                public byte Data { get; init; }
            }
            #pragma warning restore DBO005
            """;
        await VerifyBinaryObjectsGenerator(code);
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

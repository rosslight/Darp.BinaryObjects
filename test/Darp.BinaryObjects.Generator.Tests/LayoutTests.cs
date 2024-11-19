namespace Darp.BinaryObjects.Generator.Tests;

using static VerifyHelper;

public class LayoutTests
{
    [Fact]
    public async Task OneBool_TwoClasses_NoNamespaces()
    {
        const string code = """
using Darp.BinaryObjects;

[BinaryObject]
public sealed partial record EmptyRecord1;

[BinaryObject]
public sealed partial record EmptyRecord2;
""";
        await VerifyBinaryObjectsGenerator(code);
    }

    [Fact]
    public async Task OneBool_TwoClasses_SameNamespace()
    {
        const string code = """
namespace Test;

using Darp.BinaryObjects;

[BinaryObject]
public sealed partial record EmptyRecord1;

[BinaryObject]
public sealed partial record EmptyRecord2;
""";
        await VerifyBinaryObjectsGenerator(code);
    }

    [Fact]
    public async Task OneBool_TwoClasses_DifferentNamespace()
    {
        const string code = """
using Darp.BinaryObjects;

namespace Test1
{
    [BinaryObject]
    public sealed partial record EmptyRecord1;
}

namespace Test2
{
    [BinaryObject]
    public sealed partial record EmptyRecord2;
}
""";
        await VerifyBinaryObjectsGenerator(code);
    }
}

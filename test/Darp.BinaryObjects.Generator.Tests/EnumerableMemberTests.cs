namespace Darp.BinaryObjects.Generator.Tests;

public sealed class EnumerableMemberTests
{
    [Theory]
    [InlineData("Read")]
    [InlineData("Write")]
    [InlineData("All")]
    public async Task EnumerableMembersAreRejected(string options)
    {
        var code = $$"""
            using Darp.BinaryObjects;
            using System.Collections.Generic;

            [BinaryObject(BinaryOptions.{{options}})]
            public sealed partial class Payload
            {
                public byte Count { get; set; }
                [BinaryElementCount(2)] public IEnumerable<byte> Fixed { get; set; } = new byte[2];
                [BinaryElementCount(nameof(Count))] public IEnumerable<byte> Counted { get; set; } = new byte[2];
                public IEnumerable<byte> Remaining { get; set; } = new byte[2];
            }
            """;
        await VerifyHelper.VerifyBinaryObjectsGenerator(code);
    }

    [Fact]
    public async Task IgnoredEnumerableIsNotSerialized()
    {
        const string code = """
            using Darp.BinaryObjects;
            using System.Collections.Generic;

            [BinaryObject]
            public sealed partial class Payload
            {
                public byte Value { get; set; }
                [BinaryIgnore] public IEnumerable<byte> Ignored { get; set; } = new byte[0];
            }
            """;
        await VerifyHelper.VerifyBinaryObjectsGenerator(code);
    }
}

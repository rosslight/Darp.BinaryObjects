namespace Darp.BinaryObjects.Generator.Tests;

using System.Globalization;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Generator.BinarySymbol;

public sealed class ParserTests
{
    private const string CodeSetUp = """
        using Darp.BinaryObjects;
        using System;
        using System.Collections.Generic;
        using System.Diagnostics.CodeAnalysis;

        [BinaryObject]
        public partial record GeneratedObject { }

        [BinaryConstant(5)]
        public sealed record ConstantObject : IBinaryObject<ConstantObject>
        {
            public int GetByteCount() => throw new NotImplementedException();

            public bool TryWriteLittleEndian(Span<byte> destination) => throw new NotImplementedException();

            public bool TryWriteLittleEndian(Span<byte> destination, out int bytesWritten) =>
                throw new NotImplementedException();

            public bool TryWriteBigEndian(Span<byte> destination) => throw new NotImplementedException();

            public bool TryWriteBigEndian(Span<byte> destination, out int bytesWritten) => throw new NotImplementedException();

            public static bool TryReadLittleEndian(ReadOnlySpan<byte> source, [NotNullWhen(true)] out ConstantObject? value) =>
                throw new NotImplementedException();

            public static bool TryReadLittleEndian(
                ReadOnlySpan<byte> source,
                [NotNullWhen(true)] out ConstantObject? value,
                out int bytesRead
            ) => throw new NotImplementedException();

            public static bool TryReadBigEndian(ReadOnlySpan<byte> source, [NotNullWhen(true)] out ConstantObject? value) =>
                throw new NotImplementedException();

            public static bool TryReadBigEndian(
                ReadOnlySpan<byte> source,
                [NotNullWhen(true)] out ConstantObject? value,
                out int bytesRead
            ) => throw new NotImplementedException();
        }

        public sealed record VariableObject : IBinaryObject<VariableObject>
        {
            public int GetByteCount() => throw new NotImplementedException();

            public bool TryWriteLittleEndian(Span<byte> destination) => throw new NotImplementedException();

            public bool TryWriteLittleEndian(Span<byte> destination, out int bytesWritten) =>
                throw new NotImplementedException();

            public bool TryWriteBigEndian(Span<byte> destination) => throw new NotImplementedException();

            public bool TryWriteBigEndian(Span<byte> destination, out int bytesWritten) => throw new NotImplementedException();

            public static bool TryReadLittleEndian(ReadOnlySpan<byte> source, [NotNullWhen(true)] out VariableObject? value) =>
                throw new NotImplementedException();

            public static bool TryReadLittleEndian(
                ReadOnlySpan<byte> source,
                [NotNullWhen(true)] out VariableObject? value,
                out int bytesRead
            ) => throw new NotImplementedException();

            public static bool TryReadBigEndian(ReadOnlySpan<byte> source, [NotNullWhen(true)] out VariableObject? value) =>
                throw new NotImplementedException();

            public static bool TryReadBigEndian(
                ReadOnlySpan<byte> source,
                [NotNullWhen(true)] out VariableObject? value,
                out int bytesRead
            ) => throw new NotImplementedException();
        }

        public enum UShortEnum : ushort { }
        """;

    [Theory]
    [InlineData("")]
    [InlineData("[property: BinaryIgnore] int A")]
    public void Empty(string arguments)
    {
        var code = $"""
            {CodeSetUp}
            public partial record Object0_1({arguments});
            """;
        (INamedTypeSymbol symbol, Compilation compilation) = GetSymbol(code, "Object0_1");
        ParserResult parsingResult = Parser.Parse(symbol, compilation);

        parsingResult.TypeName.Name.Should().Be("Object0_1");
        parsingResult.IsError.Should().BeFalse();
        parsingResult.Diagnostics.Should().BeEmpty();
        parsingResult.Symbols.Should().HaveCount(0);
    }

    [Theory]
    [InlineData("int A", "A", "Int32", 4)]
    [InlineData("UShortEnum A", "A", "UShortEnum", 2)]
    [InlineData("[property: BinaryLength(3)] int A", "A", "Int32", 3)]
    [InlineData("ConstantObject A", "A", "ConstantObject", 5)]
    public void ConstantSymbol(string argument, string symbolName, string fieldTypeName, int fieldLength)
    {
        var code = $"""
            {CodeSetUp}
            public partial record Object0_1({argument});
            """;
        (INamedTypeSymbol symbol, Compilation compilation) = GetSymbol(code, "Object0_1");
        ParserResult parsingResult = Parser.Parse(symbol, compilation);

        parsingResult.TypeName.Name.Should().Be("Object0_1");
        parsingResult.IsError.Should().BeFalse();
        parsingResult.Diagnostics.Should().BeEmpty();
        parsingResult.Symbols.Should().HaveCount(1);
        parsingResult.Symbols[0].Should().BeOfType<ConstantSymbol>();
        ConstantSymbol symbol0 = parsingResult.Symbols[0].UnwrapConstantSymbol();
        symbol0.Symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat).Should().Be(symbolName);
        symbol0.FieldType.Name.Should().Be(fieldTypeName);
        symbol0.FieldLength.Should().Be(fieldLength);
    }

    [Theory]
    [InlineData("""int Length, [property: BinaryLength("Length")] int A""", "A", "Int32", 4, "Length")]
    public void VariableSymbol(
        string argument,
        string symbolName,
        string fieldTypeName,
        int fieldLength,
        string fieldLengthName
    )
    {
        var code = $"""
            {CodeSetUp}
            public partial record Object0_1({argument});
            """;
        (INamedTypeSymbol symbol, Compilation compilation) = GetSymbol(code, "Object0_1");
        ParserResult parsingResult = Parser.Parse(symbol, compilation);

        parsingResult.TypeName.Name.Should().Be("Object0_1");
        parsingResult.Diagnostics.Should().BeEmpty();
        parsingResult.IsError.Should().BeFalse();
        parsingResult.Symbols.Should().HaveCount(2);
        parsingResult.Symbols[0].Should().BeOfType<ConstantSymbol>();
        parsingResult.Symbols[1].Should().BeOfType<VariableSymbol>();
        VariableSymbol symbol0 = parsingResult.Symbols[1].UnwrapVariableSymbol();
        symbol0.Symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat).Should().Be(symbolName);
        symbol0.FieldType.Name.Should().Be(fieldTypeName);
        symbol0.MaxFieldLength.Should().Be(fieldLength);
        symbol0.FieldLengthName.Should().Be(fieldLengthName);
    }

    [Theory]
    [InlineData("[property: BinaryLength(8)] int[] A", "A", "int[]", "Int32", 2, 4)]
    [InlineData("[property: BinaryLength(10)] ConstantObject[] A", "A", "ConstantObject[]", "ConstantObject", 2, 5)]
    [InlineData("[property: BinaryElementCount(2)] int[] A", "A", "int[]", "Int32", 2, 4)]
    [InlineData("[property: BinaryElementCount(2), BinaryElementLength(3)] int[] A", "A", "int[]", "Int32", 2, 3)]
    [InlineData(
        "[property: BinaryElementCount(1)] ConstantObject[] A",
        "A",
        "ConstantObject[]",
        "ConstantObject",
        1,
        5
    )]
    [InlineData(
        "[property: BinaryElementCount(2)] ReadOnlyMemory<int> A",
        "A",
        "System.ReadOnlyMemory<int>",
        "Int32",
        2,
        4
    )]
    [InlineData(
        "[property: BinaryElementCount(2)] List<int> A",
        "A",
        "System.Collections.Generic.List<int>",
        "Int32",
        2,
        4
    )]
    [InlineData("[property: BinaryLength(8)] string A", "A", "string", "Char", 4, 2)]
    [InlineData("[property: BinaryElementCount(2)] string A", "A", "string", "Char", 2, 2)]
    [InlineData("[property: BinaryElementCount(4), BinaryElementLength(1)] string A", "A", "string", "Char", 4, 1)]
    public void ConstantArraySymbol(
        string argument,
        string symbolName,
        string fieldTypeName,
        string underlyingType,
        int elementCount,
        int elementLength
    )
    {
        var code = $"""
            {CodeSetUp}
            public partial record Object0_1({argument});
            """;
        (INamedTypeSymbol symbol, Compilation compilation) = GetSymbol(code, "Object0_1");
        ParserResult parsingResult = Parser.Parse(symbol, compilation);

        parsingResult.TypeName.Name.Should().Be("Object0_1");
        parsingResult.Diagnostics.Should().BeEmpty();
        parsingResult.IsError.Should().BeFalse();
        parsingResult.Symbols.Should().HaveCount(1);
        parsingResult.Symbols[0].Should().BeOfType<ConstantArraySymbol>();
        ConstantArraySymbol symbol0 = parsingResult.Symbols[0].UnwrapConstantArraySymbol();
        symbol0.Symbol.Name.Should().Be(symbolName);
        symbol0.ArrayType.ToDisplayString().Should().Be(fieldTypeName);
        symbol0.UnderlyingType.Name.Should().Be(underlyingType);
        symbol0.ElementCount.Should().Be(elementCount);
        symbol0.ElementLength.Should().Be(elementLength);
        symbol0.FieldLength.Should().Be(elementCount * elementLength);
    }

    [Theory]
    [InlineData("int[] A", "A", "int[]", "Int32", 4)]
    [InlineData("[property: BinaryElementLength(3)] int[] A", "A", "int[]", "Int32", 3)]
    [InlineData("List<int> A", "A", "System.Collections.Generic.List<int>", "Int32", 4)]
    public void ArraySymbol(
        string argument,
        string symbolName,
        string fieldTypeName,
        string underlyingType,
        int elementLength
    )
    {
        var code = $"""
            {CodeSetUp}
            public partial record Object0_1({argument});
            """;
        (INamedTypeSymbol symbol, Compilation compilation) = GetSymbol(code, "Object0_1");
        ParserResult parsingResult = Parser.Parse(symbol, compilation);

        parsingResult.TypeName.Name.Should().Be("Object0_1");
        parsingResult.Diagnostics.Should().BeEmpty();
        parsingResult.IsError.Should().BeFalse();
        parsingResult.Symbols.Should().HaveCount(1);
        parsingResult.Symbols[0].Should().BeOfType<ArraySymbol>();
        ArraySymbol symbol0 = parsingResult.Symbols[0].UnwrapArraySymbol();
        symbol0.Symbol.Name.Should().Be(symbolName);
        symbol0.ArrayType.ToDisplayString().Should().Be(fieldTypeName);
        symbol0.UnderlyingType.Name.Should().Be(underlyingType);
        symbol0.ElementLength.Should().Be(elementLength);
    }

    [Theory]
    [InlineData("""int Length, [property: BinaryElementCount("Length")] int[] A""", "A", "int[]", "Int32", "Length", 4)]
    [InlineData(
        """int Length, [property: BinaryElementCount("Length"), BinaryElementLength(3)] int[] A""",
        "A",
        "int[]",
        "Int32",
        "Length",
        3
    )]
    public void VariableCountArraySymbol(
        string argument,
        string symbolName,
        string fieldTypeName,
        string underlyingType,
        string elementCountName,
        int elementLength
    )
    {
        var code = $"""
            {CodeSetUp}
            public partial record Object0_1({argument});
            """;
        (INamedTypeSymbol symbol, Compilation compilation) = GetSymbol(code, "Object0_1");
        ParserResult parsingResult = Parser.Parse(symbol, compilation);

        parsingResult.TypeName.Name.Should().Be("Object0_1");
        parsingResult.Diagnostics.Should().BeEmpty();
        parsingResult.IsError.Should().BeFalse();
        parsingResult.Symbols.Should().HaveCount(2);
        parsingResult.Symbols[0].Should().BeOfType<ConstantSymbol>();
        parsingResult.Symbols[1].Should().BeOfType<VariableCountArraySymbol>();
        VariableCountArraySymbol symbol0 = parsingResult.Symbols[1].UnwrapVariableCountArraySymbol();
        symbol0.Symbol.Name.Should().Be(symbolName);
        symbol0.ArrayType.ToDisplayString().Should().Be(fieldTypeName);
        symbol0.UnderlyingType.Name.Should().Be(underlyingType);
        symbol0.ElementCountName.Should().Be(elementCountName);
        symbol0.ElementLength.Should().Be(elementLength);
    }

    [Theory]
    [InlineData("""int Length, [property: BinaryLength("Length")] int[] A""", "A", "int[]", "Int32", 4, "Length")]
    [InlineData(
        """int Length, [property: BinaryLength("Length"), BinaryElementLength(3)] int[] A""",
        "A",
        "int[]",
        "Int32",
        3,
        "Length"
    )]
    public void VariableLengthArraySymbol(
        string argument,
        string symbolName,
        string fieldTypeName,
        string underlyingType,
        int elementLength,
        string fieldLengthName
    )
    {
        var code = $"""
            {CodeSetUp}
            public partial record Object0_1({argument});
            """;
        (INamedTypeSymbol symbol, Compilation compilation) = GetSymbol(code, "Object0_1");
        ParserResult parsingResult = Parser.Parse(symbol, compilation);

        parsingResult.TypeName.Name.Should().Be("Object0_1");
        parsingResult.Diagnostics.Should().BeEmpty();
        parsingResult.IsError.Should().BeFalse();
        parsingResult.Symbols.Should().HaveCount(2);
        parsingResult.Symbols[0].Should().BeOfType<ConstantSymbol>();
        parsingResult.Symbols[1].Should().BeOfType<VariableLengthArraySymbol>();
        VariableLengthArraySymbol symbol0 = parsingResult.Symbols[1].UnwrapVariableLengthArraySymbol();
        symbol0.Symbol.Name.Should().Be(symbolName);
        symbol0.ArrayType.ToDisplayString().Should().Be(fieldTypeName);
        symbol0.UnderlyingType.Name.Should().Be(underlyingType);
        symbol0.ElementLength.Should().Be(elementLength);
        symbol0.FieldLengthName.Should().Be(fieldLengthName);
    }

    [Theory]
    [InlineData("Object0_1(GeneratedObject A)", "A", "GeneratedObject")]
    public void GeneratedBinaryObjectSymbol(string argument, string symbolName, string fieldTypeName)
    {
        var code = $"""
            {CodeSetUp}
            public partial record {argument};
            """;
        (INamedTypeSymbol symbol, Compilation compilation) = GetSymbol(code, "Object0_1");
        ParserResult parsingResult = Parser.Parse(symbol, compilation);

        parsingResult.TypeName.Name.Should().Be("Object0_1");
        parsingResult.Diagnostics.Should().BeEmpty();
        parsingResult.IsError.Should().BeFalse();
        parsingResult.Symbols.Should().HaveCount(1);
        parsingResult.Symbols[0].Should().BeOfType<GeneratedBinaryObjectSymbol>();
        GeneratedBinaryObjectSymbol symbol0 = parsingResult.Symbols[0].UnwrapGeneratedBinaryObjectSymbol();
        symbol0.Symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat).Should().Be(symbolName);
        symbol0.FieldType.Name.Should().Be(fieldTypeName);
    }

    [Theory]
    [InlineData("Object0_1<T>(T A) where T : IBinaryObject<T>", "A", "T")]
    [InlineData("Object0_1(VariableObject A)", "A", "VariableObject")]
    public void BinaryObjectSymbol(string argument, string symbolName, string fieldTypeName)
    {
        var code = $"""
            {CodeSetUp}
            public partial record {argument};
            """;
        (INamedTypeSymbol symbol, Compilation compilation) = GetSymbol(code, "Object0_1");
        ParserResult parsingResult = Parser.Parse(symbol, compilation);

        parsingResult.TypeName.Name.Should().Be("Object0_1");
        parsingResult.Diagnostics.Should().BeEmpty();
        parsingResult.IsError.Should().BeFalse();
        parsingResult.Symbols.Should().HaveCount(1);
        parsingResult.Symbols[0].Should().BeOfType<BinaryObjectSymbol>();
        BinaryObjectSymbol symbol0 = parsingResult.Symbols[0].UnwrapBinaryObjectSymbol();
        symbol0.Symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat).Should().Be(symbolName);
        symbol0.FieldType.Name.Should().Be(fieldTypeName);
    }

    [Theory]
    [InlineData("object A", "BinaryObjects does not support the type 'Object' of field 'A'")]
    [InlineData(
        "[property: BinaryLength(0)] int A",
        "Attribute 'BinaryLength' has invalid data '0'. Has to be at least 1 and at most 4."
    )]
    [InlineData(
        "[property: BinaryLength(5)] int A",
        "Attribute 'BinaryLength' has invalid data '5'. Has to be at least 1 and at most 4."
    )]
    [InlineData(
        "[property: BinaryLength(7)] int[] A",
        "Attribute 'BinaryLength' has invalid data '7'. Has to be multiple of 4."
    )]
    [InlineData(
        "[property: BinaryElementCount(0)] int[] A",
        "Attribute 'BinaryElementCount' has invalid data '0'. Has to be at least 1."
    )]
    [InlineData(
        "[property: BinaryElementCount(2), BinaryElementLength(0)] int[] A",
        "Attribute 'BinaryElementLength' has invalid data '0'. Has to be at least 1 and at most 4."
    )]
    public void Invalid(string argument, string diagnosticMessage)
    {
        var code = $$"""
            using Darp.BinaryObjects;

            public enum UShortEnum : ushort {};
            public partial record Object0_1({{argument}});
            """;
        (INamedTypeSymbol symbol, Compilation compilation) = GetSymbol(code, "Object0_1");
        ParserResult parsingResult = Parser.Parse(symbol, compilation);

        parsingResult.TypeName.Name.Should().Be("Object0_1");
        parsingResult.IsError.Should().BeTrue();
        parsingResult.Diagnostics.Should().HaveCount(1);
        var diagnostic0 = parsingResult.Diagnostics[0].ToDiagnostic();
        diagnostic0.Severity.Should().Be(DiagnosticSeverity.Error);
        diagnostic0.GetMessage(CultureInfo.InvariantCulture).Should().Be(diagnosticMessage);
        parsingResult.Symbols.Should().HaveCount(0);
    }

    private static (INamedTypeSymbol Type, Compilation Compilation) GetSymbol(string code, string typeName)
    {
        SyntaxTree tree = CSharpSyntaxTree.ParseText(code);

        // Get currently loaded assemblies
        PortableExecutableReference[] references = AppDomain
            .CurrentDomain.GetAssemblies()
            .Where(a => !a.IsDynamic && !string.IsNullOrEmpty(a.Location))
            .Select(a => MetadataReference.CreateFromFile(a.Location))
            .ToArray();

        var compilation = CSharpCompilation.Create(
            "TestAssembly",
            [tree],
            references.Append(MetadataReference.CreateFromFile(typeof(IBinaryObject<>).Assembly.Location)),
            new CSharpCompilationOptions(
                OutputKind.DynamicallyLinkedLibrary,
                nullableContextOptions: NullableContextOptions.Enable
            )
        );
        compilation.GetDiagnostics().Where(x => x.Severity >= DiagnosticSeverity.Warning).Should().BeEmpty();
        SemanticModel semanticModel = compilation.GetSemanticModel(tree);
        INamedTypeSymbol symbol =
            semanticModel.GetDeclaredSymbol(
                tree.GetRoot()
                    .DescendantNodes()
                    .OfType<TypeDeclarationSyntax>()
                    .First(r => r.Identifier.Text == typeName)
            ) ?? throw new ArgumentOutOfRangeException(nameof(typeName));
        if (symbol is IErrorTypeSymbol)
            throw new ArgumentOutOfRangeException(symbol.Name);
        return (symbol, compilation);
    }
}

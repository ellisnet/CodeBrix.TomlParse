using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;
using CodeBrix.TomlParse.Serialization;
using CodeBrix.TomlParse.SourceGeneration;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using SilverAssertions;
using Xunit;

namespace CodeBrix.TomlParse.Tests; //was previously: Tomlyn.Tests;

public sealed class SourceGenerationDiagnosticsTests
{
    [Fact]
    public void Generator_ResolvesKnownOptionBranchesAtBuildTime()
    {
        var source = """
            using System.Collections.Generic;
            using System.Text.Json.Serialization;
            using CodeBrix.TomlParse;
            using CodeBrix.TomlParse.Serialization;

            [TomlSourceGenerationOptions(
                PropertyNameCaseInsensitive = true,
                DefaultIgnoreCondition = TomlIgnoreCondition.Never,
                DuplicateKeyHandling = TomlDuplicateKeyHandling.LastWins,
                MappingOrder = TomlMappingOrderPolicy.Alphabetical,
                DictionaryKeyPolicy = JsonKnownNamingPolicy.CamelCase,
                PreferredObjectCreationHandling = JsonObjectCreationHandling.Populate)]
            [TomlSerializable(typeof(Person))]
            [TomlSerializable(typeof(CtorPerson))]
            internal partial class Ctx : TomlSerializerContext { }

            public sealed class Person
            {
                public string? Name { get; set; }
                public int Age { get; set; }
                public Child Child { get; } = new();
                [TomlExtensionData]
                public Dictionary<string, string>? Extra { get; set; }
            }

            public sealed class CtorPerson
            {
                [JsonConstructor]
                public CtorPerson(string? name, int age)
                {
                    Name = name;
                    Age = age;
                }

                public string? Name { get; }
                public int Age { get; }
            }

            public sealed class Child
            {
                public string? Value { get; set; }
            }
            """;

        var generatedSource = RunGeneratorTest(source).GeneratedSources.Single();

        AssertNoDynamicKnownOptionBranches(generatedSource);
    }

    [Fact]
    public void Generator_ResolvesDefaultOptionBranchesAtBuildTime()
    {
        var source = """
            using System.Collections.Generic;
            using System.Text.Json.Serialization;
            using CodeBrix.TomlParse.Serialization;

            [TomlSerializable(typeof(Person))]
            [TomlSerializable(typeof(CtorPerson))]
            internal partial class Ctx : TomlSerializerContext { }

            public sealed class Person
            {
                public string? Name { get; set; }
                public int Age { get; set; }
                public Child Child { get; } = new();
                [TomlExtensionData]
                public Dictionary<string, string>? Extra { get; set; }
            }

            public sealed class CtorPerson
            {
                [JsonConstructor]
                public CtorPerson(string? name, int age)
                {
                    Name = name;
                    Age = age;
                }

                public string? Name { get; }
                public int Age { get; }
            }

            public sealed class Child
            {
                public string? Value { get; set; }
            }
            """;

        var generatedSource = RunGeneratorTest(source).GeneratedSources.Single();

        AssertNoDynamicKnownOptionBranches(generatedSource);
    }

    private static void AssertNoDynamicKnownOptionBranches(string generatedSource)
    {
        generatedSource.Should().NotContain("Options.PropertyNameCaseInsensitive");
        generatedSource.Should().NotContain("Options.DefaultIgnoreCondition");
        generatedSource.Should().NotContain("Options.DuplicateKeyHandling");
        generatedSource.Should().NotContain("Options.MappingOrder");
        generatedSource.Should().NotContain("Options.DictionaryKeyPolicy");
        generatedSource.Should().NotContain("Options.PreferredObjectCreationHandling");
        generatedSource.Should().NotContain("if (Options.");
        generatedSource.Should().NotContain("switch (Options.");
    }

    [Fact]
    public void Generator_ReportsInvalidIndentSize()
    {
        var source = """
            using CodeBrix.TomlParse.Serialization;

            [TomlSourceGenerationOptions(IndentSize = 0)]
            [TomlSerializable(typeof(Person))]
            internal partial class Ctx : TomlSerializerContext { }

            public sealed class Person { public string Name { get; set; } = ""; }
            """;

        var diagnostics = RunGenerator(source);
        diagnostics.Any(d => d.Id == "TOMLPARSE005").Should().BeTrue();
    }

    [Fact]
    public void Generator_ReportsInvalidEnumOption()
    {
        var source = """
            using CodeBrix.TomlParse;
            using CodeBrix.TomlParse.Serialization;

            [TomlSourceGenerationOptions(NewLine = (TomlNewLineKind)42)]
            [TomlSerializable(typeof(Person))]
            internal partial class Ctx : TomlSerializerContext { }

            public sealed class Person { public string Name { get; set; } = ""; }
            """;

        var diagnostics = RunGenerator(source);
        diagnostics.Any(d => d.Id == "TOMLPARSE005").Should().BeTrue();
    }

    [Fact]
    public void Generator_ReportsInvalidMaxDepth()
    {
        var source = """
            using CodeBrix.TomlParse.Serialization;

            [TomlSourceGenerationOptions(MaxDepth = -1)]
            [TomlSerializable(typeof(Person))]
            internal partial class Ctx : TomlSerializerContext { }

            public sealed class Person { public string Name { get; set; } = ""; }
            """;

        var diagnostics = RunGenerator(source);
        diagnostics.Any(d => d.Id == "TOMLPARSE005").Should().BeTrue();
    }

    [Fact]
    public void Generator_ReportsInvalidConverterType()
    {
        var source = """
            using System;
            using CodeBrix.TomlParse.Serialization;

            public sealed class NotAConverter { public NotAConverter() { } }

            [TomlSourceGenerationOptions(Converters = new [] { typeof(NotAConverter) })]
            [TomlSerializable(typeof(Person))]
            internal partial class Ctx : TomlSerializerContext { }

            public sealed class Person { public string Name { get; set; } = ""; }
            """;

        var diagnostics = RunGenerator(source);
        diagnostics.Any(d => d.Id == "TOMLPARSE002").Should().BeTrue();
    }

    [Fact]
    public void Generator_WarnsForJsonSerializableUsage()
    {
        var source = """
            using System.Text.Json.Serialization;
            using CodeBrix.TomlParse.Serialization;

            [JsonSerializable(typeof(Person))]
            internal partial class Ctx : TomlSerializerContext { }

            public sealed class Person { public string Name { get; set; } = ""; }
            """;

        var diagnostics = RunGenerator(source);
        diagnostics.Any(d => d.Id == "TOMLPARSE008").Should().BeTrue();
    }

    [Fact]
    public void Generator_ReportsInvalidTypeInfoPropertyName()
    {
        var source = """
            using CodeBrix.TomlParse.Serialization;

            [TomlSerializable(typeof(Person), TypeInfoPropertyName = "not-valid")]
            internal partial class Ctx : TomlSerializerContext { }

            public sealed class Person { public string Name { get; set; } = ""; }
            """;

        var diagnostics = RunGenerator(source);
        (diagnostics.Any(d => d.Id == "TOMLPARSE005" && d.GetMessage().Contains("TypeInfoPropertyName", StringComparison.Ordinal))).Should().BeTrue();
    }

    [Fact]
    public void Generator_EmitsNullableObliviousLocals()
    {
        var source = """
            using System.Text.Json.Serialization;
            using CodeBrix.TomlParse.Serialization;

            public sealed class InitOptions
            {
                public string? NullableMock { get; init; }
                public string NonNullableMock { get; init; } = string.Empty;
            }

            public sealed class CtorOptions
            {
                [JsonConstructor]
                public CtorOptions(string? nullableMock, string nonNullableMock)
                {
                    NullableMock = nullableMock;
                    NonNullableMock = nonNullableMock;
                }

                public string? NullableMock { get; }
                public string NonNullableMock { get; }
            }

            [TomlSerializable(typeof(InitOptions))]
            [TomlSerializable(typeof(CtorOptions))]
            internal partial class Ctx : TomlSerializerContext { }
            """;

        var result = RunGeneratorTest(source);
        var generatedSource = string.Join(Environment.NewLine, result.GeneratedSources);

        // CodeBrix port: generated source is emitted under `#nullable disable` with no `?`
        // reference-type modifiers (upstream emitted `#nullable enable` and `string?` locals).
        // The input compilation has nullable reference types ENABLED, so this also proves an
        // NRT-enabled consumer gets no nullable warnings from generated code - RunGeneratorTest
        // fails on any warning located in a generated tree.
        result.Diagnostics.Any(d => d.Id is "CS8600" or "CS8601" or "CS8669").Should().BeFalse();
        generatedSource.Should().Contain("#nullable disable");
        generatedSource.Should().NotContain("string?");
        generatedSource.Should().Contain("string __memberValue0 = default;");
        generatedSource.Should().Contain("string __memberValue1 = default!;");
        generatedSource.Should().Contain("string __arg0 = default;");
        generatedSource.Should().Contain("string __arg1 = default!;");
    }

    [Fact]
    public void Generator_EmitsGeneratedCodeCoverageExclusionAttributes()
    {
        var source = """
            using CodeBrix.TomlParse.Serialization;

            public sealed class MetadataPayload
            {
                public string Value { get; set; } = string.Empty;
            }

            [TomlSerializable(typeof(MetadataPayload))]
            internal partial class MetadataTomlContext : TomlSerializerContext { }
            """;

        var generatedSource = RunGeneratorTest(source).GeneratedSources.Single();
        var generatedCodeAttribute = "[global::System.CodeDom.Compiler.GeneratedCode(\"CodeBrix.TomlParse.SourceGeneration\", \"" +
            (typeof(TomlSerializerContextGenerator).Assembly.GetName().Version?.ToString() ?? "0.0.0.0") +
            "\")]";
        const string excludeFromCoverageAttribute = "[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]";

        generatedSource.Should().StartWith("// <auto-generated/>");
        generatedSource.Should().Contain(generatedCodeAttribute + Environment.NewLine +
            excludeFromCoverageAttribute + Environment.NewLine +
            "partial class MetadataTomlContext");
        generatedSource.Should().Contain("    " + generatedCodeAttribute + Environment.NewLine +
            "    " + excludeFromCoverageAttribute + Environment.NewLine +
            "    private sealed class __TomlTypeInfo_MetadataPayload");
    }

    [Fact]
    public void Generator_DoesNotReferenceInternalRuntimeHelpers_FromConsumerAssembly()
    {
        var source = """
            using System.Text.Json.Serialization;
            using CodeBrix.TomlParse.Serialization;

            public sealed class Root
            {
                [JsonPropertyName("child")]
                public Child Child { get; } = new();
            }

            public sealed class Child
            {
                public string Name { get; set; } = "";
            }

            [TomlSourceGenerationOptions(PreferredObjectCreationHandling = JsonObjectCreationHandling.Populate)]
            [TomlSerializable(typeof(Root))]
            internal partial class Ctx : TomlSerializerContext { }
            """;

        var result = RunGeneratorTest(source);
        var generatedSource = string.Join(Environment.NewLine, result.GeneratedSources);

        result.Diagnostics.Any(d => d.Id == "CS0122").Should().BeFalse();
        generatedSource.Should().NotContain("TomlTableHeaderExtensionHelper");
    }

    [Fact]
    public void Generator_DoesNotReferenceInternalSingleOrArrayHelper_FromConsumerAssembly()
    {
        var source = """
            using System.Collections.Generic;
            using CodeBrix.TomlParse.Serialization;

            public sealed class ProviderConfig
            {
                [TomlSingleOrArray]
                public required IReadOnlyList<string> ApiKey { get; init; }
            }

            [TomlSerializable(typeof(ProviderConfig))]
            internal partial class Ctx : TomlSerializerContext { }
            """;

        var result = RunGeneratorTest(source);
        var generatedSource = string.Join(Environment.NewLine, result.GeneratedSources);

        result.Diagnostics.Any(d => d.Id == "CS0122").Should().BeFalse();
        generatedSource.Should().NotContain("TomlSingleOrArrayCollectionHelper");
    }

    [Fact]
    public void Generator_ReportsInvalidDerivedTypeMapping_WhenDerivedTypeIsNotAssignable()
    {
        var source = """
            using CodeBrix.TomlParse.Serialization;

            [TomlSerializable(typeof(Animal))]
            [TomlDerivedTypeMapping(typeof(Animal), typeof(NotAnimal), "cat")]
            internal partial class Ctx : TomlSerializerContext { }

            [TomlPolymorphic(TypeDiscriminatorPropertyName = "kind")]
            public abstract class Animal { }
            public sealed class NotAnimal { }
            """;

        var diagnostics = RunGenerator(source);

        diagnostics.Any(d => d.Id == "TOMLPARSE009").Should().BeTrue();
    }

    [Fact]
    public void Generator_ReportsInvalidDerivedTypeMapping_WhenDiscriminatorIsEmpty()
    {
        var source = """
            using CodeBrix.TomlParse.Serialization;

            [TomlSerializable(typeof(Animal))]
            [TomlDerivedTypeMapping(typeof(Animal), typeof(Cat), "")]
            internal partial class Ctx : TomlSerializerContext { }

            [TomlPolymorphic(TypeDiscriminatorPropertyName = "kind")]
            public abstract class Animal { }
            public sealed class Cat : Animal { }
            """;

        var diagnostics = RunGenerator(source);

        diagnostics.Any(d => d.Id == "TOMLPARSE009").Should().BeTrue();
    }

    [Fact]
    public void Generator_WarnsWhenDerivedTypeMappingBaseHasNoPolymorphicConfiguration()
    {
        var source = """
            using CodeBrix.TomlParse.Serialization;

            [TomlSerializable(typeof(Animal))]
            [TomlDerivedTypeMapping(typeof(Animal), typeof(Cat), "cat")]
            internal partial class Ctx : TomlSerializerContext { }

            public abstract class Animal { public string Name { get; set; } = ""; }
            public sealed class Cat : Animal { public int Lives { get; set; } }
            """;

        var diagnostics = RunGenerator(source);

        diagnostics.Any(d => d.Id == "TOMLPARSE010").Should().BeTrue();
    }

    private static ImmutableArray<Diagnostic> RunGenerator(string source)
        => RunGeneratorTest(source).Diagnostics;

    private static GeneratorTestResult RunGeneratorTest(string source)
    {
        var parseOptions = new CSharpParseOptions(LanguageVersion.Latest);
        var syntaxTree = CSharpSyntaxTree.ParseText(source, parseOptions);
        var compilation = CSharpCompilation.Create(
            assemblyName: "CodeBrix.TomlParse.SourceGeneration.Tests.Input",
            syntaxTrees: new[] { syntaxTree },
            references: CreateReferences(),
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));

        IIncrementalGenerator generator = new TomlSerializerContextGenerator();
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var generatorDiagnostics);

        var inputSyntaxTrees = compilation.SyntaxTrees.ToHashSet();
        var generatedSources = driver.GetRunResult()
            .Results
            .SelectMany(static result => result.GeneratedSources)
            .Select(static generated => generated.SourceText.ToString())
            .ToImmutableArray();
        var outputDiagnostics = outputCompilation.GetDiagnostics().ToImmutableArray();
        var generatedCodeWarnings = outputDiagnostics
            .Where(d => d.Severity == DiagnosticSeverity.Warning &&
                        d.Location.SourceTree is { } tree &&
                        !inputSyntaxTrees.Contains(tree))
            .ToImmutableArray();

        if (!generatedCodeWarnings.IsDefaultOrEmpty)
        {
            Assert.Fail(
                "Generated code produced compiler warnings:" + Environment.NewLine +
                string.Join(Environment.NewLine, generatedCodeWarnings.Select(static d => d.ToString())));
        }

        return new GeneratorTestResult(generatorDiagnostics.Concat(outputDiagnostics).ToImmutableArray(), generatedSources);
    }

    private readonly record struct GeneratorTestResult(ImmutableArray<Diagnostic> Diagnostics, ImmutableArray<string> GeneratedSources);

    private static IEnumerable<MetadataReference> CreateReferences()
    {
        var unique = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var references = new List<MetadataReference>();

        void Add(string path)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                return;
            }

            if (unique.Add(path))
            {
                references.Add(MetadataReference.CreateFromFile(path));
            }
        }

        // Prefer the trusted platform assemblies list to get a coherent reference set.
        var tpa = AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") as string;
        if (tpa is { Length: > 0 } tpaValue)
        {
            foreach (var path in tpaValue.Split(Path.PathSeparator))
            {
                Add(path);
            }
        }
        else
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.IsDynamic)
                {
                    continue;
                }

                Add(assembly.Location);
            }
        }

        Add(typeof(object).Assembly.Location);
        Add(typeof(Enumerable).Assembly.Location);
        Add(typeof(List<>).Assembly.Location);
        Add(typeof(JsonSerializableAttribute).Assembly.Location);
        Add(typeof(TomlSerializerContext).Assembly.Location);

        return references;
    }
}

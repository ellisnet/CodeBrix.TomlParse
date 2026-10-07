using System;
using System.IO;
using System.Text.Json.Nodes;
using CodeBrix.TomlParse.Model;
using CodeBrix.TomlParse.Parsing;
using CodeBrix.TomlParse.Syntax;
using SilverAssertions;
using Xunit;

namespace CodeBrix.TomlParse.Tests; //was previously: Tomlyn.Tests;

public sealed class Toml11TomlTestInvalidFolderTests
{
    // The toml-test corpus is copied next to the test assembly (see the .csproj).
    private const string RelativeTomlTestsDirectory = "TestFiles/toml-test";

    [Theory]
    [Trait("Category", "toml-test")]
    [MemberData(nameof(ListToml11InvalidFolderExtensions))]
    public static void Toml11Extensions_SyntaxParser_Roundtrips(string relativePath)
    {
        var (inputName, toml) = Load(relativePath);
        var doc = SyntaxParser.Parse(toml, inputName);
        var roundtrip = doc.ToString();

        if (doc.HasErrors || toml != roundtrip)
        {
            Console.WriteLine($"Testing {inputName}");
            StandardTests.Dump(toml, doc, roundtrip);
        }

        doc.HasErrors.Should().BeFalse("TOML 1.1 extension should parse without errors.");
        roundtrip.Should().Be(toml, "Syntax roundtrip should preserve input for full fidelity.");

        using var reader = new StringReader(toml);
        var docFromReader = SyntaxParser.Parse(reader, inputName);
        docFromReader.HasErrors.Should().BeFalse("TextReader input should parse without errors.");
        docFromReader.ToString().Should().Be(roundtrip, "TextReader and string syntax roundtrips must match.");
    }

    [Theory]
    [Trait("Category", "toml-test")]
    [MemberData(nameof(ListToml11InvalidFolderExtensions))]
    public static void Toml11Extensions_UntypedModel_RoundtripsSemantics(string relativePath)
    {
        var (inputName, toml) = Load(relativePath);
        var model = TomlSerializer.Deserialize<TomlTable>(toml);
        var tomlFromModel = TomlSerializer.Serialize(model);
        var model2 = TomlSerializer.Deserialize<TomlTable>(tomlFromModel);

        var json1 = ModelHelper.ToJson(model);
        var json2 = ModelHelper.ToJson(model2);
        JsonNode.DeepEquals(json1, json2).Should().BeTrue($"Untyped model must roundtrip semantics for {inputName}.");
    }

    [Fact]
    public static void MinuteOnlyOffsetDateTime_DeserializesToTomlDateTime()
    {
        var model = TomlSerializer.Deserialize<TomlTable>("no-secs = 1987-07-05T17:45Z");
        model.TryGetValue("no-secs", out var raw).Should().BeTrue();
        raw.Should().NotBeNull();
        var value = (TomlDateTime)raw;

        value.Kind.Should().Be(TomlDateTimeKind.OffsetDateTimeByZ);
        value.DateTime.UtcDateTime.Hour.Should().Be(17);
        value.DateTime.UtcDateTime.Minute.Should().Be(45);
        value.DateTime.UtcDateTime.Second.Should().Be(0);
    }

    [Fact]
    public static void MinuteOnlyLocalDateTime_DeserializesToTomlDateTime()
    {
        var model = TomlSerializer.Deserialize<TomlTable>("no-secs = 1987-07-05T17:45");
        model.TryGetValue("no-secs", out var raw).Should().BeTrue();
        raw.Should().NotBeNull();
        var value = (TomlDateTime)raw;

        value.Kind.Should().Be(TomlDateTimeKind.LocalDateTime);
        value.DateTime.DateTime.Hour.Should().Be(17);
        value.DateTime.DateTime.Minute.Should().Be(45);
        value.DateTime.DateTime.Second.Should().Be(0);
    }

    [Fact]
    public static void MinuteOnlyLocalTime_DeserializesToTomlDateTime()
    {
        var model = TomlSerializer.Deserialize<TomlTable>("no-secs = 17:45");
        model.TryGetValue("no-secs", out var raw).Should().BeTrue();
        raw.Should().NotBeNull();
        var value = (TomlDateTime)raw;

        value.Kind.Should().Be(TomlDateTimeKind.LocalTime);
        value.DateTime.DateTime.Hour.Should().Be(17);
        value.DateTime.DateTime.Minute.Should().Be(45);
        value.DateTime.DateTime.Second.Should().Be(0);
    }

    [Fact]
    public static void BasicString_HexByteEscape_Deserializes()
    {
        var model = TomlSerializer.Deserialize<TomlTable>("answer = \"\\x33\"");
        model.TryGetValue("answer", out var raw).Should().BeTrue();
        raw.Should().NotBeNull();
        ((string)raw).Should().Be("3");
    }

    [Fact]
    public static void InlineTable_TrailingComma_Deserializes()
    {
        var model = TomlSerializer.Deserialize<TomlTable>("abc = { abc = 123, }");
        model.TryGetValue("abc", out var raw).Should().BeTrue();
        raw.Should().NotBeNull();
        var inline = (TomlTable)raw;
        ((long)inline["abc"]).Should().Be(123L);
    }

    [Fact]
    public static void InlineTable_Multiline_Deserializes()
    {
        var model = TomlSerializer.Deserialize<TomlTable>("t = {a=1,\n b=2}\n");
        model.TryGetValue("t", out var raw).Should().BeTrue();
        raw.Should().NotBeNull();
        var inline = (TomlTable)raw;
        ((long)inline["a"]).Should().Be(1L);
        ((long)inline["b"]).Should().Be(2L);
    }

    public static TheoryData<string> ListToml11InvalidFolderExtensions()
    {
        Directory.Exists(TomlTestsDirectory).Should().BeTrue($"The folder `{TomlTestsDirectory}` does not exist");

        var tests = new TheoryData<string>();
        foreach (var relativePath in Toml11ValidButTomlTestMarksInvalid)
        {
            var normalized = relativePath.TrimStart('\\', '/').Replace('\\', '/');
            var file = Path.Combine(TomlTestsDirectory, normalized);
            File.Exists(file).Should().BeTrue($"The TOML test file `{file}` does not exist");
            tests.Add(normalized);
        }

        return tests;
    }

    private static (string InputName, string Toml) Load(string relativePath)
    {
        var file = Path.Combine(TomlTestsDirectory, relativePath);
        return (Path.GetFileName(file), File.ReadAllText(file));
    }

    private static readonly string[] Toml11ValidButTomlTestMarksInvalid =
    [
        @"invalid\datetime\no-secs.toml",
        @"invalid\local-datetime\no-secs.toml",
        @"invalid\local-time\no-secs.toml",
        @"invalid\string\basic-byte-escapes.toml",
        @"invalid\inline-table\trailing-comma.toml",
        @"invalid\inline-table\linebreak-01.toml",
        @"invalid\inline-table\linebreak-02.toml",
        @"invalid\inline-table\linebreak-03.toml",
        @"invalid\inline-table\linebreak-04.toml",
    ];

    private static string TomlTestsDirectory
        => Path.Combine(AppContext.BaseDirectory, RelativeTomlTestsDirectory);
}

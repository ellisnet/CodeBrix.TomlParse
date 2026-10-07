using System.Text.Json.Serialization;
using CodeBrix.TomlParse.Parsing;
using CodeBrix.TomlParse.Serialization;
using SilverAssertions;
using Xunit;

namespace CodeBrix.TomlParse.Tests; //was previously: Tomlyn.Tests;

public class NewApiExceptionLocationTests
{
    [Fact]
    public void Deserialize_InvalidScalarType_IncludesLocation()
    {
        var options = new TomlSerializerOptions
        {
            RootValueHandling = TomlRootValueHandling.WrapInRootKey,
            SourceName = "test.toml",
        };

        var ex = Assert.Throws<TomlException>(() => TomlSerializer.Deserialize<long>("value = \"abc\"\n", options));
        ex.Should().NotBeNull();
        ex.Span.HasValue.Should().BeTrue();
        ex.Line.Should().Be(1);
        ex.Column.Should().Be(9);
        ex.Message.Should().Contain("test.toml(1,9)");
    }

    [Fact]
    public void Parse_SyntaxError_IncludesLocation()
    {
        var options = new TomlSerializerOptions { SourceName = "test.toml" };

        var reader = TomlReader.Create("a =\n", options);
        reader.Read(); // StartDocument
        reader.Read(); // StartTable
        reader.Read(); // PropertyName
        var ex = Assert.Throws<TomlException>(() => reader.Read()); // missing value
        ex.Should().NotBeNull();
        ex.Span.HasValue.Should().BeTrue();
        ex.Line.Should().NotBeNull();
        ex.Line.Should().BeGreaterThan(0);
        ex.Column.Should().NotBeNull();
        ex.Column.Should().BeGreaterThan(0);
        ex.Message.Should().Contain("test.toml(");
    }

    [Fact]
    public void Parse_InvalidKeyHexEscape_IncludesLocation()
    {
        var options = new TomlSerializerOptions { SourceName = "keys.toml" };
        var reader = TomlReader.Create("\"a\\xG0\" = 1\n", options);

        var ex = Assert.Throws<TomlException>(() =>
        {
            while (reader.Read())
            {
            }
        });

        ex.Should().NotBeNull();
        ex.Span.HasValue.Should().BeTrue();
        ex.Line.Should().NotBeNull();
        ex.Line.Should().BeGreaterThan(0);
        ex.Column.Should().NotBeNull();
        ex.Column.Should().BeGreaterThan(0);
        ex.Message.Should().Contain("keys.toml(");
    }

    [Fact]
    public void Parse_InvalidDateTimeLiteral_IncludesLocation()
    {
        var options = new TomlSerializerOptions { SourceName = "dt.toml" };
        var reader = TomlReader.Create("dt = 1979-05-27T07:32:00+24:00\n", options);

        reader.Read(); // StartDocument
        reader.Read(); // StartTable
        reader.Read(); // PropertyName

        var ex = Assert.Throws<TomlException>(() => reader.Read());
        ex.Should().NotBeNull();
        ex.Span.HasValue.Should().BeTrue();
        ex.Line.Should().NotBeNull();
        ex.Line.Should().BeGreaterThan(0);
        ex.Column.Should().NotBeNull();
        ex.Column.Should().BeGreaterThan(0);
        ex.Message.Should().Contain("dt.toml(");
    }

    [Fact]
    public void Parse_InvalidUnicodeEscapeInKey_IncludesLocation()
    {
        var options = new TomlSerializerOptions { SourceName = "key.toml" };
        var reader = TomlReader.Create("\"a\\uD800\" = 1\n", options);

        var ex = Assert.Throws<TomlException>(() => reader.Read()); // StartDocument (lexer detects invalid escape)
        ex.Should().NotBeNull();
        ex.Span.HasValue.Should().BeTrue();
        ex.Line.Should().NotBeNull();
        ex.Line.Should().BeGreaterThan(0);
        ex.Column.Should().NotBeNull();
        ex.Column.Should().BeGreaterThan(0);
        ex.Message.Should().Contain("key.toml(");
    }

    [Fact]
    public void Deserialize_RootValueKeyNotFound_IncludesLocation()
    {
        var options = new TomlSerializerOptions
        {
            RootValueHandling = TomlRootValueHandling.WrapInRootKey,
            RootValueKeyName = "value",
            SourceName = "root.toml",
        };

        var ex = Assert.Throws<TomlException>(() => TomlSerializer.Deserialize<long>("other = 1\n", options));
        ex.Should().NotBeNull();
        ex.Span.HasValue.Should().BeTrue();
        ex.Line.Should().NotBeNull();
        ex.Line.Should().BeGreaterThan(0);
        ex.Column.Should().NotBeNull();
        ex.Column.Should().BeGreaterThan(0);
        ex.Message.Should().Contain("root.toml(");
    }

    public sealed class MissingRequiredModel
    {
        [JsonRequired]
        public string Name { get; set; } = string.Empty;
    }

    [Fact]
    public void Deserialize_ReflectionMissingRequiredKey_IncludesLocation()
    {
        var options = new TomlSerializerOptions { SourceName = "required.toml" };

        var ex = Assert.Throws<TomlException>(() => TomlSerializer.Deserialize<MissingRequiredModel>("other = 1\n", options));
        ex.Should().NotBeNull();
        ex.Span.HasValue.Should().BeTrue();
        ex.Line.Should().NotBeNull();
        ex.Line.Should().BeGreaterThan(0);
        ex.Column.Should().NotBeNull();
        ex.Column.Should().BeGreaterThan(0);
        ex.Message.Should().Contain("required.toml(");
    }
}

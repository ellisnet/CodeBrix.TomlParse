using CodeBrix.TomlParse.Model;
using CodeBrix.TomlParse.Parsing;
using CodeBrix.TomlParse.Serialization;
using CodeBrix.TomlParse.Syntax;
using SilverAssertions;
using Xunit;

namespace CodeBrix.TomlParse.Tests; //was previously: Tomlyn.Tests;

public sealed class Toml11ParsingTests
{
    [Fact]
    public void InlineTable_MultilineWithCommentsAndTrailingComma_IsAccepted()
    {
        var toml =
            """
            a = {
              b = 1, # comment
              c = 2,
            }
            """;

        var result = TomlSerializer.Deserialize<object>(toml);
        result.Should().BeOfType<TomlTable>();

        var table = (TomlTable)result;
        table["a"].Should().BeOfType<TomlTable>();

        var inline = (TomlTable)table["a"];
        inline["b"].Should().Be(1L);
        inline["c"].Should().Be(2L);
    }

    [Fact]
    public void LocalTime_MinuteOnly_IsAccepted()
    {
        var reader = TomlReader.Create("t = 07:32\n");
        reader.Read().Should().BeTrue(); // StartDocument
        reader.Read().Should().BeTrue(); // StartTable
        reader.Read().Should().BeTrue(); // PropertyName
        reader.PropertyName.Should().Be("t");
        reader.Read().Should().BeTrue(); // DateTime
        reader.TokenType.Should().Be(TomlTokenType.DateTime);
        var value = reader.GetTomlDateTime();
        value.Kind.Should().Be(TomlDateTimeKind.LocalTime);
    }

    [Fact]
    public void LocalDateTime_MinuteOnly_IsAccepted()
    {
        var reader = TomlReader.Create("dt = 1979-05-27T07:32\n");
        reader.Read().Should().BeTrue(); // StartDocument
        reader.Read().Should().BeTrue(); // StartTable
        reader.Read().Should().BeTrue(); // PropertyName
        reader.PropertyName.Should().Be("dt");
        reader.Read().Should().BeTrue(); // DateTime
        var value = reader.GetTomlDateTime();
        value.Kind.Should().Be(TomlDateTimeKind.LocalDateTime);
    }

    [Fact]
    public void BasicString_ByteAndEscapeEscapes_AreDecoded()
    {
        var reader = TomlReader.Create("s = \"A\\e\\x41\"\n");
        reader.Read(); // StartDocument
        reader.Read(); // StartTable
        reader.Read(); // PropertyName
        reader.Read(); // String
        reader.GetString().Should().Be("A\u001BA");
    }

    [Fact]
    public void BasicString_InvalidByteEscape_ThrowsWithLocation()
    {
        var options = new TomlSerializerOptions { RootValueHandling = TomlRootValueHandling.WrapInRootKey };
        var ex = Assert.Throws<TomlException>(() => TomlSerializer.Deserialize<string>("value = \"\\x0\"\n", options));
        ex.Should().NotBeNull();
        ex.Span.HasValue.Should().BeTrue();
        ex.Message.Should().Contain("(1,");
    }

    [Fact]
    public void TomlLexer_Default_DoesNotDecode_ButStillLexes()
    {
        var lexer = TomlLexer.Create("s = \"A\\e\\x41\"\n");
        while (lexer.MoveNext())
        {
            if (lexer.Current.Kind == TokenKind.String)
            {
                lexer.Current.StringValue.Should().BeNull();
                return;
            }
        }

        Assert.Fail("Expected a string token.");
    }

    [Fact]
    public void TomlParser_DecodeScalars_ProducesMaterializedStringValue()
    {
        var parser = TomlParser.Create(
            "s = \"A\\e\\x41\"\n",
            new CodeBrix.TomlParse.Parsing.TomlParserOptions { DecodeScalars = true });

        while (parser.MoveNext())
        {
            if (parser.Current.Kind == TomlParseEventKind.String)
            {
                parser.Current.StringValue.Should().Be("A\u001BA");
                parser.GetString().Should().Be("A\u001BA");
                return;
            }
        }

        Assert.Fail("Expected a string parse event.");
    }
}

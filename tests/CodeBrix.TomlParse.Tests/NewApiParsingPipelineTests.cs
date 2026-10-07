using System;
using System.Collections.Generic;
using CodeBrix.TomlParse.Model;
using CodeBrix.TomlParse.Parsing;
using CodeBrix.TomlParse.Serialization;
using CodeBrix.TomlParse.Syntax;
using SilverAssertions;
using Xunit;

namespace CodeBrix.TomlParse.Tests; //was previously: Tomlyn.Tests;

public class NewApiParsingPipelineTests
{
    [Fact]
    public void TomlParser_MoveNext_EmitsExpectedEvents()
    {
        var parser = TomlParser.Create(
            """
            name = "Ada"
            age = 37
            """);

        var events = new List<TomlParseEventKind>();
        var names = new List<string>();
        while (parser.MoveNext())
        {
            events.Add(parser.Current.Kind);
            if (parser.Current.Kind == TomlParseEventKind.PropertyName)
            {
                names.Add(parser.GetPropertyName());
            }
        }

        events.Should().Equal(new[]
        {
            TomlParseEventKind.StartDocument,
            TomlParseEventKind.StartTable,
            TomlParseEventKind.PropertyName,
            TomlParseEventKind.String,
            TomlParseEventKind.PropertyName,
            TomlParseEventKind.Integer,
            TomlParseEventKind.EndTable,
            TomlParseEventKind.EndDocument,
        });
        names.Should().Equal(new[] { "name", "age" });
    }

    [Fact]
    public void TomlLexer_ExposesTokensInKeyAndValueModes()
    {
        var lexer = TomlLexer.Create("age = 37\n");
        var tokens = new List<TokenKind>();

        while (lexer.MoveNext())
        {
            var token = lexer.Current;
            tokens.Add(token.Kind);

            if (token.Kind == TokenKind.Equal)
            {
                lexer.Mode = TomlLexerMode.Value;
            }
            else if (token.Kind == TokenKind.NewLine)
            {
                lexer.Mode = TomlLexerMode.Key;
            }
        }

        tokens.Should().Contain(TokenKind.BasicKey);
        tokens.Should().Contain(TokenKind.Equal);
        tokens.Should().Contain(TokenKind.Integer);
    }

    [Fact]
    public void TomlLexer_Default_DoesNotDecodeStringValues()
    {
        var lexer = TomlLexer.Create("name = \"A\\e\\x41\"\n");
        while (lexer.MoveNext())
        {
            if (lexer.Current.Kind == TokenKind.Equal)
            {
                lexer.Mode = TomlLexerMode.Value;
            }

            if (lexer.Current.Kind == TokenKind.String)
            {
                lexer.Current.StringValue.Should().BeNull();
                return;
            }
        }

        Assert.Fail("Expected to encounter a string token.");
    }

    [Fact]
    public void TomlLexer_DecodeScalars_EmitsDecodedStringValues()
    {
        var lexer = TomlLexer.Create("name = \"A\\e\\x41\"\n", new TomlLexerOptions { DecodeScalars = true });
        while (lexer.MoveNext())
        {
            if (lexer.Current.Kind == TokenKind.Equal)
            {
                lexer.Mode = TomlLexerMode.Value;
            }

            if (lexer.Current.Kind == TokenKind.String)
            {
                lexer.Current.StringValue.Should().Be("A\u001BA");
                return;
            }
        }

        Assert.Fail("Expected to encounter a string token.");
    }

    [Fact]
    public void TomlParser_TolerantMode_RecordsDiagnosticsAndTerminates()
    {
        var parser = TomlParser.Create(
            "a =\n",
            new CodeBrix.TomlParse.Parsing.TomlParserOptions { Mode = CodeBrix.TomlParse.Parsing.TomlParserMode.Tolerant },
            new TomlSerializerOptions { SourceName = "test.toml" });

        var events = new List<TomlParseEventKind>();
        Action drain = () =>
        {
            while (parser.MoveNext())
            {
                events.Add(parser.Current.Kind);
            }
        };
        drain.Should().NotThrow();

        parser.HasErrors.Should().BeTrue();
        parser.Diagnostics.HasErrors.Should().BeTrue();
        parser.Diagnostics.Count.Should().BeGreaterThan(0);
        parser.Diagnostics[0].Message.Should().Contain("Missing value");
        events.Should().Equal(new[]
        {
            TomlParseEventKind.StartDocument,
            TomlParseEventKind.StartTable,
            TomlParseEventKind.PropertyName,
            TomlParseEventKind.EndTable,
            TomlParseEventKind.EndDocument,
        });
    }

    [Fact]
    public void Deserialize_Object_ReadsNestedTablesAndArrays()
    {
        var result = TomlSerializer.Deserialize<object>(
            """
            title = "doc"
            values = [1, 2]

            [child]
            enabled = true
            """);

        result.Should().BeOfType<TomlTable>();
        var root = (TomlTable)result;
        root["title"].Should().Be("doc");
        root["values"].Should().BeOfType<TomlArray>();
        root["child"].Should().BeOfType<TomlTable>();

        var child = (TomlTable)root["child"];
        child["enabled"].Should().Be(true);
    }
}

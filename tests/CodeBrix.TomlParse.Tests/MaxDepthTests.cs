using System;
using System.IO;
using CodeBrix.TomlParse.Model;
using CodeBrix.TomlParse.Parsing;
using CodeBrix.TomlParse.Serialization;
using SilverAssertions;
using Xunit;

namespace CodeBrix.TomlParse.Tests; //was previously: Tomlyn.Tests;

public sealed class MaxDepthTests
{
    private const int DefaultMaxDepth = 64;

    [Fact]
    public void TomlSerializerOptions_NegativeMaxDepth_Throws()
    {
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            _ = TomlSerializerOptions.Default with
            {
                MaxDepth = -1,
            };
        });

        ex.ParamName.Should().Be("value");
    }

    [Fact]
    public void Deserialize_DeeplyNestedArrays_UsesDefaultMaxDepth()
    {
        var toml = CreateNestedArrayToml(DefaultMaxDepth);

        var ex = Assert.Throws<TomlException>(() => TomlSerializer.Deserialize<object>(toml));

        ex.Message.Should().Contain($"maximum depth of {DefaultMaxDepth}");
    }

    [Fact]
    public void Deserialize_DeeplyNestedArrays_AllowsCustomMaxDepth()
    {
        var toml = CreateNestedArrayToml(8);
        var options = TomlSerializerOptions.Default with { MaxDepth = 16 };

        var result = TomlSerializer.Deserialize<object>(toml, options);

        result.Should().BeOfType<TomlTable>();
        var root = (TomlTable)result;
        (GetNestedArrayDepth((TomlArray)root["value"])).Should().Be(8);
    }

    [Fact]
    public void TomlParser_MoveNext_RespectsMaxDepth()
    {
        var parser = TomlParser.Create("value = [[1]]", TomlSerializerOptions.Default with { MaxDepth = 2 });

        var ex = Assert.Throws<TomlException>(() =>
        {
            while (parser.MoveNext())
            {
            }
        });

        ex.Message.Should().Contain("maximum depth of 2");
    }

    [Fact]
    public void SyntaxParser_ParseStrict_RespectsMaxDepth()
    {
        var options = TomlSerializerOptions.Default with { MaxDepth = 2 };

        var ex = Assert.Throws<TomlException>(() => SyntaxParser.ParseStrict("value = [[1]]", options));

        ex.Message.Should().Contain("maximum depth of 2");
    }

    [Fact]
    public void TomlWriter_WriteStartArray_RespectsMaxDepth()
    {
        using var textWriter = new StringWriter();
        var writer = new TomlWriter(textWriter, TomlSerializerOptions.Default with { MaxDepth = 1 });
        writer.WriteStartDocument();
        writer.WriteStartTable();
        writer.WritePropertyName("value");

        var ex = Assert.Throws<TomlException>(() => writer.WriteStartArray());

        ex.Message.Should().Contain("maximum depth of 1");
    }

    [Fact]
    public void Serialize_TomlTableFastPath_RespectsMaxDepth()
    {
        var root = CreateNestedTableChain(depth: 3);
        var options = TomlSerializerOptions.Default with { MaxDepth = 2 };

        var ex = Assert.Throws<TomlException>(() => TomlSerializer.Serialize(root, options));

        ex.Message.Should().Contain("maximum depth of 2");
    }

    private static string CreateNestedArrayToml(int arrayDepth)
    {
        return $"value = {new string('[', arrayDepth)}1{new string(']', arrayDepth)}";
    }

    private static int GetNestedArrayDepth(TomlArray array)
    {
        var depth = 1;
        var current = array;
        while (current.Count > 0 && current[0] is TomlArray nested)
        {
            depth++;
            current = nested;
        }

        return depth;
    }

    private static TomlTable CreateNestedTableChain(int depth)
    {
        var root = new TomlTable();
        var current = root;
        for (var i = 1; i < depth; i++)
        {
            var child = new TomlTable();
            current[$"level{i}"] = child;
            current = child;
        }

        current["value"] = 1L;
        return root;
    }
}

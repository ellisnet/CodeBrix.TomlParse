using System;
using System.Text.Json.Serialization;
using SilverAssertions;
using Xunit;

namespace CodeBrix.TomlParse.Tests; //was previously: Tomlyn.Tests;

public sealed class NewApiOptionsValidationTests
{
    [Fact]
    public void RootValueKeyName_Empty_Throws()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
        {
            _ = TomlSerializerOptions.Default with
            {
                RootValueKeyName = "",
            };
        });

        ex.ParamName.Should().Be("value");
    }

    [Fact]
    public void RootValueKeyName_InvalidSurrogate_Throws()
    {
        var invalid = "\uD800";
        var ex = Assert.Throws<ArgumentException>(() =>
        {
            _ = TomlSerializerOptions.Default with
            {
                RootValueKeyName = invalid,
            };
        });

        ex.ParamName.Should().Be("value");
    }

    [Fact]
    public void RootValueWrapping_DoesNotExpandDottedRootKey()
    {
        var options = TomlSerializerOptions.Default with
        {
            RootValueHandling = TomlRootValueHandling.WrapInRootKey,
            RootValueKeyName = "a.b",
            DottedKeyHandling = TomlDottedKeyHandling.Expand,
        };

        var toml = TomlSerializer.Serialize(1, options);

        toml.Should().Contain("\"a.b\"");
        toml.Should().NotContain("[a]");

        var value = TomlSerializer.Deserialize<int>(toml, options);
        value.Should().Be(1);
    }

    [Fact]
    public void PreferredObjectCreationHandling_InvalidValue_Throws()
    {
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            _ = TomlSerializerOptions.Default with
            {
                PreferredObjectCreationHandling = (JsonObjectCreationHandling)99,
            };
        });

        ex.ParamName.Should().Be("value");
    }
}

using System.Collections.Generic;
using CodeBrix.TomlParse.Serialization;
using SilverAssertions;
using Xunit;

namespace CodeBrix.TomlParse.Tests; //was previously: Tomlyn.Tests;

public sealed class InlineTableOverrideHolder
{
    [TomlInlineTable(TomlInlineTablePolicy.Always)]
    public InlineTableOverrideChild Child { get; set; } = new() { X = 1 };

    public InlineTableOverrideChild Other { get; set; } = new() { X = 2 };
}

public sealed class InlineTableOverrideChild
{
    public int X { get; set; }
}

[TomlMappingOrder(TomlMappingOrderPolicy.Alphabetical)]
public sealed class AttributeOrderedHolder
{
    public int Z { get; set; } = 2;

    public int A { get; set; } = 1;
}

[TomlDottedKeyHandling(TomlDottedKeyHandling.Expand)]
public sealed class AttributeDottedKeyHolder
{
    [TomlPropertyName("a.b")]
    public int Value { get; set; } = 1;

    public Dictionary<string, int> Map { get; set; } = new() { ["x.y"] = 2 };
}

public sealed class AttributeStringStyleHolder
{
    [TomlStringStyle(TomlStringStyle.Basic)]
    public string FallsBackToGlobalPreferLiteral { get; set; } = "safe";

    [TomlStringStyle(TomlStringStyle.Basic, PreferLiteralWhenNoEscapes = TomlBooleanPreference.True)]
    public string OverridesPreferLiteral { get; set; } = "safe";
}

public sealed class InvalidStringStyleAttributeHolder
{
    [TomlStringStyle(TomlStringStyle.Basic)]
    public int NotString { get; set; } = 1;
}

[TomlSerializable(typeof(InlineTableOverrideHolder))]
[TomlSerializable(typeof(AttributeOrderedHolder))]
[TomlSerializable(typeof(AttributeDottedKeyHolder))]
[TomlSerializable(typeof(AttributeStringStyleHolder))]
internal partial class TestTomlStyleAttributesContext : TomlSerializerContext
{
}

public class NewApiStyleAttributeTests
{
    [Fact]
    public void PropertyInlineTableOverride_AppliesOnlyToAnnotatedProperty()
    {
        var value = new InlineTableOverrideHolder();

        var reflectionToml = TomlSerializer.Serialize(value);
        var generatedToml = TomlSerializer.Serialize(value, TestTomlStyleAttributesContext.Default.InlineTableOverrideHolder);

        reflectionToml.Should().Contain("Child = {X = 1}");
        reflectionToml.Should().Contain("[Other]");
        generatedToml.Should().Contain("Child = {X = 1}");
        generatedToml.Should().Contain("[Other]");
    }

    [Fact]
    public void TypeMappingOrderOverride_OrdersMembersAlphabetically()
    {
        var reflectionToml = TomlSerializer.Serialize(new AttributeOrderedHolder());
        var generatedToml = TomlSerializer.Serialize(new AttributeOrderedHolder(), TestTomlStyleAttributesContext.Default.AttributeOrderedHolder);

        reflectionToml.IndexOf("A = 1", System.StringComparison.Ordinal).Should().BeLessThan(reflectionToml.IndexOf("Z = 2", System.StringComparison.Ordinal));
        generatedToml.IndexOf("A = 1", System.StringComparison.Ordinal).Should().BeLessThan(generatedToml.IndexOf("Z = 2", System.StringComparison.Ordinal));
    }

    [Fact]
    public void TypeDottedKeyOverride_ExpandsMemberNamesButNotDictionaryKeys()
    {
        var reflectionToml = TomlSerializer.Serialize(new AttributeDottedKeyHolder());
        var generatedToml = TomlSerializer.Serialize(new AttributeDottedKeyHolder(), TestTomlStyleAttributesContext.Default.AttributeDottedKeyHolder);

        reflectionToml.Should().Contain("[a]");
        reflectionToml.Should().Contain("b = 1");
        reflectionToml.Should().Contain("\"x.y\" = 2");
        reflectionToml.Should().NotContain("[Map.x]");
        generatedToml.Should().Contain("[a]");
        generatedToml.Should().Contain("b = 1");
        generatedToml.Should().Contain("\"x.y\" = 2");
        generatedToml.Should().NotContain("[Map.x]");
    }

    [Fact]
    public void PropertyStringStyle_UnspecifiedPreferencesFallbackToGlobalOptions()
    {
        var options = new TomlSerializerOptions
        {
            StringStylePreferences = new TomlStringStylePreferences
            {
                PreferLiteralWhenNoEscapes = false,
            },
        };

        var toml = TomlSerializer.Serialize(new AttributeStringStyleHolder(), options);

        toml.Should().Contain("FallsBackToGlobalPreferLiteral = \"safe\"");
        toml.Should().Contain("OverridesPreferLiteral = 'safe'");
    }

    [Fact]
    public void Reflection_StringStyleAttributeRejectsNonStringMembers()
    {
        Assert.Throws<TomlException>(() => TomlSerializer.Serialize(new InvalidStringStyleAttributeHolder()));
    }
}

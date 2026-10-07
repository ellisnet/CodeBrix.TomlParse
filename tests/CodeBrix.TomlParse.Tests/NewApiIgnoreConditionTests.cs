using System.Text.Json.Serialization;
using CodeBrix.TomlParse.Model;
using CodeBrix.TomlParse.Serialization;
using SilverAssertions;
using Xunit;

namespace CodeBrix.TomlParse.Tests; //was previously: Tomlyn.Tests;

public sealed class IgnoreConditionModel
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string Optional { get; set; }

    [TomlIgnore]
    public string AlwaysIgnored { get; set; } = "ignored";

    [TomlIgnore(Condition = TomlIgnoreCondition.WhenWritingDefault)]
    public long DefaultIgnored { get; set; }
}

public sealed class DirectionalIgnoreConditionModel
{
    public int Keep { get; set; }

    [TomlIgnore(Condition = TomlIgnoreCondition.Never)]
    public int Never { get; set; }

    [TomlIgnore(Condition = TomlIgnoreCondition.WhenWriting)]
    public int TomlWriteOnly { get; set; }

    [TomlIgnore(Condition = TomlIgnoreCondition.WhenReading)]
    public int TomlReadOnly { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWriting)]
    public int JsonWriteOnly { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenReading)]
    public int JsonReadOnly { get; set; }
}

[TomlSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[TomlSerializable(typeof(IgnoreConditionModel))]
[TomlSerializable(typeof(DirectionalIgnoreConditionModel))]
internal partial class TestTomlIgnoreContext : TomlSerializerContext
{
}

public class NewApiIgnoreConditionTests
{
    [Fact]
    public void GeneratedContext_RespectsIgnoreConditions()
    {
        var context = TestTomlIgnoreContext.Default;

        var empty = new IgnoreConditionModel
        {
            Optional = null,
            AlwaysIgnored = "should_not_appear",
            DefaultIgnored = 0,
        };

        var toml = TomlSerializer.Serialize(empty, context.IgnoreConditionModel);
        toml.Should().NotContain("optional");
        toml.Should().NotContain("alwaysIgnored");
        toml.Should().NotContain("defaultIgnored");

        var populated = new IgnoreConditionModel
        {
            Optional = "x",
            AlwaysIgnored = "should_not_appear",
            DefaultIgnored = 1,
        };

        var toml2 = TomlSerializer.Serialize(populated, context.IgnoreConditionModel);
        var model = TomlSerializer.Deserialize<TomlTable>(toml2);

        model.Should().NotBeNull();
        var nonNullModel = model;
        nonNullModel.ContainsKey("optional").Should().BeTrue();
        nonNullModel.ContainsKey("defaultIgnored").Should().BeTrue();
        nonNullModel.ContainsKey("alwaysIgnored").Should().BeFalse();
    }

    [Fact]
    public void Reflection_RespectsIgnoreConditions()
    {
        var populated = new IgnoreConditionModel
        {
            Optional = "x",
            AlwaysIgnored = "should_not_appear",
            DefaultIgnored = 1,
        };

        var toml = TomlSerializer.Serialize(populated, options: TomlSerializerOptions.Default with { PropertyNamingPolicy = null });

        toml.Should().Contain("Optional");
        toml.Should().Contain("DefaultIgnored");
        toml.Should().NotContain("AlwaysIgnored");
    }

    [Fact]
    public void Deserialize_SkipsIgnoredMembers()
    {
        var context = TestTomlIgnoreContext.Default;
        var toml = """
            optional = "x"
            alwaysIgnored = "y"
            defaultIgnored = 2
            """;

        var model = TomlSerializer.Deserialize(toml, context.IgnoreConditionModel);

        model.Should().NotBeNull();
        model.Optional.Should().Be("x");
        model.DefaultIgnored.Should().Be(2);
        model.AlwaysIgnored.Should().Be("ignored");
    }

    [Fact]
    public void GeneratedContext_RespectsDirectionalIgnoreConditions()
    {
        var context = TestTomlIgnoreContext.Default;
        var toml = TomlSerializer.Serialize(new DirectionalIgnoreConditionModel
        {
            Keep = 1,
            Never = 2,
            TomlWriteOnly = 3,
            TomlReadOnly = 4,
            JsonWriteOnly = 5,
            JsonReadOnly = 6,
        }, context.DirectionalIgnoreConditionModel);

        toml.Should().Contain("keep = 1");
        toml.Should().Contain("never = 2");
        toml.Should().Contain("tomlReadOnly = 4");
        toml.Should().Contain("jsonReadOnly = 6");
        toml.Should().NotContain("tomlWriteOnly");
        toml.Should().NotContain("jsonWriteOnly");

        var deserialized = TomlSerializer.Deserialize("""
            keep = 10
            never = 20
            tomlWriteOnly = 30
            tomlReadOnly = 40
            jsonWriteOnly = 50
            jsonReadOnly = 60
            """, context.DirectionalIgnoreConditionModel);

        deserialized.Should().NotBeNull();
        deserialized.Keep.Should().Be(10);
        deserialized.Never.Should().Be(20);
        deserialized.TomlWriteOnly.Should().Be(30);
        deserialized.TomlReadOnly.Should().Be(0);
        deserialized.JsonWriteOnly.Should().Be(50);
        deserialized.JsonReadOnly.Should().Be(0);
    }

    [Fact]
    public void Reflection_RespectsDirectionalIgnoreConditions()
    {
        var toml = TomlSerializer.Serialize(new DirectionalIgnoreConditionModel
        {
            Keep = 1,
            Never = 2,
            TomlWriteOnly = 3,
            TomlReadOnly = 4,
            JsonWriteOnly = 5,
            JsonReadOnly = 6,
        });

        toml.Should().Contain("Keep = 1");
        toml.Should().Contain("Never = 2");
        toml.Should().Contain("TomlReadOnly = 4");
        toml.Should().Contain("JsonReadOnly = 6");
        toml.Should().NotContain("TomlWriteOnly");
        toml.Should().NotContain("JsonWriteOnly");

        var deserialized = TomlSerializer.Deserialize<DirectionalIgnoreConditionModel>("""
            Keep = 10
            Never = 20
            TomlWriteOnly = 30
            TomlReadOnly = 40
            JsonWriteOnly = 50
            JsonReadOnly = 60
            """);

        deserialized.Should().NotBeNull();
        deserialized.Keep.Should().Be(10);
        deserialized.Never.Should().Be(20);
        deserialized.TomlWriteOnly.Should().Be(30);
        deserialized.TomlReadOnly.Should().Be(0);
        deserialized.JsonWriteOnly.Should().Be(50);
        deserialized.JsonReadOnly.Should().Be(0);
    }
}

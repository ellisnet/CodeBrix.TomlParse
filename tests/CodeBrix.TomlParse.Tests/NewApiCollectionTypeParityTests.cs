using System.Collections.Generic;
using System.Collections.Immutable;
using CodeBrix.TomlParse.Serialization;
using SilverAssertions;
using Xunit;

namespace CodeBrix.TomlParse.Tests; //was previously: Tomlyn.Tests;

public sealed class NewApiCollectionTypeParityTests
{
    private static TomlSerializerOptions CreateRootArrayOptions()
        => new TomlSerializerOptions
        {
            RootValueHandling = TomlRootValueHandling.WrapInRootKey,
            RootValueKeyName = "value",
        };

    [Fact]
    public void Deserialize_ISet_ShouldReturnHashSet()
    {
        var options = CreateRootArrayOptions();
        var toml = """
            value = ["a", "b", "a"]
            """;

        var result = TomlSerializer.Deserialize<ISet<string>>(toml, options);

        result.Should().NotBeNull();
        result.Should().BeAssignableTo<HashSet<string>>();
        result.Count.Should().Be(2);
        result.Contains("a").Should().BeTrue();
        result.Contains("b").Should().BeTrue();
    }

    [Fact]
    public void Deserialize_HashSet_ShouldRoundTripValues()
    {
        var options = CreateRootArrayOptions();
        var toml = """
            value = [1, 2, 1]
            """;

        var result = TomlSerializer.Deserialize<HashSet<int>>(toml, options);

        result.Should().NotBeNull();
        result.Count.Should().Be(2);
        result.SetEquals(new[] { 1, 2 }).Should().BeTrue();
    }

    [Fact]
    public void Deserialize_ImmutableArray_ShouldRoundTripValues()
    {
        var options = CreateRootArrayOptions();
        var toml = """
            value = [10, 20]
            """;

        var result = TomlSerializer.Deserialize<ImmutableArray<int>>(toml, options);

        result.IsDefault.Should().BeFalse();
        result.Length.Should().Be(2);
        result[0].Should().Be(10);
        result[1].Should().Be(20);
    }

    [Fact]
    public void Deserialize_ImmutableList_ShouldRoundTripValues()
    {
        var options = CreateRootArrayOptions();
        var toml = """
            value = ["a", "b"]
            """;

        var result = TomlSerializer.Deserialize<ImmutableList<string>>(toml, options);

        result.Should().NotBeNull();
        result.Count.Should().Be(2);
        result[0].Should().Be("a");
        result[1].Should().Be("b");
    }

    [Fact]
    public void Deserialize_ImmutableHashSet_ShouldDeduplicateValues()
    {
        var options = CreateRootArrayOptions();
        var toml = """
            value = [1, 2, 1]
            """;

        var result = TomlSerializer.Deserialize<ImmutableHashSet<int>>(toml, options);

        result.Should().NotBeNull();
        result.Count.Should().Be(2);
        result.Contains(1).Should().BeTrue();
        result.Contains(2).Should().BeTrue();
    }

    [Fact]
    public void Serialize_ImmutableArray_Default_WritesEmptyArray()
    {
        var options = CreateRootArrayOptions();
        var toml = TomlSerializer.Serialize(default(ImmutableArray<int>), options);

        toml.Should().Contain("value");
        toml.Should().Contain("[]");
    }
}

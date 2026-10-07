using System;
using SilverAssertions;
using Xunit;

namespace CodeBrix.TomlParse.Tests; //was previously: Tomlyn.Tests;

public sealed class NewApiNullableValueTypeTests
{
    private sealed class NullablePayload
    {
        public int? Count { get; set; }

        public DateTimeOffset? When { get; set; }
    }

    [Fact]
    public void Deserialize_NullableValueTypes_WhenMissing_ShouldRemainNull()
    {
        var payload = TomlSerializer.Deserialize<NullablePayload>(string.Empty);

        payload.Should().NotBeNull();
        payload.Count.Should().BeNull();
        payload.When.Should().BeNull();
    }

    [Fact]
    public void Deserialize_NullableValueTypes_WhenPresent_ShouldBind()
    {
        var toml = """
            Count = 1
            When = 1979-05-27T07:32:00Z
            """;

        var payload = TomlSerializer.Deserialize<NullablePayload>(toml);

        payload.Should().NotBeNull();
        payload.Count.Should().Be(1);
        payload.When.Should().NotBeNull();
        payload.When.Value.UtcDateTime.Should().Be(new DateTime(1979, 5, 27, 7, 32, 0, DateTimeKind.Utc));
    }

    [Fact]
    public void Serialize_NullableValueTypes_WhenNull_ShouldBeOmittedByDefault()
    {
        var payload = new NullablePayload
        {
            Count = null,
            When = null,
        };

        var toml = TomlSerializer.Serialize(payload);

        toml.Should().NotContain("Count");
        toml.Should().NotContain("When");
    }
}

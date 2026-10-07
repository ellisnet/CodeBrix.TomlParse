using System;
using SilverAssertions;
using Xunit;

namespace CodeBrix.TomlParse.Tests; //was previously: Tomlyn.Tests;

public sealed class Toml11ScalarTests
{
    [Fact]
    public void Deserialize_BasicString_SupportsEscapeAndHexEscapes()
    {
        var options = new TomlSerializerOptions
        {
            RootValueHandling = TomlRootValueHandling.WrapInRootKey,
        };

        var value = TomlSerializer.Deserialize<string>("value = \"A\\e\\x41\"\n", options);
        value.Should().Be("A\u001BA");
    }

    [Fact]
    public void Deserialize_TomlDateTime_SupportsMinuteOnlyLocalTime()
    {
        var options = new TomlSerializerOptions
        {
            RootValueHandling = TomlRootValueHandling.WrapInRootKey,
        };

        var value = TomlSerializer.Deserialize<TomlDateTime>("value = 07:32\n", options);
        value.Kind.Should().Be(TomlDateTimeKind.LocalTime);
        value.DateTime.TimeOfDay.Should().Be(new TimeSpan(7, 32, 0));
    }

    [Fact]
    public void Deserialize_TomlDateTime_SupportsMinuteOnlyOffsetDateTime()
    {
        var options = new TomlSerializerOptions
        {
            RootValueHandling = TomlRootValueHandling.WrapInRootKey,
        };

        var value = TomlSerializer.Deserialize<TomlDateTime>("value = 1979-05-27T07:32Z\n", options);
        value.Kind.Should().Be(TomlDateTimeKind.OffsetDateTimeByZ);
        (value.DateTime.ToUniversalTime().Year).Should().Be(1979);
        (value.DateTime.ToUniversalTime().Month).Should().Be(5);
        (value.DateTime.ToUniversalTime().Day).Should().Be(27);
        (value.DateTime.ToUniversalTime().Hour).Should().Be(7);
        (value.DateTime.ToUniversalTime().Minute).Should().Be(32);
    }
}

using System.Collections.Generic;
using System.Text.Json.Serialization;
using SilverAssertions;
using Xunit;

namespace CodeBrix.TomlParse.Tests; //was previously: Tomlyn.Tests;

public sealed class NewApiExtensionDataTests
{
    private sealed class ExtensionDataModel
    {
        public int Known { get; set; }

        [JsonExtensionData]
        public Dictionary<string, object> Extra { get; set; } = new();
    }

    private sealed class NullableExtensionDataModel
    {
        public int Known { get; set; }

        [JsonExtensionData]
        public Dictionary<string, object> Extra { get; set; }
    }

    [Fact]
    public void Deserialize_StoresUnknownKeys_IntoExtensionData()
    {
        var model = TomlSerializer.Deserialize<ExtensionDataModel>("Known = 1\nUnknown = \"x\"\n");
        model.Should().NotBeNull();
        model.Known.Should().Be(1);
        model.Extra.ContainsKey("Unknown").Should().BeTrue();
        model.Extra["Unknown"].Should().Be("x");
    }

    [Fact]
    public void Serialize_MergesExtensionData_IntoSurroundingTable()
    {
        var model = new ExtensionDataModel
        {
            Known = 1,
            Extra = new Dictionary<string, object> { ["Unknown"] = "x" },
        };

        var toml = TomlSerializer.Serialize(model);
        toml.Should().Contain("Known");
        toml.Should().Contain("Unknown");
    }

    [Fact]
    public void Serialize_ThrowsOnExtensionDataKeyConflict()
    {
        var model = new ExtensionDataModel
        {
            Known = 1,
            Extra = new Dictionary<string, object> { ["Known"] = 2 },
        };

        Assert.Throws<TomlException>(() => TomlSerializer.Serialize(model));
    }

    [Fact]
    public void Deserialize_InitializesNullExtensionDataDictionary_WhenSettable()
    {
        var model = TomlSerializer.Deserialize<NullableExtensionDataModel>("Known = 1\nUnknown = 2\n");
        model.Should().NotBeNull();
        model.Extra.Should().NotBeNull();
        (model.Extra.ContainsKey("Unknown")).Should().BeTrue();
        model.Extra["Unknown"].Should().Be(2L);
    }
}

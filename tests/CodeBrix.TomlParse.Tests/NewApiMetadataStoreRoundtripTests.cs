using CodeBrix.TomlParse.Serialization;
using SilverAssertions;
using Xunit;

namespace CodeBrix.TomlParse.Tests; //was previously: Tomlyn.Tests;

public class NewApiMetadataStoreRoundtripTests
{
    private sealed class Sample
    {
        public int Value { get; set; }
    }

    [Fact]
    public void MetadataStore_DeserializeThenSerialize_PreservesTriviaAndHexIntegers()
    {
        var store = new TomlMetadataStore();
        var options = new TomlSerializerOptions
        {
            MetadataStore = store,
        };

        var sample = TomlSerializer.Deserialize<Sample>(
            """
            # Leading comment
            Value = 0x2A # Trailing comment
            """,
            options);

        var toml = TomlSerializer.Serialize(sample, options);

        toml.Should().Contain("# Leading comment");
        toml.Should().Contain("# Trailing comment");
        toml.Should().Contain("Value = 0x");
    }
}

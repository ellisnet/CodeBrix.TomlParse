using CodeBrix.TomlParse.Model;
using CodeBrix.TomlParse.Serialization;
using SilverAssertions;
using Xunit;

namespace CodeBrix.TomlParse.Tests; //was previously: Tomlyn.Tests;

public sealed class NewApiMetadataStoreTests
{
    private sealed class Sample
    {
    }

    [Fact]
    public void SetAndGetProperties_Works()
    {
        var store = new TomlMetadataStore();
        var instance = new Sample();
        var metadata = new TomlPropertiesMetadata();

        store.TryGetProperties(instance, out _).Should().BeFalse();

        store.SetProperties(instance, metadata);
        store.TryGetProperties(instance, out var readBack).Should().BeTrue();
        readBack.Should().BeSameAs(metadata);

        store.SetProperties(instance, metadata: null);
        store.TryGetProperties(instance, out _).Should().BeFalse();
    }

    [Fact]
    public void Deserialize_TomlTable_CapturesMetadata_WhenStoreEnabled()
    {
        var store = new TomlMetadataStore();
        var options = new TomlSerializerOptions { MetadataStore = store };

        var table = TomlSerializer.Deserialize<TomlTable>("a = 0x1 # comment\n", options);

        table.Should().NotBeNull();
        store.TryGetProperties(table, out var properties).Should().BeTrue();
        properties.Should().NotBeNull();
        (properties.TryGetProperty("a", out var propertyMetadata)).Should().BeTrue();
        propertyMetadata.Should().NotBeNull();
        propertyMetadata.DisplayKind.Should().Be(TomlPropertyDisplayKind.IntegerHexadecimal);
        propertyMetadata.Span.FileName.Should().NotBeNull();
        propertyMetadata.Span.Start.Line.Should().Be(0);
        propertyMetadata.Span.Start.Column.Should().Be(0);
        propertyMetadata.TrailingTrivia.Should().NotBeNull();
        propertyMetadata.TrailingTrivia.Count.Should().BeGreaterThan(0);
    }
}

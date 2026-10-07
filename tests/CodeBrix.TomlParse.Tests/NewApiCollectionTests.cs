using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using CodeBrix.TomlParse.Model;
using CodeBrix.TomlParse.Serialization;
using SilverAssertions;
using Xunit;

namespace CodeBrix.TomlParse.Tests; //was previously: Tomlyn.Tests;

public sealed class GeneratedCollectionHolder
{
    public List<long> Values { get; set; } = new();

    public long[] Array { get; set; } = new long[0];

    public IEnumerable<long> EnumerableValues { get; set; } = new List<long>();

    public IReadOnlyList<long> ReadOnlyValues { get; set; } = new List<long>();

    public IDictionary<string, long> Map { get; set; } = new Dictionary<string, long>();
}

public sealed class ObservableCollectionHolder
{
    public ObservableCollection<string> Global { get; set; } = new();
}

public sealed class SingleOrArrayObservableCollectionHolder
{
    [TomlSingleOrArray]
    public ObservableCollection<string> Global { get; set; } = new() { "existing" };
}

public sealed class DottedKeyDictionaryHolder
{
    public Dictionary<string, long> Map { get; set; } = new();
}

public sealed class Issue117ArrayHolder
{
    public int[] Items { get; set; }
}

public sealed class SingleOrArrayCollectionHolder
{
    public SingleOrArrayCollectionHolder()
    {
        RuntimeIdentifiers = new List<string>();
    }

    [TomlSingleOrArray]
    [JsonPropertyName("rid")]
    public List<string> RuntimeIdentifiers { get; }
}

public sealed class SingleOrArraySettableCollectionHolder
{
    [TomlSingleOrArray]
    [JsonPropertyName("rid")]
    public List<string> RuntimeIdentifiers { get; set; } = new() { "existing" };
}

public sealed class TableArrayCollectionHolder
{
    public TableArrayItem[] Foo { get; set; } = [];

    public List<TableArrayItem> Bars { get; set; } = new();

    public int[] Values { get; set; } = [];
}

public sealed class TableArrayStyleOverrideHolder
{
    [TomlTableArrayStyle(TomlTableArrayStyle.InlineArrayOfTables)]
    public TableArrayItem[] Foo { get; set; } = [];
}

public sealed class TableArrayItem
{
    public string Name { get; set; } = string.Empty;
}

[TomlSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[TomlSerializable(typeof(GeneratedCollectionHolder))]
[TomlSerializable(typeof(ObservableCollectionHolder))]
[TomlSerializable(typeof(SingleOrArrayObservableCollectionHolder))]
[TomlSerializable(typeof(DottedKeyDictionaryHolder))]
[TomlSerializable(typeof(SingleOrArrayCollectionHolder))]
[TomlSerializable(typeof(SingleOrArraySettableCollectionHolder))]
[TomlSerializable(typeof(TableArrayCollectionHolder))]
[TomlSerializable(typeof(TableArrayStyleOverrideHolder))]
internal partial class TestTomlCollectionsContext : TomlSerializerContext
{
}

[TomlSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower)]
[TomlSerializable(typeof(Issue117ArrayHolder))]
internal partial class TestTomlSnakeCaseCollectionsContext : TomlSerializerContext
{
}

public class NewApiCollectionTests
{
    [Fact]
    public void Reflection_CanRoundtripCollections()
    {
        var value = new GeneratedCollectionHolder
        {
            Values = new List<long> { 1, 2, 3 },
            Array = new long[] { 4, 5 },
            EnumerableValues = new List<long> { 6, 7 },
            ReadOnlyValues = new List<long> { 8, 9 },
            Map = new Dictionary<string, long> { ["x"] = 1, ["y"] = 2 },
        };

        var toml = TomlSerializer.Serialize(value);
        var roundtrip = TomlSerializer.Deserialize<GeneratedCollectionHolder>(toml);

        roundtrip.Should().NotBeNull();
        roundtrip.Values.Should().Equal(new[] { 1L, 2L, 3L });
        roundtrip.Array.Should().Equal(new[] { 4L, 5L });
        roundtrip.EnumerableValues.Should().Equal(new[] { 6L, 7L });
        roundtrip.ReadOnlyValues.Should().Equal(new[] { 8L, 9L });
        roundtrip.Map.Should().BeEquivalentTo(new Dictionary<string, long> { ["x"] = 1, ["y"] = 2 });
    }

    [Fact]
    public void GeneratedContext_CanRoundtripCollections()
    {
        var context = TestTomlCollectionsContext.Default;
        var value = new GeneratedCollectionHolder
        {
            Values = new List<long> { 1, 2, 3 },
            Array = new long[] { 4, 5 },
            EnumerableValues = new List<long> { 6, 7 },
            ReadOnlyValues = new List<long> { 8, 9 },
            Map = new Dictionary<string, long> { ["x"] = 1, ["y"] = 2 },
        };

        var toml = TomlSerializer.Serialize(value, context.GeneratedCollectionHolder);
        var roundtrip = TomlSerializer.Deserialize(toml, context.GeneratedCollectionHolder);

        roundtrip.Should().NotBeNull();
        roundtrip.Values.Should().Equal(new[] { 1L, 2L, 3L });
        roundtrip.Array.Should().Equal(new[] { 4L, 5L });
        roundtrip.EnumerableValues.Should().Equal(new[] { 6L, 7L });
        roundtrip.ReadOnlyValues.Should().Equal(new[] { 8L, 9L });
        roundtrip.Map.Should().BeEquivalentTo(new Dictionary<string, long> { ["x"] = 1, ["y"] = 2 });
    }

    [Fact]
    public void Reflection_CanRoundtripObservableCollection()
    {
        var value = new ObservableCollectionHolder
        {
            Global = new ObservableCollection<string> { "Hello, World!" },
        };

        var toml = TomlSerializer.Serialize(value);
        var roundtrip = TomlSerializer.Deserialize<ObservableCollectionHolder>(toml);

        roundtrip.Should().NotBeNull();
        roundtrip.Global.Should().BeAssignableTo<ObservableCollection<string>>();
        roundtrip.Global.Should().Equal(new[] { "Hello, World!" });
    }

    [Fact]
    public void GeneratedContext_CanRoundtripObservableCollection()
    {
        var context = TestTomlCollectionsContext.Default;
        var value = new ObservableCollectionHolder
        {
            Global = new ObservableCollection<string> { "Hello, World!" },
        };

        var toml = TomlSerializer.Serialize(value, context.ObservableCollectionHolder);
        var roundtrip = TomlSerializer.Deserialize(toml, context.ObservableCollectionHolder);

        roundtrip.Should().NotBeNull();
        roundtrip.Global.Should().BeAssignableTo<ObservableCollection<string>>();
        roundtrip.Global.Should().Equal(new[] { "Hello, World!" });
    }

    [Fact]
    public void Reflection_SerializesObjectCollectionsAsTableArrays()
    {
        var value = new TableArrayCollectionHolder
        {
            Foo = [new TableArrayItem { Name = "one" }, new TableArrayItem { Name = "two" }],
            Bars = new List<TableArrayItem> { new() { Name = "three" } },
            Values = [1, 2],
        };

        var toml = TomlSerializer.Serialize(value);

        toml.Should().Contain("[[Foo]]");
        toml.Should().Contain("[[Bars]]");
        toml.Should().Contain("Values = [1, 2]");
        toml.IndexOf("Values = [1, 2]", System.StringComparison.Ordinal).Should().BeLessThan(toml.IndexOf("[[Foo]]", System.StringComparison.Ordinal));
    }

    [Fact]
    public void GeneratedContext_SerializesObjectCollectionsAsTableArrays()
    {
        var context = TestTomlCollectionsContext.Default;
        var value = new TableArrayCollectionHolder
        {
            Foo = [new TableArrayItem { Name = "one" }, new TableArrayItem { Name = "two" }],
            Bars = new List<TableArrayItem> { new() { Name = "three" } },
            Values = [1, 2],
        };

        var toml = TomlSerializer.Serialize(value, context.TableArrayCollectionHolder);

        toml.Should().Contain("[[foo]]");
        toml.Should().Contain("[[bars]]");
        toml.Should().Contain("values = [1, 2]");
        toml.IndexOf("values = [1, 2]", System.StringComparison.Ordinal).Should().BeLessThan(toml.IndexOf("[[foo]]", System.StringComparison.Ordinal));
    }

    [Fact]
    public void EmptyObjectCollection_RemainsArray()
    {
        var toml = TomlSerializer.Serialize(new TableArrayCollectionHolder { Foo = [], Bars = [] });

        toml.Should().Contain("Foo = []");
        toml.Should().Contain("Bars = []");
    }

    [Fact]
    public void PropertyTableArrayStyleOverride_UsesInlineArrayOfTables()
    {
        var value = new TableArrayStyleOverrideHolder
        {
            Foo = [new TableArrayItem { Name = "one" }],
        };

        var reflectionToml = TomlSerializer.Serialize(value);
        var generatedToml = TomlSerializer.Serialize(value, TestTomlCollectionsContext.Default.TableArrayStyleOverrideHolder);

        reflectionToml.Should().Contain("Foo = [");
        reflectionToml.Should().NotContain("[[Foo]]");
        generatedToml.Should().Contain("foo = [");
        generatedToml.Should().NotContain("[[foo]]");
    }

    [Fact]
    public void GlobalInlineArrayOfTablesStyle_AppliesToTypedObjectCollections()
    {
        var options = new TomlSerializerOptions
        {
            TableArrayStyle = TomlTableArrayStyle.InlineArrayOfTables,
        };
        var value = new TableArrayCollectionHolder
        {
            Foo = [new TableArrayItem { Name = "one" }],
        };

        var toml = TomlSerializer.Serialize(value, options);

        toml.Should().Contain("Foo = [");
        toml.Should().NotContain("[[Foo]]");
    }

    [Theory]
    [InlineData(17)]
    [InlineData(33)]
    public void UntypedModel_CanReadPrimitiveArraysBeyondInitialTypedBufferCapacity(int itemCount)
    {
        var model = TomlSerializer.Deserialize<TomlTable>(CreateItemsToml(itemCount));

        model.Should().NotBeNull();
        (model.TryGetValue("items", out var rawItems)).Should().BeTrue();
        rawItems.Should().BeAssignableTo<TomlArray>();

        var items = (TomlArray)rawItems;
        items.Count.Should().Be(itemCount);
        (items.Cast<long>()).Should().Equal(GetExpectedLongItems(itemCount));
    }

    [Theory]
    [InlineData(17)]
    [InlineData(33)]
    public void Reflection_DeserializesSnakeCaseArraysBeyondInitialTypedBufferCapacity(int itemCount)
    {
        var options = new TomlSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        };

        var result = TomlSerializer.Deserialize<Issue117ArrayHolder>(CreateItemsToml(itemCount), options);

        result.Should().NotBeNull();
        result.Items.Should().Equal(GetExpectedIntItems(itemCount));
    }

    [Theory]
    [InlineData(17)]
    [InlineData(33)]
    public void GeneratedContext_DeserializesSnakeCaseArraysBeyondInitialTypedBufferCapacity(int itemCount)
    {
        var result = TomlSerializer.Deserialize<Issue117ArrayHolder>(
            CreateItemsToml(itemCount),
            TestTomlSnakeCaseCollectionsContext.Default);

        result.Should().NotBeNull();
        result.Items.Should().Equal(GetExpectedIntItems(itemCount));
    }

    [Fact]
    public void Default_DottedDictionaryKeys_RoundtripAsLiteral()
    {
        var context = TestTomlCollectionsContext.Default;
        var value = new DottedKeyDictionaryHolder
        {
            Map = new Dictionary<string, long> { ["a.b"] = 1 },
        };

        var toml = TomlSerializer.Serialize(value, context.DottedKeyDictionaryHolder);
        var model = TomlSerializer.Deserialize<TomlTable>(toml);

        model.Should().NotBeNull();
        var nonNullModel = model;
        nonNullModel.TryGetValue("map", out var mapValue).Should().BeTrue();
        mapValue.Should().BeAssignableTo<TomlTable>();
        var map = (TomlTable)mapValue;

        map.TryGetValue("a.b", out var dottedValue).Should().BeTrue();
        dottedValue.Should().Be(1L);

        var roundtrip = TomlSerializer.Deserialize(toml, context.DottedKeyDictionaryHolder);
        roundtrip.Should().NotBeNull();
        (roundtrip.Map.ContainsKey("a.b")).Should().BeTrue();
        roundtrip.Map["a.b"].Should().Be(1L);
    }

    [Fact]
    public void Reflection_GetOnlyList_WithTomlSingleOrArray_CanReadSingleValue()
    {
        var result = TomlSerializer.Deserialize<SingleOrArrayCollectionHolder>("rid = \"test\"\n");

        result.Should().NotBeNull();
        result.RuntimeIdentifiers.Should().Equal(new[] { "test" });
    }

    [Fact]
    public void Reflection_GetOnlyList_WithTomlSingleOrArray_CanReadArray()
    {
        var result = TomlSerializer.Deserialize<SingleOrArrayCollectionHolder>("rid = [\"test1\", \"test2\"]\n");

        result.Should().NotBeNull();
        result.RuntimeIdentifiers.Should().Equal(new[] { "test1", "test2" });
    }

    [Fact]
    public void Reflection_SettableList_WithTomlSingleOrArray_ReplacesOnSingleValue()
    {
        var result = TomlSerializer.Deserialize<SingleOrArraySettableCollectionHolder>("rid = \"test\"\n");

        result.Should().NotBeNull();
        result.RuntimeIdentifiers.Should().Equal(new[] { "test" });
    }

    [Fact]
    public void Reflection_WithoutTomlSingleOrArray_SingleValueStillFails()
    {
        var ex = Assert.Throws<TomlException>(() => TomlSerializer.Deserialize<GeneratedCollectionHolder>("Values = 1\n"));

        ex.Message.Should().Contain("Expected StartArray");
    }

    [Fact]
    public void GeneratedContext_GetOnlyList_WithTomlSingleOrArray_CanReadSingleValue()
    {
        var context = TestTomlCollectionsContext.Default;
        var result = TomlSerializer.Deserialize("rid = \"test\"\n", context.SingleOrArrayCollectionHolder);

        result.Should().NotBeNull();
        result.RuntimeIdentifiers.Should().Equal(new[] { "test" });
    }

    [Fact]
    public void GeneratedContext_GetOnlyList_WithTomlSingleOrArray_CanReadArray()
    {
        var context = TestTomlCollectionsContext.Default;
        var result = TomlSerializer.Deserialize("rid = [\"test1\", \"test2\"]\n", context.SingleOrArrayCollectionHolder);

        result.Should().NotBeNull();
        result.RuntimeIdentifiers.Should().Equal(new[] { "test1", "test2" });
    }

    [Fact]
    public void GeneratedContext_SettableList_WithTomlSingleOrArray_ReplacesOnSingleValue()
    {
        var context = TestTomlCollectionsContext.Default;
        var result = TomlSerializer.Deserialize("rid = \"test\"\n", context.SingleOrArraySettableCollectionHolder);

        result.Should().NotBeNull();
        result.RuntimeIdentifiers.Should().Equal(new[] { "test" });
    }

    [Fact]
    public void Reflection_ObservableCollection_WithTomlSingleOrArray_ReplacesOnSingleValue()
    {
        var result = TomlSerializer.Deserialize<SingleOrArrayObservableCollectionHolder>("Global = \"test\"\n");

        result.Should().NotBeNull();
        result.Global.Should().BeAssignableTo<ObservableCollection<string>>();
        result.Global.Should().Equal(new[] { "test" });
    }

    [Fact]
    public void GeneratedContext_ObservableCollection_WithTomlSingleOrArray_ReplacesOnSingleValue()
    {
        var context = TestTomlCollectionsContext.Default;
        var result = TomlSerializer.Deserialize("global = \"test\"\n", context.SingleOrArrayObservableCollectionHolder);

        result.Should().NotBeNull();
        result.Global.Should().BeAssignableTo<ObservableCollection<string>>();
        result.Global.Should().Equal(new[] { "test" });
    }

    private static string CreateItemsToml(int itemCount)
        => $"items = [{string.Join(",", Enumerable.Range(1, itemCount))}]";

    private static long[] GetExpectedLongItems(int itemCount)
        => Enumerable.Range(1, itemCount).Select(static value => (long)value).ToArray();

    private static int[] GetExpectedIntItems(int itemCount)
        => Enumerable.Range(1, itemCount).ToArray();
}

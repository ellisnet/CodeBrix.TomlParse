using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text.Json.Serialization;
using CodeBrix.TomlParse.Model;
using CodeBrix.TomlParse.Serialization;
using SilverAssertions;
using Xunit;

namespace CodeBrix.TomlParse.Tests; //was previously: Tomlyn.Tests;

public sealed class GeneratedPerson
{
    public string Name { get; set; } = "";

    public long Age { get; set; }
}

public sealed class GeneratedIntPerson
{
    public int Age { get; set; }
}

public sealed class GeneratedSnakePerson
{
    public string FirstName { get; set; } = "";
}

public abstract class GeneratedBaseOptions
{
    [JsonPropertyName("baseValue")]
    public string Base { get; init; }
}

public sealed class GeneratedDerivedOptions : GeneratedBaseOptions
{
    [JsonPropertyName("derivedValue")]
    public string Derived { get; init; }
}

public abstract class GeneratedOverriddenBaseOptions
{
    [JsonPropertyName("baseValue")]
    public virtual required string Base { get; init; }
}

public sealed class GeneratedOverriddenDerivedOptions : GeneratedOverriddenBaseOptions
{
    [JsonPropertyName("derivedValue")]
    public string Derived { get; init; }

    public override required string Base { get; init; }
}

public sealed class GeneratedOptionsPerson
{
    public string Name { get; set; } = "";

    public long Age { get; set; }
}

public sealed class GeneratedConvertedScalar
{
    public string Text { get; set; } = "";
}

public sealed class GeneratedConvertedScalarHolder
{
    public GeneratedConvertedScalar Value { get; set; } = new();
}

public sealed class GeneratedRuntimeConverterHolder
{
    public Guid Id { get; set; }

    public Guid? OptionalId { get; set; }
}

public sealed class GeneratedTransitiveConfig
{
    public GeneratedTransitiveBar Foo { get; set; }

    public IReadOnlyList<GeneratedTransitiveBar> Bars { get; set; } = [];

    public List<List<GeneratedTransitiveBaz>> Matrix { get; set; } = [];

    public Dictionary<string, GeneratedTransitiveBaz> BazByName { get; set; } = [];

    public object Dynamic { get; set; } = new TomlArray();

    public List<object> DynamicList { get; set; } = [];

    public Dictionary<string, object> DynamicMap { get; set; } = [];
}

public sealed class GeneratedTransitiveBar
{
    public GeneratedTransitiveBaz Baz { get; set; }
}

public sealed class GeneratedTransitiveBaz
{
    public string Value { get; set; } = "";
}

public sealed class GeneratedOrderedPerson
{
    [JsonPropertyOrder(-10)]
    public int Age { get; set; }

    [JsonPropertyOrder(10)]
    public string Name { get; set; } = "";
}

public sealed class GeneratedRequiredPerson
{
    [JsonRequired]
    public string Name { get; set; } = "";

    public int Age { get; set; }
}

public sealed class GeneratedInitOnlyPerson
{
    public string Name { get; init; } = "";

    public int Age { get; init; } = 42;
}

public sealed class GeneratedRequiredInitPerson
{
    public required string Name { get; init; } = "";

    public int Age { get; init; } = 42;
}

public sealed class GeneratedCtorInitRequiredPerson
{
    [JsonConstructor]
    public GeneratedCtorInitRequiredPerson(string name)
    {
        Name = name;
    }

    public required string Name { get; init; } = "";

    public int Age { get; init; } = 42;
}

public sealed class GeneratedExtensionDataPerson
{
    public string Name { get; set; } = "";

    [JsonExtensionData]
    public Dictionary<string, object> Extra { get; set; }
}

public sealed class GeneratedCtorPerson
{
    [JsonConstructor]
    public GeneratedCtorPerson(string firstName, int age = 42)
    {
        FirstName = firstName;
        Age = age;
    }

    [JsonPropertyName("first_name")]
    public string FirstName { get; }

    public int Age { get; }
}

public sealed class GeneratedCtorSelectionPerson
{
    public GeneratedCtorSelectionPerson()
    {
        Name = "default";
    }

    [JsonConstructor]
    public GeneratedCtorSelectionPerson(string name)
    {
        Name = name;
    }

    public string Name { get; }
}

[JsonPolymorphic(TypeDiscriminatorPropertyName = "kind")]
[JsonDerivedType(typeof(GeneratedCat), "cat")]
[JsonDerivedType(typeof(GeneratedDog), "dog")]
public interface IGeneratedAnimal
{
}

public sealed class GeneratedCat : IGeneratedAnimal
{
    public string Name { get; set; } = "";

    public int Lives { get; set; }
}

public sealed class GeneratedDog : IGeneratedAnimal
{
    public string Name { get; set; } = "";

    public bool GoodBoy { get; set; }
}

public sealed class GeneratedTwoLevelPolymorphicConfig
{
    public required GeneratedDepartLevel Depart { get; set; }
}

[TomlPolymorphic]
public abstract class GeneratedDepartLevel
{
    public required string Name { get; set; }

    public required GeneratedGroupLevel Group { get; set; }
}

public sealed class GeneratedDepart1 : GeneratedDepartLevel
{
    public required string Field1 { get; set; }
}

[TomlPolymorphic]
public abstract class GeneratedGroupLevel
{
    public required string Name { get; set; }
}

public sealed class GeneratedGroup2 : GeneratedGroupLevel
{
    public required string Field2 { get; set; }
}

public sealed class GeneratedCallbackPerson : ITomlOnSerializing, ITomlOnSerialized, ITomlOnDeserializing, ITomlOnDeserialized
{
    public string Name { get; set; } = "";

    [TomlIgnore]
    public int OnSerializingCount { get; private set; }

    [TomlIgnore]
    public int OnSerializedCount { get; private set; }

    [TomlIgnore]
    public int OnDeserializingCount { get; private set; }

    [TomlIgnore]
    public int OnDeserializedCount { get; private set; }

    [TomlIgnore]
    public bool NameWasAlreadyAssignedInOnDeserializing { get; private set; }

    [TomlIgnore]
    public string NameSeenInOnDeserialized { get; private set; }

    public void OnTomlSerializing() => OnSerializingCount++;

    public void OnTomlSerialized() => OnSerializedCount++;

    public void OnTomlDeserializing()
    {
        OnDeserializingCount++;
        NameWasAlreadyAssignedInOnDeserializing = Name == "Ada";
        Name = "from-callback";
    }

    public void OnTomlDeserialized()
    {
        OnDeserializedCount++;
        NameSeenInOnDeserialized = Name;
    }
}

public sealed class GeneratedCollectionsPayload
{
    public ImmutableArray<int> Numbers { get; set; }

    public ImmutableList<string> Names { get; set; }

    public ImmutableHashSet<int> Ids { get; set; }

    public ISet<string> Tags { get; set; }

    public HashSet<int> Values { get; set; }
}

public sealed class GeneratedNullablePayload
{
    public int? Count { get; set; }

    public DateTimeOffset? When { get; set; }
}

public sealed class GeneratedNullableReferencePayload
{
    public string NullableMock { get; init; }

    public string NonNullableMock { get; init; } = string.Empty;
}

public sealed class GeneratedIncludedInternalPropertyPayload
{
    [TomlInclude]
    internal bool MyProperty { get; set; } = true;
}

public sealed class GeneratedIncludedPrivatePropertyPayload
{
    [TomlInclude]
    private bool MyProperty { get; set; } = true;
}

public enum GeneratedEnumKind
{
    A = 0,
    B = 1,
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum GeneratedStringEnumKind
{
    LogLevel,
    Time,
    Component,
}

public sealed class GeneratedEnumAndObjectPayload
{
    public GeneratedEnumKind Kind { get; set; }

    public object Value { get; set; }
}

public sealed class GeneratedTomlObjectPayload
{
    public TomlObject Node { get; set; }
}

public sealed class GeneratedStringEnumPayload
{
    public List<GeneratedStringEnumKind> Order { get; set; } =
    [
        GeneratedStringEnumKind.LogLevel,
        GeneratedStringEnumKind.Time,
        GeneratedStringEnumKind.Component,
    ];
}

public sealed class GeneratedOptionsConverter : TomlConverter<string>
{
    public override string Read(TomlReader reader) => reader.GetString();

    public override void Write(TomlWriter writer, string value) => writer.WriteStringValue(value.ToUpperInvariant());
}

public sealed class GeneratedConvertedScalarConverter : TomlConverter<GeneratedConvertedScalar>
{
    public override GeneratedConvertedScalar Read(TomlReader reader) => new() { Text = reader.GetString() };

    public override void Write(TomlWriter writer, GeneratedConvertedScalar value) => writer.WriteStringValue(value.Text);
}

public sealed class RuntimeGuidConverter(string wireValue, Guid value) : TomlConverter<Guid>
{
    private readonly Guid _value = value;

    public override Guid Read(TomlReader reader)
    {
        reader.GetString().Should().Be(wireValue);
        return _value;
    }

    public override void Write(TomlWriter writer, Guid value)
    {
        value.Should().Be(_value);
        writer.WriteStringValue(wireValue);
    }
}

public sealed class RuntimeNullableGuidConverter(string wireValue, Guid value) : TomlConverter<Guid?>
{
    private readonly Guid _value = value;

    public override Guid? Read(TomlReader reader)
    {
        reader.GetString().Should().Be(wireValue);
        return _value;
    }

    public override void Write(TomlWriter writer, Guid? value)
    {
        value.Should().Be(_value);
        writer.WriteStringValue(wireValue);
    }
}

public sealed class CountingRuntimeGuidConverterFactory(string wireValue, Guid value) : TomlConverterFactory
{
    public int CanConvertCount { get; private set; }

    public int CreateConverterCount { get; private set; }

    public override bool CanConvert(Type typeToConvert)
    {
        CanConvertCount++;
        return typeToConvert == typeof(Guid);
    }

    public override TomlConverter CreateConverter(Type typeToConvert, TomlSerializerOptions options)
    {
        CreateConverterCount++;
        return new RuntimeGuidConverter(wireValue, value);
    }
}

public sealed class ThrowingGeneratedConverterFactory : TomlConverterFactory
{
    public override bool CanConvert(Type typeToConvert) => throw new InvalidOperationException("Source-generated converter resolution should be static.");

    public override TomlConverter CreateConverter(Type typeToConvert, TomlSerializerOptions options) => throw new InvalidOperationException("Source-generated converter resolution should be static.");
}

[TomlSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[TomlSerializable(typeof(GeneratedPerson))]
[TomlSerializable(typeof(GeneratedRuntimeConverterHolder))]
internal partial class TestTomlSerializerContext : TomlSerializerContext
{
}

[TomlSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[TomlSerializable(typeof(GeneratedPerson), TypeInfoPropertyName = "GeneratedPersonInfo")]
internal partial class TestTomlSerializerContextCustomPropertyName : TomlSerializerContext
{
}

[TomlSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[TomlSerializable(typeof(GeneratedIntPerson))]
internal partial class TestTomlSerializerContextInt : TomlSerializerContext
{
}

[TomlSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower)]
[TomlSerializable(typeof(GeneratedSnakePerson))]
internal partial class TestTomlSerializerContextSnakeCase : TomlSerializerContext
{
}

[TomlSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[TomlSerializable(typeof(GeneratedDerivedOptions))]
internal partial class TestTomlSerializerContextInheritedMembers : TomlSerializerContext
{
}

[TomlSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[TomlSerializable(typeof(GeneratedOverriddenDerivedOptions))]
internal partial class TestTomlSerializerContextOverriddenMembers : TomlSerializerContext
{
}

[TomlSourceGenerationOptions(
    WriteIndented = false,
    IndentSize = 4,
    NewLine = TomlNewLineKind.CrLf,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DictionaryKeyPolicy = JsonKnownNamingPolicy.CamelCase,
    PropertyNameCaseInsensitive = true,
    DefaultIgnoreCondition = TomlIgnoreCondition.Never,
    DuplicateKeyHandling = TomlDuplicateKeyHandling.LastWins,
    MaxDepth = 16,
    MappingOrder = TomlMappingOrderPolicy.OrderThenAlphabetical,
    DottedKeyHandling = TomlDottedKeyHandling.Expand,
    RootValueHandling = TomlRootValueHandling.WrapInRootKey,
    RootValueKeyName = "root",
    InlineTablePolicy = TomlInlineTablePolicy.WhenSmall,
    TableArrayStyle = TomlTableArrayStyle.InlineArrayOfTables,
    Converters = [typeof(GeneratedOptionsConverter)])]
[TomlSerializable(typeof(GeneratedOptionsPerson))]
internal partial class TestTomlSerializerContextWithOptions : TomlSerializerContext
{
}

[TomlSourceGenerationOptions(Converters = [typeof(ThrowingGeneratedConverterFactory), typeof(GeneratedConvertedScalarConverter)])]
[TomlSerializable(typeof(GeneratedConvertedScalarHolder))]
internal partial class TestTomlSerializerContextWithConverter : TomlSerializerContext
{
}

[TomlSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[TomlSerializable(typeof(GeneratedTransitiveConfig))]
internal partial class TestTomlSerializerContextTransitive : TomlSerializerContext
{
}

[TomlSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    MappingOrder = TomlMappingOrderPolicy.OrderThenDeclaration)]
[TomlSerializable(typeof(GeneratedOrderedPerson))]
internal partial class TestTomlSerializerContextOrdering : TomlSerializerContext
{
}

[TomlSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[TomlSerializable(typeof(GeneratedCollectionsPayload))]
internal partial class TestTomlSerializerContextCollections : TomlSerializerContext
{
}

[TomlSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[TomlSerializable(typeof(GeneratedNullablePayload))]
internal partial class TestTomlSerializerContextNullables : TomlSerializerContext
{
}

[TomlSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[TomlSerializable(typeof(GeneratedNullableReferencePayload))]
internal partial class TestTomlSerializerContextNullableReferences : TomlSerializerContext
{
}

[TomlSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[TomlSerializable(typeof(GeneratedIncludedInternalPropertyPayload))]
internal partial class TestTomlSerializerContextIncludedInternalProperty : TomlSerializerContext
{
}

[TomlSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[TomlSerializable(typeof(GeneratedIncludedPrivatePropertyPayload))]
internal partial class TestTomlSerializerContextIncludedPrivateProperty : TomlSerializerContext
{
}

[TomlSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[TomlSerializable(typeof(GeneratedEnumAndObjectPayload))]
internal partial class TestTomlSerializerContextEnumsAndObjects : TomlSerializerContext
{
}

[TomlSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[TomlSerializable(typeof(GeneratedTomlObjectPayload))]
internal partial class TestTomlSerializerContextTomlObject : TomlSerializerContext
{
}

[TomlSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[TomlSerializable(typeof(GeneratedStringEnumPayload))]
internal partial class TestTomlSerializerContextStringEnums : TomlSerializerContext
{
}

[TomlSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[TomlSerializable(typeof(GeneratedCallbackPerson))]
internal partial class TestTomlSerializerContextCallbacks : TomlSerializerContext
{
}

[TomlSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[TomlSerializable(typeof(GeneratedRequiredPerson))]
internal partial class TestTomlSerializerContextRequired : TomlSerializerContext
{
}

[TomlSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[TomlSerializable(typeof(GeneratedInitOnlyPerson))]
internal partial class TestTomlSerializerContextInitOnly : TomlSerializerContext
{
}

[TomlSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[TomlSerializable(typeof(GeneratedRequiredInitPerson))]
internal partial class TestTomlSerializerContextRequiredInit : TomlSerializerContext
{
}

[TomlSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[TomlSerializable(typeof(GeneratedCtorInitRequiredPerson))]
internal partial class TestTomlSerializerContextCtorInitRequired : TomlSerializerContext
{
}

[TomlSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DictionaryKeyPolicy = JsonKnownNamingPolicy.CamelCase)]
[TomlSerializable(typeof(GeneratedExtensionDataPerson))]
internal partial class TestTomlSerializerContextExtensionData : TomlSerializerContext
{
}

[TomlSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[TomlSerializable(typeof(GeneratedCtorPerson))]
internal partial class TestTomlSerializerContextConstructor : TomlSerializerContext
{
}

[TomlSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[TomlSerializable(typeof(GeneratedCtorSelectionPerson))]
internal partial class TestTomlSerializerContextConstructorSelection : TomlSerializerContext
{
}

[TomlSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[TomlSerializable(typeof(IGeneratedAnimal))]
internal partial class TestTomlSerializerContextPolymorphism : TomlSerializerContext
{
}

[TomlSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower)]
[TomlDerivedTypeMapping(typeof(GeneratedDepartLevel), typeof(GeneratedDepart1), "dd1")]
[TomlDerivedTypeMapping(typeof(GeneratedGroupLevel), typeof(GeneratedGroup2), "gg2")]
[TomlSerializable(typeof(GeneratedTwoLevelPolymorphicConfig))]
internal partial class TestTomlSerializerContextTwoLevelPolymorphism : TomlSerializerContext
{
}

[TomlPolymorphic(TypeDiscriminatorPropertyName = "type")]
[TomlDerivedType(typeof(GeneratedDefaultCircle))]
[TomlDerivedType(typeof(GeneratedDefaultSquare), "square")]
public abstract class GeneratedDefaultShape
{
}

public sealed class GeneratedDefaultCircle : GeneratedDefaultShape
{
    public string Color { get; set; } = "";
    public double Radius { get; set; }
}

public sealed class GeneratedDefaultSquare : GeneratedDefaultShape
{
    public string Color { get; set; } = "";
    public double Side { get; set; }
}

[TomlSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[TomlSerializable(typeof(GeneratedDefaultShape))]
internal partial class TestTomlSerializerContextDefaultDerivedType : TomlSerializerContext
{
}

[TomlPolymorphic(TypeDiscriminatorPropertyName = "kind", UnknownDerivedTypeHandling = TomlUnknownDerivedTypeHandling.FallBackToBaseType)]
[TomlDerivedType(typeof(GeneratedAttrFallbackDerived), "derived")]
public class GeneratedAttrFallbackBase
{
    public string Name { get; set; } = "";
}

public sealed class GeneratedAttrFallbackDerived : GeneratedAttrFallbackBase
{
    public int Extra { get; set; }
}

[TomlSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[TomlSerializable(typeof(GeneratedAttrFallbackBase))]
internal partial class TestTomlSerializerContextAttrFallback : TomlSerializerContext
{
}

[JsonPolymorphic(TypeDiscriminatorPropertyName = "kind", UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FallBackToBaseType)]
[JsonDerivedType(typeof(GeneratedJsonAttrFallbackDerived), "derived")]
public class GeneratedJsonAttrFallbackBase
{
    public string Name { get; set; } = "";
}

public sealed class GeneratedJsonAttrFallbackDerived : GeneratedJsonAttrFallbackBase
{
    public int Extra { get; set; }
}

[TomlSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[TomlSerializable(typeof(GeneratedJsonAttrFallbackBase))]
internal partial class TestTomlSerializerContextJsonAttrFallback : TomlSerializerContext
{
}

[TomlPolymorphic(TypeDiscriminatorPropertyName = "type")]
[TomlDerivedType(typeof(GeneratedIntDiscrimCircle), 1)]
[TomlDerivedType(typeof(GeneratedIntDiscrimSquare), 2)]
public abstract class GeneratedIntDiscrimShape
{
}

public sealed class GeneratedIntDiscrimCircle : GeneratedIntDiscrimShape
{
    public string Color { get; set; } = "";
    public double Radius { get; set; }
}

public sealed class GeneratedIntDiscrimSquare : GeneratedIntDiscrimShape
{
    public string Color { get; set; } = "";
    public double Side { get; set; }
}

[TomlSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[TomlSerializable(typeof(GeneratedIntDiscrimShape))]
internal partial class TestTomlSerializerContextIntDiscriminator : TomlSerializerContext
{
}

public class NewApiSourceGenerationTests
{
    [Fact]
    public void GeneratedContext_RespectsJsonPropertyOrder_WhenWriting()
    {
        var context = TestTomlSerializerContextOrdering.Default;
        var toml = TomlSerializer.Serialize(new GeneratedOrderedPerson { Name = "Ada", Age = 37 }, context.GeneratedOrderedPerson);

        var ageIndex = toml.IndexOf("age", StringComparison.Ordinal);
        var nameIndex = toml.IndexOf("name", StringComparison.Ordinal);
        ageIndex.Should().BeGreaterThanOrEqualTo(0);
        nameIndex.Should().BeGreaterThanOrEqualTo(0);
        ageIndex.Should().BeLessThan(nameIndex);
    }

    [Fact]
    public void GeneratedContext_CanDeserializePoco()
    {
        var context = TestTomlSerializerContext.Default;
        var toml = """
            name = "Ada"
            age = 37
            """;

        var person = TomlSerializer.Deserialize(toml, context.GeneratedPerson);

        person.Should().NotBeNull();
        person.Name.Should().Be("Ada");
        person.Age.Should().Be(37);
    }

    [Fact]
    public void GeneratedContext_CanUseCustomTypeInfoPropertyName()
    {
        var context = TestTomlSerializerContextCustomPropertyName.Default;
        var toml = """
            name = "Ada"
            age = 37
            """;

        var person = TomlSerializer.Deserialize(toml, context.GeneratedPersonInfo);

        person.Should().NotBeNull();
        person.Name.Should().Be("Ada");
        person.Age.Should().Be(37);
    }

    [Fact]
    public void GeneratedContext_CanDeserializeInitOnlyMembers()
    {
        var context = TestTomlSerializerContextInitOnly.Default;
        var toml = """
            name = "Ada"
            """;

        var person = TomlSerializer.Deserialize(toml, context.GeneratedInitOnlyPerson);

        person.Should().NotBeNull();
        person.Name.Should().Be("Ada");
        person.Age.Should().Be(42);
    }

    [Fact]
    public void GeneratedContext_CanDeserializeRequiredKeywordMembers()
    {
        var context = TestTomlSerializerContextRequiredInit.Default;
        var toml = """
            name = "Ada"
            """;

        var person = TomlSerializer.Deserialize(toml, context.GeneratedRequiredInitPerson);

        person.Should().NotBeNull();
        person.Name.Should().Be("Ada");
        person.Age.Should().Be(42);
    }

    [Fact]
    public void GeneratedContext_Throws_WhenRequiredKeywordMemberIsMissing()
    {
        var context = TestTomlSerializerContextRequiredInit.Default;
        var toml = """
            age = 37
            """;

        var exception = Assert.Throws<TomlException>(() => TomlSerializer.Deserialize(toml, context.GeneratedRequiredInitPerson));

        exception.Should().NotBeNull();
        exception.Message.Should().Contain("Missing required TOML key 'name'");
        exception.Message.Should().Contain(typeof(GeneratedRequiredInitPerson).FullName);
    }

    [Fact]
    public void GeneratedContext_Throws_WhenRequiredMemberIsMissing()
    {
        var context = TestTomlSerializerContextRequired.Default;
        var toml = """
            age = 37
            """;

        var exception = Assert.Throws<TomlException>(() => TomlSerializer.Deserialize(toml, context.GeneratedRequiredPerson));

        exception.Should().NotBeNull();
        exception.Message.Should().Contain("Missing required TOML key 'name'");
        exception.Message.Should().Contain(typeof(GeneratedRequiredPerson).FullName);
        exception.Line.Should().BeGreaterThanOrEqualTo(1);
        exception.Column.Should().BeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public void GeneratedContext_CanDeserializeConstructorBoundRequiredInitMembers()
    {
        var context = TestTomlSerializerContextCtorInitRequired.Default;
        var toml = """
            name = "Ada"
            age = 37
            """;

        var model = TomlSerializer.Deserialize(toml, context.GeneratedCtorInitRequiredPerson);

        model.Should().NotBeNull();
        model.Name.Should().Be("Ada");
        model.Age.Should().Be(37);
    }

    [Fact]
    public void GeneratedContext_PreservesConstructorDefaultsForMissingInitOnlyMembers()
    {
        var context = TestTomlSerializerContextCtorInitRequired.Default;
        var toml = """
            name = "Ada"
            """;

        var model = TomlSerializer.Deserialize(toml, context.GeneratedCtorInitRequiredPerson);

        model.Should().NotBeNull();
        model.Name.Should().Be("Ada");
        model.Age.Should().Be(42);
    }

    [Fact]
    public void GeneratedContext_CapturesJsonExtensionData_WhenReading()
    {
        var context = TestTomlSerializerContextExtensionData.Default;
        var toml = """
            name = "Ada"
            unknown = 1
            """;

        var model = TomlSerializer.Deserialize(toml, context.GeneratedExtensionDataPerson);

        model.Should().NotBeNull();
        model.Name.Should().Be("Ada");
        model.Extra.Should().NotBeNull();
        (model.Extra.ContainsKey("unknown")).Should().BeTrue();
        model.Extra["unknown"].Should().BeAssignableTo<long>();
        ((long)model.Extra["unknown"]).Should().Be(1);
    }

    [Fact]
    public void GeneratedContext_Throws_WhenExtensionDataKeyConflictsWithMemberKey_OnWrite()
    {
        var context = TestTomlSerializerContextExtensionData.Default;
        var model = new GeneratedExtensionDataPerson
        {
            Name = "Ada",
            Extra = new Dictionary<string, object> { ["name"] = "from-extra" },
        };

        var exception = Assert.Throws<TomlException>(() => TomlSerializer.Serialize(model, context.GeneratedExtensionDataPerson));

        exception.Should().NotBeNull();
        exception.Message.Should().Contain("Extension data key");
        exception.Message.Should().Contain("conflicts");
    }

    [Fact]
    public void GeneratedContext_CanDeserializeUsingJsonConstructor_AndDefaultValues()
    {
        var context = TestTomlSerializerContextConstructor.Default;
        var toml = """
            first_name = "Ada"
            """;

        var model = TomlSerializer.Deserialize(toml, context.GeneratedCtorPerson);

        model.Should().NotBeNull();
        model.FirstName.Should().Be("Ada");
        model.Age.Should().Be(42);
    }

    [Fact]
    public void GeneratedContext_Throws_WhenRequiredConstructorParameterIsMissing()
    {
        var context = TestTomlSerializerContextConstructorSelection.Default;
        var toml = """
            other = 1
            """;

        var exception = Assert.Throws<TomlException>(() => TomlSerializer.Deserialize(toml, context.GeneratedCtorSelectionPerson));

        exception.Should().NotBeNull();
        exception.Message.Should().Contain("Missing required constructor parameter 'name'");
        exception.Message.Should().Contain(typeof(GeneratedCtorSelectionPerson).FullName);
        exception.Line.Should().BeGreaterThanOrEqualTo(1);
        exception.Column.Should().BeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public void GeneratedContext_PrefersAnnotatedJsonConstructor_WhenMultipleAreAvailable()
    {
        var context = TestTomlSerializerContextConstructorSelection.Default;
        var toml = """
            name = "Ada"
            """;

        var model = TomlSerializer.Deserialize(toml, context.GeneratedCtorSelectionPerson);

        model.Should().NotBeNull();
        model.Name.Should().Be("Ada");
    }

    [Fact]
    public void GeneratedContext_CanDeserializePolymorphicInterface()
    {
        var context = TestTomlSerializerContextPolymorphism.Default;
        var toml = """
            kind = "cat"
            name = "Ada"
            lives = 9
            """;

        var model = TomlSerializer.Deserialize(toml, context.IGeneratedAnimal);

        model.Should().NotBeNull();
        model.Should().BeAssignableTo<GeneratedCat>();
        var cat = (GeneratedCat)model;
        cat.Name.Should().Be("Ada");
        cat.Lives.Should().Be(9);
    }

    [Fact]
    public void GeneratedContext_Throws_WhenPolymorphicDiscriminatorIsMissing()
    {
        var context = TestTomlSerializerContextPolymorphism.Default;
        var toml = """
            name = "Ada"
            """;

        var exception = Assert.Throws<TomlException>(() => TomlSerializer.Deserialize(toml, context.IGeneratedAnimal));

        exception.Should().NotBeNull();
        exception.Message.Should().Contain("Missing discriminator key 'kind'");
        exception.Message.Should().Contain(typeof(IGeneratedAnimal).FullName);
        exception.Line.Should().BeGreaterThanOrEqualTo(1);
        exception.Column.Should().BeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public void GeneratedContext_CanSerializePolymorphicInterface()
    {
        var context = TestTomlSerializerContextPolymorphism.Default;
        IGeneratedAnimal model = new GeneratedDog { Name = "Rex", GoodBoy = true };

        var toml = TomlSerializer.Serialize(model, context.IGeneratedAnimal);

        toml.Should().Contain("kind = \"dog\"");
        toml.Should().Contain("name = \"Rex\"");
        toml.Should().Contain("goodBoy = true");
    }

    [Fact]
    public void GeneratedContext_CanDeserializeNestedPolymorphicTables()
    {
        var context = TestTomlSerializerContextTwoLevelPolymorphism.Default;
        var toml = """
            [depart]
            "$type" = "dd1"
            name = "this is depart1"
            field1 = "depart 1 field"

            [depart.group]
            "$type" = "gg2"
            name = "this is group 2"
            field2 = "group 2 field"
            """;

        var model = TomlSerializer.Deserialize(toml, context.GeneratedTwoLevelPolymorphicConfig);

        model.Should().NotBeNull();
        model.Depart.Should().BeAssignableTo<GeneratedDepart1>();
        var depart = (GeneratedDepart1)model.Depart;
        depart.Name.Should().Be("this is depart1");
        depart.Field1.Should().Be("depart 1 field");
        depart.Group.Should().BeAssignableTo<GeneratedGroup2>();
        var group = (GeneratedGroup2)depart.Group;
        group.Name.Should().Be("this is group 2");
        group.Field2.Should().Be("group 2 field");
    }

    [Fact]
    public void GeneratedContext_CanSerializePoco()
    {
        var context = TestTomlSerializerContext.Default;
        var toml = TomlSerializer.Serialize(new GeneratedPerson { Name = "Ada", Age = 37 }, context.GeneratedPerson);
        var roundtrip = TomlSerializer.Deserialize(toml, context.GeneratedPerson);

        roundtrip.Should().NotBeNull();
        roundtrip.Name.Should().Be("Ada");
        roundtrip.Age.Should().Be(37);
    }

    [Fact]
    public void GeneratedContext_InvokesLifecycleCallbacks()
    {
        var context = TestTomlSerializerContextCallbacks.Default;
        var original = new GeneratedCallbackPerson { Name = "Ada" };
        var toml = TomlSerializer.Serialize(original, context.GeneratedCallbackPerson);

        original.OnSerializingCount.Should().Be(1);
        original.OnSerializedCount.Should().Be(1);
        original.OnDeserializingCount.Should().Be(0);
        original.OnDeserializedCount.Should().Be(0);

        var roundtrip = TomlSerializer.Deserialize(toml, context.GeneratedCallbackPerson);

        roundtrip.Should().NotBeNull();
        roundtrip.Name.Should().Be("Ada");
        roundtrip.OnDeserializingCount.Should().Be(1);
        roundtrip.OnDeserializedCount.Should().Be(1);
        roundtrip.NameWasAlreadyAssignedInOnDeserializing.Should().BeFalse();
        roundtrip.NameSeenInOnDeserialized.Should().Be("Ada");
    }

    [Fact]
    public void GeneratedContext_CanHandleIntProperties()
    {
        var context = TestTomlSerializerContextInt.Default;
        var toml = """
            age = 37
            """;

        var person = TomlSerializer.Deserialize(toml, context.GeneratedIntPerson);

        person.Should().NotBeNull();
        person.Age.Should().Be(37);
    }

    [Fact]
    public void GeneratedContext_CanApplySnakeCaseNamingPolicy()
    {
        var context = TestTomlSerializerContextSnakeCase.Default;
        var toml = """
            first_name = "Ada"
            """;

        var person = TomlSerializer.Deserialize(toml, context.GeneratedSnakePerson);

        person.Should().NotBeNull();
        person.FirstName.Should().Be("Ada");
    }

    [Fact]
    public void GeneratedContext_CanSerializeAndDeserializeInheritedProperties()
    {
        var context = TestTomlSerializerContextInheritedMembers.Default;
        var original = new GeneratedDerivedOptions
        {
            Base = "shared",
            Derived = "leaf",
        };

        var toml = TomlSerializer.Serialize(original, context);
        var roundtrip = TomlSerializer.Deserialize<GeneratedDerivedOptions>(toml, context);

        toml.Should().Contain("baseValue = \"shared\"");
        toml.Should().Contain("derivedValue = \"leaf\"");
        roundtrip.Should().NotBeNull();
        roundtrip.Base.Should().Be("shared");
        roundtrip.Derived.Should().Be("leaf");
    }

    [Fact]
    public void GeneratedContext_CanDeserializeOverriddenProperties_WithInheritedJsonPropertyName()
    {
        var context = TestTomlSerializerContextOverriddenMembers.Default;
        var toml = """
            baseValue = "shared"
            derivedValue = "leaf"
            """;

        var roundtrip = TomlSerializer.Deserialize<GeneratedOverriddenDerivedOptions>(toml, context);

        roundtrip.Should().NotBeNull();
        roundtrip.Base.Should().Be("shared");
        roundtrip.Derived.Should().Be("leaf");
    }

    [Fact]
    public void GeneratedContext_CanSerializeOverriddenProperties_WithInheritedJsonPropertyName()
    {
        var context = TestTomlSerializerContextOverriddenMembers.Default;
        var original = new GeneratedOverriddenDerivedOptions
        {
            Base = "shared",
            Derived = "leaf",
        };

        var toml = TomlSerializer.Serialize(original, context);

        toml.Should().Contain("baseValue = \"shared\"");
        toml.Should().Contain("derivedValue = \"leaf\"");
    }

    [Fact]
    public void GeneratedContext_AppliesTomlSourceGenerationOptions()
    {
        var context = TestTomlSerializerContextWithOptions.Default;
        var options = context.Options;

        options.WriteIndented.Should().BeFalse();
        options.IndentSize.Should().Be(4);
        options.NewLine.Should().Be(TomlNewLineKind.CrLf);
        options.PropertyNameCaseInsensitive.Should().BeTrue();
        options.DefaultIgnoreCondition.Should().Be(TomlIgnoreCondition.Never);
        options.DuplicateKeyHandling.Should().Be(TomlDuplicateKeyHandling.LastWins);
        options.MaxDepth.Should().Be(16);
        options.MappingOrder.Should().Be(TomlMappingOrderPolicy.OrderThenAlphabetical);
        options.DottedKeyHandling.Should().Be(TomlDottedKeyHandling.Expand);
        options.RootValueHandling.Should().Be(TomlRootValueHandling.WrapInRootKey);
        options.RootValueKeyName.Should().Be("root");
        options.InlineTablePolicy.Should().Be(TomlInlineTablePolicy.WhenSmall);
        options.TableArrayStyle.Should().Be(TomlTableArrayStyle.InlineArrayOfTables);
        options.Converters.Should().HaveCount(1);
        options.Converters[0].Should().BeAssignableTo<GeneratedOptionsConverter>();
    }

    [Fact]
    public void GeneratedContext_UsesTomlSourceGenerationOptionsConverters_ForNestedTypes()
    {
        var context = TestTomlSerializerContextWithConverter.Default;
        var toml = "Value = \"hello\"";

        var fromContext = TomlSerializer.Deserialize<GeneratedConvertedScalarHolder>(toml, context);
        var fromTypeInfo = TomlSerializer.Deserialize(toml, context.GeneratedConvertedScalarHolder);
        var fromResolverOptions = TomlSerializer.Deserialize<GeneratedConvertedScalarHolder>(
            toml,
            new TomlSerializerOptions { TypeInfoResolver = context });
        var roundtripToml = TomlSerializer.Serialize(
            new GeneratedConvertedScalarHolder { Value = new GeneratedConvertedScalar { Text = "world" } },
            context.GeneratedConvertedScalarHolder);

        fromContext.Should().NotBeNull();
        fromContext.Value.Text.Should().Be("hello");
        fromTypeInfo.Should().NotBeNull();
        fromTypeInfo.Value.Text.Should().Be("hello");
        fromResolverOptions.Should().NotBeNull();
        fromResolverOptions.Value.Text.Should().Be("hello");
        roundtripToml.Should().Contain("Value = \"world\"");
    }

    [Fact]
    public void GeneratedContext_UsesRuntimeConverters_ForGeneratedMembers()
    {
        var id = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var optionalId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var context = TestTomlSerializerContext.Default;
        var options = context.Options with
        {
            Converters =
            [
                new RuntimeGuidConverter("ref:id", id),
                new RuntimeNullableGuidConverter("ref:optional-id", optionalId),
            ],
        };

        var value = TomlSerializer.Deserialize<GeneratedRuntimeConverterHolder>(
            "id = \"ref:id\"\noptionalId = \"ref:optional-id\"",
            options);
        var toml = TomlSerializer.Serialize(
            new GeneratedRuntimeConverterHolder { Id = id, OptionalId = optionalId },
            options);

        value.Should().NotBeNull();
        value.Id.Should().Be(id);
        value.OptionalId.Should().Be(optionalId);
        toml.Should().Contain("id = \"ref:id\"");
        toml.Should().Contain("optionalId = \"ref:optional-id\"");
    }

    [Fact]
    public void GeneratedContext_ResolvesRuntimeConverters_WhenTypeInfoIsInitialized()
    {
        var id = Guid.Parse("33333333-3333-3333-3333-333333333333");
        var factory = new CountingRuntimeGuidConverterFactory("ref:id", id);
        var context = TestTomlSerializerContext.Default;
        var options = context.Options with { Converters = [factory] };

        var typeInfo = (TomlTypeInfo<GeneratedRuntimeConverterHolder>)context.GetTypeInfo(typeof(GeneratedRuntimeConverterHolder), options);
        var canConvertCount = factory.CanConvertCount;
        var createConverterCount = factory.CreateConverterCount;

        var toml = TomlSerializer.Serialize(new GeneratedRuntimeConverterHolder { Id = id }, typeInfo);
        var value = TomlSerializer.Deserialize("id = \"ref:id\"", typeInfo);

        canConvertCount.Should().BeGreaterThan(0);
        createConverterCount.Should().BeGreaterThan(0);
        factory.CanConvertCount.Should().Be(canConvertCount);
        factory.CreateConverterCount.Should().Be(createConverterCount);
        toml.Should().Contain("id = \"ref:id\"");
        value.Should().NotBeNull();
        value.Id.Should().Be(id);
    }

    [Fact]
    public void GeneratedContext_GeneratesTransitiveMemberTypes()
    {
        var context = TestTomlSerializerContextTransitive.Default;
        var value = new GeneratedTransitiveConfig
        {
            Foo = new GeneratedTransitiveBar
            {
                Baz = new GeneratedTransitiveBaz { Value = "nested" },
            },
            Bars =
            [
                new GeneratedTransitiveBar
                {
                    Baz = new GeneratedTransitiveBaz { Value = "collection" },
                },
            ],
            Matrix =
            [
                [new GeneratedTransitiveBaz { Value = "a" }],
                [new GeneratedTransitiveBaz { Value = "b" }],
            ],
            BazByName =
            {
                ["first"] = new GeneratedTransitiveBaz { Value = "dictionary" },
            },
            Dynamic = new TomlArray { "node" },
            DynamicList = [1, "two"],
            DynamicMap =
            {
                ["answer"] = 3,
                ["items"] = new TomlArray { "x", 4 },
            },
        };

        var toml = TomlSerializer.Serialize(value, context.GeneratedTransitiveConfig);
        var roundtrip = TomlSerializer.Deserialize(toml, context.GeneratedTransitiveConfig);

        (context.GetTypeInfo(typeof(GeneratedTransitiveBar), context.Options)).Should().NotBeNull();
        (context.GetTypeInfo(typeof(GeneratedTransitiveBaz), context.Options)).Should().NotBeNull();
        roundtrip.Should().NotBeNull();
        (roundtrip.Foo?.Baz?.Value).Should().Be("nested");
        (roundtrip.Bars[0].Baz?.Value).Should().Be("collection");
        roundtrip.Matrix[1][0].Value.Should().Be("b");
        roundtrip.BazByName["first"].Value.Should().Be("dictionary");
        roundtrip.Dynamic.Should().BeOfType<TomlArray>();
        ((TomlArray)roundtrip.Dynamic).Should().Equal(new TomlArray { "node" });
        roundtrip.DynamicList.Should().Equal(new object[] { 1L, "two" });
        roundtrip.DynamicMap["answer"].Should().Be(3L);
        roundtrip.DynamicMap["items"].Should().BeOfType<TomlArray>().Which.Should().Equal(new TomlArray { "x", 4L });
    }

    [Fact]
    public void GeneratedContext_CanHandleImmutableAndSetCollections()
    {
        var context = TestTomlSerializerContextCollections.Default;
        var toml = """
            numbers = [1, 2]
            names = ["a", "b"]
            ids = [1, 2, 1]
            tags = ["x", "y", "x"]
            values = [10, 20, 10]
            """;

        var payload = TomlSerializer.Deserialize(toml, context.GeneratedCollectionsPayload);

        payload.Should().NotBeNull();
        payload.Numbers.Length.Should().Be(2);
        payload.Numbers[0].Should().Be(1);
        payload.Numbers[1].Should().Be(2);
        payload.Names.Should().NotBeNull();
        payload.Names.Should().HaveCount(2);
        payload.Ids.Should().NotBeNull();
        payload.Ids.Should().HaveCount(2);
        payload.Tags.Should().NotBeNull();
        payload.Tags.Should().HaveCount(2);
        payload.Values.Should().NotBeNull();
        payload.Values.Should().HaveCount(2);

        var roundtripToml = TomlSerializer.Serialize(payload, context.GeneratedCollectionsPayload);
        var roundtrip = TomlSerializer.Deserialize(roundtripToml, context.GeneratedCollectionsPayload);

        roundtrip.Should().NotBeNull();
        roundtrip.Numbers.Should().Equal(payload.Numbers);
        roundtrip.Names.Should().Equal(payload.Names);
        roundtrip.Ids.Should().Equal(payload.Ids);
        roundtrip.Tags.Should().NotBeNull();
        (roundtrip.Tags.SetEquals(payload.Tags)).Should().BeTrue();
        roundtrip.Values.Should().NotBeNull();
        (roundtrip.Values.SetEquals(payload.Values)).Should().BeTrue();
    }

    [Fact]
    public void GeneratedContext_CanHandleNullableValueTypes()
    {
        var context = TestTomlSerializerContextNullables.Default;
        var toml = """
            count = 1
            when = 1979-05-27T07:32:00Z
            """;

        var payload = TomlSerializer.Deserialize(toml, context.GeneratedNullablePayload);

        payload.Should().NotBeNull();
        payload.Count.Should().Be(1);
        payload.When.Should().NotBeNull();

        var roundtripToml = TomlSerializer.Serialize(payload, context.GeneratedNullablePayload);
        var roundtrip = TomlSerializer.Deserialize(roundtripToml, context.GeneratedNullablePayload);

        roundtrip.Should().NotBeNull();
        roundtrip.Count.Should().Be(payload.Count);
        roundtrip.When.Should().Be(payload.When);
    }

    [Fact]
    public void GeneratedContext_CanHandleNullableReferenceTypes()
    {
        var context = TestTomlSerializerContextNullableReferences.Default;

        var emptyPayload = TomlSerializer.Deserialize("", context.GeneratedNullableReferencePayload);

        emptyPayload.Should().NotBeNull();
        emptyPayload.NullableMock.Should().BeNull();
        emptyPayload.NonNullableMock.Should().Be(string.Empty);

        var toml = """
            nullableMock = "hello"
            nonNullableMock = "world"
            """;

        var payload = TomlSerializer.Deserialize(toml, context.GeneratedNullableReferencePayload);

        payload.Should().NotBeNull();
        payload.NullableMock.Should().Be("hello");
        payload.NonNullableMock.Should().Be("world");
    }

    [Fact]
    public void GeneratedContext_IncludesInternalProperty_WithTomlInclude()
    {
        var context = TestTomlSerializerContextIncludedInternalProperty.Default;

        var toml = TomlSerializer.Serialize(new GeneratedIncludedInternalPropertyPayload(), context.GeneratedIncludedInternalPropertyPayload);
        var roundtrip = TomlSerializer.Deserialize("myProperty = false\n", context.GeneratedIncludedInternalPropertyPayload);

        toml.Should().Contain("myProperty = true");
        roundtrip.Should().NotBeNull();
        roundtrip.MyProperty.Should().BeFalse();
    }

    [Fact]
    public void GeneratedContext_IncludesPrivateProperty_WithTomlInclude_WhenWriting()
    {
        var context = TestTomlSerializerContextIncludedPrivateProperty.Default;

        var toml = TomlSerializer.Serialize(new GeneratedIncludedPrivatePropertyPayload(), context.GeneratedIncludedPrivatePropertyPayload);

        toml.Should().Contain("myProperty = true");
    }

    [Fact]
    public void GeneratedContext_CanHandleEnumsAndUntypedObjects()
    {
        var context = TestTomlSerializerContextEnumsAndObjects.Default;
        var toml = """
            kind = "a"
            value = 1
            """;

        var payload = TomlSerializer.Deserialize(toml, context.GeneratedEnumAndObjectPayload);

        payload.Should().NotBeNull();
        payload.Kind.Should().Be(GeneratedEnumKind.A);
        payload.Value.Should().BeAssignableTo<long>();
        ((long)payload.Value).Should().Be(1);

        var roundtripToml = TomlSerializer.Serialize(payload, context.GeneratedEnumAndObjectPayload);
        var roundtrip = TomlSerializer.Deserialize(roundtripToml, context.GeneratedEnumAndObjectPayload);

        roundtrip.Should().NotBeNull();
        roundtrip.Kind.Should().Be(payload.Kind);
        roundtrip.Value.Should().Be(payload.Value);
    }

    [Fact]
    public void GeneratedContext_CanHandleTomlObjectProperties()
    {
        var context = TestTomlSerializerContextTomlObject.Default;
        var toml = """
            [node]
            answer = 42
            """;

        var payload = TomlSerializer.Deserialize(toml, context.GeneratedTomlObjectPayload);

        payload.Should().NotBeNull();
        payload.Node.Should().BeAssignableTo<TomlTable>();
        (((TomlTable)payload.Node)["answer"]).Should().Be(42L);

        payload.Node = new TomlArray { "x", 2L };
        var roundtripToml = TomlSerializer.Serialize(payload, context.GeneratedTomlObjectPayload);
        var roundtrip = TomlSerializer.Deserialize(roundtripToml, context.GeneratedTomlObjectPayload);

        roundtripToml.Should().Contain("node = [\"x\", 2]");
        roundtrip.Should().NotBeNull();
        roundtrip.Node.Should().BeAssignableTo<TomlArray>();
        ((TomlArray)roundtrip.Node).Should().Equal(new TomlArray { "x", 2L });
    }

    [Fact]
    public void GeneratedContext_EnumWithJsonStringEnumConverter_UsesStrings()
    {
        var context = TestTomlSerializerContextStringEnums.Default;

        var toml = TomlSerializer.Serialize(new GeneratedStringEnumPayload(), context.GeneratedStringEnumPayload);
        var roundtrip = TomlSerializer.Deserialize(toml, context.GeneratedStringEnumPayload);

        toml.Should().Contain("order = [\"LogLevel\", \"Time\", \"Component\"]");
        roundtrip.Should().NotBeNull();
        roundtrip.Order.Should().Equal(new[]
        {
            GeneratedStringEnumKind.LogLevel,
            GeneratedStringEnumKind.Time,
            GeneratedStringEnumKind.Component,
        });
    }

    // --- Feature 1: Default Derived Type (source-gen) ---

    [Fact]
    public void GeneratedContext_DefaultDerivedType_Serialize_OmitsDiscriminator()
    {
        var context = TestTomlSerializerContextDefaultDerivedType.Default;
        GeneratedDefaultShape model = new GeneratedDefaultCircle { Color = "red", Radius = 5.0 };
        var toml = TomlSerializer.Serialize(model, context.GeneratedDefaultShape);

        toml.Should().NotContain("type");
        toml.Should().Contain("color = \"red\"");
        toml.Should().Contain("radius = 5");
    }

    [Fact]
    public void GeneratedContext_DefaultDerivedType_Serialize_WritesDiscriminator_ForNonDefault()
    {
        var context = TestTomlSerializerContextDefaultDerivedType.Default;
        GeneratedDefaultShape model = new GeneratedDefaultSquare { Color = "blue", Side = 3.0 };
        var toml = TomlSerializer.Serialize(model, context.GeneratedDefaultShape);

        toml.Should().Contain("type = \"square\"");
        toml.Should().Contain("color = \"blue\"");
        toml.Should().Contain("side = 3");
    }

    [Fact]
    public void GeneratedContext_DefaultDerivedType_Deserialize_MissingDiscriminator()
    {
        var context = TestTomlSerializerContextDefaultDerivedType.Default;
        var toml = """
            color = "red"
            radius = 5.0
            """;

        var result = TomlSerializer.Deserialize(toml, context.GeneratedDefaultShape);

        result.Should().BeOfType<GeneratedDefaultCircle>();
        var circle = (GeneratedDefaultCircle)result;
        circle.Color.Should().Be("red");
        circle.Radius.Should().Be(5.0);
    }

    [Fact]
    public void GeneratedContext_DefaultDerivedType_Deserialize_UnknownDiscriminator()
    {
        var context = TestTomlSerializerContextDefaultDerivedType.Default;
        var toml = """
            type = "triangle"
            color = "green"
            """;

        var result = TomlSerializer.Deserialize(toml, context.GeneratedDefaultShape);

        result.Should().BeOfType<GeneratedDefaultCircle>();
        (((GeneratedDefaultCircle)result).Color).Should().Be("green");
    }

    [Fact]
    public void GeneratedContext_DefaultDerivedType_Roundtrip()
    {
        var context = TestTomlSerializerContextDefaultDerivedType.Default;
        GeneratedDefaultShape original = new GeneratedDefaultCircle { Color = "red", Radius = 5.0 };
        var toml = TomlSerializer.Serialize(original, context.GeneratedDefaultShape);
        var result = TomlSerializer.Deserialize(toml, context.GeneratedDefaultShape);

        result.Should().BeOfType<GeneratedDefaultCircle>();
        var circle = (GeneratedDefaultCircle)result;
        circle.Color.Should().Be("red");
        circle.Radius.Should().Be(5.0);
    }

    // --- Feature 2: UnknownDerivedTypeHandling on attribute (source-gen) ---

    [Fact]
    public void GeneratedContext_UnknownDiscriminator_FallbackViaTomlAttribute()
    {
        var context = TestTomlSerializerContextAttrFallback.Default;
        var toml = """
            kind = "unknown"
            name = "test"
            """;

        var result = TomlSerializer.Deserialize(toml, context.GeneratedAttrFallbackBase);

        result.Should().BeOfType<GeneratedAttrFallbackBase>();
        result.Name.Should().Be("test");
    }

    [Fact]
    public void GeneratedContext_UnknownDiscriminator_FallbackViaJsonAttribute()
    {
        var context = TestTomlSerializerContextJsonAttrFallback.Default;
        var toml = """
            kind = "unknown"
            name = "test"
            """;

        var result = TomlSerializer.Deserialize(toml, context.GeneratedJsonAttrFallbackBase);

        result.Should().BeOfType<GeneratedJsonAttrFallbackBase>();
        result.Name.Should().Be("test");
    }

    // --- Feature 3: Integer Discriminators (source-gen) ---

    [Fact]
    public void GeneratedContext_IntDiscriminator_Serialize()
    {
        var context = TestTomlSerializerContextIntDiscriminator.Default;
        GeneratedIntDiscrimShape model = new GeneratedIntDiscrimCircle { Color = "red", Radius = 5.0 };
        var toml = TomlSerializer.Serialize(model, context.GeneratedIntDiscrimShape);

        toml.Should().Contain("type = \"1\"");
        toml.Should().Contain("color = \"red\"");
        toml.Should().Contain("radius = 5");
    }

    [Fact]
    public void GeneratedContext_IntDiscriminator_Deserialize()
    {
        var context = TestTomlSerializerContextIntDiscriminator.Default;
        var toml = """
            type = "2"
            color = "blue"
            side = 3.0
            """;

        var result = TomlSerializer.Deserialize(toml, context.GeneratedIntDiscrimShape);

        result.Should().BeOfType<GeneratedIntDiscrimSquare>();
        var square = (GeneratedIntDiscrimSquare)result;
        square.Color.Should().Be("blue");
        square.Side.Should().Be(3.0);
    }

    [Fact]
    public void GeneratedContext_IntDiscriminator_Roundtrip()
    {
        var context = TestTomlSerializerContextIntDiscriminator.Default;
        GeneratedIntDiscrimShape original = new GeneratedIntDiscrimCircle { Color = "red", Radius = 5.0 };
        var toml = TomlSerializer.Serialize(original, context.GeneratedIntDiscrimShape);
        var result = TomlSerializer.Deserialize(toml, context.GeneratedIntDiscrimShape);

        result.Should().BeOfType<GeneratedIntDiscrimCircle>();
        var circle = (GeneratedIntDiscrimCircle)result;
        circle.Color.Should().Be("red");
        circle.Radius.Should().Be(5.0);
    }
}

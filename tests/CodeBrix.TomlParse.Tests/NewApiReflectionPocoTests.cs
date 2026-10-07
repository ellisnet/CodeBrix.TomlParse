using System.Collections.Generic;
using System.Text.Json.Serialization;
using CodeBrix.TomlParse.Model;
using CodeBrix.TomlParse.Serialization;
using SilverAssertions;
using Xunit;

namespace CodeBrix.TomlParse.Tests; //was previously: Tomlyn.Tests;

public class NewApiReflectionPocoTests
{
    private sealed class CallbackModel : ITomlOnSerializing, ITomlOnSerialized, ITomlOnDeserializing, ITomlOnDeserialized
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

            // If this callback runs before TOML member mapping, this should be overwritten by TOML data.
            Name = "from-callback";
        }

        public void OnTomlDeserialized()
        {
            OnDeserializedCount++;
            NameSeenInOnDeserialized = Name;
        }
    }

    private sealed class Person
    {
        public string Name { get; set; } = "";

        public long Age { get; set; }
    }

    private sealed class JsonNamedPerson
    {
        [JsonPropertyName("first_name")]
        public string Name { get; set; } = "";

        public long Age { get; set; }
    }

    private abstract class JsonNamedBaseOptions
    {
        [JsonPropertyName("baseValue")]
        public string Base { get; init; }
    }

    private sealed class JsonNamedDerivedOptions : JsonNamedBaseOptions
    {
        [JsonPropertyName("derivedValue")]
        public string Derived { get; init; }
    }

    private abstract class JsonNamedOverriddenBaseOptions
    {
        [JsonPropertyName("baseValue")]
        public virtual string Base { get; init; }
    }

    private sealed class JsonNamedOverriddenDerivedOptions : JsonNamedOverriddenBaseOptions
    {
        [JsonPropertyName("derivedValue")]
        public string Derived { get; init; }

        public override string Base { get; init; }
    }

    private sealed class PrivateSetterModel
    {
        [JsonInclude]
        public int Value { get; private set; }
    }

    private sealed class IncludedFieldModel
    {
        [JsonInclude]
        public int Value;
    }

    private sealed class IncludedNonPublicPropertyModel
    {
        [TomlInclude]
        private bool MyProperty { get; set; } = true;

        public bool GetMyProperty() => MyProperty;
    }

    private sealed class StringEnumModel
    {
        public List<StringEnumValue> Values { get; set; } = new();
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    private enum StringEnumValue
    {
        First,
        Second,
    }

    private sealed class ReadOnlyPropertyModel
    {
        public int Value { get; } = 42;
    }

    private sealed class JsonConstructorModel
    {
        public JsonConstructorModel(int value)
        {
            Value = value;
        }

        public int Value { get; }
    }

    private sealed class AnnotatedJsonConstructorModel
    {
        public AnnotatedJsonConstructorModel()
        {
            Value = -1;
        }

        [JsonConstructor]
        public AnnotatedJsonConstructorModel(int value)
        {
            Value = value;
        }

        public int Value { get; }
    }

    private sealed class SingleCtorModel
    {
        public SingleCtorModel(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }

    private sealed class RequiredModel
    {
        [JsonRequired]
        public int Value { get; set; }
    }

    private sealed class EmptyModel
    {
    }

    private sealed class TomlObjectModel
    {
        public TomlObject Value { get; set; }
    }

    private sealed class MultipleAnnotatedConstructorsModel
    {
        [JsonConstructor]
        public MultipleAnnotatedConstructorsModel(int value)
        {
            Value = value;
        }

        [TomlConstructor]
        public MultipleAnnotatedConstructorsModel(string value)
        {
            Value = int.Parse(value);
        }

        public int Value { get; }
    }

    private sealed class AmbiguousConstructorsModel
    {
        public AmbiguousConstructorsModel(int value)
        {
            Value = value;
        }

        public AmbiguousConstructorsModel(string value)
        {
            Value = int.Parse(value);
        }

        public int Value { get; }
    }

    [Fact]
    public void SerializeDeserialize_Poco_UsesReflectionFallback()
    {
        var original = new Person { Name = "Ada", Age = 37 };
        var toml = TomlSerializer.Serialize(original);
        var roundtrip = TomlSerializer.Deserialize<Person>(toml);

        roundtrip.Should().NotBeNull();
        roundtrip.Name.Should().Be("Ada");
        roundtrip.Age.Should().Be(37);
    }

    [Fact]
    public void Deserialize_Poco_RespectsJsonPropertyName()
    {
        var toml = """
            first_name = "Ada"
            Age = 37
            """;

        var person = TomlSerializer.Deserialize<JsonNamedPerson>(toml);

        person.Should().NotBeNull();
        person.Name.Should().Be("Ada");
        person.Age.Should().Be(37);
    }

    [Fact]
    public void SerializeDeserialize_InheritedProperties_RespectJsonPropertyName()
    {
        var original = new JsonNamedDerivedOptions
        {
            Base = "shared",
            Derived = "leaf",
        };

        var toml = TomlSerializer.Serialize(original);
        var roundtrip = TomlSerializer.Deserialize<JsonNamedDerivedOptions>(toml);

        toml.Should().Contain("baseValue = \"shared\"");
        toml.Should().Contain("derivedValue = \"leaf\"");
        roundtrip.Should().NotBeNull();
        roundtrip.Base.Should().Be("shared");
        roundtrip.Derived.Should().Be("leaf");
    }

    [Fact]
    public void SerializeDeserialize_OverriddenProperties_RespectInheritedJsonPropertyName()
    {
        var original = new JsonNamedOverriddenDerivedOptions
        {
            Base = "shared",
            Derived = "leaf",
        };

        var toml = TomlSerializer.Serialize(original);
        var roundtrip = TomlSerializer.Deserialize<JsonNamedOverriddenDerivedOptions>(toml);

        toml.Should().Contain("baseValue = \"shared\"");
        toml.Should().Contain("derivedValue = \"leaf\"");
        roundtrip.Should().NotBeNull();
        roundtrip.Base.Should().Be("shared");
        roundtrip.Derived.Should().Be("leaf");
    }

    [Fact]
    public void Deserialize_PrivateSetter_WithJsonInclude_Works()
    {
        var model = TomlSerializer.Deserialize<PrivateSetterModel>("Value = 123\n");
        model.Should().NotBeNull();
        model.Value.Should().Be(123);
    }

    [Fact]
    public void SerializeDeserialize_PublicField_WithJsonInclude_Works()
    {
        var original = new IncludedFieldModel { Value = 7 };
        var toml = TomlSerializer.Serialize(original);
        var roundtrip = TomlSerializer.Deserialize<IncludedFieldModel>(toml);
        roundtrip.Should().NotBeNull();
        roundtrip.Value.Should().Be(7);
    }

    [Fact]
    public void SerializeDeserialize_NonPublicProperty_WithTomlInclude_Works()
    {
        var toml = TomlSerializer.Serialize(new IncludedNonPublicPropertyModel());
        var roundtrip = TomlSerializer.Deserialize<IncludedNonPublicPropertyModel>("MyProperty = false\n");

        toml.Should().Contain("MyProperty = true");
        roundtrip.Should().NotBeNull();
        (roundtrip.GetMyProperty()).Should().BeFalse();
    }

    [Fact]
    public void Serialize_ReadOnlyProperty_IsIncluded()
    {
        var toml = TomlSerializer.Serialize(new ReadOnlyPropertyModel());
        toml.Should().Contain("Value");
        toml.Should().Contain("42");
    }

    [Fact]
    public void SerializeDeserialize_EnumWithJsonStringEnumConverter_UsesStrings()
    {
        var original = new StringEnumModel { Values = [StringEnumValue.First, StringEnumValue.Second] };
        var toml = TomlSerializer.Serialize(original);
        var roundtrip = TomlSerializer.Deserialize<StringEnumModel>(toml);

        toml.Should().Contain("Values = [\"First\", \"Second\"]");
        roundtrip.Should().NotBeNull();
        roundtrip.Values.Should().Equal(original.Values);
    }

    [Fact]
    public void Deserialize_UsesSinglePublicConstructor_WhenNoParameterlessExists()
    {
        var model = TomlSerializer.Deserialize<SingleCtorModel>("Name = \"Ada\"\n");
        model.Should().NotBeNull();
        model.Name.Should().Be("Ada");
    }

    [Fact]
    public void Deserialize_BindsConstructorParameter_ByMemberName()
    {
        var model = TomlSerializer.Deserialize<JsonConstructorModel>("Value = 5\n");
        model.Should().NotBeNull();
        model.Value.Should().Be(5);
    }

    [Fact]
    public void Deserialize_UsesJsonConstructor_WhenAnnotated()
    {
        var model = TomlSerializer.Deserialize<AnnotatedJsonConstructorModel>("Value = 6\n");
        model.Should().NotBeNull();
        model.Value.Should().Be(6);
    }

    [Fact]
    public void Deserialize_ThrowsWhenRequiredMemberMissing()
    {
        Assert.Throws<TomlException>(() => TomlSerializer.Deserialize<RequiredModel>("Other = 1\n"));
    }

    [Fact]
    public void SerializeDeserialize_EmptyPoco_NoMembers_Works()
    {
        var toml = TomlSerializer.Serialize(new EmptyModel());
        var model = TomlSerializer.Deserialize<EmptyModel>(toml);
        model.Should().NotBeNull();
    }

    [Fact]
    public void SerializeDeserialize_TomlObjectProperty_UsesRuntimeModelNode()
    {
        var arrayModel = TomlSerializer.Deserialize<TomlObjectModel>("Value = [1, 2]\n");

        arrayModel.Should().NotBeNull();
        arrayModel.Value.Should().BeAssignableTo<TomlArray>();
        ((TomlArray)arrayModel.Value).Should().Equal(new TomlArray { 1L, 2L });

        var tableModel = new TomlObjectModel
        {
            Value = new TomlTable { ["Answer"] = 42L },
        };

        var toml = TomlSerializer.Serialize(tableModel);
        var roundtrip = TomlSerializer.Deserialize<TomlObjectModel>(toml);

        toml.Should().Contain("[Value]");
        roundtrip.Should().NotBeNull();
        roundtrip.Value.Should().BeAssignableTo<TomlTable>();
        (((TomlTable)roundtrip.Value)["Answer"]).Should().Be(42L);
    }

    [Fact]
    public void Deserialize_MultipleAnnotatedConstructors_ThrowsTomlException()
    {
        var ex = Assert.Throws<TomlException>(() => TomlSerializer.Deserialize<MultipleAnnotatedConstructorsModel>("Value = 1\n"));
        ex.Message.Should().Contain("Multiple constructors");
    }

    [Fact]
    public void Deserialize_AmbiguousConstructors_ThrowsTomlException()
    {
        var ex = Assert.Throws<TomlException>(() => TomlSerializer.Deserialize<AmbiguousConstructorsModel>("Value = 1\n"));
        ex.Message.Should().Contain("No suitable constructor");
    }

    [Fact]
    public void SerializeDeserialize_LifecycleCallbacks_AreInvoked()
    {
        var original = new CallbackModel { Name = "Ada" };
        var toml = TomlSerializer.Serialize(original);

        original.OnSerializingCount.Should().Be(1);
        original.OnSerializedCount.Should().Be(1);
        original.OnDeserializingCount.Should().Be(0);
        original.OnDeserializedCount.Should().Be(0);

        var roundtrip = TomlSerializer.Deserialize<CallbackModel>(toml);

        roundtrip.Should().NotBeNull();
        roundtrip.Name.Should().Be("Ada");
        roundtrip.OnDeserializingCount.Should().Be(1);
        roundtrip.OnDeserializedCount.Should().Be(1);
        roundtrip.NameWasAlreadyAssignedInOnDeserializing.Should().BeFalse();
        roundtrip.NameSeenInOnDeserialized.Should().Be("Ada");
    }
}

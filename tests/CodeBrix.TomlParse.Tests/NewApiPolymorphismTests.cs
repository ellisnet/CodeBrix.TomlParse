using System;
using System.Text.Json.Serialization;
using CodeBrix.TomlParse.Serialization;
using SilverAssertions;
using Xunit;

namespace CodeBrix.TomlParse.Tests; //was previously: Tomlyn.Tests;

public class NewApiPolymorphismTests
{
    [TomlPolymorphic(TypeDiscriminatorPropertyName = "kind")]
    [TomlDerivedType(typeof(Cat), "cat")]
    [TomlDerivedType(typeof(Dog), "dog")]
    private abstract class Animal
    {
        public string Name { get; set; }
    }

    private sealed class Cat : Animal
    {
        public int Lives { get; set; }
    }

    private sealed class Dog : Animal
    {
        public bool GoodBoy { get; set; }
    }

    private sealed class TwoLevelPolymorphicConfig
    {
        public required DepartLevel Depart { get; set; }
    }

    [TomlDerivedType(typeof(Depart1), "dd1")]
    private abstract class DepartLevel
    {
        public required string Name { get; set; }

        public required GroupLevel Group { get; set; }
    }

    private sealed class Depart1 : DepartLevel
    {
        public required string Field1 { get; set; }
    }

    [TomlDerivedType(typeof(Group2), "gg2")]
    private abstract class GroupLevel
    {
        public required string Name { get; set; }
    }

    private sealed class Group2 : GroupLevel
    {
        public required string Field2 { get; set; }
    }

    [Fact]
    public void Serialize_Polymorphic_WritesDiscriminator()
    {
        Animal value = new Cat { Name = "Nyan", Lives = 9 };
        var toml = TomlSerializer.Serialize(value);

        toml.Should().Contain("kind");
        toml.Should().Contain("cat");
        toml.Should().Contain("Name");
        toml.Should().Contain("Nyan");
        toml.Should().Contain("Lives");
        toml.Should().Contain("9");
    }

    [Fact]
    public void Deserialize_Polymorphic_UsesDiscriminatorWhenNotFirst()
    {
        var toml =
            """
            kind = "cat"
            Name = "Nyan"
            Lives = 9
            """;

        var result = TomlSerializer.Deserialize<Animal>(toml);

        result.Should().BeOfType<Cat>();
        var cat = (Cat)result;
        cat.Name.Should().Be("Nyan");
        cat.Lives.Should().Be(9);
    }

    [Fact]
    public void Deserialize_Polymorphic_CanReadNestedPolymorphicTables()
    {
        var toml =
            """
            [Depart]
            "$type" = "dd1"
            Name = "this is depart1"
            Field1 = "depart 1 field"

            [Depart.Group]
            "$type" = "gg2"
            Name = "this is group 2"
            Field2 = "group 2 field"
            """;

        var result = TomlSerializer.Deserialize<TwoLevelPolymorphicConfig>(toml);

        result.Should().NotBeNull();
        result.Depart.Should().BeAssignableTo<Depart1>();
        var depart = (Depart1)result.Depart;
        depart.Name.Should().Be("this is depart1");
        depart.Field1.Should().Be("depart 1 field");
        depart.Group.Should().BeAssignableTo<Group2>();
        var group = (Group2)depart.Group;
        group.Name.Should().Be("this is group 2");
        group.Field2.Should().Be("group 2 field");
    }

    [Fact]
    public void Deserialize_Polymorphic_UnknownDiscriminator_FailsByDefault()
    {
        var toml =
            """
            kind = "fox"
            Name = "X"
            """;

        Assert.Throws<TomlException>(() => TomlSerializer.Deserialize<Animal>(toml));
    }

    [TomlPolymorphic(TypeDiscriminatorPropertyName = "kind")]
    [TomlDerivedType(typeof(DerivedPet), "derived")]
    private class Pet
    {
        public string Name { get; set; }
    }

    private sealed class DerivedPet : Pet
    {
        public int Age { get; set; }
    }

    [Fact]
    public void Deserialize_Polymorphic_UnknownDiscriminator_CanFallbackToBase()
    {
        var toml =
            """
            kind = "unknown"
            Name = "Base"
            extra = 123
            """;

        var options = new TomlSerializerOptions
        {
            PolymorphismOptions = new TomlPolymorphismOptions
            {
                TypeDiscriminatorPropertyName = "kind",
                UnknownDerivedTypeHandling = TomlUnknownDerivedTypeHandling.FallBackToBaseType,
            },
        };

        var result = TomlSerializer.Deserialize<Pet>(toml, options);
        result.Should().BeOfType<Pet>();
        result.Name.Should().Be("Base");
    }

    [JsonPolymorphic(TypeDiscriminatorPropertyName = "kind")]
    [JsonDerivedType(typeof(JsonCat), "cat")]
    private abstract class JsonAnimal
    {
        public string Name { get; set; }
    }

    private sealed class JsonCat : JsonAnimal
    {
        public int Lives { get; set; }
    }

    [Fact]
    public void Deserialize_Polymorphic_RespectsSystemTextJsonAttributes()
    {
        var toml =
            """
            kind = "cat"
            Name = "Ada"
            Lives = 9
            """;

        var result = TomlSerializer.Deserialize<JsonAnimal>(toml);
        result.Should().BeOfType<JsonCat>();
        (((JsonCat)result).Lives).Should().Be(9);
    }

    // --- Feature 1: Default Derived Type (no discriminator) ---

    [TomlPolymorphic(TypeDiscriminatorPropertyName = "type")]
    [TomlDerivedType(typeof(DefaultCircle))]
    [TomlDerivedType(typeof(DefaultSquare), "square")]
    private abstract class DefaultShape
    {
        public string Color { get; set; }
    }

    private sealed class DefaultCircle : DefaultShape
    {
        public double Radius { get; set; }
    }

    private sealed class DefaultSquare : DefaultShape
    {
        public double Side { get; set; }
    }

    [Fact]
    public void Serialize_DefaultDerivedType_OmitsDiscriminator()
    {
        DefaultShape value = new DefaultCircle { Color = "red", Radius = 5.0 };
        var toml = TomlSerializer.Serialize(value);

        toml.Should().NotContain("type");
        toml.Should().Contain("Color");
        toml.Should().Contain("red");
        toml.Should().Contain("Radius");
        toml.Should().Contain("5");
    }

    [Fact]
    public void Serialize_NonDefaultDerivedType_WritesDiscriminator()
    {
        DefaultShape value = new DefaultSquare { Color = "blue", Side = 3.0 };
        var toml = TomlSerializer.Serialize(value);

        toml.Should().Contain("type = \"square\"");
        toml.Should().Contain("Color");
        toml.Should().Contain("blue");
        toml.Should().Contain("Side");
        toml.Should().Contain("3");
    }

    [Fact]
    public void Deserialize_DefaultDerivedType_WhenMissingDiscriminator()
    {
        var toml =
            """
            Color = "red"
            Radius = 5.0
            """;

        var result = TomlSerializer.Deserialize<DefaultShape>(toml);

        result.Should().BeOfType<DefaultCircle>();
        var circle = (DefaultCircle)result;
        circle.Color.Should().Be("red");
        circle.Radius.Should().Be(5.0);
    }

    [Fact]
    public void Deserialize_DefaultDerivedType_WhenUnknownDiscriminator()
    {
        var toml =
            """
            type = "triangle"
            Color = "green"
            """;

        var result = TomlSerializer.Deserialize<DefaultShape>(toml);

        result.Should().BeOfType<DefaultCircle>();
        var circle = (DefaultCircle)result;
        circle.Color.Should().Be("green");
    }

    [Fact]
    public void Deserialize_NonDefaultDerivedType_WithDiscriminator()
    {
        var toml =
            """
            type = "square"
            Color = "blue"
            Side = 3.0
            """;

        var result = TomlSerializer.Deserialize<DefaultShape>(toml);

        result.Should().BeOfType<DefaultSquare>();
        var square = (DefaultSquare)result;
        square.Color.Should().Be("blue");
        square.Side.Should().Be(3.0);
    }

    [Fact]
    public void Roundtrip_DefaultDerivedType()
    {
        DefaultShape original = new DefaultCircle { Color = "red", Radius = 5.0 };
        var toml = TomlSerializer.Serialize(original);
        var result = TomlSerializer.Deserialize<DefaultShape>(toml);

        result.Should().BeOfType<DefaultCircle>();
        var circle = (DefaultCircle)result;
        circle.Color.Should().Be("red");
        circle.Radius.Should().Be(5.0);
    }

    [Fact]
    public void Roundtrip_NonDefaultDerivedType()
    {
        DefaultShape original = new DefaultSquare { Color = "blue", Side = 3.0 };
        var toml = TomlSerializer.Serialize(original);
        var result = TomlSerializer.Deserialize<DefaultShape>(toml);

        result.Should().BeOfType<DefaultSquare>();
        var square = (DefaultSquare)result;
        square.Color.Should().Be("blue");
        square.Side.Should().Be(3.0);
    }

    // Default derived type with JsonDerivedType (no discriminator is 1-arg constructor)
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "kind")]
    [JsonDerivedType(typeof(JsonDefaultCircle))]
    [JsonDerivedType(typeof(JsonDefaultSquare), "square")]
    private abstract class JsonDefaultShape
    {
        public string Color { get; set; }
    }

    private sealed class JsonDefaultCircle : JsonDefaultShape
    {
        public double Radius { get; set; }
    }

    private sealed class JsonDefaultSquare : JsonDefaultShape
    {
        public double Side { get; set; }
    }

    [Fact]
    public void Serialize_JsonDefaultDerivedType_OmitsDiscriminator()
    {
        JsonDefaultShape value = new JsonDefaultCircle { Color = "red", Radius = 5.0 };
        var toml = TomlSerializer.Serialize(value);

        toml.Should().NotContain("kind");
        toml.Should().Contain("Color");
        toml.Should().Contain("Radius");
    }

    [Fact]
    public void Deserialize_JsonDefaultDerivedType_WhenMissingDiscriminator()
    {
        var toml =
            """
            Color = "red"
            Radius = 5.0
            """;

        var result = TomlSerializer.Deserialize<JsonDefaultShape>(toml);

        result.Should().BeOfType<JsonDefaultCircle>();
        (((JsonDefaultCircle)result).Color).Should().Be("red");
        (((JsonDefaultCircle)result).Radius).Should().Be(5.0);
    }

    // --- Feature 2: UnknownDerivedTypeHandling on attribute ---

    [TomlPolymorphic(TypeDiscriminatorPropertyName = "kind", UnknownDerivedTypeHandling = TomlUnknownDerivedTypeHandling.FallBackToBaseType)]
    [TomlDerivedType(typeof(AttrFallbackDerived), "derived")]
    private class AttrFallbackBase
    {
        public string Name { get; set; }
    }

    private sealed class AttrFallbackDerived : AttrFallbackBase
    {
        public int Extra { get; set; }
    }

    [Fact]
    public void Deserialize_UnknownDiscriminator_FallbackViaAttribute()
    {
        var toml =
            """
            kind = "unknown"
            Name = "test"
            """;

        // No options override needed - attribute sets FallBackToBaseType
        var result = TomlSerializer.Deserialize<AttrFallbackBase>(toml);

        result.Should().BeOfType<AttrFallbackBase>();
        result.Name.Should().Be("test");
    }

    [Fact]
    public void Deserialize_UnknownDiscriminator_AttributeOverridesOptions()
    {
        var toml =
            """
            kind = "unknown"
            Name = "test"
            """;

        // Options say Fail, but attribute says FallBackToBaseType - attribute wins
        var options = new TomlSerializerOptions
        {
            PolymorphismOptions = new TomlPolymorphismOptions
            {
                UnknownDerivedTypeHandling = TomlUnknownDerivedTypeHandling.Fail,
            },
        };

        var result = TomlSerializer.Deserialize<AttrFallbackBase>(toml, options);

        result.Should().BeOfType<AttrFallbackBase>();
        result.Name.Should().Be("test");
    }

    // Test: attribute says Fail explicitly, options say FallBackToBaseType - attribute wins
    [TomlPolymorphic(TypeDiscriminatorPropertyName = "kind", UnknownDerivedTypeHandling = TomlUnknownDerivedTypeHandling.Fail)]
    [TomlDerivedType(typeof(AttrFailDerived), "derived")]
    private class AttrFailBase
    {
        public string Name { get; set; }
    }

    private sealed class AttrFailDerived : AttrFailBase
    {
        public int Extra { get; set; }
    }

    [Fact]
    public void Deserialize_UnknownDiscriminator_AttributeFailOverridesFallbackOptions()
    {
        var toml =
            """
            kind = "unknown"
            Name = "test"
            """;

        var options = new TomlSerializerOptions
        {
            PolymorphismOptions = new TomlPolymorphismOptions
            {
                UnknownDerivedTypeHandling = TomlUnknownDerivedTypeHandling.FallBackToBaseType,
            },
        };

        Assert.Throws<TomlException>(() => TomlSerializer.Deserialize<AttrFailBase>(toml, options));
    }

    // Test: JsonPolymorphic attribute sets FallBackToBaseType
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "kind", UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FallBackToBaseType)]
    [JsonDerivedType(typeof(JsonAttrFallbackDerived), "derived")]
    private class JsonAttrFallbackBase
    {
        public string Name { get; set; }
    }

    private sealed class JsonAttrFallbackDerived : JsonAttrFallbackBase
    {
        public int Extra { get; set; }
    }

    [Fact]
    public void Deserialize_UnknownDiscriminator_JsonAttributeFallback()
    {
        var toml =
            """
            kind = "unknown"
            Name = "test"
            """;

        var result = TomlSerializer.Deserialize<JsonAttrFallbackBase>(toml);

        result.Should().BeOfType<JsonAttrFallbackBase>();
        result.Name.Should().Be("test");
    }

    // Test: TomlPolymorphic overrides JsonPolymorphic when both present
    [TomlPolymorphic(TypeDiscriminatorPropertyName = "kind", UnknownDerivedTypeHandling = TomlUnknownDerivedTypeHandling.Fail)]
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "kind", UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FallBackToBaseType)]
    [TomlDerivedType(typeof(TomlOverridesJsonDerived), "derived")]
    private class TomlOverridesJsonBase
    {
        public string Name { get; set; }
    }

    private sealed class TomlOverridesJsonDerived : TomlOverridesJsonBase
    {
        public int Extra { get; set; }
    }

    [Fact]
    public void Deserialize_UnknownDiscriminator_TomlAttributeOverridesJsonAttribute()
    {
        var toml =
            """
            kind = "unknown"
            Name = "test"
            """;

        // TomlPolymorphic says Fail, JsonPolymorphic says FallBack - Toml wins
        Assert.Throws<TomlException>(() => TomlSerializer.Deserialize<TomlOverridesJsonBase>(toml));
    }

    [Fact]
    public void Options_RejectsUnspecifiedUnknownDerivedTypeHandling()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            _ = new TomlPolymorphismOptions
            {
                UnknownDerivedTypeHandling = TomlUnknownDerivedTypeHandling.Unspecified,
            };
        });
    }

    // --- Feature 3: Integer Discriminators ---

    [TomlPolymorphic(TypeDiscriminatorPropertyName = "type")]
    [TomlDerivedType(typeof(IntDiscrimCircle), 1)]
    [TomlDerivedType(typeof(IntDiscrimSquare), 2)]
    private abstract class IntDiscrimShape
    {
        public string Color { get; set; }
    }

    private sealed class IntDiscrimCircle : IntDiscrimShape
    {
        public double Radius { get; set; }
    }

    private sealed class IntDiscrimSquare : IntDiscrimShape
    {
        public double Side { get; set; }
    }

    [Fact]
    public void Serialize_IntDiscriminator_WritesAsString()
    {
        IntDiscrimShape value = new IntDiscrimCircle { Color = "red", Radius = 5.0 };
        var toml = TomlSerializer.Serialize(value);

        toml.Should().Contain("type = \"1\"");
        toml.Should().Contain("Color");
        toml.Should().Contain("Radius");
    }

    [Fact]
    public void Deserialize_IntDiscriminator_MatchesByString()
    {
        var toml =
            """
            type = "1"
            Color = "red"
            Radius = 5.0
            """;

        var result = TomlSerializer.Deserialize<IntDiscrimShape>(toml);

        result.Should().BeOfType<IntDiscrimCircle>();
        var circle = (IntDiscrimCircle)result;
        circle.Color.Should().Be("red");
        circle.Radius.Should().Be(5.0);
    }

    [Fact]
    public void Roundtrip_IntDiscriminator()
    {
        IntDiscrimShape original = new IntDiscrimSquare { Color = "blue", Side = 3.0 };
        var toml = TomlSerializer.Serialize(original);
        var result = TomlSerializer.Deserialize<IntDiscrimShape>(toml);

        result.Should().BeOfType<IntDiscrimSquare>();
        var square = (IntDiscrimSquare)result;
        square.Color.Should().Be("blue");
        square.Side.Should().Be(3.0);
    }

    // JsonDerivedType int discriminators (already supported at runtime)
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
    [JsonDerivedType(typeof(JsonIntDiscrimCircle), 1)]
    [JsonDerivedType(typeof(JsonIntDiscrimSquare), 2)]
    private abstract class JsonIntDiscrimShape
    {
        public string Color { get; set; }
    }

    private sealed class JsonIntDiscrimCircle : JsonIntDiscrimShape
    {
        public double Radius { get; set; }
    }

    private sealed class JsonIntDiscrimSquare : JsonIntDiscrimShape
    {
        public double Side { get; set; }
    }

    [Fact]
    public void Roundtrip_JsonIntDiscriminator()
    {
        JsonIntDiscrimShape original = new JsonIntDiscrimCircle { Color = "red", Radius = 5.0 };
        var toml = TomlSerializer.Serialize(original);

        toml.Should().Contain("type = \"1\"");

        var result = TomlSerializer.Deserialize<JsonIntDiscrimShape>(toml);

        result.Should().BeOfType<JsonIntDiscrimCircle>();
        (((JsonIntDiscrimCircle)result).Color).Should().Be("red");
        (((JsonIntDiscrimCircle)result).Radius).Should().Be(5.0);
    }
}

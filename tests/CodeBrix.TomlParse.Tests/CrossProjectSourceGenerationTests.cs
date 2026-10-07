using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using CodeBrix.TomlParse.Serialization;
using SilverAssertions;
using Xunit;

namespace CodeBrix.TomlParse.Tests; //was previously: Tomlyn.Tests;

[TomlSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[TomlSerializable(typeof(GeneratedCrossProjectAnimal))]
[TomlSerializable(typeof(GeneratedCrossProjectEnvelope))]
[TomlDerivedTypeMapping(typeof(GeneratedCrossProjectAnimal), typeof(GeneratedCrossProjectCat), "cat")]
[TomlDerivedTypeMapping(typeof(GeneratedCrossProjectAnimal), typeof(GeneratedCrossProjectDog), "dog")]
internal partial class GeneratedCrossProjectContext : TomlSerializerContext
{
}

[TomlPolymorphic(TypeDiscriminatorPropertyName = "kind")]
public abstract class GeneratedCrossProjectAnimal
{
    public string Name { get; set; } = "";
}

public sealed class GeneratedCrossProjectCat : GeneratedCrossProjectAnimal
{
    public int Lives { get; set; }
}

public sealed class GeneratedCrossProjectDog : GeneratedCrossProjectAnimal
{
    public bool GoodBoy { get; set; }
}

public sealed class GeneratedCrossProjectEnvelope
{
    public List<GeneratedCrossProjectAnimal> Animals { get; set; } = [];
}

[TomlSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[TomlSerializable(typeof(GeneratedMappedShape))]
[TomlDerivedTypeMapping(typeof(GeneratedMappedShape), typeof(GeneratedMappedCircle))]
[TomlDerivedTypeMapping(typeof(GeneratedMappedShape), typeof(GeneratedMappedSquare), "square")]
internal partial class GeneratedMappedShapeContext : TomlSerializerContext
{
}

[TomlPolymorphic(TypeDiscriminatorPropertyName = "type")]
public abstract class GeneratedMappedShape
{
    public string Color { get; set; } = "";
}

public sealed class GeneratedMappedCircle : GeneratedMappedShape
{
    public double Radius { get; set; }
}

public sealed class GeneratedMappedSquare : GeneratedMappedShape
{
    public double Side { get; set; }
}

[TomlSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[TomlSerializable(typeof(GeneratedPreferredAnimal))]
[TomlDerivedTypeMapping(typeof(GeneratedPreferredAnimal), typeof(GeneratedMappedFox), "fox")]
[TomlDerivedTypeMapping(typeof(GeneratedPreferredAnimal), typeof(GeneratedIgnoredDog), "cat")]
internal partial class GeneratedPreferredAnimalContext : TomlSerializerContext
{
}

[TomlPolymorphic(TypeDiscriminatorPropertyName = "kind")]
[TomlDerivedType(typeof(GeneratedPreferredCat), "cat")]
public abstract class GeneratedPreferredAnimal
{
    public string Name { get; set; } = "";
}

public sealed class GeneratedPreferredCat : GeneratedPreferredAnimal
{
    public int Lives { get; set; }
}

public sealed class GeneratedMappedFox : GeneratedPreferredAnimal
{
    public int Tricks { get; set; }
}

public sealed class GeneratedIgnoredDog : GeneratedPreferredAnimal
{
    public bool GoodBoy { get; set; }
}

[TomlSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[TomlSerializable(typeof(GeneratedMappedNumberShape))]
[TomlDerivedTypeMapping(typeof(GeneratedMappedNumberShape), typeof(GeneratedMappedNumberCircle), 1)]
[TomlDerivedTypeMapping(typeof(GeneratedMappedNumberShape), typeof(GeneratedMappedNumberSquare), 2)]
internal partial class GeneratedMappedNumberShapeContext : TomlSerializerContext
{
}

[TomlPolymorphic(TypeDiscriminatorPropertyName = "type")]
public abstract class GeneratedMappedNumberShape
{
}

public sealed class GeneratedMappedNumberCircle : GeneratedMappedNumberShape
{
    public double Radius { get; set; }
}

public sealed class GeneratedMappedNumberSquare : GeneratedMappedNumberShape
{
    public double Side { get; set; }
}

[TomlSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[TomlSerializable(typeof(GeneratedMappedNode))]
[TomlSerializable(typeof(GeneratedMappedNodeHolder))]
[TomlDerivedTypeMapping(typeof(IGeneratedMappedNode), typeof(GeneratedMappedNode), "node")]
internal partial class GeneratedMappedNodeContext : TomlSerializerContext
{
}

[TomlPolymorphic(TypeDiscriminatorPropertyName = "kind")]
public interface IGeneratedMappedNode
{
    string Name { get; }

    int Level { get; }
}

public sealed class GeneratedMappedNode : IGeneratedMappedNode
{
    public string Name { get; set; } = "";

    public int Level { get; set; }
}

public sealed class GeneratedMappedNodeHolder
{
    public IGeneratedMappedNode Value { get; set; }
}

public sealed class CrossProjectSourceGenerationTests
{
    [Fact]
    public void GeneratedContext_CanSerializeAndDeserializeCrossProjectRoot()
    {
        var context = GeneratedCrossProjectContext.Default;
        GeneratedCrossProjectAnimal value = new GeneratedCrossProjectDog { Name = "Rex", GoodBoy = true };

        var toml = TomlSerializer.Serialize(value, context.GeneratedCrossProjectAnimal);
        var roundtrip = TomlSerializer.Deserialize(toml, context.GeneratedCrossProjectAnimal);

        toml.Should().Contain("kind = \"dog\"");
        roundtrip.Should().BeOfType<GeneratedCrossProjectDog>();
        (((GeneratedCrossProjectDog)roundtrip).GoodBoy).Should().BeTrue();
    }

    [Fact]
    public void GeneratedContext_AutoIncludesContextMappedDerivedTypes()
    {
        var context = GeneratedCrossProjectContext.Default;

        var catTypeInfo = context.GetTypeInfo(typeof(GeneratedCrossProjectCat), context.Options);
        var dogTypeInfo = context.GetTypeInfo(typeof(GeneratedCrossProjectDog), context.Options);

        catTypeInfo.Should().NotBeNull();
        dogTypeInfo.Should().NotBeNull();
    }

    [Fact]
    public void GeneratedContext_CanRoundtripCollectionsOfCrossProjectValues()
    {
        var context = GeneratedCrossProjectContext.Default;
        var payload = new GeneratedCrossProjectEnvelope
        {
            Animals =
            [
                new GeneratedCrossProjectCat { Name = "Mog", Lives = 9 },
                new GeneratedCrossProjectDog { Name = "Rex", GoodBoy = true },
            ],
        };

        var toml = TomlSerializer.Serialize(payload, context.GeneratedCrossProjectEnvelope);
        var roundtrip = TomlSerializer.Deserialize(toml, context.GeneratedCrossProjectEnvelope);

        roundtrip.Should().NotBeNull();
        roundtrip.Animals.Should().HaveCount(2);
        roundtrip.Animals[0].Should().BeOfType<GeneratedCrossProjectCat>();
        roundtrip.Animals[1].Should().BeOfType<GeneratedCrossProjectDog>();
    }

    [Fact]
    public void GeneratedContext_ContextMappingsSupportDefaultDerivedTypes()
    {
        var context = GeneratedMappedShapeContext.Default;
        GeneratedMappedShape value = new GeneratedMappedCircle { Color = "red", Radius = 5.0 };

        var toml = TomlSerializer.Serialize(value, context.GeneratedMappedShape);
        var missing = TomlSerializer.Deserialize(
            """
            color = "red"
            radius = 5.0
            """,
            context.GeneratedMappedShape);
        var unknown = TomlSerializer.Deserialize(
            """
            type = "triangle"
            color = "blue"
            radius = 2.0
            """,
            context.GeneratedMappedShape);

        toml.Should().NotContain("type");
        missing.Should().BeOfType<GeneratedMappedCircle>();
        unknown.Should().BeOfType<GeneratedMappedCircle>();
    }

    [Fact]
    public void GeneratedContext_ContextMappingsRespectAttributePrecedence()
    {
        var context = GeneratedPreferredAnimalContext.Default;
        var preferred = TomlSerializer.Deserialize(
            """
            kind = "cat"
            name = "Ada"
            lives = 9
            """,
            context.GeneratedPreferredAnimal);
        var additive = TomlSerializer.Deserialize(
            """
            kind = "fox"
            name = "Pip"
            tricks = 3
            """,
            context.GeneratedPreferredAnimal);

        preferred.Should().BeOfType<GeneratedPreferredCat>();
        (((GeneratedPreferredCat)preferred).Lives).Should().Be(9);
        additive.Should().BeOfType<GeneratedMappedFox>();
        (((GeneratedMappedFox)additive).Tricks).Should().Be(3);
    }

    [Fact]
    public void GeneratedContext_ContextMappingsSupportIntegerDiscriminators()
    {
        var context = GeneratedMappedNumberShapeContext.Default;
        GeneratedMappedNumberShape value = new GeneratedMappedNumberCircle { Radius = 4.5 };

        var toml = TomlSerializer.Serialize(value, context.GeneratedMappedNumberShape);
        var roundtrip = TomlSerializer.Deserialize(
            """
            type = "2"
            side = 3.0
            """,
            context.GeneratedMappedNumberShape);

        toml.Should().Contain("type = \"1\"");
        roundtrip.Should().BeOfType<GeneratedMappedNumberSquare>();
        (((GeneratedMappedNumberSquare)roundtrip).Side).Should().Be(3.0);
    }

    [Fact]
    public void GeneratedContext_ContextMappingsSupportInterfaceRootTypes()
    {
        var context = GeneratedMappedNodeContext.Default;
        IGeneratedMappedNode value = new GeneratedMappedNode { Name = "Root", Level = 4 };

        var toml = TomlSerializer.Serialize(value, typeof(IGeneratedMappedNode), context);
        var roundtrip = (IGeneratedMappedNode)TomlSerializer.Deserialize(toml, typeof(IGeneratedMappedNode), context);

        (context.GetTypeInfo(typeof(IGeneratedMappedNode), context.Options)).Should().NotBeNull();
        toml.Should().Contain("kind = \"node\"");
        toml.Should().Contain("name = \"Root\"");
        toml.Should().Contain("level = 4");
        roundtrip.Should().BeOfType<GeneratedMappedNode>();
        roundtrip.Name.Should().Be("Root");
        roundtrip.Level.Should().Be(4);
    }

    [Fact]
    public void GeneratedContext_ContextMappingsSupportInterfaceMemberTypes()
    {
        var context = GeneratedMappedNodeContext.Default;
        var holder = new GeneratedMappedNodeHolder
        {
            Value = new GeneratedMappedNode { Name = "Child", Level = 3 },
        };

        var toml = TomlSerializer.Serialize(holder, context.GeneratedMappedNodeHolder);
        var roundtrip = TomlSerializer.Deserialize(toml, context.GeneratedMappedNodeHolder);

        toml.Should().Contain("kind = \"node\"");
        toml.Should().Contain("name = \"Child\"");
        toml.Should().Contain("level = 3");
        roundtrip.Should().NotBeNull();
        roundtrip.Value.Should().BeOfType<GeneratedMappedNode>();
        roundtrip.Value.Name.Should().Be("Child");
        roundtrip.Value.Level.Should().Be(3);
    }
}

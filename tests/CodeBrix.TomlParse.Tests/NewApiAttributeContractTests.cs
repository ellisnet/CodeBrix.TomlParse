using System;
using System.Linq;
using CodeBrix.TomlParse.Serialization;
using SilverAssertions;
using Xunit;

namespace CodeBrix.TomlParse.Tests; //was previously: Tomlyn.Tests;

public class NewApiAttributeContractTests
{
    [Fact]
    public void ExportedTomlAttributes_HaveUniqueTypeNames()
    {
        var duplicateNames = typeof(TomlSerializer).Assembly
            .GetExportedTypes()
            .Where(static type => type.Name.StartsWith("Toml", StringComparison.Ordinal) &&
                                  type.Name.EndsWith("Attribute", StringComparison.Ordinal))
            .GroupBy(static type => type.Name, StringComparer.Ordinal)
            .Where(static group => group.Count() > 1)
            .Select(static group => group.Key)
            .ToArray();

        duplicateNames.Should().BeEmpty();
    }

    [Fact]
    public void ExportedTomlAttributes_InheritTomlAttribute()
    {
        var nonDerivedAttributes = typeof(TomlSerializer).Assembly
            .GetExportedTypes()
            .Where(static type => type != typeof(TomlAttribute) &&
                                  type.IsSubclassOf(typeof(Attribute)) &&
                                  type.Name.StartsWith("Toml", StringComparison.Ordinal) &&
                                  type.Name.EndsWith("Attribute", StringComparison.Ordinal))
            .Where(static type => !typeof(TomlAttribute).IsAssignableFrom(type))
            .Select(static type => type.FullName)
            .ToArray();

        nonDerivedAttributes.Should().BeEmpty();
    }

    [Fact]
    public void TomlPropertyNameAttribute_IsOnlyAvailableFromSerializationNamespace()
    {
        var exportedTypes = typeof(TomlSerializer).Assembly
            .GetExportedTypes()
            .Where(static type => type.Name == nameof(TomlPropertyNameAttribute))
            .Select(static type => type.FullName)
            .OrderBy(static fullName => fullName, StringComparer.Ordinal)
            .ToArray();

        exportedTypes.Should().Equal(new[] { "CodeBrix.TomlParse.Serialization.TomlPropertyNameAttribute" });
    }

    [Fact]
    public void FormattingAttributes_RejectInvalidEnumValues()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new TomlTableArrayStyleAttribute((TomlTableArrayStyle)42));
        Assert.Throws<ArgumentOutOfRangeException>(() => new TomlInlineTableAttribute((TomlInlineTablePolicy)42));
        Assert.Throws<ArgumentOutOfRangeException>(() => new TomlStringStyleAttribute((TomlStringStyle)42));
        Assert.Throws<ArgumentOutOfRangeException>(() => new TomlMappingOrderAttribute((TomlMappingOrderPolicy)42));
        Assert.Throws<ArgumentOutOfRangeException>(() => new TomlDottedKeyHandlingAttribute((TomlDottedKeyHandling)42));
    }
}

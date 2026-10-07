using CodeBrix.TomlParse.Serialization;
using SilverAssertions;
using Xunit;

namespace CodeBrix.TomlParse.Tests; //was previously: Tomlyn.Tests;

public sealed class BomInputTests
{
    [Fact]
    public void TomlReader_StringWithBom_IsAccepted()
    {
        var reader = TomlReader.Create("\uFEFFa = 1\n");
        reader.Read().Should().BeTrue();
        reader.Read().Should().BeTrue();
        reader.Read().Should().BeTrue();
        reader.PropertyName.Should().Be("a");
        reader.Read().Should().BeTrue();
        reader.GetInt64().Should().Be(1L);
    }
}

using CodeBrix.TomlParse.Model;
using SilverAssertions;
using Xunit;

namespace CodeBrix.TomlParse.Tests; //was previously: Tomlyn.Tests;

public sealed class NewApiUntypedModelTests
{
    [Fact]
    public void DeserializeTomlTable_PreservesTableArraysAsTomlTableArray()
    {
        var toml = """
            [[statuses]]
            id = 1
            """;

        var table = TomlSerializer.Deserialize<TomlTable>(toml);

        table.Should().NotBeNull();
        (table.TryGetValue("statuses", out var statuses)).Should().BeTrue();
        statuses.Should().BeAssignableTo<TomlTableArray>();
        (((TomlTableArray)statuses).Count).Should().Be(1);
    }

    [Fact]
    public void DeserializeTomlTable_TableArrayChildTablesAttachToLastElement()
    {
        var toml = """
            [[statuses]]
            id = 1

            [statuses.metadata]
            result_type = "recent"
            """;

        var table = TomlSerializer.Deserialize<TomlTable>(toml);

        table.Should().NotBeNull();
        (table.TryGetValue("statuses", out var statuses)).Should().BeTrue();
        statuses.Should().BeAssignableTo<TomlTableArray>();
        var tableArray = (TomlTableArray)statuses;
        tableArray.Should().HaveCount(1);
        (tableArray[0].TryGetValue("metadata", out var metadata)).Should().BeTrue();
        metadata.Should().BeAssignableTo<TomlTable>();
        (((TomlTable)metadata)["result_type"]).Should().Be("recent");
    }
}

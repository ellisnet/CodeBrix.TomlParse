using System;
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

    [Theory]
    [InlineData("a = 1\na = 2\n")]
    [InlineData("a = 1\n\n[other]\nx = 1\nx = \"two\"\n")]
    [InlineData("point = { x = 1, x = 2 }\n")]
    [InlineData("a = 1\n\n[a]\nb = 2\n")]
    public void DeserializeTomlTable_refuses_a_duplicate_key_by_default(string toml)
    {
        //Act
        Action act = () => TomlSerializer.Deserialize<TomlTable>(toml);

        //Assert
        act.Should().Throw<TomlException>();
    }

    [Fact]
    public void DeserializeTomlTable_names_the_duplicate_key()
    {
        //Act
        Action act = () => TomlSerializer.Deserialize<TomlTable>("first = 1\nsecond = 2\nfirst = 3\n");

        //Assert
        act.Should().Throw<TomlException>().WithMessage("*first*");
    }

    [Fact]
    public void DeserializeTomlTable_keeps_the_last_value_of_a_duplicate_key_when_asked_to()
    {
        //Arrange
        var options = new TomlSerializerOptions { DuplicateKeyHandling = TomlDuplicateKeyHandling.LastWins };

        //Act
        var table = TomlSerializer.Deserialize<TomlTable>("a = 1\na = 2\n", options);

        //Assert
        table["a"].Should().Be(2L);
    }

    [Fact]
    public void DeserializeTomlTable_still_reads_a_table_that_is_built_up_in_several_places()
    {
        //Arrange
        var toml = """
            server.host = "localhost"
            server.port = 8080

            [server.limits]
            requests = 10

            [[application]]
            name = "one"

            [[application]]
            name = "two"
            """;

        //Act
        var table = TomlSerializer.Deserialize<TomlTable>(toml);

        //Assert
        var server = (TomlTable)table["server"];
        server["host"].Should().Be("localhost");
        server["port"].Should().Be(8080L);
        ((TomlTable)server["limits"])["requests"].Should().Be(10L);
        ((TomlTableArray)table["application"]).Count.Should().Be(2);
    }
}

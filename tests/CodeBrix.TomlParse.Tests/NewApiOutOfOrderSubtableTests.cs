using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using CodeBrix.TomlParse.Model;
using CodeBrix.TomlParse.Serialization;
using SilverAssertions;
using Xunit;

namespace CodeBrix.TomlParse.Tests; //was previously: Tomlyn.Tests;

public sealed class OutOfOrderSubtableRoot
{
    [JsonPropertyName("msbuild")]
    public OutOfOrderSubtableMsBuild MSBuild { get; } = new();

    [JsonPropertyName("github")]
    public OutOfOrderSubtableGitHub GitHub { get; } = new();
}

public sealed class OutOfOrderSubtableMsBuild
{
    public string Project { get; set; } = string.Empty;

    public Dictionary<string, object> Properties { get; } = new();
}

public sealed class OutOfOrderSubtableGitHub
{
    public string User { get; set; } = string.Empty;

    public string Repo { get; set; } = string.Empty;
}

public sealed class NestedTableArrayRoot
{
    public List<NestedTableArrayCommand> Command { get; set; }
}

public sealed class NestedTableArrayCommand
{
    public string CommandId { get; set; }

    public List<NestedTableArraySlash> Slash { get; set; }
}

public sealed class NestedTableArraySlash
{
    public string Path { get; set; }

    public string Help { get; set; }

    public NestedTableArrayArgs Args { get; set; }
}

public sealed class NestedTableArrayArgs
{
    public string Surface { get; set; }
}

[TomlSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower,
    PreferredObjectCreationHandling = JsonObjectCreationHandling.Populate)]
[TomlSerializable(typeof(OutOfOrderSubtableRoot))]
[TomlSerializable(typeof(NestedTableArrayRoot))]
internal partial class TestOutOfOrderSubtableContext : TomlSerializerContext
{
}

public class NewApiOutOfOrderSubtableTests
{
    private const string SampleToml =
        """
        [msbuild]
        project = "HelloWorld.csproj"

        [github]
        user = "u"
        repo = "r"

        [msbuild.properties]
        PublishReadyToRun = false
        """;

    private const string NestedTableArrayToml =
        """
        intent_catalog_schema_version = 2

        [[command]]
        command_id = "open_file"

        [[command.slash]]
        path = "/file open"
        help = "Open file"
        args = { surface = "editor" }

        [[command]]
        command_id = "open_folder"

        [[command.slash]]
        path = "/file pick"
        help = "Pick file"
        """;

    [Fact]
    public void Reflection_AllowsOutOfOrderSubtableExtensions()
    {
        var result = TomlSerializer.Deserialize<OutOfOrderSubtableRoot>(SampleToml, new TomlSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            PreferredObjectCreationHandling = JsonObjectCreationHandling.Populate,
            SourceName = "repro.toml",
        });

        result.Should().NotBeNull();
        result.MSBuild.Project.Should().Be("HelloWorld.csproj");
        result.GitHub.User.Should().Be("u");
        result.GitHub.Repo.Should().Be("r");
        result.MSBuild.Properties["PublishReadyToRun"].Should().Be(false);
    }

    [Fact]
    public void SourceGenerated_AllowsOutOfOrderSubtableExtensions()
    {
        var context = TestOutOfOrderSubtableContext.Default;
        var result = TomlSerializer.Deserialize(SampleToml, context.OutOfOrderSubtableRoot);

        result.Should().NotBeNull();
        result.MSBuild.Project.Should().Be("HelloWorld.csproj");
        result.GitHub.User.Should().Be("u");
        result.GitHub.Repo.Should().Be("r");
        result.MSBuild.Properties["PublishReadyToRun"].Should().Be(false);
    }

    [Fact]
    public void TypedDictionary_AllowsOutOfOrderSubtableExtensions()
    {
        var result = TomlSerializer.Deserialize<Dictionary<string, object>>(SampleToml, new TomlSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            SourceName = "repro.toml",
        });

        result.Should().NotBeNull();
        (result.TryGetValue("msbuild", out var rawMsBuild)).Should().BeTrue();
        rawMsBuild.Should().BeOfType<TomlTable>();

        var msbuild = (TomlTable)rawMsBuild;
        msbuild["project"].Should().Be("HelloWorld.csproj");
        msbuild["properties"].Should().BeOfType<TomlTable>();

        var properties = (TomlTable)msbuild["properties"];
        properties["PublishReadyToRun"].Should().Be(false);
    }

    [Fact]
    public void Reflection_AllowsNestedTableArraysInsideTableArrays()
    {
        var result = TomlSerializer.Deserialize<NestedTableArrayRoot>(NestedTableArrayToml, new TomlSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            SourceName = "issue-124.toml",
        });

        AssertNestedTableArrayResult(result);
    }

    [Fact]
    public void SourceGenerated_AllowsNestedTableArraysInsideTableArrays()
    {
        var context = TestOutOfOrderSubtableContext.Default;
        var result = TomlSerializer.Deserialize(NestedTableArrayToml, context.NestedTableArrayRoot);

        AssertNestedTableArrayResult(result);
    }

    private static void AssertNestedTableArrayResult(NestedTableArrayRoot result)
    {
        result.Should().NotBeNull();
        result.Command.Should().HaveCount(2);

        var firstCommand = result.Command[0];
        firstCommand.CommandId.Should().Be("open_file");
        firstCommand.Slash.Should().HaveCount(1);
        var firstSlash = firstCommand.Slash[0];
        firstSlash.Path.Should().Be("/file open");
        firstSlash.Help.Should().Be("Open file");
        firstSlash.Args.Should().NotBeNull();
        firstSlash.Args.Surface.Should().Be("editor");

        var secondCommand = result.Command[1];
        secondCommand.CommandId.Should().Be("open_folder");
        secondCommand.Slash.Should().HaveCount(1);
        var secondSlash = secondCommand.Slash[0];
        secondSlash.Path.Should().Be("/file pick");
        secondSlash.Help.Should().Be("Pick file");
    }
}

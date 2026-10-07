using System.IO;
using System.Text;
using CodeBrix.TomlParse.Model;
using CodeBrix.TomlParse.Serialization;
using SilverAssertions;
using Xunit;

namespace CodeBrix.TomlParse.Tests; //was previously: Tomlyn.Tests;

public sealed class NewApiMetadataStreamOverloadsTests
{
    private sealed class BuiltInContext : TomlSerializerContext
    {
        public override TomlTypeInfo GetTypeInfo(System.Type type, TomlSerializerOptions options)
        {
            if (type == typeof(int)) return GetBuiltInTypeInfo<int>(options);
            if (type == typeof(TomlTable)) return GetBuiltInTypeInfo<TomlTable>(options);
            return null;
        }
    }

    [Fact]
    public void Serialize_Stream_ObjectTypeInfo_WritesToml()
    {
        var context = new BuiltInContext();
        var options = context.Options with { RootValueHandling = TomlRootValueHandling.WrapInRootKey };

        var typeInfo = context.GetTypeInfo(typeof(int), options);
        typeInfo.Should().NotBeNull();

        using var stream = new MemoryStream();
        TomlSerializer.Serialize(stream, 123, typeInfo);

        stream.Position = 0;
        using var reader = new StreamReader(stream);
        var toml = reader.ReadToEnd();

        toml.Should().Contain("value");
        toml.Should().Contain("123");
    }

    [Fact]
    public void Deserialize_Stream_ObjectTypeInfo_Works()
    {
        var context = new BuiltInContext();
        var typeInfo = context.GetTypeInfo(typeof(TomlTable), context.Options);
        typeInfo.Should().NotBeNull();

        using var stream = new MemoryStream();
        using (var writer = new StreamWriter(stream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false), bufferSize: 1024, leaveOpen: true))
        {
            writer.Write("a = 1\n");
        }

        stream.Position = 0;
        var table = (TomlTable)TomlSerializer.Deserialize(stream, typeInfo);
        ((long)table["a"]).Should().Be(1L);
    }

    [Fact]
    public void Deserialize_String_ObjectTypeInfo_Works()
    {
        var context = new BuiltInContext();
        var typeInfo = context.GetTypeInfo(typeof(TomlTable), context.Options);
        typeInfo.Should().NotBeNull();

        var table = (TomlTable)TomlSerializer.Deserialize("a = 1", typeInfo);
        ((long)table["a"]).Should().Be(1L);
    }
}

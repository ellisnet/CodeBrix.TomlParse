using System.IO;
using System.Text;
using CodeBrix.TomlParse.Model;
using SilverAssertions;
using Xunit;

namespace CodeBrix.TomlParse.Tests; //was previously: Tomlyn.Tests;

public sealed class NewApiUtf8StreamExceptionLocationTests
{
    [Fact]
    public void Deserialize_Stream_SyntaxError_ThrowsTomlExceptionWithLocation()
    {
        using var stream = new MemoryStream();
        using (var writer = new StreamWriter(stream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false), bufferSize: 1024, leaveOpen: true))
        {
            writer.Write("a = [\n");
        }

        stream.Position = 0;
        var ex = Assert.Throws<TomlException>(() => TomlSerializer.Deserialize<TomlTable>(stream));
        ex.Should().NotBeNull();
        ex.Span.Should().NotBeNull();

        ex.Line.Should().Be(2);
        ex.Column.Should().BeGreaterThan(0);
    }

    [Fact]
    public void Deserialize_Stream_WithBom_Succeeds()
    {
        using var stream = new MemoryStream();
        using (var writer = new StreamWriter(stream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true), bufferSize: 1024, leaveOpen: true))
        {
            writer.Write("a = 1\n");
        }

        stream.Position = 0;
        var table = TomlSerializer.Deserialize<TomlTable>(stream);

        table.Should().NotBeNull();
        ((long)table["a"]).Should().Be(1L);
    }
}

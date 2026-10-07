using System;
using CodeBrix.TomlParse.Serialization;
using SilverAssertions;
using Xunit;

namespace CodeBrix.TomlParse.Tests; //was previously: Tomlyn.Tests;

internal sealed class GeneratedBytePairModel
{
    public byte First { get; set; }

    public byte Second { get; set; }
}

[TomlSerializable(typeof(GeneratedBytePairModel))]
internal partial class TestBuiltInConvertersContext : TomlSerializerContext
{
}

public sealed class NewApiBuiltInConvertersTests
{
    private sealed class GuidModel
    {
        public Guid Id { get; set; }
    }

    private enum SampleEnum
    {
        None = 0,
        First = 1,
        Second = 2,
    }

    private sealed class EnumModel
    {
        public SampleEnum Value { get; set; }
    }

    private sealed class ByteModel
    {
        public byte B { get; set; }
    }

    private sealed class BytePairModel
    {
        public byte First { get; set; }

        public byte Second { get; set; }
    }

    private sealed class DateTimeOffsetModel
    {
        public DateTimeOffset Value { get; set; }
    }

    [Fact]
    public void Guid_Roundtrip()
    {
        var model = new GuidModel { Id = Guid.NewGuid() };
        var toml = TomlSerializer.Serialize(model);
        var roundtrip = TomlSerializer.Deserialize<GuidModel>(toml);
        roundtrip.Should().NotBeNull();
        roundtrip.Id.Should().Be(model.Id);
    }

    [Fact]
    public void Enum_Roundtrip_Integer()
    {
        var model = new EnumModel { Value = SampleEnum.Second };
        var toml = TomlSerializer.Serialize(model);
        var roundtrip = TomlSerializer.Deserialize<EnumModel>(toml);
        roundtrip.Should().NotBeNull();
        roundtrip.Value.Should().Be(model.Value);
    }

    [Fact]
    public void Enum_ReadsStringName()
    {
        var model = TomlSerializer.Deserialize<EnumModel>("Value = \"Second\"");
        model.Should().NotBeNull();
        model.Value.Should().Be(SampleEnum.Second);
    }

    [Fact]
    public void Byte_Overflow_ReportsLocation()
    {
        var ex = Assert.Throws<TomlException>(() => TomlSerializer.Deserialize<ByteModel>("B = 256\n"));
        ex.Should().NotBeNull();
        ex.Line.Should().Be(1);
        ex.Column.Should().Be(5);
    }

    [Fact]
    public void Byte_Overflow_AggregatesMultipleLocations()
    {
        var ex = Assert.Throws<TomlException>(() => TomlSerializer.Deserialize<BytePairModel>("First = 256\nSecond = 300\n"));

        ex.Should().NotBeNull();
        ex.Diagnostics.Count.Should().Be(2);
        ex.Diagnostics[0].Span.Start.Line.Should().Be(0);
        ex.Diagnostics[0].Span.Start.Column.Should().Be(8);
        ex.Diagnostics[0].Message.Should().Contain("TOML integer value 256 is out of range.");
        ex.Diagnostics[1].Span.Start.Line.Should().Be(1);
        ex.Diagnostics[1].Span.Start.Column.Should().Be(9);
        ex.Diagnostics[1].Message.Should().Contain("TOML integer value 300 is out of range.");
    }

    [Fact]
    public void Byte_Overflow_GeneratedContextAggregatesMultipleLocations()
    {
        var context = TestBuiltInConvertersContext.Default;

        var ex = Assert.Throws<TomlException>(() => TomlSerializer.Deserialize("First = 256\nSecond = 300\n", context.GeneratedBytePairModel));

        ex.Should().NotBeNull();
        ex.Diagnostics.Count.Should().Be(2);
        ex.Diagnostics[0].Span.Start.Line.Should().Be(0);
        ex.Diagnostics[0].Span.Start.Column.Should().Be(8);
        ex.Diagnostics[0].Message.Should().Contain("TOML integer value 256 is out of range.");
        ex.Diagnostics[1].Span.Start.Line.Should().Be(1);
        ex.Diagnostics[1].Span.Start.Column.Should().Be(9);
        ex.Diagnostics[1].Message.Should().Contain("TOML integer value 300 is out of range.");
    }

    [Fact]
    public void DateTimeOffset_RejectsLocalDateTime_ByDefault()
    {
        var ex = Assert.Throws<TomlException>(() => TomlSerializer.Deserialize<DateTimeOffsetModel>("Value = 1979-05-27T07:32:00\n"));
        ex.Should().NotBeNull();
        ex.Line.Should().Be(1);
        ex.Column.Should().NotBeNull();
        ex.Column.Should().BeGreaterThan(0);
    }

    private sealed class DateOnlyModel
    {
        public DateOnly Date { get; set; }
    }

    private sealed class TimeOnlyModel
    {
        public TimeOnly Time { get; set; }
    }

    [Fact]
    public void DateOnly_Roundtrip()
    {
        var model = new DateOnlyModel { Date = new DateOnly(2024, 10, 31) };
        var toml = TomlSerializer.Serialize(model);
        var roundtrip = TomlSerializer.Deserialize<DateOnlyModel>(toml);
        roundtrip.Should().NotBeNull();
        roundtrip.Date.Should().Be(model.Date);
    }

    [Fact]
    public void TimeOnly_Roundtrip_MinuteOnly()
    {
        var model = TomlSerializer.Deserialize<TimeOnlyModel>("Time = 07:32\n");
        model.Should().NotBeNull();
        model.Time.Should().Be(new TimeOnly(7, 32, 0));
    }
}

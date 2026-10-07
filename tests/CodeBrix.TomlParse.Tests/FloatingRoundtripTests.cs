// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using CodeBrix.TomlParse.Model;
using SilverAssertions;
using Xunit;

namespace CodeBrix.TomlParse.Tests; //was previously: Tomlyn.Tests;

public class FloatingRoundtripTests
{

    // CodeBrix port: member data instead of inline data - xUnit's duplicate-data analyzer
    // compares by value, and 0.0 == -0.0, so negative zero (written upstream both as -0.0 and
    // double.NegativeZero, the same value) could not be listed inline beside positive zero.
    public static readonly TheoryData<double> DoublesToRoundtrip =
    [
        0.0, -0.0, 1.0, -1.0,
        double.PositiveInfinity, double.NegativeInfinity, double.NaN,
        double.Epsilon, -double.Epsilon,
    ];

    [Theory]
    [MemberData(nameof(DoublesToRoundtrip))]
    // [TestCase(0.1)] - These fail due to float-as-double roundtrip behavior in TomlTable.
    // [TestCase(0.99)]
    // [TestCase(0.3)]
    // [TestCase(double.MinValue)]
    // [TestCase(double.MaxValue)]
    public void TestDoublesRoundtrip(double number)
    {
        // we want to increment f64 by the smallest possible value
        // and verify it changes the serialized value
        // IEEE 754 compliance means -0.0 and +0.0 are different
        // special values +inf=inf, -inf, nan=+nan, -nan
        var model = new TomlTable
        {
            ["float"] = (float)number,
            ["double"] = number
        };
        var toml = TomlSerializer.Serialize(model);
        var parsed = TomlSerializer.Deserialize<TomlTable>(toml);
        parsed.Should().NotBeNull();
        var parsedTable = parsed;
        var parsedDouble = (double)parsedTable["double"];
        var parsedFloat = (double)parsedTable["float"];
        (number == parsedDouble || double.IsNaN(number)).Should().BeTrue($"(f64->str->f64) expected {number:g30} but got {parsedDouble:g30}. \nString form: \n{toml}");
        parsedDouble.Should().Be(number);
        ((float)number == parsedFloat || double.IsNaN(number)).Should().BeTrue($"(f64->f32->str->f64->f32) expected {(float)number:g30} but got {parsedFloat:g30}. \nString form: \n{toml}");
        parsedFloat.Should().Be((float)number);
    }

    // CodeBrix port: member data instead of inline data - xUnit's duplicate-data analyzer
    // compares by value, and 0.0 == -0.0, so negative zero (written upstream both as -0.0 and
    // float.NegativeZero, the same value) could not be listed inline beside positive zero.
    public static readonly TheoryData<float> FloatsToRoundtrip =
    [
        0.0f, -0.0f, 1.0f, -1.0f,
        float.PositiveInfinity, float.NegativeInfinity, float.NaN,
        float.Epsilon, -float.Epsilon,
    ];

    [Theory]
    [MemberData(nameof(FloatsToRoundtrip))]
    // [TestCase(0.1f)] - These fail due to float-as-double roundtrip behavior in TomlTable.
    // [TestCase(0.99f)]
    // [TestCase(0.3f)]
    // [TestCase(float.MinValue)]
    // [TestCase(float.MaxValue)]
    public void TestFloatsRoundtrip(float number)
    {
        var model = new TomlTable
        {
            ["float"] = number,
            ["double"] = (double)number
        };
        var toml = TomlSerializer.Serialize(model);

        var parsed = TomlSerializer.Deserialize<TomlTable>(toml);
        parsed.Should().NotBeNull();
        var parsedTable = parsed;
        var parsedDouble = (double)parsedTable["double"];
        var parsedFloatAsDouble = (double)parsedTable["float"];
        ((double)number == parsedDouble || double.IsNaN(number)).Should().BeTrue($"(f32->f64->str->f64) expected double {(double)number:g64} but got double {parsedDouble:g64}. \nString form: \n{toml}");
        (number == (float)parsedFloatAsDouble || double.IsNaN(number)).Should().BeTrue($"(f32->str->f64->f32) expected float {number:g64} but got float {(float)parsedFloatAsDouble:g64}. \nString form: \n{toml}");

    }
}

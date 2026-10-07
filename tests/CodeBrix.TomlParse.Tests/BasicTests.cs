// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using CodeBrix.TomlParse.Model;
using CodeBrix.TomlParse.Parsing;
using SilverAssertions;
using Xunit;

namespace CodeBrix.TomlParse.Tests; //was previously: Tomlyn.Tests;

public class BasicTests
{
    [Fact]
    public void TestTableArraysContainingPrimitiveArraysSerialize()
    {
        var test = @"[[table_array]]
primitive_list = [4, 5, 6]
";

        var model = TomlSerializer.Deserialize<TomlTable>(test);
        model.Should().NotBeNull();
        var tomlOut = TomlSerializer.Serialize(model);

        tomlOut.ReplaceLineEndings("\n").Should().Be(test.ReplaceLineEndings("\n"));
    }

    [Fact]
    public void TestHelloWorld()
    {
        var toml = @"global = ""this is a string""
# This is a comment of a table
[my_table]
key = 1 # Comment a key
value = true
 list = [4, 5, 6]
";

        var model = TomlSerializer.Deserialize<TomlTable>(toml);
        model.Should().NotBeNull();
        var nonNullModel = model;
        // Prints "this is a string"
        var global = nonNullModel["global"];
        Console.WriteLine($"found global = \"{global}\"");
        global.Should().Be("this is a string");
        // Prints 1
        var key = ((TomlTable)nonNullModel["my_table"])["key"];
        Console.WriteLine($"found key = {key}");
        key.Should().Be(1L);
        // Check list
        var list = (TomlArray)((TomlTable)nonNullModel["my_table"])["list"];
        Console.WriteLine($"found list = {string.Join(", ", list)}");
        list.Should().Equal(new TomlArray() { 4, 5, 6 });
    }

    [Theory]
    [InlineData(7, 32, 0, 0)]
    [InlineData(7, 32, 0, 999)]
    [InlineData(0, 32, 0, 0)]
    public void TestLocalTime(int hour, int minute, int second, int millisecond)
    {
        var toml = $@"time = {hour:D2}:{minute:D2}:{second:D2}.{millisecond:D3}";
        var model = TomlSerializer.Deserialize<TomlTable>(toml);
        model.Should().NotBeNull();
        var localTime = (TomlDateTime)model["time"];

        localTime.DateTime.Hour.Should().Be(hour);
        localTime.DateTime.Minute.Should().Be(minute);
        localTime.DateTime.Second.Should().Be(second);
        localTime.DateTime.Millisecond.Should().Be(millisecond);
    }

    [Fact]
    public void TestEmptyComment()
    {
        var input = "#\n";
        var doc = SyntaxParser.Parse(input);
        var docAsStr = doc.ToString();
        docAsStr.Should().Be(input);
    }

    [Fact]
    public void TestIntegerOverflowIsRejected()
    {
        // TOML requires an error when an integer cannot be represented as a
        // signed 64-bit value; a positive literal in [2^63, 2^64-1] must not
        // silently wrap to a negative long.
        (SyntaxParser.Parse("a = 9223372036854775807").HasErrors).Should().BeFalse();  // long.MaxValue
        (SyntaxParser.Parse("a = -9223372036854775808").HasErrors).Should().BeFalse(); // long.MinValue
        (SyntaxParser.Parse("a = 9223372036854775808").HasErrors).Should().BeTrue();   // 2^63
        (SyntaxParser.Parse("a = 18446744073709551615").HasErrors).Should().BeTrue();  // 2^64-1
    }

    [Fact]
    public void SimpleTest()
    {
        var test = @"[table-1]
key1 = ""some string""    # This is a comment
key2 = 123
Key3 = true
Key4 = false
Key5 = +inf

[table-2]
key1 = ""another string""
key2 = 456
";
        var doc = SyntaxParser.Parse(test);
        doc.ToString().Should().Be(test);
    }

    [Fact]
    public void TestInlineArray()
    {
        var input = @"x = [1,
2,
3
]
";
        var model = TomlSerializer.Deserialize<TomlTable>(input);
        model.Should().NotBeNull();
        var array = model["x"] as TomlArray;
        array.Should().NotBeNull();
        var nonNullArray = array;
        nonNullArray.Count.Should().Be(3);
        nonNullArray[0].Should().Be(1L);
        nonNullArray[1].Should().Be(2L);
        nonNullArray[2].Should().Be(3L);
    }
}

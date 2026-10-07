// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using CodeBrix.TomlParse.Parsing;
using SilverAssertions;
using Xunit;

namespace CodeBrix.TomlParse.Tests; //was previously: Tomlyn.Tests;

public class ValidatorTests
{
    [Fact]
    public void TestNoErrors()
    {
        var input = @"a = 1";
        var doc = SyntaxParser.Parse(input);
        doc.HasErrors.Should().BeFalse("The document should not have any errors");
        var docAsStr = doc.ToString();
        docAsStr.Should().Be(input);
    }

    [Fact]
    public void TestRedefineKey()
    {
        var input = @"a = 1
a = true
";
        var doc = SyntaxParser.Parse(input);
        var roundTrip = doc.ToString();
        StandardTests.Dump(input, doc, roundTrip);
        doc.HasErrors.Should().BeTrue("The document should have errors");
    }

    [Fact]
    public void TestRedefineKeyAndTable()
    {
        var input = @"a = 1
[a]
b = 1
";
        var doc = SyntaxParser.Parse(input);
        var roundTrip = doc.ToString();
        StandardTests.Dump(input, doc, roundTrip);
        doc.HasErrors.Should().BeTrue("The document should have errors");
    }

    [Fact]
    public void TestTableArray()
    {
        var input = @"[[a]]
b = 1
[[a]]
b = true
";
        var doc = SyntaxParser.Parse(input);
        var roundTrip = doc.ToString();
        StandardTests.Dump(input, doc, roundTrip);
        doc.HasErrors.Should().BeFalse("The document should not have any errors");
    }

    [Fact]
    public void TestTableArrayNested()
    {
        var input = @"[[a]]
b = 1
[[a.c]]
b = true
";
        var doc = SyntaxParser.Parse(input);
        var roundTrip = doc.ToString();
        StandardTests.Dump(input, doc, roundTrip);
        doc.HasErrors.Should().BeFalse("The document should not have any errors");
    }

    [Fact]
    public void TestTableArrayAndInvalidTable()
    {
        var input = @"[[a]]
b = 1
[[a.b]]
c = true
[a.b]
d = true
";
        var doc = SyntaxParser.Parse(input);
        var roundTrip = doc.ToString();
        StandardTests.Dump(input, doc, roundTrip);
        doc.HasErrors.Should().BeTrue("The document should have errors");
    }

    [Fact]
    public void TestAllowTab()
    {
        var input = "";
        input += "tab_multiline_basic=\"\"\"\t\"\"\"\n";
        input += "tab_singleline_literal='\t'\n";
        input += "tab_multiline_literal='''\t'''\n";
        var doc = SyntaxParser.Parse(input);
        doc.HasErrors.Should().BeFalse("The document should not have errors");
    }

}

// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;
using System.Globalization;
using SilverAssertions;
using Xunit;

namespace CodeBrix.TomlParse.Tests; //was previously: Tomlyn.Tests;

public class TomlDateTimeTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("IV")]
    [InlineData("fr-FR")]
    [InlineData("es-ES")]
    public void TestIConvertibleToString(string cultureName)
    {
        var cultureInfo = cultureName is null ? null : new CultureInfo(cultureName);

        var dateTime = new TomlDateTime(new DateTime(2022, 1, 27), 0, TomlDateTimeKind.LocalDate);

        var converted = Convert.ToString(dateTime, cultureInfo);

        converted.Should().Be("2022-01-27");
    }
}

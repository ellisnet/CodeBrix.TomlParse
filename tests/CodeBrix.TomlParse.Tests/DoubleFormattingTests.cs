// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using SilverAssertions;
using Xunit;

namespace CodeBrix.TomlParse.Tests; //was previously: Tomlyn.Tests;

public sealed class DoubleFormattingTests
{
    private sealed class Model
    {
        public double Value { get; set; }
    }

    [Fact]
    public void SerializeDeserialize_Double_Roundtrips()
    {
        const double value = 126.92842769438576;

        var toml = TomlSerializer.Serialize(new Model { Value = value });
        var roundtrip = TomlSerializer.Deserialize<Model>(toml);

        roundtrip.Should().NotBeNull();
        roundtrip.Value.Should().Be(value);
    }
}

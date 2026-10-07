// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using CodeBrix.TomlParse.Helpers;
using CodeBrix.TomlParse.Model;
using CodeBrix.TomlParse.Parsing;
using CodeBrix.TomlParse.Syntax;
using SilverAssertions;
using Xunit;

namespace CodeBrix.TomlParse.Tests; //was previously: Tomlyn.Tests;

/// <summary>
/// Tests against the official TOML specs of TOML https://github.com/BurntSushi/toml-test
/// </summary>
public class StandardTests
{
    private const string InvalidSpec = "invalid";
    private const string ValidSpec = "valid";

    // The toml-test corpus is copied next to the test assembly (see the .csproj).
    private const string RelativeTomlTestsDirectory = "TestFiles/toml-test";

    private static readonly string[] Toml11ValidButTomlTestMarksInvalid =
    [
        // TOML v1.1.0 additions (toml-test suite still marks them invalid).
        "/invalid/datetime/no-secs.toml",            // minute-only times
        "/invalid/local-datetime/no-secs.toml",
        "/invalid/local-time/no-secs.toml",
        "/invalid/string/basic-byte-escapes.toml",   // \xHH basic string escape
        "/invalid/inline-table/trailing-comma.toml", // trailing commas in inline tables
        "/invalid/inline-table/linebreak-01.toml",   // TOML 1.1 allows newlines in inline tables
        "/invalid/inline-table/linebreak-02.toml",
        "/invalid/inline-table/linebreak-03.toml",
        "/invalid/inline-table/linebreak-04.toml",
    ];

    private static readonly string[] Toml10SpecFolders =
    [
        "/valid/spec-1.0.0/",
        "/invalid/spec-1.0.0/",
    ];

    [Theory]
    [Trait("Category", "toml-test")]
    [MemberData(nameof(ListTomlFiles), ValidSpec)]
    public static void SpecValid(string relativePath)
    {
        ValidateSpec(ValidSpec, relativePath);
    }

    [Theory]
    [Trait("Category", "toml-test")]
    [MemberData(nameof(ListTomlFiles), InvalidSpec)]
    public static void SpecInvalid(string relativePath)
    {
        ValidateSpec(InvalidSpec, relativePath);
    }

    private static void ValidateSpec(string type, string relativePath)
    {
        // The theory data carries only the corpus-relative path (short, stable test names);
        // the .toml text and, for valid cases, the expected .json are loaded here.
        var file = Path.Combine(TomlTestsDirectory, relativePath);
        var inputName = Path.GetFileName(file);
        var toml = File.ReadAllText(file);
        string json = null;
        if (type == ValidSpec)
        {
            var jsonFile = Path.ChangeExtension(file, "json");
            File.Exists(jsonFile).Should().BeTrue($"The json file `{jsonFile}` does not exist");
            json = File.ReadAllText(jsonFile, Encoding.UTF8);
        }

        var doc = SyntaxParser.Parse(toml, inputName);
        var roundtrip = doc.ToString();
        switch (type)
        {
            case ValidSpec:
                if (doc.HasErrors || toml != roundtrip)
                {
                    Console.WriteLine($"Testing {inputName}");
                    Dump(toml, doc, roundtrip);
                }
                doc.HasErrors.Should().BeFalse("Unexpected parsing errors");
                // Only in the case of a valid spec we check for rountrip
                roundtrip.Should().Be(toml, "The roundtrip doesn't match");

                // Read the original json (toml-test encodes datetimes as strings; avoid Json.NET date coercion).
                var expectedJson = NormalizeJson(ParseTomlTestJson(json));
                // Convert to the untyped model.
                var model = TomlSerializer.Deserialize<TomlTable>(toml);
                // Convert the model into the expected json
                var computedJson = ModelHelper.ToJson(model);
                AssertTomlTestJsonEquivalent(expectedJson, computedJson, inputName, toml, doc, roundtrip);

                var tomlFromModel = TomlSerializer.Serialize(model);

                var model2 = TomlSerializer.Deserialize<TomlTable>(tomlFromModel);
                var computedJson2 = ModelHelper.ToJson(model2);
                AssertTomlTestJsonEquivalent(expectedJson, computedJson2, inputName, toml, doc, roundtrip, tomlFromModel);
                break;
            case InvalidSpec:
                if (!doc.HasErrors)
                {
                    Console.WriteLine($"Testing {inputName}");
                    Dump(toml, doc, roundtrip);
                }

                doc.HasErrors.Should().BeTrue("The TOML requires parsing/validation errors");
                break;
        }

        {
            using var reader = new StringReader(toml);
            var docFromReader = SyntaxParser.Parse(reader, inputName);
            var roundtripFromReader = docFromReader.ToString();
            if (roundtrip != roundtripFromReader)
            {
                Console.WriteLine($"Testing {inputName}");
                Dump(toml, doc, roundtrip);
            }
            roundtripFromReader.Should().Be(roundtrip, "The TextReader version doesn't match with the string version");
        }
    }

    private static JsonNode NormalizeJson(JsonNode token)
    {
        if (token is JsonArray array)
        {
            var newArray = new JsonArray();
            foreach (var item in array)
            {
                newArray.Add(NormalizeJson(item));
            }

            return newArray;
        }
        else if (token is JsonObject obj)
        {
            var newObject = new JsonObject();

            var items = new List<KeyValuePair<string, JsonNode>>();
            foreach (var item in obj)
            {
                items.Add(item);
            }

            foreach (var item in items.OrderBy(x => x.Key))
            {
                newObject.Add(item.Key, NormalizeJson(item.Value));
            }

            return newObject;
        }
        else if (token is JsonValue value && value.TryGetValue<string>(out var str))
        {
            if (string.CompareOrdinal(str, "inf") == 0) return JsonValue.Create("+inf");
            if (str.Length > 0 && char.IsDigit(str[0]) && str.Contains('.') && str.Contains('e'))
            {
                if (double.TryParse(str, NumberStyles.Float, CultureInfo.InvariantCulture, out var doubleValue))
                {
                    return JsonValue.Create(TomlFormatHelper.ToString(doubleValue));
                }
            }
        }

        // A JsonNode can belong to only one parent: hand back a detached copy.
        return token?.DeepClone();
    }

    internal static void Dump(string input, DocumentSyntax doc, string roundtrip)
    {
        Console.WriteLine();
        DisplayHeader("input");
        Console.WriteLine(input);

        Console.WriteLine();
        DisplayHeader("round-trip");
        Console.WriteLine(roundtrip);

        if (doc.Diagnostics.Count > 0)
        {
            Console.WriteLine();
            DisplayHeader("messages");

            foreach (var syntaxMessage in doc.Diagnostics)
            {
                Console.WriteLine(syntaxMessage);
            }
        }
    }

    internal static void DisplayHeader(string name)
    {
        Console.WriteLine($"// ----------------------------------------------------------");
        Console.WriteLine($"// {name}");
        Console.WriteLine($"// ----------------------------------------------------------");
    }

    public static TheoryData<string> ListTomlFiles(string type)
    {
        var directory = Path.Combine(TomlTestsDirectory, type);

        Directory.Exists(directory).Should().BeTrue($"The folder `{directory}` does not exist");

        var tests = new TheoryData<string>();
        foreach (var file in Directory.EnumerateFiles(directory, "*.toml", SearchOption.AllDirectories).Order(StringComparer.Ordinal))
        {
            var normalizedFile = file.Replace('\\', '/');

            for (var i = 0; i < Toml10SpecFolders.Length; i++)
            {
                if (normalizedFile.IndexOf(Toml10SpecFolders[i], StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    goto next_file;
                }
            }

            if (type == InvalidSpec)
            {
                // The toml-test "invalid/encoding" suite validates raw UTF-8 byte-level correctness.
                // CodeBrix.TomlParse's test harness feeds TOML as text (string/TextReader), so these cases are not applicable here.
                if (normalizedFile.IndexOf("/invalid/encoding/", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    goto next_file;
                }

                for (var i = 0; i < Toml11ValidButTomlTestMarksInvalid.Length; i++)
                {
                    if (normalizedFile.EndsWith(Toml11ValidButTomlTestMarksInvalid[i], StringComparison.OrdinalIgnoreCase))
                    {
                        goto next_file;
                    }
                }
            }

            tests.Add(Path.GetRelativePath(TomlTestsDirectory, file).Replace('\\', '/'));

            next_file: ;
        }
        return tests;
    }

    private static JsonObject ParseTomlTestJson(string json)
        // System.Text.Json never coerces strings to dates, so toml-test's string-encoded
        // datetimes arrive untouched.
        => JsonNode.Parse(json).AsObject();

    private static readonly JsonSerializerOptions IndentedJson = new() { WriteIndented = true };

    private static string AsString(JsonNode node)
        => node is JsonValue value && value.TryGetValue<string>(out var text) ? text : null;

    private static void AssertTomlTestJsonEquivalent(JsonNode expected, JsonNode actual, string inputName, string toml, DocumentSyntax doc, string roundtrip, string tomlFromModel = null)
    {
        if (TryCompareTomlTestJson(expected, actual, out var difference))
        {
            return;
        }

        Console.WriteLine($"Testing {inputName}");
        Dump(toml, doc, roundtrip);
        DisplayHeader("json");
        Console.WriteLine(actual.ToJsonString(IndentedJson));
        DisplayHeader("expected json");
        Console.WriteLine(expected.ToJsonString(IndentedJson));
        if (tomlFromModel is not null)
        {
            DisplayHeader("toml from model");
            Console.WriteLine(tomlFromModel);
        }

        Assert.Fail($"TOML test JSON mismatch: {difference}");
    }

    private static bool TryCompareTomlTestJson(JsonNode expected, JsonNode actual, out string difference)
    {
        var path = "$";
        if (TryCompareTomlTestJsonCore(expected, actual, ref path, out difference))
        {
            return true;
        }

        difference = $"{path}: {difference}";
        return false;
    }

    private static bool TryCompareTomlTestJsonCore(JsonNode expected, JsonNode actual, ref string path, out string difference)
    {
        if (expected.GetValueKind() != actual.GetValueKind())
        {
            difference = $"Expected token type {expected.GetValueKind()} but was {actual.GetValueKind()}.";
            return false;
        }

        switch (expected.GetValueKind())
        {
            case JsonValueKind.Object:
                return TryCompareTomlTestJsonObject(expected.AsObject(), actual.AsObject(), ref path, out difference);
            case JsonValueKind.Array:
                return TryCompareTomlTestJsonArray(expected.AsArray(), actual.AsArray(), ref path, out difference);
            default:
                if (JsonNode.DeepEquals(expected, actual))
                {
                    difference = string.Empty;
                    return true;
                }

                difference = $"Expected `{expected}` but was `{actual}`.";
                return false;
        }
    }

    private static bool TryCompareTomlTestJsonObject(JsonObject expected, JsonObject actual, ref string path, out string difference)
    {
        if (TryGetTomlTestTypedValue(expected, out var expectedType, out var expectedValue))
        {
            if (!TryGetTomlTestTypedValue(actual, out var actualType, out var actualValue))
            {
                difference = "Expected a typed TOML value object.";
                return false;
            }

            if (!string.Equals(expectedType, actualType, StringComparison.Ordinal))
            {
                difference = $"Expected type `{expectedType}` but was `{actualType}`.";
                return false;
            }

            return expectedType switch
            {
                "string" => CompareString(expectedValue, actualValue, out difference),
                "bool" => CompareBool(expectedValue, actualValue, out difference),
                "integer" => CompareInteger(expectedValue, actualValue, out difference),
                "float" => CompareFloat(expectedValue, actualValue, out difference),
                "datetime" => CompareDateTime(expectedValue, actualValue, out difference),
                "array" => TryCompareTomlTestJsonCore(expectedValue, actualValue, ref path, out difference),
                _ => TryCompareTomlTestJsonCore(expectedValue, actualValue, ref path, out difference),
            };
        }

        var expectedProperties = expected.ToList();
        var actualProperties = actual.ToDictionary(p => p.Key, p => p.Value, StringComparer.Ordinal);

        foreach (var expectedProperty in expectedProperties)
        {
            if (!actualProperties.TryGetValue(expectedProperty.Key, out var actualValue))
            {
                difference = $"Missing property `{expectedProperty.Key}`.";
                return false;
            }

            var previous = path;
            path = $"{path}.{expectedProperty.Key}";
            if (!TryCompareTomlTestJsonCore(expectedProperty.Value, actualValue, ref path, out difference))
            {
                return false;
            }
            path = previous;
        }

        if (actualProperties.Count != expectedProperties.Count)
        {
            var extra = actualProperties.Keys.Except(expectedProperties.Select(p => p.Key), StringComparer.Ordinal).FirstOrDefault();
            difference = extra is null ? "Object size mismatch." : $"Unexpected property `{extra}`.";
            return false;
        }

        difference = string.Empty;
        return true;
    }

    private static bool TryGetTomlTestTypedValue(JsonObject obj, out string type, out JsonNode value)
    {
        type = string.Empty;
        value = default;

        if (!obj.TryGetPropertyValue("type", out var typeToken) || !obj.TryGetPropertyValue("value", out var valueToken))
        {
            return false;
        }

        if (typeToken is null || typeToken.GetValueKind() != JsonValueKind.String)
        {
            return false;
        }

        type = AsString(typeToken) ?? string.Empty;
        value = valueToken;
        return type.Length > 0;
    }

    private static bool TryCompareTomlTestJsonArray(JsonArray expected, JsonArray actual, ref string path, out string difference)
    {
        if (expected.Count != actual.Count)
        {
            difference = $"Expected array length {expected.Count} but was {actual.Count}.";
            return false;
        }

        for (var index = 0; index < expected.Count; index++)
        {
            var previous = path;
            path = $"{path}[{index}]";
            if (!TryCompareTomlTestJsonCore(expected[index], actual[index], ref path, out difference))
            {
                return false;
            }
            path = previous;
        }

        difference = string.Empty;
        return true;
    }

    private static bool CompareString(JsonNode expected, JsonNode actual, out string difference)
    {
        var expectedValue = AsString(expected);
        var actualValue = AsString(actual);
        if (string.Equals(expectedValue, actualValue, StringComparison.Ordinal))
        {
            difference = string.Empty;
            return true;
        }

        difference = $"Expected string `{expectedValue}` but was `{actualValue}`.";
        return false;
    }

    private static bool CompareBool(JsonNode expected, JsonNode actual, out string difference)
    {
        if (!TryParseBool(AsString(expected), out var expectedValue) ||
            !TryParseBool(AsString(actual), out var actualValue))
        {
            difference = "Invalid boolean representation.";
            return false;
        }

        if (expectedValue == actualValue)
        {
            difference = string.Empty;
            return true;
        }

        difference = $"Expected bool `{expectedValue}` but was `{actualValue}`.";
        return false;
    }

    private static bool CompareInteger(JsonNode expected, JsonNode actual, out string difference)
    {
        if (!long.TryParse(AsString(expected), NumberStyles.Integer, CultureInfo.InvariantCulture, out var expectedValue) ||
            !long.TryParse(AsString(actual), NumberStyles.Integer, CultureInfo.InvariantCulture, out var actualValue))
        {
            difference = "Invalid integer representation.";
            return false;
        }

        if (expectedValue == actualValue)
        {
            difference = string.Empty;
            return true;
        }

        difference = $"Expected integer `{expectedValue}` but was `{actualValue}`.";
        return false;
    }

    private static bool CompareFloat(JsonNode expected, JsonNode actual, out string difference)
    {
        if (!TryParseFloat(AsString(expected), out var expectedValue) ||
            !TryParseFloat(AsString(actual), out var actualValue))
        {
            difference = "Invalid float representation.";
            return false;
        }

        if (double.IsNaN(expectedValue) && double.IsNaN(actualValue))
        {
            difference = string.Empty;
            return true;
        }

        if (expectedValue.Equals(actualValue))
        {
            difference = string.Empty;
            return true;
        }

        difference = $"Expected float `{expected}` but was `{actual}`.";
        return false;
    }

    private static bool CompareDateTime(JsonNode expected, JsonNode actual, out string difference)
    {
        var expectedText = AsString(expected);
        var actualText = AsString(actual);

        if (string.Equals(expectedText, actualText, StringComparison.Ordinal))
        {
            difference = string.Empty;
            return true;
        }

        if (expectedText is not null && actualText is not null)
        {
            var expectedNormalized = NormalizeTomlTestDateTimeText(expectedText);
            var actualNormalized = NormalizeTomlTestDateTimeText(actualText);
            if (string.Equals(expectedNormalized, actualNormalized, StringComparison.Ordinal))
            {
                difference = string.Empty;
                return true;
            }
        }

        difference = $"Expected datetime `{expectedText}` but was `{actualText}`.";
        return false;
    }

    private static string NormalizeTomlTestDateTimeText(string text)
    {
        // toml-test encodes timestamps as RFC3339 strings, often normalizing fractional seconds
        // to millisecond precision (e.g. ".600"). CodeBrix.TomlParse may preserve the original precision
        // (e.g. ".6"). Normalize by trimming trailing zeros from the fractional part.
        var dotIndex = text.IndexOf('.');
        if (dotIndex < 0)
        {
            return text;
        }

        var index = dotIndex + 1;
        while (index < text.Length && char.IsDigit(text[index]))
        {
            index++;
        }

        if (index == dotIndex + 1)
        {
            return text;
        }

        var fracEnd = index;
        while (fracEnd > dotIndex + 1 && text[fracEnd - 1] == '0')
        {
            fracEnd--;
        }

        if (fracEnd == dotIndex + 1)
        {
            // Fraction becomes empty: remove the '.' as well.
            return text.Substring(0, dotIndex) + text.Substring(index);
        }

        if (fracEnd == index)
        {
            return text;
        }

        return text.Substring(0, fracEnd) + text.Substring(index);
    }

    private static bool TryParseBool(string text, out bool value)
    {
        if (string.Equals(text, "true", StringComparison.OrdinalIgnoreCase))
        {
            value = true;
            return true;
        }

        if (string.Equals(text, "false", StringComparison.OrdinalIgnoreCase))
        {
            value = false;
            return true;
        }

        value = default;
        return false;
    }

    private static bool TryParseFloat(string text, out double value)
    {
        if (text is null)
        {
            value = default;
            return false;
        }

        if (string.Equals(text, "nan", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(text, "+nan", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(text, "-nan", StringComparison.OrdinalIgnoreCase))
        {
            value = double.NaN;
            return true;
        }

        if (string.Equals(text, "inf", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(text, "+inf", StringComparison.OrdinalIgnoreCase))
        {
            value = double.PositiveInfinity;
            return true;
        }

        if (string.Equals(text, "-inf", StringComparison.OrdinalIgnoreCase))
        {
            value = double.NegativeInfinity;
            return true;
        }

        // toml-test values are typically JSON numbers-as-strings, sometimes using normalized exponent forms.
        // Double parsing already accepts both "1e06" and "1e+06".
        return double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
    }

    private static string TomlTestsDirectory
        => Path.Combine(AppContext.BaseDirectory, RelativeTomlTestsDirectory);
}

# CodeBrix.TomlParse

A fully managed, high-performance TOML library for .NET - read, parse, validate, update and create `.toml` files.
CodeBrix.TomlParse is provided as a .NET 10 library and associated `CodeBrix.TomlParse.BsdLicenseForever` NuGet package.

CodeBrix.TomlParse supports applications and assemblies that target Microsoft .NET version 10.0 and later.
Microsoft .NET version 10.0 is a Long-Term Supported (LTS) version of .NET, and was released on Nov 11, 2025; and will be actively supported by Microsoft until Nov 14, 2028.
Please update your C#/.NET code and projects to the latest LTS version of Microsoft .NET.

## Installation

```
dotnet add package CodeBrix.TomlParse.BsdLicenseForever
```

Note that the NuGet package ID and the namespace are different - there is no package named plain `CodeBrix.TomlParse`:

* NuGet package ID: `CodeBrix.TomlParse.BsdLicenseForever`
* Assembly and primary namespace: `CodeBrix.TomlParse` - i.e. `using CodeBrix.TomlParse;`

XML documentation (IntelliSense) ships alongside the assembly.

CodeBrix.TomlParse has no dependencies other than .NET. Its source generator ships inside the package and is used by the compiler only.

## CodeBrix.TomlParse supports:

* The TOML 1.1 specification, verified against a standard TOML conformance test corpus
* A `System.Text.Json`-style object serializer - `TomlSerializer.Deserialize<T>` / `TomlSerializer.Serialize` - for your own classes and records
* An untyped document model (`TomlTable`, `TomlArray`, `TomlTableArray`) for reading and editing TOML of any shape
* A lossless, round-trippable syntax tree that preserves every comment and whitespace character - for surgical, byte-exact edits
* Keeping comments across a read/modify/write cycle with a metadata store
* Naming policies (snake_case, camelCase, kebab-case), case-insensitive matching, and TOML-specific plus `System.Text.Json` attributes
* Custom converters, converter factories, polymorphism with type discriminators, extension data and serialization callbacks
* A source generator for trimming- and NativeAOT-safe serialization, included in the package
* Precise error reporting with file name, line and column
* Output formatting control: inline tables, arrays of tables, string styles, key ordering, dotted keys, indentation and line endings
* An incremental lexer and parser for tools that only scan documents

## Sample Code

### Read, update and write a TOML file

```csharp
using System;
using System.IO;
using CodeBrix.TomlParse;
using CodeBrix.TomlParse.Model;

var text = File.ReadAllText("settings.toml");
TomlTable doc = TomlSerializer.Deserialize<TomlTable>(text);

var title = (string)doc["title"];
var database = (TomlTable)doc["database"];
var port = (long)database["port"];          // TOML integers are long
Console.WriteLine($"{title}: port {port}");

database["port"] = 6543L;
doc["owner"] = new TomlTable { ["name"] = "Ada" };
File.WriteAllText("settings.toml", TomlSerializer.Serialize(doc));
```

### Map TOML onto your own classes

```csharp
using System.Collections.Generic;
using System.Text.Json;
using CodeBrix.TomlParse;

public sealed class ServerConfig
{
    public string ServerName { get; set; } = "";
    public int MaxConnections { get; set; } = 100;
    public List<Route> Routes { get; set; } = new();
}

public sealed class Route
{
    public string Path { get; set; } = "";
}

var options = new TomlSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower };

var config = TomlSerializer.Deserialize<ServerConfig>("""
    server_name = "alpha"
    max_connections = 10

    [[routes]]
    path = "/"
    """, options);

string toml = TomlSerializer.Serialize(config, options);
```

### Edit a file without disturbing its comments or layout

```csharp
using System.IO;
using System.Linq;
using CodeBrix.TomlParse.Parsing;
using CodeBrix.TomlParse.Syntax;

var syntax = SyntaxParser.Parse(File.ReadAllText("settings.toml"), sourceName: "settings.toml");
var portLine = syntax.Tables.OfType<TableSyntax>()
    .First(t => t.Name.ToString().Trim() == "database")
    .Items.First(kv => kv.Key.ToString().Trim() == "port");

var integer = (IntegerValueSyntax)portLine.Value;
var oldToken = integer.Token;
integer.Token = new SyntaxToken(TokenKind.Integer, "6543")
{
    LeadingTrivia = oldToken.LeadingTrivia,
    TrailingTrivia = oldToken.TrailingTrivia,
};
integer.Value = 6543;

File.WriteAllText("settings.toml", syntax.ToString()); // everything else is byte-for-byte unchanged
```

### Source-generated serialization (trimming / NativeAOT)

```csharp
using System.Text.Json.Serialization;
using CodeBrix.TomlParse;
using CodeBrix.TomlParse.Serialization;

[TomlSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower)]
[TomlSerializable(typeof(ServerConfig))]
internal partial class AppTomlContext : TomlSerializerContext
{
}

ServerConfig config = TomlSerializer.Deserialize(toml, AppTomlContext.Default.ServerConfig);
```

## Documentation

The NuGet package includes `AGENT-README.txt`, a complete API reference and usage guide written for AI coding agents - point your agent at that file when it is writing code against this library.

Additional sample code and usage examples are available in the `CodeBrix.TomlParse.Tests` project:
https://github.com/ellisnet/CodeBrix.TomlParse/tree/main/tests/CodeBrix.TomlParse.Tests

## License

CodeBrix.TomlParse is licensed under the BSD 2-Clause License - see the
[LICENSE](https://github.com/ellisnet/CodeBrix.TomlParse/blob/main/LICENSE) file.

For licensing and provenance information about the open source code included in
this package, see [THIRD-PARTY-NOTICES.txt](https://github.com/ellisnet/CodeBrix.TomlParse/blob/main/THIRD-PARTY-NOTICES.txt).

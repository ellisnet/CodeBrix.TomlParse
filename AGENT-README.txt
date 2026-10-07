================================================================================
AGENT-README: CodeBrix.TomlParse
A Guide for AI Coding Agents - CONSUMING the
CodeBrix.TomlParse.BsdLicenseForever NuGet package
================================================================================

OVERVIEW
========
CodeBrix.TomlParse is a fully managed TOML 1.1 library for .NET 10 or later.
Use it whenever a program has to read, parse, validate, update or create a
.toml file. It offers four layers, from highest to lowest level:

  1. An object serializer (TomlSerializer) in the style of
     System.Text.Json: TOML text <-> your own C# classes and records, with
     naming policies, attributes, converters, polymorphism, extension data
     and precise error locations.
  2. An untyped document model (TomlTable / TomlArray / TomlTableArray) for
     working with TOML whose shape you do not know or do not want to model.
  3. A lossless syntax tree (SyntaxParser -> DocumentSyntax) that keeps
     every comment, blank line and whitespace character, so a file can be
     edited surgically and written back byte-for-byte apart from the edit.
  4. An incremental lexer and parser (TomlLexer, TomlParser) for tools that
     want tokens or parse events.

A Roslyn source generator ships inside the package. It emits serialization
metadata for [TomlSerializable] contexts, which is the trimming- and
NativeAOT-safe path for typed (de)serialization. Nothing extra has to be
installed to use it.

CodeBrix.TomlParse is a port of the Tomlyn library. Every namespace is
rooted at "CodeBrix.TomlParse", mapped from upstream like this:

    Tomlyn                -> CodeBrix.TomlParse
    Tomlyn.Model          -> CodeBrix.TomlParse.Model
    Tomlyn.Parsing        -> CodeBrix.TomlParse.Parsing
    Tomlyn.Serialization  -> CodeBrix.TomlParse.Serialization
    Tomlyn.Syntax         -> CodeBrix.TomlParse.Syntax
    Tomlyn.Text           -> CodeBrix.TomlParse.Text
    Tomlyn.Helpers        -> CodeBrix.TomlParse.Helpers

Type names are unchanged (TomlSerializer, TomlTable, ...). The generator's
diagnostics are TOMLPARSE001-TOMLPARSE011, and the NativeAOT feature switch is
"CodeBrix.TomlParse.TomlSerializer.IsReflectionEnabledByDefault". Do NOT use
the upstream namespaces or names - they do not exist in this package.


INSTALLATION
============
PackageId:  CodeBrix.TomlParse.BsdLicenseForever

    dotnet add package CodeBrix.TomlParse.BsdLicenseForever

There is no package named plain "CodeBrix.TomlParse"; that is the assembly
and root namespace name.

Dependencies:  none other than .NET itself. The package's source generator
               is loaded by the compiler only; it adds no runtime dependency.
License:       BSD-2-Clause
Requirements:  a project targeting net10.0 or later. The source generator
               needs an up-to-date .NET 10 SDK: its C# compiler must be at
               least as new as the one the generator was built against. On
               an older SDK the reflection-based and document-model APIs
               still work, but [TomlSerializable] contexts are not generated.


KEY NAMESPACES / USINGS
=======================
    using CodeBrix.TomlParse;                // TomlSerializer, TomlSerializerOptions,
                                             // TomlException, TomlDateTime, TomlTypeInfo<T>,
                                             // option enums (TomlIgnoreCondition, ...)
    using CodeBrix.TomlParse.Model;          // TomlTable, TomlArray, TomlTableArray,
                                             // TomlObject, metadata types
    using CodeBrix.TomlParse.Serialization;  // attributes, TomlConverter<T>,
                                             // TomlSerializerContext, TomlMetadataStore,
                                             // TomlReader, TomlWriter, callbacks
    using CodeBrix.TomlParse.Parsing;        // SyntaxParser, TomlLexer, TomlParser
    using CodeBrix.TomlParse.Syntax;         // DocumentSyntax and every syntax node
    using CodeBrix.TomlParse.Text;           // TomlSourceSpan, TomlTextPosition
    using CodeBrix.TomlParse.Helpers;        // TomlFormatHelper (TOML literal formatting)

Naming policies and several attributes are shared with System.Text.Json, so
typed code usually also needs:

    using System.Text.Json;                  // JsonNamingPolicy
    using System.Text.Json.Serialization;    // JsonKnownNamingPolicy, [JsonPropertyName], ...


CORE API REFERENCE
==================

TomlSerializer (static class, namespace CodeBrix.TomlParse)
-----------------------------------------------------------
Every method exists for string, Stream (UTF-8) and TextReader input, and for
string, Stream and TextWriter output.

  T      Deserialize<T>(string toml, TomlSerializerOptions options = null)
  T      Deserialize<T>(string toml, TomlSerializerContext context)
  T      Deserialize<T>(string toml, TomlTypeInfo<T> typeInfo)
  object Deserialize(string toml, Type returnType, TomlSerializerOptions options = null)
  object Deserialize(string toml, Type returnType, TomlSerializerContext context)
  object Deserialize(string toml, TomlTypeInfo typeInfo)
         (same overloads with Stream stream / TextReader reader as first arg)

  bool   TryDeserialize<T>(string toml, out T value, TomlSerializerOptions options = null)
  bool   TryDeserialize<T>(string toml, TomlSerializerContext context, out T value)
  bool   TryDeserialize(string toml, Type returnType, out object value,
                        TomlSerializerOptions options = null)
  bool   TryDeserialize(string toml, Type returnType,
                        TomlSerializerContext context, out object value)
         (same overloads with Stream / TextReader input)
         Returns false (value = default) instead of throwing when the TOML
         is invalid or cannot be mapped.

  string Serialize<T>(T value, TomlSerializerOptions options = null)
  string Serialize<T>(T value, TomlSerializerContext context)
  string Serialize<T>(T value, TomlTypeInfo<T> typeInfo)
  string Serialize(object value, Type inputType, TomlSerializerOptions options = null)
  string Serialize(object value, Type inputType, TomlSerializerContext context)
  string Serialize(object value, TomlTypeInfo typeInfo)
  void   Serialize<T>(Stream stream, T value, ...)      // same option/context/typeInfo forms
  void   Serialize<T>(TextWriter writer, T value, ...)
  void   Serialize(Stream stream, object value, Type inputType, ...)
  void   Serialize(TextWriter writer, object value, Type inputType, ...)

Deserialize<TomlTable>(...) gives the untyped document model; Serialize of a
TomlTable writes it back out.

TomlSerializerOptions (record, init-only properties)
----------------------------------------------------
  TomlSerializerOptions.Default          shared default instance
  PropertyNamingPolicy        JsonNamingPolicy   null = CLR names as-is (case-sensitive!)
                                                 e.g. JsonNamingPolicy.SnakeCaseLower,
                                                 CamelCase, KebabCaseLower
  DictionaryKeyPolicy         JsonNamingPolicy   null; applied to dictionary keys when writing
  PropertyNameCaseInsensitive bool               false; case-insensitive key matching on read
  PreferredObjectCreationHandling  JsonObjectCreationHandling  Replace (or Populate)
  DefaultIgnoreCondition      TomlIgnoreCondition  WhenWritingNull
  DuplicateKeyHandling        TomlDuplicateKeyHandling  Error
  MappingOrder                TomlMappingOrderPolicy    Declaration
  DottedKeyHandling           TomlDottedKeyHandling     Literal (or Expand)
  RootValueHandling           TomlRootValueHandling     Error (non-table roots)
  RootValueKeyName            string             "value" (used by WrapInRootKey)
  WriteIndented               bool               true
  IndentSize                  int                2
  NewLine                     TomlNewLineKind    Lf (or CrLf)
  InlineTablePolicy           TomlInlineTablePolicy  Never (WhenSmall, Always)
  TableArrayStyle             TomlTableArrayStyle    Headers ([[a]]) or InlineArrayOfTables
  StringStylePreferences      TomlStringStylePreferences  basic/literal/multiline choices
  MaxDepth                    int                0 = built-in default of 64
  Converters                  IReadOnlyList<TomlConverter>  custom converters
  PolymorphismOptions         TomlPolymorphismOptions  discriminator name, runtime mappings
  MetadataStore               ITomlMetadataStore  null; capture comments/spans on read
                                                 and replay them on write
  SourceName                  string             file name used in error messages
  TypeInfoResolver            ITomlTypeInfoResolver  advanced: custom metadata resolver

Because options is a record, derive variants with `options with { ... }`.

Document model (namespace CodeBrix.TomlParse.Model)
---------------------------------------------------
  TomlTable        IDictionary<string, object>, insertion-ordered.
                   new TomlTable()             a [table]
                   new TomlTable(inline: true) an inline { ... } table
                   table["key"], Add, Remove, ContainsKey, TryGetValue, Keys, Count
  TomlArray        IList<object>   a TOML array  [1, 2, 3]
  TomlTableArray   IList<TomlTable> an array of tables  [[servers]]
  TomlObject       common base of the three containers (Kind: ObjectKind)

  Scalar values are plain .NET values:
      TOML string   -> string
      TOML integer  -> long        (always long - cast with (long), not (int))
      TOML float    -> double
      TOML boolean  -> bool
      date / time   -> TomlDateTime (namespace CodeBrix.TomlParse)

  When BUILDING a table, store integers as long (5432L) so they round-trip
  as TOML integers.

TomlDateTime (record struct, namespace CodeBrix.TomlParse)
----------------------------------------------------------
  new TomlDateTime(int year, int month, int day)        a local date
  new TomlDateTime(DateTime datetime)                   a local date-time
  new TomlDateTime(DateTimeOffset DateTime, int SecondPrecision, TomlDateTimeKind Kind)
                                                        any kind, explicit precision
  .DateTime (DateTimeOffset), .Kind, .SecondPrecision, .ToString()
  TomlDateTimeKind: OffsetDateTimeByZ (…Z), OffsetDateTimeByNumber (…+02:00),
                    LocalDateTime, LocalDate, LocalTime

Attributes (namespace CodeBrix.TomlParse.Serialization)
-------------------------------------------------------
Member-level:
  [TomlPropertyName("key")]     rename (also honours [JsonPropertyName])
  [TomlIgnore]                  skip (Condition = Always (default) / Never /
                                WhenWritingNull / WhenWritingDefault; also
                                [JsonIgnore])
  [TomlInclude]                 include a non-public member ([JsonInclude])
  [TomlPropertyOrder(n)]        ordering ([JsonPropertyOrder])
  [TomlRequired]                must be present on read ([JsonRequired])
  [TomlExtensionData]           IDictionary<string, object> catches unknown keys
                                ([JsonExtensionData])
  [TomlSingleOrArray]           collection accepts `x = "a"` or `x = ["a", "b"]`
  [TomlTableArrayStyle(...)]    per-member [[a]] vs a = [{...}]
  [TomlInlineTable]             per-member inline-table output
  [TomlStringStyle(...)]        per-member string style (basic, literal, multiline)
  [TomlConverter(typeof(C))]    per-member/type converter (reflection path only)
  [JsonObjectCreationHandling(...)]  replace vs populate for one member
Type-level:
  [TomlConstructor]             pick the deserialization constructor ([JsonConstructor])
  [TomlMappingOrder(...)]       member ordering policy for the type
  [TomlDottedKeyHandling(...)]  dotted-key handling for the type's members
  [TomlPolymorphic] + [TomlDerivedType(typeof(D), "tag")]   polymorphism
                                ([JsonPolymorphic] / [JsonDerivedType] also work)
Context-level (source generation):
  [TomlSerializable(typeof(Root))]                 a root type to generate for
  [TomlSourceGenerationOptions(...)]               compile-time options, e.g.
        PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower,
        Converters = [typeof(MyConverter)]
  [TomlDerivedTypeMapping(typeof(Base), typeof(Derived), "tag")]
TOML-specific attributes win when both a Toml* and a Json* attribute are present.

Source generation (TomlSerializerContext)
-----------------------------------------
    [TomlSerializable(typeof(MyConfig))]
    internal partial class MyTomlContext : TomlSerializerContext { }

The generator adds MyTomlContext.Default and one TomlTypeInfo<T> property
per root (and per type reachable from a root), named after the type:
MyTomlContext.Default.MyConfig. Pass that TomlTypeInfo<T> (or the context)
to TomlSerializer. The context class MUST be partial (else TOMLPARSE001).
Generated code compiles cleanly whether or not the consuming project enables
nullable reference types.

Converters
----------
    public sealed class UpperCaseConverter : TomlConverter<string>
    {
        public override string Read(TomlReader reader) => reader.GetString().ToUpperInvariant();
        public override void Write(TomlWriter writer, string value) => writer.WriteStringValue(value);
    }
  Register via options (Converters = [new UpperCaseConverter()]), via
  [TomlConverter(typeof(...))] (reflection), or via
  [TomlSourceGenerationOptions(Converters = [typeof(...)])] (generated).
  TomlConverterFactory handles open generic / runtime-inspected types.
  TomlReader: Read(), TokenType, GetString(), GetInt64(), GetDouble(),
    GetDecimal(), GetBoolean(), GetTomlDateTime(), GetRawText(), Skip(),
    PropertyNameEquals(string), CreateException(string).
  TomlWriter: WritePropertyName, WriteStringValue, WriteIntegerValue,
    WriteFloatValue, WriteBooleanValue, WriteDateTimeValue,
    WriteStartTable/WriteEndTable, WriteStartInlineTable/WriteEndInlineTable,
    WriteStartArray/WriteEndArray, WriteStartTableArray/WriteEndTableArray.

Callbacks
---------
  ITomlOnSerializing.OnTomlSerializing(), ITomlOnSerialized.OnTomlSerialized(),
  ITomlOnDeserializing.OnTomlDeserializing(), ITomlOnDeserialized.OnTomlDeserialized()
  - implement on your type; typical use is validation after reading.

Errors (TomlException, namespace CodeBrix.TomlParse)
----------------------------------------------------
  Thrown for syntax errors and mapping errors.
  .SourceName  file name (from TomlSerializerOptions.SourceName)
  .Line        1-based line   (int?)
  .Column      1-based column (int?)
  .Offset      0-based character offset (int?)
  .Span        TomlSourceSpan? (start/end positions)
  .Diagnostics DiagnosticsBag with every collected message
  Message format: "file.toml(line,col) : error : text".

Comments across a read/modify/write cycle (TomlMetadataStore)
-------------------------------------------------------------
    var store = new TomlMetadataStore();
    var options = new TomlSerializerOptions { MetadataStore = store };
    var table = TomlSerializer.Deserialize<TomlTable>(toml, options);
    // ... modify table ...
    var updated = TomlSerializer.Serialize(table, options);  // comments re-emitted
  Works for TomlTable and for your own classes. It preserves comments and
  number formats (e.g. hex integers), not exact blank lines or spacing. For a
  byte-exact edit use the syntax tree.
  store.TryGetProperties(obj, out TomlPropertiesMetadata md) and
  md.TryGetProperty("key", out TomlPropertyMetadata p) expose LeadingTrivia,
  TrailingTrivia, Span and DisplayKind per key.

Syntax tree (namespaces CodeBrix.TomlParse.Parsing / .Syntax)
-------------------------------------------------------------
  DocumentSyntax SyntaxParser.Parse(string toml, string sourceName = null,
                                    bool validate = true)
  DocumentSyntax SyntaxParser.Parse(TextReader reader, string sourceName = null,
                                    bool validate = true)
  DocumentSyntax SyntaxParser.ParseStrict(string toml, string sourceName = null,
                                          bool validate = true)
                 (Parse is tolerant: errors go to doc.Diagnostics. ParseStrict
                 throws TomlException on the first error.)
  doc.ToString()   reproduces the source text exactly (after any edits)
  doc.HasErrors, doc.Diagnostics (DiagnosticsBag of DiagnosticMessage)
  doc.KeyValues    SyntaxList<KeyValueSyntax>  top-level key = value lines
  doc.Tables       SyntaxList<TableSyntaxBase> [table] / [[array]] sections
  TableSyntaxBase: .Name (KeySyntax), .Items (SyntaxList<KeyValueSyntax>)
  KeyValueSyntax:  .Key (KeySyntax), .Value (ValueSyntax)
  Values: StringValueSyntax, IntegerValueSyntax, FloatValueSyntax,
          BooleanValueSyntax, DateTimeValueSyntax, ArraySyntax, InlineTableSyntax
          - each scalar has .Value and a .Token (SyntaxToken) whose text is
          what ToString() writes.
  Trivia (comments, whitespace) hangs off tokens: SyntaxNode.LeadingTrivia /
  TrailingTrivia (List<SyntaxTrivia>).
  Traversal: node.Descendants(), node.Tokens(), or subclass SyntaxVisitor.
  Building: new KeyValueSyntax("key", new StringValueSyntax("v")),
            new TableSyntax("name"), SyntaxList<T>.Add / RemoveChildAt.

Low level (namespace CodeBrix.TomlParse.Parsing)
------------------------------------------------
  TomlLexer   tokenizer over a string/TextReader (TomlLexerOptions, TomlToken)
  TomlParser  TomlParser.Create(toml, TomlParserOptions, TomlSerializerOptions);
              while (parser.MoveNext()) { var e = parser.Current; /* TomlParseEvent */ }
              TomlParserOptions.Mode = TomlParserMode.Strict or Tolerant.


COMPLETE EXAMPLES
=================
Every example below compiles and runs against the current package.

Example 1 - Read, update and write a .toml file with the document model
-----------------------------------------------------------------------
    using System;
    using System.IO;
    using CodeBrix.TomlParse;
    using CodeBrix.TomlParse.Model;

    var path = "settings.toml";
    var options = new TomlSerializerOptions { SourceName = path };
    var text = File.ReadAllText(path);
    TomlTable doc = TomlSerializer.Deserialize<TomlTable>(text, options);

    var title = (string)doc["title"];
    var db = (TomlTable)doc["database"];
    var port = (long)db["port"];                 // integers are long
    var ports = (TomlArray)db["ports"];
    Console.WriteLine($"{title}: {db["host"]}:{port} ({ports.Count} ports)");

    db["port"] = 6543L;
    db["enabled"] = true;
    doc["owner"] = new TomlTable { ["name"] = "Ada" };
    File.WriteAllText(path, TomlSerializer.Serialize(doc));   // comments dropped - see Example 2

Example 2 - Update while keeping comments
-----------------------------------------
    using CodeBrix.TomlParse;
    using CodeBrix.TomlParse.Model;
    using CodeBrix.TomlParse.Serialization;

    var store = new TomlMetadataStore();
    var keepComments = new TomlSerializerOptions { MetadataStore = store };
    var text = File.ReadAllText(path);
    var table = TomlSerializer.Deserialize<TomlTable>(text, keepComments);
    ((TomlTable)table["database"])["port"] = 6543L;
    File.WriteAllText(path, TomlSerializer.Serialize(table, keepComments));
    // "# comment" lines and trailing "# comments" survive; blank lines may not.

Example 3 - Byte-exact surgical edit with the syntax tree
---------------------------------------------------------
    using System.Linq;
    using CodeBrix.TomlParse.Parsing;
    using CodeBrix.TomlParse.Syntax;

    var syntax = SyntaxParser.Parse(File.ReadAllText(path), sourceName: path);
    if (syntax.HasErrors)
    {
        foreach (var message in syntax.Diagnostics) Console.WriteLine(message);
        return;
    }
    var dbTable = syntax.Tables.OfType<TableSyntax>()
        .First(t => t.Name.ToString().Trim() == "database");
    var portKv = dbTable.Items.First(kv => kv.Key.ToString().Trim() == "port");
    var integer = (IntegerValueSyntax)portKv.Value;
    var oldToken = integer.Token;
    integer.Token = new SyntaxToken(TokenKind.Integer, "6543")
    {
        LeadingTrivia = oldToken.LeadingTrivia,   // keeps the space after '='
        TrailingTrivia = oldToken.TrailingTrivia, // keeps " # comment"
    };
    integer.Value = 6543;
    File.WriteAllText(path, syntax.ToString()); // everything else is untouched

Example 4 - Typed configuration classes
---------------------------------------
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

    var options = new TomlSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
    };
    var cfg = TomlSerializer.Deserialize<ServerConfig>("""
        server_name = "alpha"
        max_connections = 10
        [[routes]]
        path = "/"
        [[routes]]
        path = "/api"
        """, options);
    string toml = TomlSerializer.Serialize(cfg, options);

Example 5 - Source-generated (trimming / NativeAOT safe)
--------------------------------------------------------
    using System.Text.Json.Serialization;
    using CodeBrix.TomlParse;
    using CodeBrix.TomlParse.Serialization;

    [TomlSourceGenerationOptions(
        PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower)]
    [TomlSerializable(typeof(ServerConfig))]
    internal partial class AppTomlContext : TomlSerializerContext
    {
    }

    ServerConfig cfg = TomlSerializer.Deserialize(toml, AppTomlContext.Default.ServerConfig);
    string text = TomlSerializer.Serialize(cfg, AppTomlContext.Default.ServerConfig);

Example 6 - Error handling
--------------------------
    using CodeBrix.TomlParse;
    using CodeBrix.TomlParse.Model;

    try
    {
        var badOptions = new TomlSerializerOptions { SourceName = "bad.toml" };
        TomlSerializer.Deserialize<TomlTable>("a = \n", badOptions);
    }
    catch (TomlException ex)
    {
        Console.WriteLine($"{ex.SourceName} line {ex.Line} col {ex.Column}");
        // ex.Message: "bad.toml(1,5) : error : Missing value after `=`."
    }

    if (!TomlSerializer.TryDeserialize<ServerConfig>(userText, out var config, options))
    {
        // invalid TOML or wrong shape - config is null
    }

Example 7 - Create a new TOML document from scratch
---------------------------------------------------
    using System;
    using CodeBrix.TomlParse;
    using CodeBrix.TomlParse.Model;

    var created = new TomlTable
    {
        ["title"] = "Generated",
        ["point"] = new TomlTable(inline: true) { ["x"] = 1L, ["y"] = 2L },
        ["released"] = new TomlDateTime(2026, 10, 7),
        ["servers"] = new TomlTableArray
        {
            new TomlTable { ["name"] = "alpha" },
            new TomlTable { ["name"] = "beta" },
        },
    };
    Console.WriteLine(TomlSerializer.Serialize(created));
    // title = "Generated"
    // point = {x = 1, y = 2}
    // released = 2026-10-07
    // [[servers]]
    // name = "alpha"
    //
    // [[servers]]
    // name = "beta"

Example 8 - Validate a file without mapping it
----------------------------------------------
    using CodeBrix.TomlParse.Parsing;

    var check = SyntaxParser.Parse(File.ReadAllText(path), sourceName: path);
    foreach (var message in check.Diagnostics)
    {
        Console.WriteLine(message);
        // e.g. settings.toml(2,1) : error : The key `a` is already defined at (1,1) ...
    }
    bool valid = !check.HasErrors;


MINIMUM VIABLE PROJECT TEMPLATE
===============================
TomlDemo.csproj:

    <Project Sdk="Microsoft.NET.Sdk">
      <PropertyGroup>
        <OutputType>Exe</OutputType>
        <TargetFramework>net10.0</TargetFramework>
      </PropertyGroup>
      <ItemGroup>
        <PackageReference Include="CodeBrix.TomlParse.BsdLicenseForever" Version="*" />
      </ItemGroup>
    </Project>

  (Pin Version to the current published version for reproducible builds.)

Program.cs:

    using System;
    using CodeBrix.TomlParse;
    using CodeBrix.TomlParse.Model;

    var table = TomlSerializer.Deserialize<TomlTable>("name = \"demo\"\nport = 8080\n");
    table["port"] = 9090L;
    Console.Write(TomlSerializer.Serialize(table));


PERFORMANCE TIPS
================
- TomlSerializerOptions is an immutable record: create it once and share it
  rather than building new options for every call.
- Prefer a source-generated context for hot paths and for trimmed or
  NativeAOT applications - its metadata is produced at compile time instead
  of by reflection at run time.
- Read from a Stream or TextReader for large files instead of first loading
  the whole file into a string.
- Use the TomlTable model only when the shape is dynamic; typed POCOs map
  faster and catch shape errors early.
- TomlParser (incremental events) and TomlLexer are allocation-light; use
  them for tools that only scan a document.
- Use TryDeserialize where invalid input is expected (user-edited config) to
  avoid the cost of exceptions.


COMMON PITFALLS TO AVOID
========================
- Keys are matched CASE-SENSITIVELY against CLR names by default
  (PropertyNamingPolicy is null, as in System.Text.Json). `port = 8080`
  does NOT bind to `public int Port` unless you set
  PropertyNamingPolicy (e.g. JsonNamingPolicy.SnakeCaseLower / CamelCase),
  PropertyNameCaseInsensitive = true, or [TomlPropertyName("port")].
  Unbound keys are silently ignored, so the symptom is default values.
- Integers in the document model are long and floats are double:
  `(int)table["port"]` throws InvalidCastException - use `(long)` and
  convert. When building a TomlTable, write 5432L, not 5432.
- Dates and times are TomlDateTime, not DateTime. Use .DateTime and .Kind.
- A plain Serialize(...) does not keep comments. Use a TomlMetadataStore in
  the options (comments kept) or edit the syntax tree (byte-exact).
- Syntax-tree scalars render their Token text, not .Value. Setting only
  IntegerValueSyntax.Value changes nothing in ToString(); replace the Token
  (Example 3) and carry its LeadingTrivia/TrailingTrivia across.
- A [TomlSerializable] context must be `partial` and derive from
  TomlSerializerContext. [JsonSerializable] on such a context is not
  supported (TOMLPARSE008) - use [TomlSerializable].
- [TomlConverter] attributes are honoured by the reflection path only; for a
  generated context register converters with
  [TomlSourceGenerationOptions(Converters = [...])].
- Disabling reflection must happen before the first TomlSerializer call:
  AppContext.SetSwitch(
      "CodeBrix.TomlParse.TomlSerializer.IsReflectionEnabledByDefault", false);
  When the app publishes with PublishAot=true the package already sets it to
  false. Override with <TomlParseIsReflectionEnabledByDefault>true</...> in
  the project file.
- TOML 1.1 is the supported version. Documents that rely on TOML 1.0-only
  restrictions being enforced (e.g. rejecting a trailing comma in an inline
  table) parse successfully under 1.1 rules.
- Duplicate keys are an error by default (TomlDuplicateKeyHandling.Error).
- Root values that are not tables cannot be serialized unless
  RootValueHandling = TomlRootValueHandling.WrapInRootKey.


WHAT THIS PACKAGE DOES NOT DO
=============================
- It does not read or write JSON, YAML or INI; it only uses System.Text.Json
  types (naming policies, attributes) for API familiarity.
- It does not validate a document against a schema; it reports TOML syntax
  errors and type-mapping errors only.
- It does not watch files or reload configuration; it is not a
  Microsoft.Extensions.Configuration provider.
- It does not support TOML versions before 1.1.
- The metadata store does not reproduce exact whitespace and blank lines -
  only the syntax tree is byte-exact.


WORKING EXAMPLES ON GITHUB
==========================
The test project exercises every feature area:
https://github.com/ellisnet/CodeBrix.TomlParse/tree/main/tests/CodeBrix.TomlParse.Tests

  Document model (TomlTable) ............ TomlTests.cs, NewApiUntypedModelTests.cs,
                                          BasicTests.cs
  Typed (reflection) serialization ...... NewApiReflectionPocoTests.cs,
                                          SerializationTests.cs,
                                          NewApiCollectionTests.cs
  Source generation ..................... NewApiSourceGenerationTests.cs,
                                          SourceGenerationDiagnosticsTests.cs,
                                          CrossProjectSourceGenerationTests.cs
  Attributes ............................ NewApiAttributeContractTests.cs,
                                          NewApiStyleAttributeTests.cs,
                                          NewApiIgnoreConditionTests.cs
  Converters ............................ NewApiBuiltInConvertersTests.cs,
                                          NewApiConverterAttributeTests.cs
  Polymorphism .......................... NewApiPolymorphismTests.cs,
                                          CrossProjectPolymorphismTests.cs
  Extension data ........................ NewApiExtensionDataTests.cs
  Object creation (replace/populate) .... NewApiObjectCreationHandlingTests.cs
  Comments via metadata store ........... NewApiMetadataStoreTests.cs,
                                          NewApiMetadataStoreRoundtripTests.cs
  Syntax tree / lossless round trip ..... SyntaxTests.cs, StandardTests.cs
  Lexer / parser events ................. NewApiParsingPipelineTests.cs,
                                          AllocationFreeParsingTests.cs
  Errors and locations .................. NewApiExceptionLocationTests.cs,
                                          NewApiUtf8StreamExceptionLocationTests.cs
  Dates and times ....................... TomlDateTimeTests.cs
  TOML 1.1 features ..................... Toml11ParsingTests.cs, Toml11ScalarTests.cs
  Conformance (toml-test suite) ......... StandardTests.cs,
                                          Toml11TomlTestInvalidFolderTests.cs


QUICK REFERENCE CARD
====================
--- Read ---
Untyped:         var t = TomlSerializer.Deserialize<TomlTable>(text);
Typed:           var c = TomlSerializer.Deserialize<MyConfig>(text, options);
Generated:       var c = TomlSerializer.Deserialize(text, Ctx.Default.MyConfig);
No-throw:        if (TomlSerializer.TryDeserialize<MyConfig>(text, out var c, options)) ...
From file:       using var s = File.OpenRead(path); TomlSerializer.Deserialize<T>(s, options);

--- Write ---
To string:       TomlSerializer.Serialize(value, options)
To stream:       TomlSerializer.Serialize(stream, value, options)

--- Document model ---
Value:           (string)t["k"], (long)t["n"], (double)t["f"], (bool)t["b"], (TomlDateTime)t["d"]
Containers:      (TomlTable)t["tbl"], (TomlArray)t["arr"], (TomlTableArray)t["aot"]
Build:           new TomlTable { ["k"] = "v", ["n"] = 1L }, new TomlTable(inline: true)

--- Options ---
snake_case keys: new TomlSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower }
Ignore case:     PropertyNameCaseInsensitive = true
Keep comments:   MetadataStore = new TomlMetadataStore()
Error file name: SourceName = path

--- Syntax tree ---
Parse:           var doc = SyntaxParser.Parse(text, sourceName: path);
Errors:          doc.HasErrors, doc.Diagnostics
Round trip:      doc.ToString()

--- Source generation ---
[TomlSerializable(typeof(MyConfig))]
internal partial class Ctx : TomlSerializerContext { }

--- Errors ---
TomlException:   ex.SourceName, ex.Line, ex.Column, ex.Offset, ex.Span, ex.Diagnostics

Target: .NET 10 or later
License: BSD-2-Clause


================================================================================
END OF AGENT-README
================================================================================

================================================================================
MAINTAINER-README: CodeBrix.TomlParse
Notes for people and agents MAINTAINING this repository - not for package
consumers
================================================================================

If you are CONSUMING the NuGet package, stop reading and open AGENT-README.txt
instead. Everything below is about the repository itself: how it is laid out,
how it builds, how it is tested, how it is packaged, and the conventions the
source follows.


PURPOSE AND SCOPE
=================
This repository produces exactly one NuGet package:

    PackageId:  CodeBrix.TomlParse.BsdLicenseForever
    Assembly:   CodeBrix.TomlParse
    Project:    src/CodeBrix.TomlParse/CodeBrix.TomlParse.csproj
    License:    BSD-2-Clause
    Consumer documentation: AGENT-README.txt (repo root)

The library is a TOML 1.1 stack: a lexer and incremental parser, a lossless
syntax tree, an untyped document model (TomlTable), and a System.Text.Json-
style serializer with a Roslyn source generator. The generator is a second
project in src/ that is NOT a package of its own: its DLL is packed into the
library's package under analyzers/dotnet/cs.


REPOSITORY LAYOUT
=================
    src/CodeBrix.TomlParse/          the library project (the only packable
                                     project in the repo)
      (root)                         TomlSerializer, TomlSerializerOptions
                                     (+ the option enums), TomlException,
                                     TomlDateTime, TomlTypeInfo<T>,
                                     ITomlTypeInfoResolver, TomlDerivedType,
                                     InternalsVisibleTo.cs
      Helpers/                       TomlFormatHelper, RFC 3339 date handling,
                                     key validation
      Model/                         TomlTable, TomlArray, TomlTableArray,
                                     TomlObject, metadata (trivia/span) types
      Parsing/                       TomlLexer, TomlParser, SyntaxParser and
                                     their options/events
      Serialization/                 attributes, TomlConverter<T>,
                                     TomlSerializerContext, TomlReader,
                                     TomlWriter, TomlMetadataStore, callbacks
        Converters/                  built-in scalar/collection converters
        Internal/                    reflection resolver, resolver pipeline,
                                     model reader/writer, pooled buffers
      Syntax/                        DocumentSyntax and every syntax node,
                                     SyntaxVisitor, SyntaxValidator,
                                     DiagnosticsBag
      Text/                          TomlSourceSpan, TomlTextPosition
      ILLink.Substitutions.xml       embedded trimmer substitution: stubs
                                     reflection off when the feature switch
                                     is false
      buildTransitive/
        CodeBrix.TomlParse.BsdLicenseForever.targets
                                     packed into the nupkg; NuGet imports it
                                     into every consumer (named for the
                                     PackageId on purpose). Sets the feature
                                     switch to false under PublishAot.

    src/CodeBrix.TomlParse.SourceGeneration/
                                     the Roslyn incremental source generator
                                     (netstandard2.0, IsPackable=false)
      TomlSerializerContextGenerator.cs
      AnalyzerReleases.Shipped.md /  analyzer release tracking for the
      AnalyzerReleases.Unshipped.md  TOMLPARSE001-011 diagnostics (RS2008)

    tests/CodeBrix.TomlParse.Tests/  the xUnit v3 test project
      TestFiles/toml-test/           the toml-test conformance corpus (valid/
                                     and invalid/ .toml files with expected
                                     .json), copied to the output folder

    CodeBrix.TomlParse.slnx          the solution; its Solution Items folder
                                     carries .gitignore, AGENT-README.txt,
                                     EXTRAS-README.txt, global.json,
                                     icon-codebrix-128.png, LICENSE,
                                     MAINTAINER-README.txt, README-INDEX.txt,
                                     README.md and THIRD-PARTY-NOTICES.txt;
                                     its Tests folder carries the test project

    global.json                      selects the Microsoft.Testing.Platform
                                     test runner. Does NOT pin an SDK version.
                                     See TESTING below.

Source folders map to namespaces (Model/ -> CodeBrix.TomlParse.Model, ...).


BUILDING
========
Standard SDK build from the repo root:

    dotnet restore CodeBrix.TomlParse.slnx
    dotnet build   CodeBrix.TomlParse.slnx
    dotnet build   CodeBrix.TomlParse.slnx --configuration Release

Expected result: 0 warnings, 0 errors - including the xUnit analyzers on the
test project. GenerateDocumentationFile is ON for the library, so every
public member carries an XML doc comment; fix CS1591 at the source and never
add <NoWarn>.

The library targets net10.0 only. The generator targets netstandard2.0 - the
family-sanctioned exception for Roslyn components, which the compiler/IDE
host loads - and therefore sets <LangVersion>latest</LangVersion> (the
netstandard2.0 default is C# 7.3, which cannot compile file-scoped
namespaces). Both exceptions are confined to that one project.

The library references the generator with OutputItemType="Analyzer"
ReferenceOutputAssembly="false", so it is built first. A packaging target in
the library csproj (_ResolveCodeBrixTomlParseGeneratorForPackaging) asks the
generator project for its output path and FAILS the pack if the DLL does not
exist, so a package can never silently ship without its generator.

THE ROSLYN HOST FLOOR
---------------------
The generator's Microsoft.CodeAnalysis.CSharp reference is the oldest
compiler that can load it in a consumer's build. It is pinned to the compiler
that ships in .NET SDK 10.0.400, the same floor as the CodeBrix.Redis
analyzers. The test project pins the same version. Move both together, and
only deliberately: raising it drops consumers on older SDKs (for them, the
reflection and document-model APIs still work but contexts are not
generated). Microsoft.CodeAnalysis.Analyzers and System.Text.Json in the
generator are build-time/compile-time only (PrivateAssets="all").


TESTING
=======
    dotnet test --solution CodeBrix.TomlParse.slnx

THE TEST RUNNER IS Microsoft.Testing.Platform (MTP), selected by global.json at
the repo root:

    { "test": { "runner": "Microsoft.Testing.Platform" } }

That file does NOT pin an SDK version, so the newest installed .NET 10 SDK is
still used; it exists solely to select the runner. Keep it committed - without
it `dotnet test` falls back to the VSTest bridge, which xUnit v3 no longer
supports on the .NET 10 SDK ("Testing with VSTest target is no longer
supported by Microsoft.Testing.Platform"). MTP output ends in a "Test run
summary:" block.

If `dotnet test --solution` ever reports that zero tests ran, run the test
assembly directly - it is a self-hosting executable:

    dotnet build tests/CodeBrix.TomlParse.Tests
    dotnet tests/CodeBrix.TomlParse.Tests/bin/Debug/net10.0/CodeBrix.TomlParse.Tests.dll

The test project is xUnit v3 + SilverAssertions, with Microsoft.NET.Test.Sdk
and xunit.runner.visualstudio. No coverage collector is referenced (the
CodeBrix family dropped coverlet.collector). It also references:
  * the generator project TWICE OVER in one item: OutputItemType="Analyzer"
    runs the generator over the [TomlSerializable] contexts the tests
    declare, and ReferenceOutputAssembly="true" lets
    SourceGenerationDiagnosticsTests drive the generator in-process through a
    CSharpGeneratorDriver;
  * Microsoft.CodeAnalysis.CSharp, for those in-process compilations.
The library grants InternalsVisibleTo to the test assembly
(src/CodeBrix.TomlParse/InternalsVisibleTo.cs).

No network access, environment variables or setup steps are required: the
toml-test corpus is committed under TestFiles/toml-test and copied next to the
test assembly on build. StandardTests runs every valid and invalid case (TOML
1.0-only spec folders and the raw-byte invalid/encoding cases are excluded, and
nine cases the corpus marks invalid that TOML 1.1 accepts are exercised by
Toml11TomlTestInvalidFolderTests instead). Theory data carries only the
corpus-relative path; the test reads the files.

TestFiles/toml-test/.gitattributes marks *.toml as -text. Keep it: several
cases depend on exact line endings, and a CRLF conversion on a Windows
checkout would break them.

SourceGenerationDiagnosticsTests.RunGeneratorTest compiles every snippet with
nullable reference types ENABLED and fails if any warning lands in a
generated file. That is the guard for the generator's `#nullable disable`
output (see NOTES).


PACKAGING AND PUBLISHING
========================
GeneratePackageOnBuild is true, so every build of the library project emits a
fresh .nupkg (bin/<Configuration>/CodeBrix.TomlParse.BsdLicenseForever.*.nupkg).

Versioning is the CodeBrix date-stamped scheme, computed in the csproj from
System.DateTime.UtcNow as 1.<years-since-base>.<day-of-year>.<minute-of-day>.
It is monotonically increasing but is NOT SemVer. Two builds inside the same
UTC minute produce the SAME version - never publish two packages from one
minute. Do not replace the version block with a literal <Version>.

What ships inside the nupkg:
    lib/net10.0/CodeBrix.TomlParse.dll + .xml
    analyzers/dotnet/cs/CodeBrix.TomlParse.SourceGeneration.dll
    buildTransitive/CodeBrix.TomlParse.BsdLicenseForever.targets
    icon-codebrix-128.png    (PackageIcon)
    README.md                (PackageReadmeFile)
    AGENT-README.txt         (the consumer guide)
    THIRD-PARTY-NOTICES.txt
The package has NO NuGet dependencies. MAINTAINER-README.txt,
EXTRAS-README.txt, README-INDEX.txt and the test corpus are repo-only.

PackageLicenseExpression is BSD-2-Clause and PackageRequireLicenseAcceptance is
true. THIRD-PARTY-NOTICES.txt travels in the package because the upstream
BSD-2-Clause licence requires binary redistributions to reproduce its notice.

Jeremy publishes; agents never push packages or tags.


CODING CONVENTIONS
==================
  - Nullable reference types are OFF (library, generator and tests). Never
    use '?' on reference types and never use the null-forgiveness '!'
    operator. Value-type nullables (int?, TomlSourceSpan?) are fine.
  - No <ImplicitUsings> and no global usings; every file lists its own using
    directives in one block, System.* first, alphabetical.
  - File-scoped namespaces only.
  - XML doc comments on every public member; fix CS1591 at the source.
  - No warning suppression anywhere (<NoWarn>, #pragma warning).
  - Ported files keep their upstream copyright header and carry
    "//was previously: <upstream-namespace>;" on the namespace line. Preserve
    both; never fabricate a header on a genuinely new file.
  - Tests: xUnit v3 + SilverAssertions fluent assertions; thread
    TestContext.Current.CancellationToken into cancellable calls. NEW test
    files are named <ClassUnderTest>Tests.cs with snake_case or
    MemberName_snake_case_description methods and //Arrange //Act //Assert
    sections; the ported upstream test methods keep their original names.


PROVENANCE AND VENDORED SOURCES
===============================
CodeBrix.TomlParse is a port of Tomlyn (https://github.com/xoofx/Tomlyn,
BSD-2-Clause, Copyright (c) 2019-2026 Alexandre Mutel), taken from the main
branch at commit 353e75d (release 2.10.1 plus one later fix). The library,
the source generator and the NUnit test suite were ported; the benchmarks,
NativeAOT smoke test, strong-name-signed project and documentation site were
not. Full notices and the list of modifications are in THIRD-PARTY-NOTICES.txt.

Namespace mapping applied to every ported file:
    Tomlyn                -> CodeBrix.TomlParse
    Tomlyn.Helpers        -> CodeBrix.TomlParse.Helpers
    Tomlyn.Model          -> CodeBrix.TomlParse.Model
    Tomlyn.Parsing        -> CodeBrix.TomlParse.Parsing
    Tomlyn.Serialization  -> CodeBrix.TomlParse.Serialization
    Tomlyn.Syntax         -> CodeBrix.TomlParse.Syntax
    Tomlyn.Text           -> CodeBrix.TomlParse.Text
    Tomlyn.SourceGeneration -> CodeBrix.TomlParse.SourceGeneration
    Tomlyn.Tests          -> CodeBrix.TomlParse.Tests
Identifiers renamed with it: generator diagnostics TOMLYN001-011 ->
TOMLPARSE001-011; feature switch -> CodeBrix.TomlParse.TomlSerializer.
IsReflectionEnabledByDefault; MSBuild property ->
TomlParseIsReflectionEnabledByDefault.

tests/CodeBrix.TomlParse.Tests/TestFiles/toml-test/ is the toml-test corpus
(https://github.com/BurntSushi/toml-test, MIT) at commit 74eb956 - the commit
upstream referenced as a git submodule - committed verbatim with its LICENSE.
To refresh it, download that repository at the new commit and replace the
valid/ and invalid/ folders (and .gitattributes) wholesale; then re-run the
suite, because new corpus cases may need entries in the TOML 1.1 exception
lists in StandardTests / Toml11TomlTestInvalidFolderTests.

The port was produced by ~/ClaudeHome/port_codebrix_tomlparse.py (namespace
and preprocessor pass) and ~/ClaudeHome/port_tomlparse_tests_nunit.py
(NUnit -> xUnit v3 + SilverAssertions), followed by the compiler-driven NRT
strip; these live outside the repository and are not needed to build it.


NOTES
=====
- Generated code is emitted under `#nullable disable` with reference-type
  names written WITHOUT `?` (FullyQualifiedNullableFormat in the generator is
  deliberately the plain fully-qualified format). Upstream emitted
  `#nullable enable` and `string?` locals; that output produced CS8764 /
  CS8669 warnings against this NRT-oblivious library and, for any consumer
  with NRT enabled, CS8669 on every nullable member. Do not reintroduce
  `#nullable enable` or the nullable display format in the generator.
- Upstream test SourceGenerationDiagnosticsTests.Generator_PreservesNullable-
  ReferenceLocals asserted the old output; it is now
  Generator_EmitsNullableObliviousLocals and asserts the new contract.
- FloatingRoundtripTests uses TheoryData instead of [InlineData] for its
  double/float cases: xUnit's duplicate-data analyzer treats 0.0 and -0.0 as
  equal, so negative zero cannot sit beside positive zero inline. Upstream's
  separate `-0.0` and `NegativeZero` cases were the same value and are one
  case here.
- Keys bind to CLR members case-sensitively by default (null
  PropertyNamingPolicy, as in System.Text.Json). That is upstream behaviour,
  documented as a consumer pitfall in AGENT-README.txt - not a bug.

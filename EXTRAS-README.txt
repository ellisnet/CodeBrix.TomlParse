================================================================================
EXTRAS-README: CodeBrix.TomlParse
Samples, tools and other content in this repository that is not part of a NuGet
package
================================================================================

This repository ships no sample applications, demos or command-line tools.
Exactly one project is packable - src/CodeBrix.TomlParse - and everything else
listed below exists only to build, test or document it. The source generator
project is the one exception that reaches consumers: it is not a package of
its own, but its DLL is packed INTO the CodeBrix.TomlParse.BsdLicenseForever
package (see MAINTAINER-README.txt).

For runnable, compilable usage of the library, read the test project: the
"WORKING EXAMPLES ON GITHUB" section of AGENT-README.txt maps each feature area
to the test file that exercises it.


SOURCE GENERATOR PROJECT
========================
    src/CodeBrix.TomlParse.SourceGeneration/

The Roslyn source generator for [TomlSerializable] contexts. netstandard2.0,
IsPackable=false; shipped inside the library package under
analyzers/dotnet/cs.


TEST PROJECT
============
    tests/CodeBrix.TomlParse.Tests/

The only non-package project in the solution besides the generator. xUnit v3;
run it as described in MAINTAINER-README.txt (TESTING).

    tests/CodeBrix.TomlParse.Tests/TestFiles/toml-test/

Test data only: the toml-test TOML conformance corpus (MIT; its LICENSE file
sits beside it, and THIRD-PARTY-NOTICES.txt records the source commit). It is
copied to the test output folder and never packed.

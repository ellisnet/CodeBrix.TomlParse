; Unshipped analyzer releases
; https://github.com/dotnet/roslyn-analyzers/blob/main/src/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
TOMLPARSE001 | CodeBrix.TomlParse.SourceGeneration | Error | Toml serializer context must be partial
TOMLPARSE002 | CodeBrix.TomlParse.SourceGeneration | Error | Invalid converter type
TOMLPARSE003 | CodeBrix.TomlParse.SourceGeneration | Error | Unsupported member type
TOMLPARSE004 | CodeBrix.TomlParse.SourceGeneration | Error | Unsupported dictionary key type
TOMLPARSE005 | CodeBrix.TomlParse.SourceGeneration | Error | Invalid source generation option
TOMLPARSE006 | CodeBrix.TomlParse.SourceGeneration | Error | Invalid extension data member
TOMLPARSE007 | CodeBrix.TomlParse.SourceGeneration | Error | Invalid polymorphism configuration
TOMLPARSE008 | CodeBrix.TomlParse.SourceGeneration | Warning | JsonSerializable is not supported on TOML contexts
TOMLPARSE009 | CodeBrix.TomlParse.SourceGeneration | Error | Invalid derived type mapping
TOMLPARSE010 | CodeBrix.TomlParse.SourceGeneration | Warning | Derived type mapping base type has no polymorphic configuration
TOMLPARSE011 | CodeBrix.TomlParse.SourceGeneration | Error | Invalid TOML attribute usage

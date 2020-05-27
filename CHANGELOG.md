# F0.Analyzers
CHANGELOG

## vNext

## v0.3.0 (2020-05-27)
### Analyzers
- Added `GoToStatementConsideredHarmful` diagnostic analyzer, reporting _Warning_ if a `goto` statement is used.

## v0.2.0 (2020-05-21)
### Analyzers
- Changed `ObjectInitializer` code refactoring, assigning `default operator` if language version is C# 7.0 or lower.
- Changed `ObjectInitializer` code refactoring, registering code action only if language version is C# 3.0 or greater.

### NuGet package
- Marked as a development-only dependency, which prevents the package from being included as a dependency in other packages.

## v0.1.0 (2020-05-14)
### Analyzers
- Added `ObjectInitializer` code refactoring, assigning `default literal` to all publicly settable fields and properties.

### NuGet package
- Uploaded first version, targeting `.NET Standard 1.3`.

### Visual Studio Extension
- Published initial version, supporting `Visual Studio 2017` and `Visual Studio 2019`.

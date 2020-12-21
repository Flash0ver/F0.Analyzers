# F0.Analyzers
CHANGELOG

## vNext
### Visual Studio Extension
- Removed _install_ and _uninstall_ scripts (NuGet tools).

## v0.6.0 (2020-11-21)
### Analyzers
- Changed `ObjectInitializer` code refactoring, additionally assigning inherited members from `base` classes.

## v0.5.0 (2020-09-29)
### Analyzers
- Changed `ObjectInitializer` code refactoring, additionally assigning `internal` fields and properties within files in the same assembly or _friend_ assemblies.

## v0.4.1 (2020-07-11)
### Analyzers
- Fixed `ObjectInitializer` code refactoring, assigning values to static members, causing compiler error CS1914.

## v0.4.0 (2020-06-26)
### Analyzers
- Changed `ObjectInitializer` code refactoring, assigning matching _locals_ and _members_ if existing in the current context.

### NuGet package
- Suppressed package dependencies.

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

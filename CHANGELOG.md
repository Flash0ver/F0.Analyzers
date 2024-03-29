# F0.Analyzers
CHANGELOG

## vNext

## v0.10.0 (2022-03-21)
### Analyzers
- Added `IdentifiersShouldNotContainUnderscores` diagnostic suppressor, suppressing _Warning CA1707_ for _MSTest_, _NUnit_ and _xUnit.net_ test methods.

## v0.9.0 (2022-02-04)
### Analyzers
- Added `ImplicitRecordClassDeclaration` diagnostic analyzer, reporting _Warning F02001_ for `record` declarations without the `class` or `struct` keyword.
- Added `DeclareRecordClassExplicitly` code fix provider, adding the optional `class` keyword to `record` declarations that are _reference types_.
- Fixed `PreferPatternMatchingNullCheckOverComparisonWithNull` diagnostic analyzer, throwing `System.NullReferenceException` on invocations of `bool`-returning `static` methods named `Equals` with a single `null` argument.

### NuGet package
- Added support for `.NET SDK 6.0`.
- Removed support for `.NET SDK 5.0`.

### Visual Studio Extension
- Added support for `Visual Studio 2022`.
- Removed support for `Visual Studio 2017` and `Visual Studio 2019`.

## v0.8.0 (2021-10-13)
### Analyzers
- Changed `ObjectInitializer` code refactoring, additionally supporting _implicit object creation expressions_ (i.e. _target-typed `new` expressions_), introduced in _C# 9.0_.

## v0.7.1 (2021-06-13)
### Analyzers
- Fixed `UsePatternMatchingNullCheckInsteadOfComparisonWithNull` code fix provider, when fixing `null` check expressions in arguments.

## v0.7.0 (2021-05-11)
### Analyzers
- Added `PreferPatternMatchingNullCheckOverComparisonWithNull` diagnostic analyzer, reporting _Warning F01001_ for `null` check by equality, reporting _Info F01002_ for `null` check by identity.
- Added `UsePatternMatchingNullCheckInsteadOfComparisonWithNull` code fix provider, replacing both equality `null` checks (_F01001_) and identity `null` checks (_F01002_) with either `is null` or `is not null` pattern expressions.

### NuGet package
- Changed target framework from `.NET Standard 1.3` to `.NET Standard 2.0`.

## v0.6.1 (2021-04-18)
### Analyzers
- Fixed `ObjectInitializer` code refactoring, no longer registering code actions with collection types.

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
- Added `GoToStatementConsideredHarmful` diagnostic analyzer, reporting _Warning F00001_ if a `goto` statement is used.

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

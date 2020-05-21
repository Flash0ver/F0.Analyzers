# F0.Analyzers
Code refactorings and diagnostic analyzers with code fixes for C# projects, based on the .NET Compiler Platform ("Roslyn") Code Analysis.

[![F0.Analyzers on fuget.org](https://www.fuget.org/packages/F0.Analyzers/badge.svg)](https://www.fuget.org/packages/F0.Analyzers)

## NuGet
[NuGet package](https://www.nuget.org/packages/F0.Analyzers/)

## Visual Studio Marketplace
[Visual Studio Extension](https://marketplace.visualstudio.com/items?itemName=Flash0Ware.F0-Analyzers-VS)

## API Browser / Package Viewer
[fuget](https://www.fuget.org/packages/F0.Analyzers)\
[DotNetApis](http://dotnetapis.com/pkg/F0.Analyzers)\
[NuGet Must Haves](https://nugetmusthaves.com/Package/F0.Analyzers)\
[NuGet Trends](https://nugettrends.com/packages?months=12&ids=F0.Analyzers)

## Remarks
Consuming _code refactorings_ via `PackageReference` requires at least _Visual Studio 2019 version 16.5.0_.

> Analyzer authors can now distribute custom code refactorings as a NuGet package when previously it could only be distributed as a VSIX.

## Code Refactorings

### Object Initializer

_C# 3.0 or greater_

This code refactoring adds an object initializer, assigning the `default value expression` to all publicly settable fields and properties.
- C# 7.0 or lower: `default operator`
- C# 7.1 or greater: `default literal`

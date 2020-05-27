# Object Initializer

CodeRefactoringProvider: [ObjectInitializer.cs](../../source/production/F0.Analyzers/CodeAnalysis/CodeRefactorings/ObjectInitializer.cs)

|            |                           |
|------------|---------------------------|
| Title      | Create Object Initializer |
| Language   | C# 3.0 or greater         |
| Applies to | `[0.1.0,)`                |

## Summary

This code refactoring adds an object initializer, assigning the `default value expression` to all publicly settable fields and properties.
- C# 7.0 or lower: `default operator`
- C# 7.1 or greater: `default literal`

## Remarks

C# language features
- The `default operator` feature is available since C# 2.0.
- The `object initializer` feature is available since C# 3.0.
- The `default literal` feature is available since C# 7.1.

This code refactoring (titled `Create Object Initializer`) creates an object initializer from any object creation expression (`new operator`) and initializes all members which are both accessible and non-readonly by assigning the default value.

If the csproj's `LangVersion` is C# 7.1 or greater, then the `default literal` is assigned, else the `default operator` is assigned. For language versions lower than C# 3.0 no code refactoring is registered.

## Example

Before:
```cs
new ValueTuple<bool, int, string>();
```

After:
```cs
new ValueTuple<bool, int, string>()
{
    Item1 = default,
    Item2 = default,
    Item3 = default
};
```

## See also

- [Object initializers](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/object-and-collection-initializers#object-initializers)
- [default value expressions](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/operators/default)
- [new operator](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/operators/new-operator)
- [C# version 2.0](https://docs.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-version-history#c-version-20)
- [C# version 3.0](https://docs.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-version-history#c-version-30)
- [C# version 7.1](https://docs.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-version-history#c-version-71)

## History

- [0.2.0](../../CHANGELOG.md#v020-2020-05-21)
- [0.1.0](../../CHANGELOG.md#v010-2020-05-14)
